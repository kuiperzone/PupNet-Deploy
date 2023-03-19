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

using System.Runtime.InteropServices;

namespace KuiperZone.PupNet;

/// <summary>
/// Converts dotnet publish "runtime" ("-r") value into <see cref="Architecture"/> value.
/// Also converts into string suitable for use with <see cref="PackageBuilder"/> subclass.
/// </summary>
public class RuntimeConverter
{
    /// <summary>
    /// Static constructor.
    /// </summary>
    static RuntimeConverter()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SystemOS = OSPlatform.Windows;

            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                DefaultRuntime = "win-arm64";
            }
            else
            {
                DefaultRuntime = "win-x64";
            }
        }
        else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SystemOS = OSPlatform.OSX;
            DefaultRuntime = "osx-x64";
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                SystemOS = OSPlatform.FreeBSD;
            }
            else
            {
                SystemOS = OSPlatform.Linux;
            }

            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                DefaultRuntime = "linux-arm64";
            }
            else
            {
                DefaultRuntime = "linux-x64";
            }
        }
    }

    /// <summary>
    /// Constructor with dotnet runtime-id value. If not empty, extracts target CPU architecture.
    /// If null of empty, defaults to development arch.
    /// </summary>
    public RuntimeConverter(string? runtime)
    {
        if (!string.IsNullOrEmpty(runtime))
        {
            RuntimeId = runtime.ToLowerInvariant();
        }

        // Common rids include: linux-x64, linux-arm64, win-x64 etc.
        // Going to work for: X64, Arm64, Arm, X86
        // If not matched, leave at system arch.
        foreach (var item in Enum.GetValues<Architecture>())
        {
            if (RuntimeId.EndsWith("-" + item.ToString().ToLowerInvariant()))
            {
                RuntimeArch = item;
                IsArchUncertain = false;
                break;
            }
        }

        if (RuntimeId.StartsWith("linux") || RuntimeId.StartsWith("rhel") || RuntimeId.StartsWith("tizen"))
        {
            IsLinuxRuntime = true;
            DefaultPackage = PackageKind.AppImage.CanBuildOnSystem() ? PackageKind.AppImage : PackageKind.Zip;
        }
        else
        if (RuntimeId.StartsWith("win"))
        {
            IsWindowsRuntime = true;
            DefaultPackage = PackageKind.Setup.CanBuildOnSystem() ? PackageKind.Setup : PackageKind.Zip;
        }
        else
        if (RuntimeId.StartsWith("osx"))
        {
            IsOsxRuntime = true;
            DefaultPackage = PackageKind.Zip;
        }
        else
        {
            DefaultPackage = PackageKind.Zip;
        }
    }

    /// <summary>
    /// Gets system OS, i.e. "Windows", "Linux" or "OSX".
    /// </summary>
    public static OSPlatform SystemOS { get; }

    /// <summary>
    /// Gets the default runtime.
    /// </summary>
    public static string DefaultRuntime { get; }

    /// <summary>
    /// Gets the dotnet publish runtime ID (rid) value.
    /// </summary>
    public string RuntimeId { get; } = DefaultRuntime;

    /// <summary>
    /// Convenience. Gets whether <see cref="RuntimePlatform"/> is linux.
    /// </summary>
    public bool IsLinuxRuntime { get; }

    /// <summary>
    /// Convenience. Gets whether <see cref="RuntimePlatform"/> is windows.
    /// </summary>
    public bool IsWindowsRuntime { get; }

    /// <summary>
    /// Convenience. Gets whether <see cref="RuntimePlatform"/> is for OSX.
    /// </summary>
    public bool IsOsxRuntime { get; }

    /// <summary>
    /// Gets the runtime converted to architecture.
    /// </summary>
    public Architecture RuntimeArch { get; } = RuntimeInformation.OSArchitecture;

    /// <summary>
    /// Gets whether runtime-id could NOT be mapped to <see cref="RuntimeArch"/> with certainty.
    /// </summary>
    public bool IsArchUncertain { get; } = true;

    /// <summary>
    /// Gets default package kind given runtime-id.
    /// </summary>
    public PackageKind DefaultPackage { get; }

    /// <summary>
    /// Gets the architecture tailored to suite the package <see cref="PackageKind"/> value.
    /// </summary>
    public string GetPackageArch(PackageKind kind)
    {
        // Hard to get any definitive ARCH list for other package kinds.
        // We commonly see "x86_64" and "aarch64" in examples, rather than "x64" and "arm64".
        // We can add further exceptions here according to kind.

        if (kind == PackageKind.AppImage || kind == PackageKind.Flatpak)
        {
            // AppImage, flatpak
            if (RuntimeArch == Architecture.X64)
            {
                return "x86_64";
            }
            else
            if (RuntimeArch == Architecture.Arm64)
            {
                return "aarch64";
            }
        }
        else
        if (kind == PackageKind.Deb)
        {
            // DEB Example: amd64 (not x64 or x86_64)
            // https://wiki.debian.org/ArchitectureSpecificsMemo
            if (RuntimeArch == Architecture.X64)
            {
                return "amd64";
            }

            if (RuntimeArch == Architecture.Arm64)
            {
                return "arm64";
            }

            if (RuntimeArch == Architecture.X86)
            {
                // Not sure about this?
                // https://en.wikipedia.org/wiki/X32_ABI
                return "x32";
            }
        }
        else
        if (kind == PackageKind.Rpm)
        {
            // RPM Example: (aarch64 not arm64)
            // https://koji.fedoraproject.org/koji/buildinfo?buildID=2108850
            if (RuntimeArch == Architecture.X64)
            {
                return "x86_64";
            }

            if (RuntimeArch == Architecture.Arm64)
            {
                return "aarch64";
            }

            if (RuntimeArch == Architecture.X86)
            {
                return "i686";
            }
        }

        // Windows and any remaining
        // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesallowed
        return RuntimeArch.ToString().ToLower();
    }

    /// <summary>
    /// Returns RuntimeId.
    /// </summary>
    public override string ToString()
    {
        return RuntimeId;
    }
}

