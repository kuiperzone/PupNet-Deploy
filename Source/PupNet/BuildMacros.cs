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

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KuiperZone.PupNet;

public static class MacroNames
{
    // ConfDecoder Names - do not change as will break configs out in the wild
    public const string AppBase = "APP_BASE";
    public const string AppName = "APP_NAME";
    public const string AppId = "APP_ID";
    public const string AppSummary = "APP_SUMMARY";
    public const string AppLicense = "APP_LICENSE";
    public const string AppVendor = "APP_VENDOR";
    public const string AppUrl = "APP_URL";

    // PackageBuilder Names - do not change as will break configs out in the wild
    public const string AppVersion = "APP_VERSION";
    public const string PackRelease = "PACK_RELEASE";
    public const string OutputKind = "OUTPUT_KIND";
    public const string DotnetRuntime = "DOTNET_RUNTIME";
    public const string BuildArch = "BUILD_ARCH";
    public const string BuildTarget = "BUILD_TARGET";
    public const string OutputPath = "OUTPUT_PATH";
    public const string IsoDate = "ISO_DATE";

    // BuildTree Names - do not change as will break configs out in the wild
    public const string DesktopId = "DESKTOP_ID";
    public const string AppMetaName = "APP_META_NAME";
    public const string AppDir = "APP_DIR";
    public const string AppShare = "APP_SHARE";
    public const string PublishBin = "PUBLISH_BIN";
    public const string LaunchExec = "LAUNCH_EXEC";
}

/// <summary>
/// Declares and defines macros for use in fields and file contents. When expanding, simple search-replace is
/// used. Therefore important to specify ${NAME}, and not $NAME.
/// </summary>
public class BuildMacros
{
    /// <summary>
    /// Default constructor. Example values.
    /// </summary>
    public BuildMacros()
        : this(new BuildTree(new ConfDecoder()))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public BuildMacros(BuildTree tree)
    {
        Conf = tree.Conf;
        Args = tree.Conf.Args;

        OutputKind = Args.Kind;
        BuildTarget = Args.Build;
        BuildArch = Conf.GetBuildArch();
        DotnetRuntime = Args.Runtime;
        AppVersion = SplitVersion(Conf.AppVersionRelease, out string temp);
        PackRelease = temp;

        OutputDirectory = GetOutputDirectory();
        OutputName = GetOutputName(OutputKind);

        var dict = new SortedDictionary<string, string>();

        dict.Add(MacroNames.AppBase, Conf.AppBase);
        dict.Add(MacroNames.AppName, Conf.AppName);
        dict.Add(MacroNames.AppId, Conf.AppId);
        dict.Add(MacroNames.AppSummary, Conf.AppSummary);
        dict.Add(MacroNames.AppLicense, Conf.AppLicense);
        dict.Add(MacroNames.AppVendor, Conf.AppVendor);
        dict.Add(MacroNames.AppUrl, Conf.AppUrl ?? "");

        dict.Add(MacroNames.AppVersion, AppVersion);
        dict.Add(MacroNames.PackRelease, PackRelease);
        dict.Add(MacroNames.OutputKind, OutputKind.ToString().ToLowerInvariant());
        dict.Add(MacroNames.DotnetRuntime, DotnetRuntime);
        dict.Add(MacroNames.BuildArch, BuildArch);
        dict.Add(MacroNames.BuildTarget, BuildTarget);
        dict.Add(MacroNames.OutputPath, Path.Combine(OutputDirectory, OutputName));
        dict.Add(MacroNames.IsoDate, DateTime.UtcNow.ToString("yyyy-MM-dd"));

        dict.Add(MacroNames.DesktopId, tree.DesktopId);
        dict.Add(MacroNames.AppMetaName, tree.AppMetaName);
        dict.Add(MacroNames.AppDir, tree.AppDir);
        dict.Add(MacroNames.PublishBin, tree.PublishBin);
        dict.Add(MacroNames.AppShare, tree.AppShare);
        dict.Add(MacroNames.LaunchExec, tree.LaunchExec);

        Dictionary = dict;
    }

    public ArgDecoder Args { get; }
    public ConfDecoder Conf { get; }

    /// <summary>
    /// Gets package output kinds.
    /// </summary>
    public PackKind OutputKind { get; }

    /// <summary>
    /// Release or Debug.
    /// </summary>
    public string BuildTarget { get; }

    /// <summary>
    /// Target arch.
    /// </summary>
    public string BuildArch { get; }

    /// <summary>
    /// Gets the dotnet runtime.
    /// </summary>
    public string DotnetRuntime { get; }

    /// <summary>
    /// Gets the application version. This is the configured version, excluding any Release suffix.
    /// </summary>
    public string AppVersion { get; }

    /// <summary>
    /// Gets the package release. This is the suffix of the configured version.
    /// </summary>
    public string PackRelease { get; } = "1";

    /// <summary>
    /// Gets output directory.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Gets output filename.
    /// </summary>
    public string OutputName { get; }

    /// <summary>
    /// Gets a dictionary of macros.
    /// </summary>
    public IReadOnlyDictionary<string, string> Dictionary { get; }

    /// <summary>
    /// Expand all macros in text content. Simple search replace. Case sensitive.
    /// </summary>
    [return: NotNullIfNotNull("content")]
    public string? Expand(string? content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            foreach (var item in Dictionary)
            {
                content = content.Replace("${" + item.Key + "}", item.Value);
            }
        }

        return content;
    }

    /// <summary>
    /// Expand all macros in text content items.
    /// </summary>
    public IReadOnlyCollection<string> Expand(IEnumerable<string> content)
    {
        var list = new List<string>();

        foreach (var item in content)
        {
            list.Add(Expand(item)!);
        }

        return list;
    }

    /// <summary>
    /// Overrides and output contents.
    /// </summary>
    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var item in Dictionary)
        {
            builder.AppendLine($"${{{item.Key}}}={item.Value}");
        }

        return builder.ToString().Trim();
    }

    private static string SplitVersion(string version, out string release)
    {
        release = "1";

        if (!string.IsNullOrEmpty(version))
        {
            int p0 = version.IndexOf("[");
            var len = version.IndexOf("]") - p0 - 1;

            if (p0 > 0 && len > 0)
            {
                var temp = version.Substring(p0 + 1, len).Trim();
                version = version.Substring(0, p0).Trim();

                if (temp.Length != 0)
                {
                    release = temp;
                }
            }
        }

        return version;
    }

    private string GetOutputDirectory()
    {
        var argDir = Path.GetDirectoryName(Args.Output);

        if (argDir != null)
        {
            if (Path.IsPathFullyQualified(argDir))
            {
                return argDir;
            }

            return Path.Combine(Conf.OutputDirectory, argDir);
        }

        return Conf.OutputDirectory;
    }

    private string GetOutputName(PackKind kind)
    {
        var output = Path.GetFileName(Args.Output);

        if (output != null)
        {
            return output;
        }

        output = Conf.AppBase;

        if (Conf.OutputVersion && !string.IsNullOrEmpty(AppVersion))
        {
            output += $"-{AppVersion}-{PackRelease}";
        }

        output += $".{BuildArch}";

        if (kind == PackKind.AppImage)
        {
            return output + ".AppImage";
        }

        if (kind == PackKind.WinSetup)
        {
            return output + ".exe";
        }

        return output + "." + kind.ToString().ToLowerInvariant();
    }
}

