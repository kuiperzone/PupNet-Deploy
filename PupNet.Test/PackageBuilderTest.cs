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

using KuiperZone.PupNet.Builders;

namespace KuiperZone.PupNet.Test;

public class PackageBuilderTest
{
    [Fact]
    public void DefaultIcons_Available()
    {
        Assert.NotEmpty(PackageBuilder.DefaultGuiIcons);

        foreach (var item in PackageBuilder.DefaultGuiIcons)
        {
            Assert.True(File.Exists(item));
        }
    }

    [Fact]
    public void AppImage_DecodesOK()
    {
        var builder = new AppImageBuilder(new DummyConf());
        AssertOK(builder, PackageKind.AppImage);
        Assert.EndsWith("usr/share/metainfo/net.example.helloworld.appdata.xml", builder.MetaBuildPath);

        // Skip arch - depends on test system -- covered in other tests
        Assert.StartsWith("HelloWorld-5.4.3-2.", builder.OutputName);
        Assert.EndsWith(".AppImage", builder.OutputName);
    }

    [Fact]
    public void Flatpak_DecodesOK()
    {
        var builder = new FlatpakBuilder(new DummyConf());
        AssertOK(builder, PackageKind.Flatpak);
        Assert.EndsWith("usr/share/metainfo/net.example.helloworld.metainfo.xml", builder.MetaBuildPath);

        Assert.StartsWith("HelloWorld-5.4.3-2.", builder.OutputName);
        Assert.EndsWith(".flatpak", builder.OutputName);
    }

    [Fact]
    public void Rpm_DecodesOK()
    {
        var builder = new RpmBuilder(new DummyConf());
        AssertOK(builder, PackageKind.Rpm);
        Assert.EndsWith("usr/share/metainfo/net.example.helloworld.metainfo.xml", builder.MetaBuildPath);

        Assert.Equal("RPMS", builder.OutputName);
    }

    [Fact]
    public void Debian_DecodesOK()
    {
        var builder = new DebianBuilder(new DummyConf());
        AssertOK(builder, PackageKind.Deb);
        Assert.EndsWith("usr/share/metainfo/net.example.helloworld.metainfo.xml", builder.MetaBuildPath);

        Assert.StartsWith("helloworld_5.4.3-2", builder.OutputName);
        Assert.EndsWith(".deb", builder.OutputName);
    }

    [Fact]
    public void Setup_DecodesOK()
    {
        var builder = new SetupBuilder(new DummyConf());
        AssertOK(builder, PackageKind.Setup);
        Assert.Null(builder.MetaBuildPath);

        Assert.StartsWith("HelloWorldSetup-5.4.3-2.", builder.OutputName);
        Assert.EndsWith(".exe", builder.OutputName);
    }

    [Fact]
    public void Zip_DecodesOK()
    {
        var builder = new ZipBuilder(new DummyConf());
        AssertOK(builder, PackageKind.Zip);
        Assert.Null(builder.MetaBuildPath);

        Assert.StartsWith("HelloWorld-5.4.3-2.", builder.OutputName);
        Assert.EndsWith(".zip", builder.OutputName);
    }

    private void AssertOK(PackageBuilder builder, PackageKind kind)
    {
        Assert.Equal(kind, builder.Kind);
        Assert.Equal(kind.TargetsWindows(), !builder.IsLinuxExclusive);

        var appExecName = builder.Runtime.IsWindowsRuntime ? "HelloWorld.exe" : "HelloWorld";
        Assert.Equal(appExecName, builder.AppExecName);
        Assert.Equal("5.4.3", builder.AppVersion);
        Assert.Equal("2", builder.PackageRelease);

        // Not fully qualified as no assert files
        Assert.Equal("Deploy", builder.OutputDirectory);

        if (builder.IsLinuxExclusive)
        {
            Assert.EndsWith($"usr/bin", builder.BuildUsrBin);
            Assert.EndsWith($"usr/share", builder.BuildUsrShare);
            Assert.EndsWith($"usr/share/metainfo", builder.BuildShareMeta);
            Assert.EndsWith($"usr/share/applications", builder.BuildShareApplications);
            Assert.EndsWith($"usr/share/icons", builder.BuildShareIcons);

            Assert.EndsWith($"usr/share/applications/net.example.helloworld.desktop", builder.DesktopBuildPath);

            Assert.Equal($"Assets/Icon.svg", builder.IconSource);

            Assert.Contains($"Assets/Icon.svg", builder.IconPaths.Keys);
            Assert.Contains($"Assets/Icon.32x32.png", builder.IconPaths.Keys);
            Assert.Contains($"Assets/Icon.64x64.png", builder.IconPaths.Keys);
            Assert.DoesNotContain($"Assets/Icon.ico", builder.IconPaths.Keys);
        }
        else
        if (builder.IsWindowsExclusive)
        {
            Assert.Null(builder.BuildUsrBin);
            Assert.Null(builder.BuildUsrShare);
            Assert.Null(builder.BuildShareMeta);
            Assert.Null(builder.BuildShareApplications);
            Assert.Null(builder.BuildShareIcons);

            Assert.Null(builder.DesktopBuildPath);

            // Linux sep is ok
            Assert.Equal($"Assets/Icon.ico", builder.IconSource);
            Assert.True(builder.IconPaths.Count == 0);
        }
        else
        if (builder.IsOsxExclusive)
        {
            // Currently unknown
            Assert.EndsWith($"usr/bin", builder.BuildUsrBin);
            Assert.EndsWith($"usr/share", builder.BuildUsrShare);
        }

        Assert.NotEmpty(builder.BuildAppBin);
    }
}