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

using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Flatpak package.
/// </summary>
public class FlatpakBuilder : PackageBuilder
{
    private const string RootName = "AppDir";

    /// <summary>
    /// Constructor.
    /// </summary>
    public FlatpakBuilder(ConfigurationReader conf)
        : base(conf, PackKind.Flatpak, RootName)
    {
        PublishBin = BuildUsrBin ?? throw new ArgumentNullException(nameof(BuildUsrBin));
        DesktopExec = AppExecName;

        // Not used
        ManifestPath = Path.Combine(PackRoot, Configuration.AppId + ".yml");
        ManifestContent = GetFlatpakManifest();

        // Do the build
        var temp = Path.Combine(PackRoot, "build");
        var state = Path.Combine(PackRoot, "state");
        var repo = Path.Combine(PackRoot, "repo");
        var output = Path.Combine(OutputDirectory, OutputName);

        var cmd = $"flatpak-builder {Configuration.FlatpakBuilderArgs}";

        if (Arguments.Arch != null)
        {
            // Explicit only (otherwise leave it to utility to determine)
            cmd += $" --arch ${Arguments.Arch}";
        }

        cmd += $" --repo=\"{repo}\" --force-clean \"{temp}\" --state-dir \"{state}\" \"{ManifestPath}\"";

        var list = new List<string>();

        list.Add(cmd);
        list.Add($"flatpak build-bundle \"{repo}\" \"{output}\" {Configuration.AppId}");

        if (Arguments.IsRun)
        {
            list.Add($"flatpak-builder --run \"{temp}\" \"{ManifestPath}\" ${Configuration.AppId} --state-dir \"{state}\"");
        }

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

    private string GetFlatpakManifest()
    {
        var sb = new StringBuilder();

        // NOTE. Yaml file must be saved next to BuildRoot directory
        sb.AppendLine($"app-id: {Configuration.AppId}");
        sb.AppendLine($"runtime: {Configuration.FlatpakPlatformRuntime}");
        sb.AppendLine($"runtime-version: '{Configuration.FlatpakPlatformVersion}'");
        sb.AppendLine($"sdk: {Configuration.FlatpakPlatformSdk}");
        sb.AppendLine($"command: {Configuration.AppBaseName}");
        sb.AppendLine($"modules:");
        sb.AppendLine($"  - name: {Configuration.AppId}");
        sb.AppendLine($"    buildsystem: simple");
        sb.AppendLine($"    build-commands:");
        sb.AppendLine($"      - mkdir -p /app/bin");
        sb.AppendLine($"      - cp -rn bin/* /app/bin");
        sb.AppendLine($"      - mkdir -p /app/share");
        sb.AppendLine($"      - cp -rn share/* /app/share");
        sb.AppendLine($"    sources:");
        sb.AppendLine($"      - type: dir");
        sb.AppendLine($"        path: ${RootName}/usr/");

        if (Configuration.FlatpakFinishArgs.Count != 0)
        {
            sb.AppendLine($"finish-args:");

            foreach (var item in Configuration.FlatpakFinishArgs)
            {
                sb.Append("  - ");
                sb.AppendLine(item);
            }
        }

        return sb.ToString().TrimEnd();
    }

}

