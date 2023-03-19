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

namespace KuiperZone.PupNet.Test;

public class BuildHostTest
{
    [Fact]
    public void Constructor_AppImage_OK()
    {
        var host = new BuildHost(new DummyConf(PackageKind.AppImage));
        Assert.Equal(PackageKind.AppImage, host.Builder.Kind);
        Assert.Equal(2, host.PublishCommands.Count);
    }

    [Fact]
    public void Constructor_Flatpak_OK()
    {
        var host = new BuildHost(new DummyConf(PackageKind.Flatpak));
        Assert.Equal(PackageKind.Flatpak, host.Builder.Kind);
        Assert.Equal(2, host.PublishCommands.Count);
    }

    [Fact]
    public void Constructor_Rpm_OK()
    {
        var host = new BuildHost(new DummyConf(PackageKind.Rpm));
        Assert.Equal(PackageKind.Rpm, host.Builder.Kind);
        Assert.Equal(2, host.PublishCommands.Count);
    }

    [Fact]
    public void Constructor_Debian_OK()
    {
        var host = new BuildHost(new DummyConf(PackageKind.Deb));
        Assert.Equal(PackageKind.Deb, host.Builder.Kind);
        Assert.Equal(2, host.PublishCommands.Count);
    }

    [Fact]
    public void Constructor_Setup_OK()
    {
        var host = new BuildHost(new DummyConf(PackageKind.Setup));
        Assert.Equal(PackageKind.Setup, host.Builder.Kind);
        Assert.Equal(2, host.PublishCommands.Count);
    }

    [Fact]
    public void Constructor_Zip_OK()
    {
        var host = new BuildHost(new DummyConf(PackageKind.Zip));
        Assert.Equal(PackageKind.Zip, host.Builder.Kind);
        Assert.Equal(2, host.PublishCommands.Count);
    }
}

