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
using KuiperZone.PupNet.Builders;

namespace KuiperZone.PupNet;

/// <summary>
/// Expands macros for use in fields and file contents. When expanding, simple search-replace
/// is used. Therefore important to specify "${NAME}", and not "$NAME".
/// </summary>
public class MacrosExpander
{
    private readonly SortedDictionary<string, string> _sorted = new();

    /// <summary>
    /// Default constructor. Example values only.
    /// </summary>
    public MacrosExpander()
        : this(new AppImageBuilder(new ConfigurationReader()))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public MacrosExpander(PackageBuilder builder)
    {
        var args = builder.Arguments;
        var conf = builder.Configuration;

        Builder = builder;

        var dict = new Dictionary<MacroId, string>();

        dict.Add(MacroId.AppBaseName, conf.AppBaseName);
        dict.Add(MacroId.AppFriendlyName, conf.AppFriendlyName);
        dict.Add(MacroId.AppId, conf.AppId);
        dict.Add(MacroId.ShortSummary, conf.ShortSummary);
        dict.Add(MacroId.LicenseId, conf.LicenseId);
        dict.Add(MacroId.VendorName, conf.VendorName);
        dict.Add(MacroId.VendorCopyright, conf.VendorCopyright ?? "");
        dict.Add(MacroId.VendorUrl, conf.VendorUrl ?? "");
        dict.Add(MacroId.VendorEmail, conf.VendorEmail ?? "");
        dict.Add(MacroId.IsTerminalApp, conf.IsTerminalApp.ToString().ToLowerInvariant());
        dict.Add(MacroId.PrimeCategory, conf.PrimeCategory ?? "");

        dict.Add(MacroId.AppVersion, builder.AppVersion);
        dict.Add(MacroId.PackKind, args.Kind.ToString().ToLowerInvariant());
        dict.Add(MacroId.DotnetRuntime, builder.Architecture.RuntimeId);
        dict.Add(MacroId.BuildArch, builder.Architecture.RuntimeArch.ToString().ToLowerInvariant());
        dict.Add(MacroId.BuildTarget, args.Build);
        dict.Add(MacroId.BuildDate, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        dict.Add(MacroId.BuildYear, DateTime.UtcNow.ToString("yyyy"));
        dict.Add(MacroId.BuildRoot, builder.AppRoot);
        dict.Add(MacroId.BuildShare, builder.BuildUsrShare ?? "");
        dict.Add(MacroId.PublishBin, builder.PublishBin);
        dict.Add(MacroId.DesktopExec, builder.DesktopExec);

        // For lookup
        Dictionary = dict;

        foreach (var item in Dictionary)
        {
            // For operations
            _sorted.Add(item.Key.ToVar(), item.Value);
        }
    }

    /// <summary>
    /// Gets the builder instance.
    /// </summary>
    public PackageBuilder Builder { get; }

    /// <summary>
    /// Gets a dictionary of macros.
    /// </summary>
    public IReadOnlyDictionary<MacroId, string> Dictionary { get; }

    /// <summary>
    /// Expand all macros in text content. Simple search replace. Case sensitive.
    /// </summary>
    [return: NotNullIfNotNull("content")]
    public string? Expand(string? content, ICollection<string>? warnings = null, string? itemName = null)
    {
        if (!string.IsNullOrEmpty(content) && content.Contains("${"))
        {
            foreach (var item in _sorted)
            {
                content = content.Replace(item.Key, item.Value);
            }

            if (warnings != null)
            {
                AddInvalidMacros(content, warnings, itemName);
            }
        }

        return content;
    }

    /// <summary>
    /// Expand all macros in text content items.
    /// </summary>
    public IReadOnlyCollection<string> Expand(IEnumerable<string> content, ICollection<string>? warnings = null, string? itemName = null)
    {
        var list = new List<string>();

        foreach (var item in content)
        {
            list.Add(Expand(item, warnings, itemName));
        }

        return list;
    }

    /// <summary>
    /// Overrides and output contents.
    /// </summary>
    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var item in _sorted)
        {
            builder.AppendLine($"{item.Key} = {item.Value}");
        }

        return builder.ToString().Trim();
    }

    private static void AddInvalidMacros(string content, ICollection<string> warnings, string? itemName)
    {
        int p0 = content.IndexOf("${");

        if (p0 > -1)
        {
            string s = content.Substring(p0, Math.Max(content.Length - p0, 5)) + "...";

            // Find terminator
            int temp = content.IndexOf("${", p0 + 1);
            int p1 = content.IndexOf("}", p0 + 1);

            if (p1 > p0 && (temp < 0 || p1 < temp))
            {
                // abc ${INV}
                // 0123456789
                int cnt = p1 - p0 + 1;
                s = content.Substring(p0, cnt);
            }

            s = $"Invalid macro {s}";

            if (!string.IsNullOrEmpty(itemName))
            {
                s += $" in {itemName}";
            }

            if (!warnings.Contains(s))
            {
                warnings.Add(s);
            }
        }
    }

}

