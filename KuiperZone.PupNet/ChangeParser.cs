// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-25
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

using System.Globalization;
using System.Security;
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// A class which reads changelog information and translates it to a sequence of <see cref="ChangeItem"/> values.
/// The class is immutable and is given a filename or string content on construction. The content is parsed, and
/// change related information extracted, with superfluous surround information ignored. The input format is form:
/// + 1.0.0;[OthersIgnored;]2023-05-01
/// - Change description 1
/// - Change description 2
/// - etc.
/// + 2.0.0;[OthersIgnored;]2023-05-02]
/// In the above, the first item in the "header" is always the version, and the last always the date, with the ';' being
/// a separator. The date is expected to be in form "yyyy-MM-dd", but can be in any DateTime parsable form. Additional
/// items between these two are ignored.
/// </summary>
public class ChangeParser
{
    /// <summary>
    /// Version header prefix character.
    /// </summary>
    public const char HeaderPrefix = '+';

    /// <summary>
    /// Version header separator character.
    /// </summary>
    public const char HeaderSeparator = ';';

    /// <summary>
    /// Change item prefix character.
    /// </summary>
    public const char ChangePrefix = '-';

    /// <summary>
    /// Constructor which reads the file content. Also serves as a default (empty) constructor.
    /// </summary>
    public ChangeParser(string? filename = null)
        : this(string.IsNullOrEmpty(filename) ? Array.Empty<string>() : File.ReadAllLines(filename))
    {
    }

    /// <summary>
    /// Constructor with CHANGE file content lines.
    /// </summary>
    public ChangeParser(IEnumerable<string> content)
    {
        var items = new List<ChangeItem>();
        Items = items;

        ChangeItem? header = null;
        string? change = null;

        foreach (var s in content)
        {
            var line = s.Trim();
            var tempHeader = TryParseHeader(line);

            if (tempHeader != null)
            {
                // New header
                AppendChange(items, ref change);

                header = tempHeader;
                items.Add(header);
                continue;
            }

            if (header != null)
            {
                if (line.Length == 0)
                {
                    // An empty line - break current change
                    // The next line must either be a new header or a new change item
                    AppendChange(items, ref change);
                    continue;
                }

                // Allow "- description", but not "------"
                if (line.StartsWith(ChangePrefix) && !line.StartsWith(new string(ChangePrefix, 2)))
                {
                    // New change item
                    AppendChange(items, ref change);
                    change = line.TrimStart(ChangePrefix, ' ');
                    continue;
                }

                if (change != null)
                {
                    // Buffer multiple lines, as long as no empty lines between
                    change += ' ' + line;
                    continue;
                }

                // Sequence broken, will need a new header to start
                header = null;
            }

            change = null;
        }

        // Append trailing change
        AppendChange(items, ref change);
    }

    /// <summary>
    /// Gets the change items.
    /// </summary>
    public IReadOnlyCollection<ChangeItem> Items { get; }

    /// <summary>
    /// Overrides. Equivalent to ToString(false).
    /// </summary>
    public override string ToString()
    {
        return ToTextString();
    }

    /// <summary>
    /// Returns multiline string output. If appstream is true, the result is formatted for inclusion in AppStream metadata. according to options.
    /// /// </summary>
    public string ToString(bool appstream)
    {
        if (appstream)
        {
            return ToAppStreamString();
        }

        return ToTextString();
    }

    private string ToAppStreamString()
    {
        // Example HTML:
        // <release version="1.3.1" date="2023-05-01"><description><ul>
        // <li>Bugfix: Fix package creation when file path of contents contain spaces (enclose file path with quotes when executing chmod)</li>
        // </ul></description></release>
        bool started = false;
        var sb = new StringBuilder();

        foreach (var item in Items)
        {
            if (item.IsHeader)
            {
                if (started)
                {
                    // Terminate last
                    sb.Append('\n');
                    sb.Append("</ul></description></release>");
                    sb.Append("\n\n");
                }

                started = true;

                sb.Append("<release version=\"");
                sb.Append(SecurityElement.Escape(item.Version));
                sb.Append("\" date=\"");
                sb.Append(item.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                sb.Append("\">");
                sb.Append("<description><ul>");
            }
            else
            if (started)
            {
                sb.Append('\n');
                sb.Append("<li>");
                sb.Append(SecurityElement.Escape(item.Change));
                sb.Append("</li>");
            }
        }

        if (started)
        {
            // Trailing termination
            sb.Append('\n');
            sb.Append("</ul></description></release>");
        }


        return sb.ToString();
    }

    private string ToTextString()
    {
        bool started = false;
        var sb = new StringBuilder();

        foreach (var item in Items)
        {
            if (item.IsHeader)
            {
                if (started)
                {
                    // Spacer
                    sb.Append("\n\n");
                }

                started = true;

                sb.Append(HeaderPrefix);
                sb.Append(' ');
                sb.Append(item.Version);

                sb.Append(ChangeParser.HeaderSeparator);
                sb.Append(item.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            else
            if (started)
            {
                sb.Append('\n');
                sb.Append(ChangePrefix);
                sb.Append(' ');
                sb.Append(item.Change);
            }
        }

        return sb.ToString();
    }

    private static void AppendChange(List<ChangeItem> list, ref string? change)
    {
        if (!string.IsNullOrEmpty(change))
        {
            list.Add(new ChangeItem(change));
        }

        change = null;
    }

    private static ChangeItem? TryParseHeader(string line)
    {
        const int MaxVersion = 25;

        // Allow "+ ", but not "++++"
        if (line.StartsWith(HeaderPrefix) && !line.StartsWith(new string(HeaderPrefix, 2)))
        {
            var items = line.Split(HeaderSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (items.Length > 1 && DateTime.TryParse(items[^1], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                string version = items[0].TrimStart(HeaderPrefix, ' ');

                if (version.Length > 0 && version.Length <= MaxVersion)
                {
                    return new ChangeItem(version, date);
                }
            }
        }

        return null;
    }

}