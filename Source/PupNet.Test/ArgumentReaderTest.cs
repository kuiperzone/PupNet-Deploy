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
        var args = new ArgumentReader("-h");
        Assert.True(args.ShowHelp);

        args = new ArgumentReader("--help");
        Assert.True(args.ShowHelp);
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
        Assert.Equal(ArgumentReader.DefaultRuntime, args.Runtime);

        args = new ArgumentReader("-r test1");
        Assert.Equal("test1", args.Runtime);

        args = new ArgumentReader("--runtime test2");
        Assert.Equal("test2", args.Runtime);
    }

    [Fact]
    public void Kind_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Equal(ArgumentReader.DefaultKind, args.Kind);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            args = new ArgumentReader("-k rpm");
            Assert.Equal(PackKind.Rpm, args.Kind);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            args = new ArgumentReader("-k zip");
            Assert.Equal(PackKind.Zip, args.Kind);
        }
    }

    [Fact]
    public void Kinds_AssertsInvalid()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Throws<ArgumentException>(() => new ArgumentReader("-k winsetup"));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => new ArgumentReader("-k deb"));
        }
    }

    [Fact]
    public void AppVersion_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Null(args.AppVersion);

        args = new ArgumentReader("-v 5.4.3[2]");
        Assert.Equal("5.4.3[2]", args.AppVersion);

        args = new ArgumentReader("--app-version 5.4.3[2]");
        Assert.Equal("5.4.3[2]", args.AppVersion);
    }

    // [Fact] Disable
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

        args = new ArgumentReader("-a arch1");
        Assert.Equal("arch1", args.Arch);

        args = new ArgumentReader("--arch arch2");
        Assert.Equal("arch2", args.Arch);
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
    public void New_DecodeOK()
    {
        var args = new ArgumentReader();
        Assert.Equal(NewKind.None, args.New);

        args = new ArgumentReader("FileName --new conf");
        Assert.Equal(NewKind.Conf, args.New);
        Assert.Equal("FileName", args.Value);

        args = new ArgumentReader("-n meta");
        Assert.Equal(NewKind.Meta, args.New);
    }
}