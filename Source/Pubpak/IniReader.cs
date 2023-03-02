// -----------------------------------------------------------------------------
// PROJECT   : Pubpak
// COPYRIGHT : Andy Thomas (C) 2022-23
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/Pubpak
//
// Pubpak is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// Pubpak is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with Pubpak. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

using System.Text;

namespace KuiperZone.Pubpak;

/// <summary>
/// Supported feature flags for <see cref="IniReader"/>.
/// </summary>
public enum IniOptions
{
    /// <summary>
    /// Simple key-value strings on single lines.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// Strip value of surrounding quote characters.
    /// </summary>
    StripQuotes = 0x0001,

    /// <summary>
    /// Support multi-line values.
    /// </summary>
    MultiLine = 0x0002,

    /// <summary>
    /// Default options.
    /// </summary>
    Default = StripQuotes | MultiLine,
}

/// <summary>
/// Reads simple INI content providing a name-value dictionary.
/// </summary>
public class IniReader
{
    /// <summary>
    /// Multi-line value start quote.
    /// </summary>
    public const string StartMultiQuote = "{[";

    /// <summary>
    /// Multi-line value end quote.
    /// </summary>
    public const string EndMultiQuote = "]}";

    private readonly string _string = "";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public IniReader(IniOptions opts = IniOptions.Default)
    {
        Options = opts;
        Values = new Dictionary<string, string>();
    }

    /// <summary>
    /// Constructor with multi-line content.
    /// </summary>
    public IniReader(string path, IniOptions opts = IniOptions.Default)
    {
        Options = opts;
        Values = Parse(File.ReadAllLines(path));
        Filepath = Path.GetFullPath(path);
        _string = GetString(Values);
    }

    /// <summary>
    /// Constructor with content lines.
    /// </summary>
    public IniReader(string[] content, IniOptions opts = IniOptions.Default)
    {
        Options = opts;
        Values = Parse(content);
        _string = GetString(Values);
    }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string Filepath { get; } = "";

    /// <summary>
    /// Supports multiple
    /// </summary>
    public IniOptions Options { get; }

    /// <summary>
    /// Gets the values. The key is ordinal case insensitive.
    /// </summary>
    public IReadOnlyDictionary<string, string> Values { get; }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override string ToString()
    {
        return _string;
    }

    private static string GetString(IReadOnlyDictionary<string, string> dict)
    {
        var sb = new StringBuilder();

        foreach (var pair in dict)
        {
            sb.Append(pair.Key);
            sb.Append(" = ");


            if (!pair.Value.Contains('\n'))
            {
                sb.AppendLine(pair.Value);
            }
            else
            {
                sb.AppendLine(StartMultiQuote);

                foreach (var item in pair.Value.Split('\n', StringSplitOptions.TrimEntries))
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.Append("    ");
                        sb.AppendLine(item);
                    }
                }

                sb.AppendLine(EndMultiQuote);
            }
        }

        return sb.ToString().Trim();
    }

    private Dictionary<string, string> Parse(string[] content)
    {
        int n = 0;
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        while (n < content.Length)
        {
            var hold = n;
            var name = ParseNameValue(content, ref n, out string value);

            if (name.Length != 0 && value.Length != 0 && !dict.TryAdd(name, value))
            {
                throw new ArgumentException(GetError($"Repeated key {name}", hold));
            }


        }

        return dict;
    }

    private string ParseNameValue(string[] content, ref int num, out string value)
    {
        int hold = num;
        var line = content[num++].Trim();

        if (line.Length == 0 || line.StartsWith('#') || line.StartsWith("//"))
        {
            // Comment or empty
            value = "";
            return "";
        }

        int pos = line.IndexOf('=');

        if (pos > 0)
        {
            var name = line.Substring(0, pos).Trim();
            value = line.Substring(pos + 1).Trim();

            if (Options.HasFlag(IniOptions.MultiLine) && value.StartsWith(StartMultiQuote))
            {
                var builder = new StringBuilder();
                line = value.Substring(StartMultiQuote.Length);

                while (true)
                {
                    line = line.Trim();
                    pos = line.IndexOf(EndMultiQuote);

                    if (pos > -1)
                    {
                        builder.Append(line.Substring(0, pos));
                        value = builder.ToString().Trim();
                        return name;
                    }

                    builder.AppendLine(line);

                    if (num < content.Length)
                    {
                        line = content[num++];
                        continue;
                    }

                    throw new ArgumentException(GetError("No multi-line termination", hold));
                }
            }
            else
            if (Options.HasFlag(IniOptions.StripQuotes) && value.Length > 1)
            {
                if ((value.StartsWith('"') && value.EndsWith('"')) ||
                    (value.StartsWith('\'') && value.EndsWith('\'')))
                {
                    value = value.Substring(1, value.Length - 2);
                }
            }

            return name;
        }

        throw new ArgumentException(GetError("Syntax error", hold));
    }

    private string GetError(string msg, int num)
    {
        if (!string.IsNullOrEmpty(Filepath))
        {
            throw new ArgumentException($"{msg} in {Path.GetFileName(Filepath)} at #{num}");
        }

        throw new ArgumentException($"{msg} at #{num}");
    }
}