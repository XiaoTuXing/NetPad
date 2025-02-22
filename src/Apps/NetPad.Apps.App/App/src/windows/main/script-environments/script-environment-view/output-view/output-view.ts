import {bindable} from "aurelia";
import {ScriptEnvironment, Settings} from "@domain";

export class OutputView {
    @bindable public environment: ScriptEnvironment;
    @bindable public close: () => void;
    public view = "Results";

    constructor(private settings: Settings) {
    }
}
