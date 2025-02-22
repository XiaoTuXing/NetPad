import {IBackgroundService, WithDisposables} from "@common";
import {ConfirmSaveCommand, IEventBus, IIpcGateway, RequestNewScriptNameCommand, YesNoCancel} from "@domain";

/**
 * This is utilized for the Web app, not the Electron app.
 * This enables opening specific dialog windows when running the web app.
 */
export class WebDialogBackgroundService extends WithDisposables implements IBackgroundService {
    constructor(@IEventBus readonly eventBus: IEventBus,
                @IIpcGateway readonly ipcGateway: IIpcGateway
    ) {
        super();
    }

    public start(): Promise<void> {
        this.addDisposable(
            this.eventBus.subscribeToServer(ConfirmSaveCommand, async msg => {
                await this.confirmSave(msg);
            })
        );

        this.addDisposable(
            this.eventBus.subscribeToServer(RequestNewScriptNameCommand, async msg => {
                await this.requestNewScriptName(msg);
            })
        );

        return Promise.resolve(undefined);
    }

    public stop(): void {
        this.dispose();
    }

    private async confirmSave(command: ConfirmSaveCommand) {
        const ync: YesNoCancel = confirm(command.message) ? "Yes" : "No";

        await this.ipcGateway.send("Respond", command.id, ync);
    }

    private async requestNewScriptName(command: RequestNewScriptNameCommand) {
        const newName = prompt("Name:", command.currentScriptName);

        await this.ipcGateway.send("Respond", command.id, newName || null);
    }
}
