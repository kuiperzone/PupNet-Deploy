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

namespace KuiperZone.PupNet;

/// <summary>
/// Defines new file kind.
/// </summary>
public enum NewKind
{
    /// <summary>
    /// None. Invalid. Empty.
    /// </summary>
    None = 0,

    /// <summary>
    /// Conf file.
    /// </summary>
    Conf,

    /// <summary>
    /// Desktop file.
    /// </summary>
    Desktop,

    /// <summary>
    /// AppStream metadata.
    /// </summary>
    Meta,

    /// <summary>
    /// All kinds.
    /// </summary>
    All,
}

/// <summary>
/// Extension methods.
/// </summary>
public static class NewKindExtension
{
    /// <summary>
    /// Gets file extension.
    /// </summary>
    public static string GetFileExt(this NewKind kind)
    {
        switch (kind)
        {
            case NewKind.Conf: return Program.ConfExt;
            case NewKind.Desktop: return ".desktop";
            case NewKind.Meta: return ".metainfo.xml";
            default: return "";
        }
    }
}
