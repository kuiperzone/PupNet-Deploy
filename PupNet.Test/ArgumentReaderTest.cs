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

namespace KuiperZone.PupNet.Test;

public class ArgumentReaderTest
{
    [Fact]
    public void Version_DecodeOK()
    {
        // Also test verbose here
        var args = new ArgumentReader("--version --verbose");
        Assert.True(args.ShowVersion);
        Assert.True(args.IsVerbose);
    }

    [Fact]
    public void Help_DecodeOK()
    {
        var args = new ArgumentReader("-h Macros");
        Assert.Equal("macros", args.ShowHelp);

        args = new ArgumentReader("--help");
        Assert.Equal("true", args.ShowHelp);
    }

    [Fact]
    public void Value_DecodeOK()
    {
        // Default
        var args = new ArgumentReader("f1.conf");
        Assert.Equal("f1.conf", args.Value);

        args = new ArgumentReader("f2.conf");
        Assert.Equal("f2.conf", args.Value);
    }

    [Fact]
    public void RuntimeId_DecodeOK()
    {
        // Default - changes depending on system
        var args = new ArgumentReader();
        Assert.Equal(RuntimeConverter.DefaultRuntime, args.Runtime);

        args = new ArgumentReader("-r test1");
        Assert.Equal("test1", args.Runtime);

        args = new ArgumentReader("--runtime test2");
        Assert.Equal("test2", args.Runtime);
    }

    [Fact]
    public void Kind_DecodeOK()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var args = new ArgumentReader();
            Assert.Equal(PackageKind.AppImage, args.Kind);

            args = new ArgumentReader("-k rpm");
            Assert.Equal(PackageKind.Rpm, args.Kind);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var args = new ArgumentReader();
            Assert.Equal(PackageKind.Setup, args.Kind);

            args = new ArgumentReader("-k zip");
            Assert.Equal(PackageKind.Zip, args.Kind);
        }
    }

    [Fact]
    public void VersionRelease_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Null(args.VersionRelease);

        args = new ArgumentReader("-v 5.4.3[2]");
        Assert.Equal("5.4.3[2]", args.VersionRelease);

        args = new ArgumentReader("--app-version 5.4.3[2]");
        Assert.Equal("5.4.3[2]", args.VersionRelease);
    }

    [Fact]
    public void Property_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Null(args.Property);

        args = new ArgumentReader("-p DEBUG");
        Assert.Equal("DEBUG", args.Property);

        args = new ArgumentReader("--property DEBUG");
        Assert.Equal("DEBUG", args.Property);
    }

    [Fact]
    public void Output_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Null(args.Output);

        args = new ArgumentReader("-o OutputName");
        Assert.Equal("OutputName", args.Output);

        args = new ArgumentReader("--output OutputName");
        Assert.Equal("OutputName", args.Output);
    }

    [Fact]
    public void Arch_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Null(args.Arch);

        args = new ArgumentReader("--arch archname");
        Assert.Equal("archname", args.Arch);
    }

    [Fact]
    public void Run_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.False(args.IsRun);

        args = new ArgumentReader("-u");
        Assert.True(args.IsRun);

        args = new ArgumentReader("--run");
        Assert.True(args.IsRun);
    }

    [Fact]
    public void SkipYes_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.False(args.IsSkipYes);

        args = new ArgumentReader("-y");
        Assert.True(args.IsSkipYes);

        args = new ArgumentReader("--skip-yes");
        Assert.True(args.IsSkipYes);
    }

    [Fact]
    public void Project_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Null(args.Project);

        args = new ArgumentReader("-j path.csproj");
        Assert.Equal("path.csproj", args.Project);

        args = new ArgumentReader("--project path.csproj");
        Assert.Equal("path.csproj", args.Project);
    }

    [Fact]
    public void New_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Null(args.NewFile);

        args = new ArgumentReader("FileName --new Conf");
        Assert.Equal("conf", args.NewFile);
        Assert.Equal("FileName", args.Value);

        args = new ArgumentReader("FileName --new");
        Assert.Equal("true", args.NewFile);

        args = new ArgumentReader("-n Meta");
        Assert.Equal("meta", args.NewFile);
    }

}