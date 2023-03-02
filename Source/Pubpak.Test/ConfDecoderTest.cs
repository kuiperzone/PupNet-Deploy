// -----------------------------------------------------------------------------
// PROJECT   : Pubpak
// COPYRIGHT : Andy Thomas (C) 2022-23
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/Pubpak
//
// Pubpak is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// Pubpak is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with Pubpak. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

namespace KuiperZone.Pubpak.Test;

public class ConfDecoderTest
{
    [Fact]
    public void AppBase_Mandatory_DecodeOK()
    {
        Assert.Equal("helloworld", new DummyConf(PackKind.AppImage).AppBase);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.AppBase)));
    }

    [Fact]
    public void AppId_Mandatory_DecodeOK()
    {
        Assert.Equal("net.example.helloword", new DummyConf(PackKind.AppImage).AppId);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.AppId)));
    }

    [Fact]
    public void AppName_Mandatory_DecodeOK()
    {
        Assert.Equal("Hello World", new DummyConf(PackKind.AppImage).AppName);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.AppName)));
    }

    [Fact]
    public void AppVersionRelease_Mandatory_DecodeOK()
    {
        Assert.Equal("5.4.3[3]", new DummyConf(PackKind.AppImage).AppVersionRelease);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.AppVersionRelease)).AppVersionRelease);
    }

    [Fact]
    public void AppSummary_Mandatory_DecodeOK()
    {
        Assert.Equal("Test application only", new DummyConf(PackKind.AppImage).AppSummary);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.AppSummary)).AppSummary);
    }

    [Fact]
    public void DesktopCategories_Mandatory_DecodeOK()
    {
        Assert.Equal("Utility;Programming", new DummyConf(PackKind.AppImage).DesktopCategory);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.DesktopCategory)));
    }

    [Fact]
    public void DesktopTerminal_Mandatory_DecodeOK()
    {
        Assert.True(new DummyConf(PackKind.AppImage).DesktopTerminal);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.DesktopTerminal)));
    }

    [Fact]
    public void DotnetProjectPath_Optional_DecodeOK()
    {
        Assert.Equal("HelloProject", new DummyConf(PackKind.AppImage).DotnetProjectPath);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfDecoder.DotnetProjectPath)).DotnetProjectPath);
    }

    [Fact]
    public void DotnetPublishArgs_Optional_DecodeOK()
    {
        Assert.Equal("--self-contained true", new DummyConf(PackKind.AppImage).DotnetPublishArgs);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfDecoder.DotnetPublishArgs)).DotnetPublishArgs);
    }

    [Fact]
    public void DotnetPostPublish_Optional_DecodeOK()
    {
        Assert.Equal(new string[]{"PostPublishCommand.sh"}, new DummyConf(PackKind.AppImage).DotnetPostPublish);
        Assert.Empty(new DummyConf(PackKind.AppImage, nameof(ConfDecoder.DotnetPostPublish)).DotnetPostPublish);
    }

    [Fact]
    public void OutputVersionName_Mandatory_DecodeOK()
    {
        Assert.True(new DummyConf(PackKind.AppImage).OutputVersion);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.OutputVersion)));
    }

    [Fact]
    public void AppImageCommand_Mandatory_DecodeOK()
    {
        Assert.Equal("appimagetool", new DummyConf(PackKind.AppImage).AppImageCommand);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfDecoder.AppImageCommand)));
    }

    [Fact]
    public void ToString_VisualInspection()
    {
        Console.WriteLine(new DummyConf(PackKind.AppImage).ToString());
    }

}