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

using System.Runtime.InteropServices;

namespace KuiperZone.PupNet;

/// <summary>
/// Defines deployable package kinds.
/// </summary>
public enum DeployKind
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
    /// Setup file. Windows only.
    /// </summary>
    Setup,
}

/// <summary>
/// Extension methods.
/// </summary>
public static class DeployKindExtension
{
    /// <summary>
    /// Gets file extension.
    /// </summary>
    public static string GetFileExt(this DeployKind kind)
    {
        switch (kind)
        {
            case DeployKind.Zip: return ".zip";
            case DeployKind.AppImage: return ".AppImage";
            case DeployKind.Deb: return ".deb";
            case DeployKind.Rpm: return ".rpm";
            case DeployKind.Flatpak: return ".flatpak";
            case DeployKind.Setup: return ".exe";
            default: throw new ArgumentException($"Invalid {nameof(DeployKind)} {kind}");
        }
    }

    /// <summary>
    /// Gets whether compatible with linux.
    /// </summary>
    public static bool IsLinux(this DeployKind kind, bool exclusive = false)
    {
        switch (kind)
        {
            case DeployKind.Zip:
            case DeployKind.AppImage:
            case DeployKind.Deb:
            case DeployKind.Rpm:
            case DeployKind.Flatpak:
                return !exclusive || (!IsWindows(kind) && !IsOsx(kind));
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets whether compatible with windows.
    /// </summary>
    public static bool IsWindows(this DeployKind kind, bool exclusive = false)
    {
        if (kind == DeployKind.Zip || kind == DeployKind.Setup)
        {
            return !exclusive || (!IsLinux(kind) && !IsOsx(kind));
        }

        return false;
    }

    /// <summary>
    /// Gets whether compatible with OSX.
    /// </summary>
    public static bool IsOsx(this DeployKind kind, bool exclusive = false)
    {
        if (kind == DeployKind.Zip)
        {
            return !exclusive || (!IsLinux(kind) && !IsOsx(kind));
        }

        return false;
    }

    /// <summary>
    /// Returns true if the package kind can be built on this system.
    /// </summary>
    public static bool CanBuildOnSystem(this DeployKind kind)
    {
        if (kind.IsLinux() && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return true;
        }

        if (kind.IsWindows() && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return true;
        }

        if (kind.IsOsx() && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return true;
        }

        return false;
    }

}