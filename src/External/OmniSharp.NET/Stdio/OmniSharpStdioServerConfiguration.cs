using System;
using System.Diagnostics;

namespace OmniSharp.Stdio
{
    internal class OmniSharpStdioServerConfiguration : OmniSharpServerConfiguration
    {
        public OmniSharpStdioServerConfiguration(Func<Process> processGetter) : base(OmniSharpServerProtocolType.Stdio)
        {
            ProcessGetter = processGetter ?? throw new ArgumentNullException(nameof(processGetter));
        }

        public OmniSharpStdioServerConfiguration(string executablePath, string args) : base(OmniSharpServerProtocolType.Stdio)
        {
            ExecutablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
            ExecutableArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public Func<Process>? ProcessGetter { get; }
        public string? ExecutablePath { get; }
        public string? ExecutableArgs { get; }
        public bool ExternallyManagedProcess => ProcessGetter != null;
    }
}
