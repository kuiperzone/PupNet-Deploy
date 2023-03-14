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
/// Converts dotnet publish "runtime" ("-r") value into an architecture string suitable for
/// <see cref="PackageBuilder"/> subclass.
/// </summary>
public class ArchitectureConverter
{
    private string _string;

    /// <summary>
    /// Static constructor.
    /// </summary>
    static ArchitectureConverter()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
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
            DefaultRuntime = "osx-x64";
        }
        else
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            DefaultRuntime = "linux-arm64";
        }
        else
        {
            DefaultRuntime = "linux-x64";
        }
    }

    /// <summary>
    /// Constructor. Determines final architecture string via ToString() method. If xarch is not null or empty,
    /// ToString() returns this value.
    /// </summary>
    public ArchitectureConverter(PackKind kind, string? runtime, string? xarch = null)
    {
        Kind = kind;
        IsUncertain = true;
        RuntimeId = runtime?.Trim()?.ToLowerInvariant() ?? DefaultRuntime;
        IsWindowsRuntime = RuntimeId.StartsWith("win");

        if (RuntimeId.EndsWith("-x64"))
        {
            IsUncertain = false;
            ArchRuntime = Architecture.X64;
        }
        else
        if (RuntimeId.EndsWith("-arm64"))
        {
            IsUncertain = false;
            ArchRuntime = Architecture.Arm64;
        }
        else
        if (RuntimeId.EndsWith("-arm"))
        {
            IsUncertain = false;
            ArchRuntime = Architecture.Arm;
        }
        else
        if (RuntimeId.EndsWith("-x86"))
        {
            IsUncertain = false;
            ArchRuntime = Architecture.X86;
        }

        if (string.IsNullOrWhiteSpace(xarch))
        {
            // Hard to get any definitive ARCH list for other package kinds.
            // We commonly see "x86_64" and "aarch64" in examples, rather than "x64" and "arm64".
            // We can add further exceptions here according to kind.
            _string = ArchRuntime.ToString().ToLower();

            // RPM Example:
            // https://koji.fedoraproject.org/koji/buildinfo?buildID=2108850

            // Windows:
            // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesallowed

            if (kind != PackKind.WinSetup && kind != PackKind.Deb)
            {
                if (ArchRuntime == Architecture.X64)
                {
                    _string = "x86_64";
                }
                else
                if (ArchRuntime == Architecture.Arm64)
                {
                    _string = "aarch64";
                }
            }
        }
        else
        {
            _string = xarch.Trim();
        }
    }

    /// <summary>
    /// Gets the default runtime.
    /// </summary>
    public static string DefaultRuntime { get; }

    /// <summary>
    /// Gets the package kind.
    /// </summary>
    public PackKind Kind { get; }

    /// <summary>
    /// Gets the dotnet publish runtime ID (rid) value.
    /// </summary>
    public string RuntimeId { get; }

    /// <summary>
    /// Gets whether <see cref="RuntimeId"/> appears to be a windows runtime.
    /// </summary>
    public bool IsWindowsRuntime { get; }

    /// <summary>
    /// Gets the runtime converted to architecture.
    /// </summary>
    public Architecture ArchRuntime { get; } = RuntimeInformation.OSArchitecture;

    /// <summary>
    /// Gets whether the <see cref="ArchRuntime"/> was mapped from <see cref="RuntimeId"/> with certainty.
    /// </summary>
    public bool IsUncertain { get; }

    /// <summary>
    /// Gets the architecture as a string.
    /// </summary>
    public override string ToString()
    {
        return _string;
    }
}

