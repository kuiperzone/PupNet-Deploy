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

using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Expands macros for use in fields and file contents. When expanding, simple search-replace
/// is used. Therefore important to specify "${NAME}", and not "$NAME".
/// </summary>
public class MacrosExpander
{
    private const string SpecialPrefix = "$$+";

    /// <summary>
    /// Default constructor. Example values and unit test only.
    /// </summary>
    public MacrosExpander()
        : this(new BuilderFactory().Create(new ConfigurationReader(true)))
    {
    }

    /// <summary>
    /// Production constructor.
    /// </summary>
    public MacrosExpander(PackageBuilder builder)
    {
        var args = builder.Arguments;
        var conf = builder.Configuration;
        Builder = builder;

        var dict = new Dictionary<MacroId, string>();

        dict.Add(MacroId.LocalDirectory, conf.LocalDirectory);
        dict.Add(MacroId.AppBaseName, conf.AppBaseName);
        dict.Add(MacroId.AppFriendlyName, conf.AppFriendlyName);
        dict.Add(MacroId.AppId, conf.AppId);
        dict.Add(MacroId.AppShortSummary, conf.AppShortSummary);
        dict.Add(MacroId.AppLicenseId, conf.AppLicenseId);
        dict.Add(MacroId.PublisherName, conf.PublisherName);
        dict.Add(MacroId.PublisherCopyright, conf.PublisherCopyright ?? "");
        dict.Add(MacroId.PublisherLinkName, conf.PublisherLinkName ?? "");
        dict.Add(MacroId.PublisherLinkUrl, conf.PublisherLinkUrl ?? "");
        dict.Add(MacroId.PublisherEmail, conf.PublisherEmail ?? "");
        dict.Add(MacroId.DesktopNoDisplay, conf.DesktopNoDisplay.ToString().ToLowerInvariant());
        dict.Add(MacroId.DesktopIntegrate, (!conf.DesktopNoDisplay).ToString().ToLowerInvariant());
        dict.Add(MacroId.DesktopTerminal, conf.DesktopTerminal.ToString().ToLowerInvariant());
        dict.Add(MacroId.PrimeCategory, conf.PrimeCategory ?? "");

        dict.Add(MacroId.AppVersion, builder.AppVersion);
        dict.Add(MacroId.PackageRelease, builder.PackageRelease);
        dict.Add(MacroId.DeployKind, args.Kind.ToString().ToLowerInvariant());
        dict.Add(MacroId.DotnetRuntime, builder.Runtime.RuntimeId);
        dict.Add(MacroId.BuildArch, builder.Runtime.RuntimeArch.ToString().ToLowerInvariant());
        dict.Add(MacroId.BuildTarget, args.Build);
        dict.Add(MacroId.BuildDate, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        dict.Add(MacroId.BuildYear, DateTime.UtcNow.ToString("yyyy"));
        dict.Add(MacroId.BuildRoot, builder.BuildRoot);
        dict.Add(MacroId.BuildShare, builder.BuildUsrShare ?? "");
        dict.Add(MacroId.BuildAppBin, builder.BuildAppBin);

        dict.Add(MacroId.InstallBin, builder.InstallBin);
        dict.Add(MacroId.InstallExec, builder.InstallExec);

        if (conf.AppDescription.Count != 0)
        {
            dict.Add(MacroId.AppStreamDescriptionXml, GetXmlDescription(conf.AppDescription));
        }
        else
        {
            // Macro cannot be empty - use mandatory AppShortSummary instead
            dict.Add(MacroId.AppStreamDescriptionXml, $"<p>{SecurityElement.Escape(conf.AppShortSummary)}</p>");
        }

        if (builder.ChangeLog.Items.Count != 0)
        {
            dict.Add(MacroId.AppStreamChangelogXml, builder.ChangeLog.ToString(true));
        }
        else
        {
            // Macro cannot be empty - manufacture minimal change
            var change = $"<release version=\"{builder.AppVersion}\" date=\"{DateTime.UtcNow.ToString("yyyy-MM-dd")}\">";
            dict.Add(MacroId.AppStreamChangelogXml, change);
        }

        Dictionary = dict;
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
    public string? Expand(string? content, bool escape, string? itemName = null)
    {
        if (!string.IsNullOrEmpty(content) && content.Contains("${"))
        {
            // Temporary state which prevents recursion in replacement
            content = content.Replace("$", SpecialPrefix + "$");

            foreach (var item in Dictionary)
            {
                // NB. XML keys are special and should not be escaped
                bool escapeThis = escape && !item.Key.ContainsXml();
                content = content.Replace(SpecialPrefix + item.Key.ToVar(), escapeThis ? SecurityElement.Escape(item.Value) : item.Value);
            }

            // Check here
            CheckForInvalidMacros(content, Builder.WarningSink, itemName);

            // Undo temporary modification
            content = content.Replace(SpecialPrefix + "$", "$");
        }

        return content;
    }

    /// <summary>
    /// Overload.
    /// </summary>
    [return: NotNullIfNotNull("content")]
    public string? Expand(string? content, string? itemName = null)
    {
        return Expand(content, false, itemName);
    }

    /// <summary>
    /// Expand all macros in text content items.
    /// </summary>
    public IReadOnlyCollection<string> Expand(IEnumerable<string> content, bool escape, string? itemName = null)
    {
        var list = new List<string>();

        foreach (var item in content)
        {
            list.Add(Expand(item, escape, itemName));
        }

        return list;
    }

    /// <summary>
    /// Overload.
    /// </summary>
    public IReadOnlyCollection<string> Expand(IEnumerable<string> content, string? itemName = null)
    {
        return Expand(content, false, itemName);
    }

    /// <summary>
    /// Overrides and output contents.
    /// </summary>
    public override string ToString()
    {
        return ToString(false, false);
    }

    /// <summary>
    /// Provides detail information.
    /// </summary>
    public string ToString(bool verbose, bool includeXml)
    {
        var sb = new StringBuilder();
        var sorted = new SortedDictionary<string, MacroId>();

        foreach (var item in Dictionary.Keys)
        {
            if (includeXml || !item.ContainsXml())
            {
                sorted.Add(item.ToName(), item);
            }
        }

        bool more = false;

        foreach (var item in sorted)
        {
            if (verbose)
            {
                if (more)
                {
                    sb.AppendLine();
                }

                more = true;
                sb.Append("** ");
                sb.Append(item.Value.ToVar());
                sb.AppendLine(" **");

                sb.AppendLine(item.Value.ToHint());

                var value = Dictionary[item.Value];

                if (!string.IsNullOrEmpty(value))
                {
                    sb.Append("Example: ");
                    sb.AppendLine($"{item.Value.ToVar()} = {Dictionary[item.Value]}");
                }
            }
            else
            {
                sb.AppendLine($"{item.Value.ToVar()} = {Dictionary[item.Value]}");
            }
        }

        return sb.ToString().Trim();
    }

    private static void CheckForInvalidMacros(string content, ICollection<string> warnings, string? itemName)
    {
        const string MatchPrefix = SpecialPrefix + "${";

        int p0 = content.IndexOf(MatchPrefix);

        if (p0 > -1)
        {
            string varStr = content.Substring(p0, Math.Max(content.Length - p0, 5)) + "...";

            // Find terminator
            int next = content.IndexOf(MatchPrefix, p0 + 1);
            int p1 = content.IndexOf("}", p0 + 1);

            if (p1 > p0 && (next < 0 || p1 < next))
            {
                // abc ${INV}
                // 0123456789
                int cnt = p1 - p0 + 1;
                varStr = content.Substring(p0, cnt);
            }

            varStr = "Invalid macro " + varStr.Replace(MatchPrefix, "${");

            if (!string.IsNullOrEmpty(itemName))
            {
                varStr += $" in {itemName}";
            }

            if (!warnings.Contains(varStr))
            {
                warnings.Add(varStr);
            }
        }
    }

    private static string GetXmlDescription(IEnumerable<string> description)
    {
        bool newPara = true;
        var sb = new StringBuilder();

        foreach (var item in description)
        {
            // Empty lines only appear mid-content and never consecutive
            if (newPara)
            {
                sb.Append("<p>");
                sb.Append(SecurityElement.Escape(item));
                newPara = false;
            }
            else
            if (string.IsNullOrEmpty(item))
            {
                sb.Append("</p>");
                sb.Append("\n\n");
                newPara = true;
            }
            else
            {
                sb.Append('\n');
                sb.Append(SecurityElement.Escape(item));
            }
        }

        if (sb.Length != 0)
        {
            sb.Append("</p>");
        }

        return sb.ToString();
    }

}

