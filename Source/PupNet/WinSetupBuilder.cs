// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-23
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/PupNet
//
// PupNet is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// PupNet is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with PupNet. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

using System.Runtime.InteropServices;
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Windows Setup package.
/// </summary>
public class WinSetupBuilder : PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public WinSetupBuilder(ConfigurationReader conf)
        : base(conf, PackKind.WinSetup)
    {
        PublishBin = Path.Combine(AppRoot, "Publish");
        DesktopExec = AppExecName;

        // We do not set the content here
        ManifestPath = Path.Combine(Root, Configuration.AppBaseName + ".iss");
        ManifestContent = GetInnoFile();

        var list = new List<string>();
        list.Add($"iscc /O\"{OutputDirectory}\" \"{ManifestPath}\"");
        PackageCommands = list;
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string DesktopExec{ get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string PublishBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestPath { get; }

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
    public override bool SupportsRunOnBuild { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool CheckInstalled()
    {
        return WriteVersion("issc", "/version", true);
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void WriteVersion()
    {
        WriteVersion("issc", "/version");
    }

    private string GetInnoFile()
    {
        // We don't actually need install, build sections.
        var sb = new StringBuilder();

        sb.AppendLine($"[Setup]");
        sb.AppendLine($"AppName={Configuration.AppFriendlyName}");
        sb.AppendLine($"AppVersion={AppVersion}");
        sb.AppendLine($"AppVerName={Configuration.AppFriendlyName} {AppVersion}");
        sb.AppendLine($"VersionInfoVersion={AppVersion}");
        sb.AppendLine($"OutputDir={OutputDirectory}");
        sb.AppendLine($"OutputBaseFilename={Path.GetFileNameWithoutExtension(OutputName)}");
        sb.AppendLine($"AppPublisher={Configuration.AppVendor}");

        sb.AppendLine($"AppCopyright=Copyright (C) {Configuration.AppVendor}");

        if (!string.IsNullOrEmpty(Configuration.AppUrl))
        {
            sb.AppendLine($"AppPublisherURL={Configuration.AppUrl}");
            sb.AppendLine($"AppSupportURL={Configuration.AppUrl}");
            sb.AppendLine($"AppUpdatesURL={Configuration.AppUrl}");
        }

        // SignTool=
        // https://jrsoftware.org/ishelp/index.php?topic=filessection&anchor=signonce

        sb.AppendLine($"DefaultDirName={{autopf}}\\{Configuration.AppBaseName}");

        // sb.AppendLine($"LicenseFile={#BUILD_DIR}\LICENSE");
        // sb.AppendLine($"InfoBeforeFile={#BUILD_DIR}\CHANGES");

        if (!string.IsNullOrEmpty(IconSource))
        {
            sb.AppendLine($"SetupIconFile={IconSource}");
        }

        sb.AppendLine($"AllowNoIcons=yes");
        sb.AppendLine($"MinVersion=6.2");
        sb.AppendLine($"AppId={Configuration.AppId}");

        sb.AppendLine($"ArchitecturesAllowed={Architecture}");
        sb.AppendLine($"ArchitecturesInstallIn64BitMode={Architecture}");

        sb.AppendLine();
        sb.AppendLine($"[Files]");
        sb.AppendLine($"Source: \"{PublishBin}\\*.exe\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs signonce;");
        sb.AppendLine($"Source: \"{PublishBin}\\*.dll\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs signonce;");
        sb.AppendLine($"Source: \"{PublishBin}\\*\"; Excludes: \"*.exe,*.dll\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs;");

        sb.AppendLine();
        sb.AppendLine("[Tasks]");
        sb.AppendLine($"Name: \"desktopicon\"; Description: \"Create a &Desktop Icon\"; GroupDescription: \"Additional icons:\"; Flags: unchecked");
        sb.AppendLine();
        sb.AppendLine($"[REGISTRY]");
        sb.AppendLine();
        sb.AppendLine($"[Icons]");
        sb.AppendLine($"Name: \"{{group}}\\{Configuration.AppFriendlyName}\"; Filename: \"{{app}}\\{AppExecName}\"");
        sb.AppendLine($"Name: \"{{userdesktop}}\\{Configuration.AppFriendlyName}\"; Filename: \"{{app}}\\{AppExecName}\"; Tasks: desktopicon");
        sb.AppendLine();
        sb.AppendLine($"[Run]");
        sb.AppendLine($"Filename: \"{{app}}\\{AppExecName}\"; Description: Start Application Now; Flags: postinstall nowait skipifsilent");
        sb.AppendLine();
        sb.AppendLine($"[InstallDelete]");
        sb.AppendLine($"Type: filesandordirs; Name: \"{{app}}\\*\";");
        sb.AppendLine();
        sb.AppendLine($"[UninstallRun]");
        sb.AppendLine();
        sb.AppendLine($"[UninstallDelete]");
        sb.AppendLine($"Type: dirifempty; Name: \"{{app}}\"");

        return sb.ToString().TrimEnd();
    }

}

