using NetPad.CQs;
using NetPad.Scripts;
using NetPad.UiInterop;

namespace NetPad.Web.UiInterop;

public class WebWindowService : IUiWindowService
{
    private readonly IIpcService _ipcService;

    public WebWindowService(IIpcService ipcService)
    {
        _ipcService = ipcService;
    }

    public Task MaximizeMainWindowAsync()
    {
        return Task.CompletedTask;
    }

    public Task MinimizeMainWindowAsync()
    {
        return Task.CompletedTask;
    }

    public Task ToggleAlwaysOnTopMainWindowAsync()
    {
        return Task.CompletedTask;
    }

    public Task OpenMainWindowAsync()
    {
        throw new PlatformNotSupportedException();
    }

    public async Task OpenSettingsWindowAsync(string? tab = null)
    {
        var command = new OpenWindowCommand("settings");
        command.Options.Height = 0.5;
        command.Options.Width = 0.5;

        if (tab != null) command.Metadata.Add("tab", tab);

        await _ipcService.SendAsync(command);
    }

    public async Task OpenScriptConfigWindowAsync(Script script, string? tab = null)
    {
        var command = new OpenWindowCommand("script-config");
        command.Options.Height = 2 / 3.0;
        command.Options.Width = 4 / 5.0;

        command.Metadata.Add("script-id", script.Id);
        if (tab != null) command.Metadata.Add("tab", tab);

        await _ipcService.SendAsync(command);
    }

    public async Task OpenDataConnectionWindowAsync(Guid? dataConnectionId)
    {
        var command = new OpenWindowCommand("data-connection");
        command.Options.Height = 2 / 3.0;
        command.Options.Width = 4 / 5.0;

        if (dataConnectionId != null) command.Metadata.Add("data-connection-id", dataConnectionId);

        await _ipcService.SendAsync(command);
    }
}
