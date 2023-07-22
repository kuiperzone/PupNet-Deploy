// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-23
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/PupNet
//
// PupNet is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// PupNet is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along
// with PupNet. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

using System.Text;

namespace KuiperZone.PupNet.Builders;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Windows Setup package.
/// Leverages InnoSetup. Application installed into user space.
/// </summary>
public class SetupBuilder : PackageBuilder
{
    private const string PromptBat = "CommandPrompt.bat";

    /// <summary>
    /// Constructor.
    /// </summary>
    public SetupBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Setup)
    {
        BuildAppBin = Path.Combine(BuildRoot, "Publish");

        // Not used. This is decided at install time via interaction with user.
        InstallBin = "";

        // We do not set the content here
        ManifestBuildPath = Path.Combine(Root, Configuration.AppBaseName + ".iss");
        ManifestContent = GetInnoFile();

        var list = new List<string>();
        list.Add($"iscc /O\"{OutputDirectory}\" \"{ManifestBuildPath}\"");
        PackageCommands = list;
    }

    /// <summary>
    /// Gets terminal windows icon.
    /// </summary>
    public static string TerminalIcon { get; } = Path.Combine(AssemblyDirectory, "terminal.ico");

    /// <summary>
    /// Implements.
    /// </summary>
    public override string OutputName
    {
        get { return GetOutputName(Configuration.SetupVersionOutput, Configuration.SetupSuffixOutput, Architecture, ".exe"); }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string Architecture
    {
        get
        {
            if (Arguments.Arch != null)
            {
                return Arguments.Arch;
            }

            // Where supported, these seem to match the Architecture enum names.
            // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesallowed
            return Runtime.RuntimeArch.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string BuildAppBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string InstallBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestBuildPath { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestContent { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsStartCommand { get; } = true;

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsPostRun { get; } = false;

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void Create(string? desktop, string? metainfo)
    {
        base.Create(desktop, metainfo);

        if (Configuration.StartCommand != null &&
            !Configuration.StartCommand.Equals(AppExecName, StringComparison.InvariantCultureIgnoreCase))
        {
            var path = Path.Combine(BuildAppBin, Configuration.StartCommand + ".bat");
            var script = $"start {InstallExec} %*";
            Operations.WriteFile(path, script);
        }

        if (Configuration.SetupCommandPrompt != null)
        {
            var title = EscapeBat(Configuration.SetupCommandPrompt);
            var cmd = EscapeBat(Configuration.StartCommand ?? Configuration.AppBaseName);
            var path  = Path.Combine(BuildAppBin, PromptBat);

            var echoCopy = Configuration.PublisherCopyright != null ? $"& echo {EscapeBat(Configuration.PublisherCopyright)}" : null;

            var script = $"start cmd /k \"cd /D %userprofile% & title {title} & echo {cmd} {AppVersion} {echoCopy} & set path=%path%;%~dp0\"";
            Operations.WriteFile(path, script);

        }
    }

    private static string? EscapeBat(string? s)
    {
        // \ & | > < ^
        s = s?.Replace("^", "^^");

        s = s?.Replace("\\", "^\\");
        s = s?.Replace("&", "^&");
        s = s?.Replace("|", "^|");
        s = s?.Replace("<", "^<");
        s = s?.Replace(">", "^>");

        s = s?.Replace("%", "");

        return s;
    }

    private string GetInnoFile()
    {
        // We don't actually need install, build sections.
        var sb = new StringBuilder();

        sb.AppendLine($"[Setup]");
        sb.AppendLine($"AppName={Configuration.AppFriendlyName}");
        sb.AppendLine($"AppId={Configuration.AppId}");
        sb.AppendLine($"AppVersion={AppVersion}");
        sb.AppendLine($"AppVerName={Configuration.AppFriendlyName} {AppVersion}");
        sb.AppendLine($"VersionInfoVersion={AppVersion}");
        sb.AppendLine($"OutputDir={OutputDirectory}");
        sb.AppendLine($"OutputBaseFilename={Path.GetFileNameWithoutExtension(OutputName)}");
        sb.AppendLine($"AppPublisher={Configuration.PublisherName}");
        sb.AppendLine($"AppCopyright={Configuration.PublisherCopyright}");
        sb.AppendLine($"AppPublisherURL={Configuration.PublisherLinkUrl}");
        sb.AppendLine($"InfoBeforeFile={Configuration.AppChangeFile}");
        sb.AppendLine($"LicenseFile={Configuration.AppLicenseFile}");
        sb.AppendLine($"SetupIconFile={PrimaryIcon}");

        sb.AppendLine($"DefaultGroupName={Configuration.AppFriendlyName}");
        sb.AppendLine($"DefaultDirName={{autopf}}\\{Configuration.AppBaseName}");

        sb.AppendLine($"AllowNoIcons=yes");
        sb.AppendLine($"MinVersion={Configuration.SetupMinWindowsVersion}");

        if (Architecture == "x64" || Architecture == "arm64")
        {
            // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesallowed
            sb.AppendLine($"ArchitecturesAllowed={Architecture}");

            // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesinstallin64bitmode
            sb.AppendLine($"ArchitecturesInstallIn64BitMode={Architecture}");
        }


        sb.AppendLine($"PrivilegesRequired={(Configuration.SetupAdminInstall ? "admin" : "lowest")}");

        if (PrimaryIcon != null)
        {
            sb.AppendLine($"UninstallDisplayIcon={{app}}\\{Path.GetFileName(PrimaryIcon)}");
        }

        if (!string.IsNullOrEmpty(Configuration.SetupSignTool))
        {
            sb.AppendLine($"SignTool={Configuration.SetupSignTool}");
        }

        sb.AppendLine();
        sb.AppendLine($"[Files]");
        sb.AppendLine($"Source: \"{BuildAppBin}\\*.exe\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs signonce;");
        sb.AppendLine($"Source: \"{BuildAppBin}\\*.dll\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs signonce;");
        sb.AppendLine($"Source: \"{BuildAppBin}\\*\"; Excludes: \"*.exe,*.dll\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs;");

        if (PrimaryIcon != null)
        {
            sb.AppendLine($"Source: \"{PrimaryIcon}\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs;");
        }

        if (Configuration.SetupCommandPrompt != null)
        {
            // Need this below
            sb.AppendLine($"Source: \"{TerminalIcon}\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs;");
        }

        sb.AppendLine();
        sb.AppendLine("[Tasks]");

        if (!Configuration.DesktopNoDisplay)
        {
            sb.AppendLine($"Name: \"desktopicon\"; Description: \"Create a &Desktop Icon\"; GroupDescription: \"Additional icons:\"; Flags: unchecked");
        }

        sb.AppendLine();
        sb.AppendLine($"[REGISTRY]");
        sb.AppendLine();
        sb.AppendLine($"[Icons]");

        if (!Configuration.DesktopNoDisplay)
        {
            sb.AppendLine($"Name: \"{{group}}\\{Configuration.AppFriendlyName}\"; Filename: \"{{app}}\\{AppExecName}\"");
            sb.AppendLine($"Name: \"{{userdesktop}}\\{Configuration.AppFriendlyName}\"; Filename: \"{{app}}\\{AppExecName}\"; Tasks: desktopicon");
        }

        // Still put CommandPrompt and Home Page link DesktopNoDisplay is true
        if (Configuration.SetupCommandPrompt != null)
        {
            // Give special terminal icon rather meaningless default .bat icon
            var name = Path.GetFileName(TerminalIcon);
            sb.AppendLine($"Name: \"{{group}}\\{Configuration.SetupCommandPrompt}\"; Filename: \"{{app}}\\{PromptBat}\"; IconFilename: \"{{app}}\\{name}\"");
        }

        if (Configuration.PublisherLinkName != null && Configuration.PublisherLinkUrl != null)
        {
            sb.AppendLine($"Name: \"{{group}}\\{Configuration.PublisherLinkName}\"; Filename: \"{Configuration.PublisherLinkUrl}\"");
        }

        sb.AppendLine();
        sb.AppendLine($"[Run]");

        if (!Configuration.DesktopNoDisplay)
        {
            sb.AppendLine($"Filename: \"{{app}}\\{AppExecName}\"; Description: Start Application Now; Flags: postinstall nowait skipifsilent");
        }

        sb.AppendLine();
        sb.AppendLine("[InstallDelete]");
        sb.AppendLine("Type: filesandordirs; Name: \"{app}\\*\";");
        sb.AppendLine("Type: filesandordirs; Name: \"{group}\\*\";");
        sb.AppendLine();
        sb.AppendLine("[UninstallRun]");
        sb.AppendLine();
        sb.AppendLine("[UninstallDelete]");
        sb.AppendLine("Type: dirifempty; Name: \"{app}\"");

        return sb.ToString().TrimEnd();
    }

}

