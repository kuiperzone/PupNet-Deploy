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

using System.Runtime.InteropServices;

namespace KuiperZone.PupNet.Builders;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for AppImage package.
/// https://docs.appimage.org/reference/appdir.html
/// </summary>
public class AppImageBuilder : PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public AppImageBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.AppImage)
    {
        BuildAppBin = BuildUsrBin ?? throw new ArgumentNullException(nameof(BuildUsrBin));

        // No leading '/' here
        InstallBin = "usr/bin";

        // Not used
        ManifestBuildPath = null;
        ManifestContent = null;

        var cmds = new List<string>();

        // Do the build
        cmds.Add($"{AppImageTool} {Configuration.AppImageArgs} \"{BuildRoot}\" \"{OutputPath}\"");

        if (Arguments.IsRun)
        {
            cmds.Add(OutputPath);
        }

        PackageCommands = cmds;
    }

    /// <summary>
    /// Gets the embedded appimagetool version.
    /// </summary>
    public const string AppImageVersion = "Version 13 (2020-12-31)";

    /// <summary>
    /// Gets full path to embedded appimagetool. Null if architecture not supported.
    /// </summary>
    public static string? AppImageTool { get; } = GetAppImageTool();

    /// <summary>
    /// Overrides.
    /// </summary>
    public override string? MetaBuildPath
    {
        get
        {
            if (BuildShareMeta != null)
            {
                // Older style name currently required
                return $"{BuildShareMeta}/{Configuration.AppId}.appdata.xml";
            }

            return null;
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
    public override bool SupportsStartCommand { get; } = false;

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsPostRun { get; } = true;

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void Create(string? desktop, string? metainfo)
    {
        base.Create(desktop, metainfo);

        // We need a bodge fix to get AppImage to pass validation. We need desktop and meta in
        // two places, one place for AppImage builder itself, and the other to get the meta to
        // pass validation. See: https://github.com/AppImage/AppImageKit/issues/603#issuecomment-355105387
        Operations.WriteFile(Path.Combine(BuildRoot, Configuration.AppId + ".desktop"), desktop);
        Operations.WriteFile(Path.Combine(BuildRoot, Configuration.AppId + ".appdata.xml"), metainfo);

        if (IconSource != null)
        {
            Operations.CopyFile(IconSource, Path.Combine(BuildRoot, Configuration.AppId + Path.GetExtension(IconSource)));
        }

        // IMPORTANT - Create AppRun link
        // ln -s {target} {link}
        Operations.Execute($"ln -s \"{InstallExec}\" \"{Path.Combine(BuildRoot, "AppRun")}\"");
    }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void BuildPackage()
    {
        if (Arguments.Arch != null)
        {
            // Used by AppImageTool
            // Otherwise leave to auto-detect
            Environment.SetEnvironmentVariable("ARCH", Arguments.Arch);
        }

        base.BuildPackage();
    }

    private static string? GetAppImageTool()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X64)
            {
                return Path.Combine(AssemblyDirectory, "appimagetool-x86_64.AppImage");
            }

            if (RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
            {
                return Path.Combine(AssemblyDirectory, "appimagetool-aarch64.AppImage");
            }
        }

        return null;
    }

}

