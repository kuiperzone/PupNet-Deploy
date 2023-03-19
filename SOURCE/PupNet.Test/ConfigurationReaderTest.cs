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
        Assert.Equal("HelloWorld", new DummyConf(PackageKind.AppImage).AppBaseName);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppBaseName)));
    }

    [Fact]
    public void AppId_Mandatory_DecodeOK()
    {
        Assert.Equal("net.example.helloworld", new DummyConf(PackageKind.AppImage).AppId);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppId)));
    }

    [Fact]
    public void AppFriendlyName_Mandatory_DecodeOK()
    {
        Assert.Equal("Hello World", new DummyConf(PackageKind.AppImage).AppFriendlyName);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppFriendlyName)));
    }

    [Fact]
    public void AppVersionRelease_Mandatory_DecodeOK()
    {
        Assert.Equal("5.4.3[2]", new DummyConf(PackageKind.AppImage).AppVersionRelease);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppVersionRelease)).AppVersionRelease);
    }

    [Fact]
    public void AppShortSummary_Mandatory_DecodeOK()
    {
        // Use of angle brackets deliberate
        Assert.Equal("Test <application> only", new DummyConf(PackageKind.AppImage).AppShortSummary);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppShortSummary)).AppShortSummary);
    }

    [Fact]
    public void AppLicenseId_Mandatory_DecodeOK()
    {
        Assert.Equal("LicenseRef-LICENSE", new DummyConf(PackageKind.AppImage).AppLicenseId);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppLicenseId)).AppLicenseId);
    }

    [Fact]
    public void AppLicenseFile_Optional_DecodeOK()
    {
        Assert.Equal("LICENSE", new DummyConf(PackageKind.AppImage).AppLicenseFile);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppLicenseFile)).AppLicenseFile);
    }

    [Fact]
    public void PublisherName_Mandatory_DecodeOK()
    {
        // Use of angle brackets deliberate
        Assert.Equal("Kuiper Zone", new DummyConf(PackageKind.AppImage).PublisherName);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.PublisherName)).PublisherName);
    }

    [Fact]
    public void PublisherCopyright_Optional_DecodeOK()
    {
        Assert.Equal("Copyright Kuiper Zone", new DummyConf(PackageKind.AppImage).PublisherCopyright);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.PublisherCopyright)).PublisherCopyright);
    }

    [Fact]
    public void PublisherLinkName_Optional_DecodeOK()
    {
        Assert.Equal("kuiper.zone", new DummyConf(PackageKind.AppImage).PublisherLinkName);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.PublisherLinkName)).PublisherLinkName);
    }

    [Fact]
    public void PublisherLinkUrl_Optional_DecodeOK()
    {
        Assert.Equal("https://kuiper.zone", new DummyConf(PackageKind.AppImage).PublisherLinkUrl);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.PublisherLinkUrl)).PublisherLinkUrl);
    }

    [Fact]
    public void PublisherEmail_Optional_DecodeOK()
    {
        Assert.Equal("email@example.net", new DummyConf(PackageKind.AppImage).PublisherEmail);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.PublisherEmail)).PublisherEmail);
    }

    [Fact]
    public void DesktopFile_Optional_DecodeOK()
    {
        Assert.Equal("app.desktop", new DummyConf(PackageKind.AppImage).DesktopFile);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.DesktopFile)).DesktopFile);
    }

    [Fact]
    public void MetaFile_Optional_DecodeOK()
    {
        Assert.Equal("metainfo.xml", new DummyConf(PackageKind.AppImage).MetaFile);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.MetaFile)).MetaFile);
    }

    [Fact]
    public void DotnetProjectPath_Optional_DecodeOK()
    {
        Assert.Equal("HelloProject", new DummyConf(PackageKind.AppImage).DotnetProjectPath);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.DotnetProjectPath)).DotnetProjectPath);
    }

    [Fact]
    public void DotnetPublishArgs_Optional_DecodeOK()
    {
        Assert.Equal("--self-contained true", new DummyConf(PackageKind.AppImage).DotnetPublishArgs);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.DotnetPublishArgs)).DotnetPublishArgs);
    }

    [Fact]
    public void DotnetPostPublish_Optional_DecodeOK()
    {
        Assert.Equal("PostPublishCommand.sh", new DummyConf(PackageKind.AppImage).DotnetPostPublish);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.DotnetPostPublish)).DotnetPostPublish);
    }

    [Fact]
    public void DotnetPostPublishOnWindows_Optional_DecodeOK()
    {
        Assert.Equal("PostPublishCommandOnWindows.bat", new DummyConf(PackageKind.AppImage).DotnetPostPublishOnWindows);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.DotnetPostPublishOnWindows)).DotnetPostPublishOnWindows);
    }

    [Fact]
    public void OutputVersionName_Mandatory_DecodeOK()
    {
        Assert.True(new DummyConf(PackageKind.AppImage).OutputVersion);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.OutputVersion)));
    }

    [Fact]
    public void SetupCommandPrompt_Optional_DecodeOK()
    {
        Assert.Equal("Command Prompt", new DummyConf(PackageKind.AppImage).SetupPrompt);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.SetupPrompt)).SetupPrompt);
    }

    [Fact]
    public void SetupSignTool_Optional_DecodeOK()
    {
        Assert.Equal("signtool.exe", new DummyConf(PackageKind.AppImage).SetupSignTool);
        Assert.Null(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.SetupSignTool)).SetupSignTool);
    }

    [Fact]
    public void SetupMinWindowsVersion_Mandatory_DecodeOK()
    {
        Assert.Equal("6.9", new DummyConf(PackageKind.AppImage).SetupMinWindowsVersion);
        Assert.Throws<ArgumentException>(() => new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.SetupMinWindowsVersion)));
    }

    [Fact]
    public void ToString_VisualInspection()
    {
        Console.WriteLine(new DummyConf(PackageKind.AppImage).ToString(false));
    }

}