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
    public void AppBaseName_Mandatory_DecodeOK()
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
    public void AppFriendlyName_Mandatory_DecodeOK()
    {
        Assert.Equal("Hello World", new DummyConf(PackKind.AppImage).AppFriendlyName);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.AppFriendlyName)));
    }

    [Fact]
    public void VersionRelease_Mandatory_DecodeOK()
    {
        Assert.Equal("5.4.3[2]", new DummyConf(PackKind.AppImage).VersionRelease);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.VersionRelease)).VersionRelease);
    }

    [Fact]
    public void ShortSummary_Mandatory_DecodeOK()
    {
        Assert.Equal("Test application only", new DummyConf(PackKind.AppImage).ShortSummary);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.ShortSummary)).ShortSummary);
    }

    [Fact]
    public void DesktopFile_Optional_DecodeOK()
    {
        Assert.Equal("app.desktop", new DummyConf(PackKind.AppImage).DesktopFile);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.DesktopFile)).DesktopFile);
    }

    [Fact]
    public void MetaFile_Optional_DecodeOK()
    {
        Assert.Equal("metainfo.xml", new DummyConf(PackKind.AppImage).MetaFile);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.MetaFile)).MetaFile);
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
    public void SetupMinWindowsVersion_Mandatory_DecodeOK()
    {
        Assert.Equal("6.9", new DummyConf(PackKind.AppImage).SetupMinWindowsVersion);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.SetupMinWindowsVersion)));
    }

    [Fact]
    public void SetupSignTool_Optional_DecodeOK()
    {
        Assert.Equal("signtool.exe", new DummyConf(PackKind.AppImage).SetupSignTool);
        Assert.Null(new DummyConf(PackKind.AppImage, nameof(ConfigurationReader.SetupSignTool)).SetupSignTool);
    }

    [Fact]
    public void ToString_VisualInspection()
    {
        Console.WriteLine(new DummyConf(PackKind.AppImage).ToString());
    }

}