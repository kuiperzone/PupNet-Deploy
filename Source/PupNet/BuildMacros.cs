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

/// <summary>
/// Declares and defines macros for use in fields and file contents. When expanding, simple search-replace is
/// used. Therefore important to specify ${NAME}, and not $NAME.
/// </summary>
public class BuildMacros
{
    // ConfDecoder Names - do not change as will break configs out in the wild
    public const string AppBase = nameof(AppBase);
    public const string AppName = nameof(AppName);
    public const string AppId = nameof(AppId);
    public const string AppSummary = nameof(AppSummary);
    public const string AppLicense = nameof(AppLicense);
    public const string AppVendor = nameof(AppVendor);
    public const string AppUrl = nameof(AppUrl);

    // PackageBuilder Names - do not change as will break configs out in the wild
    public const string AppVersion = nameof(AppVersion);
    public const string PackRelease = nameof(PackRelease);
    public const string OutputKind = nameof(OutputKind);
    public const string DotnetRuntime = nameof(DotnetRuntime);
    public const string BuildArch = nameof(BuildArch);
    public const string BuildTarget = nameof(BuildTarget);
    public const string OutputPath = nameof(OutputPath);
    public const string IsoDate = nameof(IsoDate);

    // BuildTree Names - do not change as will break configs out in the wild
    public const string DesktopId = nameof(DesktopId);
    public const string AppMetaName = nameof(AppMetaName);
    public const string AppDir = nameof(AppDir);
    public const string AppShare = nameof(AppShare);
    public const string PublishBin = nameof(PublishBin);
    public const string LaunchExec = nameof(LaunchExec);

    /// <summary>
    /// Default constructor. Example values.
    /// </summary>
    public BuildMacros()
        : this(new PackageBuilder(new ConfDecoder()))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public BuildMacros(PackageBuilder builder)
    {
        var conf = builder.Conf;
        var tree = builder.Tree;
        var dict = new SortedDictionary<string, string>();

        dict.Add(AppBase, conf.AppBase);
        dict.Add(AppName, conf.AppName);
        dict.Add(AppId, conf.AppId);
        dict.Add(AppSummary, conf.AppSummary);
        dict.Add(AppLicense, conf.AppLicense);
        dict.Add(AppVendor, conf.AppVendor);
        dict.Add(AppUrl, conf.AppUrl ?? "");

        dict.Add(AppVersion, builder.AppVersion ?? "");
        dict.Add(PackRelease, builder.PackRelease);
        dict.Add(OutputKind, builder.OutputKind.ToString().ToLowerInvariant());
        dict.Add(DotnetRuntime, builder.DotnetRuntime);
        dict.Add(BuildArch, builder.BuildArch);
        dict.Add(BuildTarget, builder.BuildTarget);
        dict.Add(OutputPath, Path.Combine(builder.OutputDirectory, builder.OutputName));
        dict.Add(IsoDate, DateTime.UtcNow.ToString("yyyy-MM-dd"));

        dict.Add(DesktopId, tree.DesktopId);
        dict.Add(AppMetaName, tree.AppMetaName);
        dict.Add(AppDir, tree.AppDir);
        dict.Add(PublishBin, tree.PublishBin);
        dict.Add(AppShare, tree.AppShare);
        dict.Add(LaunchExec, tree.LaunchExec);

        Dictionary = dict;
    }

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
            builder.AppendLine($"{item.Key}={item.Value}");
        }

        return builder.ToString().Trim();
    }

}

