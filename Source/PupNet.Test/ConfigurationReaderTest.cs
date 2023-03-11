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

public class ConfigurationReaderTest
{
    [Fact]
    public void AppBase_Mandatory_DecodeOK()
    {
        Assert.Equal("HelloWorld", new DummyConf(PackKind.AppImage).AppBaseName);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.AppBaseName)));
    }

    [Fact]
    public void AppId_Mandatory_DecodeOK()
    {
        Assert.Equal("net.example.helloword", new DummyConf(PackKind.AppImage).AppId);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.AppId)));
    }

    [Fact]
    public void AppName_Mandatory_DecodeOK()
    {
        Assert.Equal("Hello World", new DummyConf(PackKind.AppImage).AppFriendlyName);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.AppFriendlyName)));
    }

    [Fact]
    public void AppVersionRelease_Mandatory_DecodeOK()
    {
        Assert.Equal("5.4.3[3]", new DummyConf(PackKind.AppImage).AppVersionRelease);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.AppVersionRelease)).AppVersionRelease);
    }

    [Fact]
    public void AppSummary_Mandatory_DecodeOK()
    {
        Assert.Equal("Test application only", new DummyConf(PackKind.AppImage).AppSummary);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.AppSummary)).AppSummary);
    }

    [Fact]
    public void IsTerminal_Mandatory_DecodeOK()
    {
        Assert.True(new DummyConf(PackKind.AppImage).IsTerminal);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.IsTerminal)));
    }


    [Fact]
    public void DesktopEntry_Optional_DecodeOK()
    {
        Assert.Equal("app.desktop", new DummyConf(PackKind.AppImage).DesktopEntry);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.DesktopEntry)).DesktopEntry);
    }

    [Fact]
    public void DotnetProjectPath_Optional_DecodeOK()
    {
        Assert.Equal("HelloProject", new DummyConf(PackKind.AppImage).DotnetProjectPath);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.DotnetProjectPath)).DotnetProjectPath);
    }

    [Fact]
    public void DotnetPublishArgs_Optional_DecodeOK()
    {
        Assert.Equal("--self-contained true", new DummyConf(PackKind.AppImage).DotnetPublishArgs);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.DotnetPublishArgs)).DotnetPublishArgs);
    }

    [Fact]
    public void DotnetPostPublish_Optional_DecodeOK()
    {
        Assert.Equal(new string[]{"PostPublishCommand.sh"}, new DummyConf(PackKind.AppImage).DotnetPostPublish);
        Assert.Empty(new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.DotnetPostPublish)).DotnetPostPublish);
    }

    [Fact]
    public void OutputVersionName_Mandatory_DecodeOK()
    {
        Assert.True(new DummyConf(PackKind.AppImage).OutputVersion);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.OutputVersion)));
    }

    [Fact]
    public void ToString_VisualInspection()
    {
        Console.WriteLine(new DummyConf(PackKind.AppImage).ToString());
    }

}