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
    public void AppImage_DecodesOK()
    {
        var host = new BuildHost(new DummyConf(DeployKind.AppImage));
        AssertOK(host);
    }

    [Fact]
    public void Flatpak_DecodesOK()
    {
        var host = new BuildHost(new DummyConf(DeployKind.Flatpak));
        AssertOK(host);
    }

    [Fact]
    public void Rpm_DecodesOK()
    {
        var host = new BuildHost(new DummyConf(DeployKind.Rpm));
        AssertOK(host);
    }

    [Fact]
    public void Deb_DecodesOK()
    {
        var host = new BuildHost(new DummyConf(DeployKind.Deb));
        AssertOK(host);
    }

    [Fact]
    public void Setup_DecodesOK()
    {
        var host = new BuildHost(new DummyConf(DeployKind.Setup));
        AssertOK(host);
    }

    [Fact]
    public void Zip_DecodesOK()
    {
        var host = new BuildHost(new DummyConf(DeployKind.Zip));
        AssertOK(host);
    }

    private void AssertOK(BuildHost host)
    {
        Console.WriteLine("DUMMY: " + host.GetType().Name);
        Console.WriteLine(host.ToString(true));
        Console.WriteLine();
        Console.WriteLine();
    }
}