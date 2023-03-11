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

public class PackageBuilderTest
{
    [Fact]
    public void AppImage_DecodesOK()
    {
        var builder = new BuildHost(new DummyConf(PackKind.AppImage));

        AssertOK(builder);
    }

    [Fact]
    public void Flatpak_DecodesOK()
    {
        var builder = new BuildHost(new DummyConf(PackKind.Flatpak));

        AssertOK(builder);
    }

    [Fact]
    public void Rpm_DecodesOK()
    {
        var builder = new BuildHost(new DummyConf(PackKind.Rpm));

        AssertOK(builder);
    }

    private void AssertOK(BuildHost builder)
    {
        Console.WriteLine(builder.ToString(true));
    }
}