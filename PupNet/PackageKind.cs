// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-24
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

using System.Runtime.InteropServices;

namespace KuiperZone.PupNet;

/// <summary>
/// Defines deployable package kinds.
/// </summary>
public enum PackageKind
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
    public static string GetFileExt(this PackageKind kind)
    {
        switch (kind)
        {
            case PackageKind.Zip: return ".zip";
            case PackageKind.AppImage: return ".AppImage";
            case PackageKind.Deb: return ".deb";
            case PackageKind.Rpm: return ".rpm";
            case PackageKind.Flatpak: return ".flatpak";
            case PackageKind.Setup: return ".exe";
            default: throw new ArgumentException($"Invalid {nameof(PackageKind)} {kind}");
        }
    }

    /// <summary>
    /// Gets whether compatible with linux.
    /// </summary>
    public static bool TargetsLinux(this PackageKind kind, bool exclusive = false)
    {
        switch (kind)
        {
            case PackageKind.Zip:
            case PackageKind.AppImage:
            case PackageKind.Deb:
            case PackageKind.Rpm:
            case PackageKind.Flatpak:
                return !exclusive || (!TargetsWindows(kind) && !TargetsOsx(kind));
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets whether compatible with windows.
    /// </summary>
    public static bool TargetsWindows(this PackageKind kind, bool exclusive = false)
    {
        if (kind == PackageKind.Zip || kind == PackageKind.Setup)
        {
            return !exclusive || (!TargetsLinux(kind) && !TargetsOsx(kind));
        }

        return false;
    }

    /// <summary>
    /// Gets whether compatible with OSX.
    /// </summary>
    public static bool TargetsOsx(this PackageKind kind, bool exclusive = false)
    {
        if (kind == PackageKind.Zip)
        {
            return !exclusive || (!TargetsLinux(kind) && !TargetsOsx(kind));
        }

        return false;
    }

    /// <summary>
    /// Returns true if the package kind can be built on this system.
    /// </summary>
    public static bool CanBuildOnSystem(this PackageKind kind)
    {
        if (kind.TargetsLinux() && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return true;
        }

        if (kind.TargetsWindows() && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return true;
        }

        if (kind.TargetsOsx() && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return true;
        }

        return false;
    }

}