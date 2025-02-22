import {CancellationToken, editor, languages, Range} from "monaco-editor";
import {IScriptService, ISession} from "@domain";
import {EditorUtil, ICodeActionProvider, ICommandProvider} from "@application";
import {IOmniSharpService} from "../omnisharp-service";
import {Converter, TextChangeUtil} from "../utils";
import * as api from "../api";

export class OmniSharpCodeActionProvider implements ICodeActionProvider, ICommandProvider {
    private readonly commandId = "omnisharp.runCodeAction";
    private readonly excludedCodeActionIdentifiers: (string | ((str: string) => boolean))[] = [
        "Convert_to_Program_Main_style_program",
        "Remove Unnecessary Usings",
        "Convert_to_Program_Main_style_program",
        id => id.indexOf("in new file") >= 0,
        id => id.indexOf("Move type ") >= 0,
        id => id.indexOf("Rename file ") >= 0,
    ];

    constructor(
        @IOmniSharpService private readonly omnisharpService: IOmniSharpService,
        @ISession private readonly session: ISession,
        @IScriptService private readonly scriptService: IScriptService) {
    }

    public provideCommands(): { id: string; handler: (accessor: unknown, ...args: unknown[]) => void; }[] {
        return [{
            id: this.commandId,
            handler: (accessor: unknown, ...args: unknown[]) => {
                return this.runCodeAction(args[0] as string, args[1] as editor.ITextModel, args[2] as api.RunCodeActionRequest);
            }
        }];
    }

    public async provideCodeActions(model: editor.ITextModel, range: Range, context: languages.CodeActionContext, token: CancellationToken): Promise<languages.CodeActionList> {
        const scriptId = EditorUtil.getScriptId(model);

        const request = new api.GetCodeActionsRequest({
            line: range.startLineNumber,
            column: range.startColumn,
            applyChangesTogether: false,
            selection: !range ? undefined : Converter.monacoRangeToApiRange(range)
        });

        const response = await this.omnisharpService.getCodeActions(scriptId, request, new AbortController().signalFrom(token));

        if (!response || !response.codeActions) {
            return {
                actions: [],
                dispose: () => {
                    // do nothing
                }
            }
        }

        const codeActions: languages.CodeAction[] = [];

        for (const codeAction of this.filterCodeActions(response.codeActions)) {
            const runRequest = new api.RunCodeActionRequest({
                identifier: codeAction.identifier,
                line: request.line,
                column: request.column,
                applyChangesTogether: request.applyChangesTogether,
                selection: request.selection,
                applyTextChanges: false,
                wantsTextChanges: true,
                wantsAllCodeActionOperations: false
            });

            const codeActionName = codeAction.name || "(no name)";

            codeActions.push({
                title: codeActionName,
                command: {
                    id: this.commandId,
                    title: codeActionName,
                    arguments: [scriptId, model, runRequest]
                }
            });
        }

        return {
            actions: codeActions,
            dispose: () => {
                // do nothing
            }
        }
    }

    private async runCodeAction(scriptId: string, model: editor.ITextModel, runRequest: api.RunCodeActionRequest) {

        const versionBeforeRequest = model.getVersionId();

        const response = await this.omnisharpService.runCodeAction(scriptId, runRequest);

        if (!response || !response.changes || versionBeforeRequest !== model.getVersionId()) {
            return true;
        }

        const modifications: api.LinePositionSpanTextChange[] = [];

        for (const change of response.changes) {
            if (change.modificationType === "Modified") {
                modifications.push(...(change as api.ModifiedFileResponse).changes);
            } else if (change.modificationType === "Renamed") {
                console.warn("Not handling Rename modification types")
            } else if (change.modificationType === "Opened") {
                console.warn("Not handling Open modification types")
            }
        }

        if (modifications.length) {
            await TextChangeUtil.applyTextChanges(model, modifications, this.session, this.scriptService);
        }
    }

    private filterCodeActions(actions: api.OmniSharpCodeAction[]): api.OmniSharpCodeAction[] {
        return actions.filter(a => {
            if (!a.identifier)
                return true;

            for (const excludedCodeActionIdentifier of this.excludedCodeActionIdentifiers) {
                if (typeof excludedCodeActionIdentifier === "string") {
                    if (a.identifier.indexOf(excludedCodeActionIdentifier) >= 0)
                        return false;
                } else if (excludedCodeActionIdentifier(a.identifier))
                    return false;
            }

            return true;
        });
    }
}
