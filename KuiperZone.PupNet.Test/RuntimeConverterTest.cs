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

using System.Runtime.InteropServices;
using KuiperZone.PupNet;
using Xunit;

namespace PupNet.Test;

public class RuntimeConverterTest
{
    [Fact]
    public void Constructor_Default_MapSystemArch()
    {
        var r = new RuntimeConverter(null);
        Assert.Equal(RuntimeConverter.DefaultRuntime, r.RuntimeId);
    }

    [Fact]
    public void Constructor_LinuxX64_MapsLinuxX64()
    {
        // Also test verbose here
        var r = new RuntimeConverter("Linux-X64");
        Assert.Equal("linux-x64", r.RuntimeId);
        Assert.Equal(Architecture.X64, r.BuildArch);
        Assert.False(r.IsArchUncertain);
        Assert.True(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        r = new RuntimeConverter("linux-musl-x64");
        Assert.Equal("linux-musl-x64", r.RuntimeId);
        Assert.Equal(Architecture.X64, r.BuildArch);
        Assert.True(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_LinuxArm64_MapsLinuxArm64()
    {
        var r = new RuntimeConverter("Linux-Arm64");
        Assert.Equal("linux-arm64", r.RuntimeId);
        Assert.Equal(Architecture.Arm64, r.BuildArch);
        Assert.True(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_LinuxArm_MapsLinuxArm()
    {
        var r = new RuntimeConverter("Linux-Arm");
        Assert.Equal("linux-arm", r.RuntimeId);
        Assert.Equal(Architecture.Arm, r.BuildArch);
        Assert.True(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_LinuxX86_MapsLinuxX86()
    {
        var r = new RuntimeConverter("Linux-X86");
        Assert.Equal("linux-x86", r.RuntimeId);
        Assert.Equal(Architecture.X86, r.BuildArch);
        Assert.True(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_WinX64_MapsWindowsX64()
    {
        var r = new RuntimeConverter("Win-X64");
        Assert.Equal("win-x64", r.RuntimeId);
        Assert.Equal(Architecture.X64, r.BuildArch);
        Assert.False(r.IsLinuxRuntime);
        Assert.True(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_Win10Arm64_MapsWindowsArm64()
    {
        var r = new RuntimeConverter("Win10-Arm64");
        Assert.Equal("win10-arm64", r.RuntimeId);
        Assert.Equal(Architecture.Arm64, r.BuildArch);
        Assert.False(r.IsLinuxRuntime);
        Assert.True(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_OsxX64_MapsOsxX64()
    {
        var r = new RuntimeConverter("OSX-X64");
        Assert.Equal("osx-x64", r.RuntimeId);
        Assert.Equal(Architecture.X64, r.BuildArch);
        Assert.False(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_Android_MapsLinuxUncertain()
    {
        var r = new RuntimeConverter("android-arm64");
        Assert.Equal("android-arm64", r.RuntimeId);
        Assert.Equal(Architecture.Arm64, r.BuildArch);
        Assert.False(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        Assert.False(r.IsArchUncertain);
    }

    [Fact]
    public void Constructor_Tizen_MapsLinuxUncertain()
    {
        var r = new RuntimeConverter("tizen.7.0.0");
        Assert.Equal("tizen.7.0.0", r.RuntimeId);
        Assert.True(r.IsLinuxRuntime);
        Assert.False(r.IsWindowsRuntime);

        // Arch unknown
        Assert.True(r.IsArchUncertain);
    }

    [Fact]
    public void ToArchitecture_MapsOK()
    {
        Assert.Equal(Architecture.X64, RuntimeConverter.ToArchitecture("x64"));
        Assert.Equal(Architecture.X64, RuntimeConverter.ToArchitecture("x86_64"));

        Assert.Equal(Architecture.Arm64, RuntimeConverter.ToArchitecture("arm64"));
        Assert.Equal(Architecture.Arm64, RuntimeConverter.ToArchitecture("aarch64"));
        Assert.Equal(Architecture.Arm64, RuntimeConverter.ToArchitecture("arm_aarch64"));

        Assert.Equal(Architecture.Arm, RuntimeConverter.ToArchitecture("arm"));
        Assert.Equal(Architecture.Arm, RuntimeConverter.ToArchitecture("armhf"));

        Assert.Equal(Architecture.X86, RuntimeConverter.ToArchitecture("x86"));
        Assert.Equal(Architecture.X86, RuntimeConverter.ToArchitecture("i686"));

        Assert.Throws<ArgumentException>(() => RuntimeConverter.ToArchitecture("jdue"));
    }

}
