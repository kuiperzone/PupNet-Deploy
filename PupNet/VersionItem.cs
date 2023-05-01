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

using System.Globalization;
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Immutable version-item class.
/// </summary>
public class VersionItem
{
    /// <summary>
    /// Constructor which sets <see cref="Change"/> and <see cref="IsHeader"/> to false.
    /// </summary>
    public VersionItem(string change)
    {
        Change = change;
    }

    /// <summary>
    /// Constructor which sets <see cref="Version"/>, <see cref="Date"/> and <see cref="IsHeader"/> to true.
    /// </summary>
    public VersionItem(string version, DateTime date, string? title = null, string? contact = null)
    {
        IsHeader = true;
        Version = version;
        Date = date;
        Title = title;
        Contact = contact;
    }

    /// <summary>
    /// Gets whether this is a header item.
    /// </summary>
    public bool IsHeader { get; }

    /// <summary>
    /// Gets the header version. It is null where <see cref="IsHeader"/> is false, and a valid string when true.
    /// </summary>
    public string? Version { get; }

    /// <summary>
    /// Gets the header date. It is default where <see cref="IsHeader"/> is false, and a valid date value when true.
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// Gets the header title. It is null where <see cref="IsHeader"/> is false, and a valid string when true.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Gets the header contact. It is null where <see cref="IsHeader"/> is false, and a string value when true.
    /// </summary>
    public string? Contact { get; }

    /// <summary>
    /// Gets the change description. It is null where <see cref="IsHeader"/> is true, and a valid single-line
    /// description where false.
    /// </summary>
    public string? Change { get; }

    /// <summary>
    /// Overrides. Calls ToString(null).
    /// </summary>
    public override string ToString()
    {
        return ToString(null);
    }

    /// <summary>
    /// Returns either "+ Version;[Title;][Contact;]yyyy-MM-dd", or "- Change".
    /// </summary>
    public string ToString(string? defaultContact)
    {
        const char Seperator = ';';

        if (Change != null)
        {
            return "- " + Change;
        }

        var sb = new StringBuilder("+ ");
        sb.Append(Version);

        if (!string.IsNullOrEmpty(Title))
        {
            sb.Append(Seperator);
            sb.Append(Title);
        }

        if (!string.IsNullOrEmpty(Contact))
        {
            sb.Append(Seperator);
            sb.Append(Contact);
        }
        else
        if (!string.IsNullOrEmpty(defaultContact))
        {
            sb.Append(Seperator);
            sb.Append(defaultContact);
        }

        sb.Append(Seperator);
        sb.Append(Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        return sb.ToString();
    }
}