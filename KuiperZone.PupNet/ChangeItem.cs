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

namespace KuiperZone.PupNet;

/// <summary>
/// Immutable change-item class.
/// </summary>
public class ChangeItem : IEquatable<ChangeItem>
{
    /// <summary>
    /// Constructor which sets <see cref="Change"/> and <see cref="IsHeader"/> to false.
    /// </summary>
    public ChangeItem(string change)
    {
        Change = change;
    }

    /// <summary>
    /// Constructor which sets <see cref="Version"/>, <see cref="Date"/> and <see cref="IsHeader"/> to true.
    /// </summary>
    public ChangeItem(string version, DateTime date)
    {
        IsHeader = true;
        Version = version;
        Date = date;
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
    /// Gets the change description. It is null where <see cref="IsHeader"/> is true, and a valid single-line
    /// description where false.
    /// </summary>
    public string? Change { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public bool Equals(ChangeItem? other)
    {
        if (other == null)
        {
            return false;
        }

        return IsHeader == other.IsHeader && Change == other.Change &&
            Version == other.Version && Date.Equals(other.Date);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override bool Equals(object? other)
    {
        return Equals(other as ChangeItem);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Version, Date, Change);
    }
}