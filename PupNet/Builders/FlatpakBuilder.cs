// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-24
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
/// Extends <see cref="PackageBuilder"/> for Flatpak package.
/// </summary>
public class FlatpakBuilder : PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public FlatpakBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Flatpak)
    {
        BuildAppBin = BuildUsrBin ?? throw new ArgumentNullException(nameof(BuildUsrBin));

        // Not used
        InstallBin = "";

        ManifestContent = GetFlatpakManifest();
        ManifestBuildPath = Path.Combine(Root, Configuration.AppBaseName + ".yml");

        var temp = Path.Combine(Root, "build");
        var state = Path.Combine(Root, "state");
        var repo = Path.Combine(Root, "repo");

        var cmd = $"flatpak-builder {Configuration.FlatpakBuilderArgs}";

        if (Arguments.Arch != null)
        {
            // Explicit only (otherwise leave it to utility to determine)
            cmd += $" --arch ${Arguments.Arch}";
        }

        cmd += $" --repo=\"{repo}\" --force-clean \"{temp}\" --state-dir \"{state}\" \"{ManifestBuildPath}\"";

        var list = new List<string>();

        list.Add(cmd);
        list.Add($"flatpak build-bundle \"{repo}\" \"{OutputPath}\" {Configuration.AppId}");

        if (Arguments.IsRun)
        {
            list.Add($"flatpak-builder --run \"{temp}\" \"{ManifestBuildPath}\" ${Configuration.AppId} --state-dir \"{state}\"");
        }

        PackageCommands = list;
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

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.X64)
            {
                return "x86_64";
            }

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.Arm64)
            {
                return "aarch64";
            }

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.X86)
            {
                return "i686";
            }

            return Runtime.RuntimeArch.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string OutputName
    {
        get { return GetOutputName(true, Architecture, ".flatpak"); }
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

    private string GetFlatpakManifest()
    {
        var sb = new StringBuilder();

        // NOTE. Yaml file must be saved next to BuildRoot directory
        sb.AppendLine($"app-id: {Configuration.AppId}");
        sb.AppendLine($"runtime: {Configuration.FlatpakPlatformRuntime}");
        sb.AppendLine($"runtime-version: '{Configuration.FlatpakPlatformVersion}'");
        sb.AppendLine($"sdk: {Configuration.FlatpakPlatformSdk}");
        sb.AppendLine($"command: {InstallExec}");
        sb.AppendLine($"modules:");
        sb.AppendLine($"  - name: {Configuration.PackageName}");
        sb.AppendLine($"    buildsystem: simple");
        sb.AppendLine($"    build-commands:");
        sb.AppendLine($"      - mkdir -p /app/bin");
        sb.AppendLine($"      - cp -rn bin/* /app/bin");
        sb.AppendLine($"      - mkdir -p /app/share");
        sb.AppendLine($"      - cp -rn share/* /app/share");
        sb.AppendLine($"    sources:");
        sb.AppendLine($"      - type: dir");
        sb.AppendLine($"        path: {AppRootName}/usr/");

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

