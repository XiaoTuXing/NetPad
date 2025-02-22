import {bindable, ILogger} from "aurelia";
import {ViewModelBase} from "@application";

export class TooltipCustomAttribute extends ViewModelBase {
    @bindable text?: string;

    constructor(private readonly element: Element, @ILogger logger: ILogger) {
        super(logger);
    }

    public async attached() {
        await this.getOrInitTooltip();
    }

    private async getOrInitTooltip() {
        const Tooltip = (await import("bootstrap")).Tooltip;

        let tooltip = Tooltip.getInstance(this.element);

        if (!tooltip && this.text) {
            tooltip = new Tooltip(this.element, {
                title: this.text,
                animation: true
            });

            this.addDisposable(tooltip);
        }

        return tooltip;
    }

    private async textChanged(newValue: string) {
        (await this.getOrInitTooltip())?.setContent({ ".tooltip-inner": newValue });
    }
}
