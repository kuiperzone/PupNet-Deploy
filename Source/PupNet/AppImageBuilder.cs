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

namespace KuiperZone.PupNet;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for AppImage package.
/// </summary>
public class AppImageBuilder : PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public AppImageBuilder(ConfigurationReader conf)
        : base(conf, PackKind.AppImage)
    {
        PublishBin = BuildUsrBin ?? throw new ArgumentNullException(nameof(BuildUsrBin));
        DesktopExec = $"usr/bin/{AppExecName}";

        // Not used
        ManifestPath = null;
        ManifestContent = null;

        var cmds = new List<string>();

        // Do the build
        cmds.Add($"{AppImageTool} {Configuration.AppImageArgs} \"{AppRoot}\" \"{OutputPath}\"");

        if (Arguments.IsRun)
        {
            cmds.Add(OutputPath);
        }

        PackageCommands = cmds;
    }

    /// <summary>
    /// Gets full path to embedded appimagetool. Null if architecture not supported.
    /// </summary>
    public static string? AppImageTool { get; } = GetAppImageTool();

    /// <summary>
    /// Overrides.
    /// </summary>
    public override string? MetaInfoPath
    {
        get
        {
            if (BuildShareMeta != null)
            {
                // Older style name currently required
                return Path.Combine(BuildShareMeta, Configuration.AppId + ".appdata.xml");
            }

            return null;
        }
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
    public override bool SupportsRunOnBuild { get; } = true;

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool CheckInstalled()
    {
        return AppImageTool != null;
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void WriteVersion()
    {
        WriteVersion(AppImageTool, "--version");
    }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void Create(string? desktop, string? metainfo)
    {
        base.Create(desktop, metainfo);

        // We need a bodge fix to get AppImage to pass validation. We need desktop and meta in
        // two places, one place for AppImage builder itself, and the other to get the meta to
        // pass validation. See: https://github.com/AppImage/AppImageKit/issues/603#issuecomment-355105387
        Operations.WriteFile(Path.Combine(AppRoot, Configuration.AppId + ".desktop"), desktop);
        Operations.WriteFile(Path.Combine(AppRoot, Configuration.AppId + ".appdata.xml"), metainfo);

        if (IconSource != null)
        {
            Operations.CopyFile(IconSource, Path.Combine(AppRoot, Configuration.AppId + Path.GetExtension(IconSource)));
        }

        // IMPORTANT - Create AppRun link
        // ln -s {target} {link}
        Operations.Execute($"ln -s \"{DesktopExec}\" \"{Path.Combine(AppRoot, "AppRun")}\"");
    }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void BuildPackage()
    {
        if (Arguments.Arch != null)
        {
            // Used by AppImageTool
            Environment.SetEnvironmentVariable("ARCH", Arguments.Arch);
        }

        base.BuildPackage();
    }

    private static string? GetAppImageTool()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X64)
            {
                return Path.Combine(AssemblyDirectory, "appimagetool-x86_64.AppImage");
            }

            if (RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
            {
                return Path.Combine(AssemblyDirectory, "appimagetool-aarch64.AppImage");
            }
        }

        return null;
    }

}

