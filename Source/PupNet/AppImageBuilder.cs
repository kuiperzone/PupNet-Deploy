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
        : base(conf, PackKind.AppImage, "AppDir")
    {
        // Path to embedded
        if (AppImageTool == null)
        {
            throw new InvalidOperationException($"{PackKind.AppImage} not supported on {ConfigurationReader.GetOSArch()}");
        }

        var output = Path.Combine(OutputDirectory, OutputName);

        PublishBin = BuildUsrBin ?? throw new ArgumentNullException(nameof(BuildUsrBin));
        DesktopExec = $"usr/bin/{AppExecName}";

        // Not used
        ManifestPath = null;
        ManifestContent = null;

        var cmds = new List<string>();

        // Do the build
        cmds.Add($"{BuildAssets.AppImageTool} {Configuration.AppImageArgs} \"{BuildRoot}\" \"{output}\"");

        if (Arguments.IsRun)
        {
            cmds.Add(output);
        }

        PackageCommands = cmds;
    }

    /// <summary>
    /// Gets full path to embedded appimagetool. Null if architecture not supported.
    /// </summary>
    public static string? AppImageTool { get; } = GetAppImageTool();

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
    /// Overrides and extends.
    /// </summary>
    public override void BuildPackage(string? desktop, string? metainfo)
    {
        if (Arguments.Arch != null)
        {
            // Used by AppImageTool
            Environment.SetEnvironmentVariable("ARCH", Arguments.Arch);
        }

        if (desktop != null)
        {
            // We need a bodge fix to get AppImage to pass validation.
            // In effect, we need two .desktop files. One at root, and one under applications.
            // See: https://github.com/AppImage/AppImageKit/issues/603#issuecomment-355105387https://github.com/AppImage/AppImageKit/issues/603
            Operations.WriteFile(Path.Combine(BuildRoot, Configuration.AppId + ".desktop"), desktop);
        }

        if (PrimeIconSource != null)
        {
            var path = Path.Combine(BuildRoot, Configuration.AppId + Path.GetExtension(PrimeIconSource));
            Operations.CopyFile(PrimeIconSource, path);
        }

        // IMPORTANT - Create AppRun link
        // ln -s {target} {link}
        Operations.Execute($"ln -s \"{DesktopExec}\" \"{Path.Combine(BuildRoot, "AppRun")}\"");

        base.BuildPackage(desktop, metainfo);
    }

    private static string? GetAppImageTool()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return Path.Combine(AssemblyDirectory, "appimagetool-x86_64.AppImage");
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return Path.Combine(AssemblyDirectory, "appimagetool-aarch64.AppImage");
        }

        return null;
    }

}

