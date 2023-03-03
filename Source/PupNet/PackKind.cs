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

namespace KuiperZone.PupNet;

/// <summary>
/// Defines package kinds.
/// </summary>
public enum PackKind
{
    /// <summary>
    /// Simple zip. All platforms.
    /// </summary>
    Zip = 0,

    /// <summary>
    /// AppImage. Linux only.
    /// </summary>
    AppImage,

    /// <summary>
    /// Debian package. Linux only.
    /// </summary>
    Deb,

    /// <summary>
    /// RPM package. Linux only.
    /// </summary>
    Rpm,

    /// <summary>
    /// Flatpak. Linux only.
    /// </summary>
    Flatpak,

    /// <summary>
    /// Windows setup. Windows only.
    /// </summary>
    WinSetup,
}

/// <summary>
/// Extension methods.
/// </summary>
public static class PackKindExtension
{
    /// <summary>
    /// Gets file extension.
    /// </summary>
    public static string GetFileExt(this PackKind kind)
    {
        switch (kind)
        {
            case PackKind.Zip: return ".zip";
            case PackKind.AppImage: return ".AppImage";
            case PackKind.Deb: return ".deb";
            case PackKind.Rpm: return ".rpm";
            case PackKind.Flatpak: return ".flatpak";
            case PackKind.WinSetup: return ".exe";
            default: throw new ArgumentException($"Invalid {nameof(PackKind)} {kind}");
        }
    }

    /// <summary>
    /// Gets whether compatible with linux.
    /// </summary>
    public static bool IsLinux(this PackKind kind)
    {
        switch (kind)
        {
            case PackKind.Zip:
            case PackKind.AppImage:
            case PackKind.Deb:
            case PackKind.Rpm:
            case PackKind.Flatpak:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets whether compatible with windows.
    /// </summary>
    public static bool IsWindows(this PackKind kind)
    {
        switch (kind)
        {
            case PackKind.Zip:
            case PackKind.WinSetup:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets whether compatible with OSX.
    /// </summary>
    public static bool IsOsx(this PackKind kind)
    {
        switch (kind)
        {
            case PackKind.Zip:
                return true;
            default:
                return false;
        }
    }

}