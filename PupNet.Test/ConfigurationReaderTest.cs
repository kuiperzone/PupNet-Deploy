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

namespace KuiperZone.PupNet.Test;

public class ConfigurationReaderTest
{
    [Fact]
    public void AppBaseName_Mandatory_DecodeOK()
    {
        Assert.Equal("HelloWorld", Create().AppBaseName);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.AppBaseName)));
    }

    [Fact]
    public void AppId_Mandatory_DecodeOK()
    {
        Assert.Equal("net.example.helloworld", Create().AppId);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.AppId)));
    }

    [Fact]
    public void AppFriendlyName_Mandatory_DecodeOK()
    {
        Assert.Equal("Hello World", Create().AppFriendlyName);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.AppFriendlyName)));
    }

    [Fact]
    public void AppVersionRelease_Mandatory_DecodeOK()
    {
        Assert.Equal("5.4.3[2]", Create().AppVersionRelease);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.AppVersionRelease)).AppVersionRelease);
    }

    [Fact]
    public void AppShortSummary_Mandatory_DecodeOK()
    {
        // Use of angle brackets deliberate
        Assert.Equal("Test <application> only", Create().AppShortSummary);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.AppShortSummary)).AppShortSummary);
    }

    [Fact]
    public void AppDescription_Optional_DecodeOK()
    {
        // Use of angle brackets deliberate
        var lines = Create().AppDescription;
        var exp = new string[] { "Para1-Line1", "<Para1-Line2>", "", "- Bullet1", "* Bullet2", "Para2-Line1 has ${MACRO_VAR}" };

        Assert.Equal(exp, lines);
        Assert.Empty(Create(nameof(ConfigurationReader.AppDescription)).AppDescription);
    }

    [Fact]
    public void AppLicenseId_Mandatory_DecodeOK()
    {
        Assert.Equal("LicenseRef-LICENSE", Create().AppLicenseId);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.AppLicenseId)).AppLicenseId);
    }

    [Fact]
    public void AppLicenseFile_Optional_DecodeOK()
    {
        Assert.Equal("LICENSE", Create().AppLicenseFile);
        Assert.Null(Create(nameof(ConfigurationReader.AppLicenseFile)).AppLicenseFile);
    }

    [Fact]
    public void AppChangeFile_Optional_DecodeOK()
    {
        Assert.Equal("CHANGELOG", Create().AppChangeFile);
        Assert.Null(Create(nameof(ConfigurationReader.AppChangeFile)).AppChangeFile);
    }

    [Fact]
    public void PublisherName_Mandatory_DecodeOK()
    {
        // Use of angle brackets deliberate
        Assert.Equal("Kuiper Zone", Create().PublisherName);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.PublisherName)).PublisherName);
    }

    [Fact]
    public void PublisherCopyright_Optional_DecodeOK()
    {
        Assert.Equal("Copyright Kuiper Zone", Create().PublisherCopyright);
        Assert.Null(Create(nameof(ConfigurationReader.PublisherCopyright)).PublisherCopyright);
    }

    [Fact]
    public void PublisherLinkName_Optional_DecodeOK()
    {
        Assert.Equal("kuiper.zone", Create().PublisherLinkName);
        Assert.Null(Create(nameof(ConfigurationReader.PublisherLinkName)).PublisherLinkName);
    }

    [Fact]
    public void PublisherLinkUrl_Optional_DecodeOK()
    {
        Assert.Equal("https://kuiper.zone", Create().PublisherLinkUrl);
        Assert.Null(Create(nameof(ConfigurationReader.PublisherLinkUrl)).PublisherLinkUrl);
    }

    [Fact]
    public void PublisherEmail_Optional_DecodeOK()
    {
        Assert.Equal("email@example.net", Create().PublisherEmail);
        Assert.Null(Create(nameof(ConfigurationReader.PublisherEmail)).PublisherEmail);
    }

    [Fact]
    public void StartCommand_Optional_DecodeOK()
    {
        Assert.Equal("helloworld", Create().StartCommand);
        Assert.Null(Create(nameof(ConfigurationReader.StartCommand)).StartCommand);
    }

    [Fact]
    public void DesktopNoDisplay_Bool_IsTrue()
    {
        Assert.True(Create().DesktopNoDisplay);
        Assert.False(Create(nameof(ConfigurationReader.DesktopNoDisplay)).DesktopNoDisplay);
    }

    [Fact]
    public void DesktopTerminal_Bool_IsFalse()
    {
        Assert.False(Create().DesktopTerminal);
        Assert.True(Create(nameof(ConfigurationReader.DesktopTerminal)).DesktopTerminal);
    }

    [Fact]
    public void DesktopFile_Optional_DecodeOK()
    {
        Assert.Equal("app.desktop", Create().DesktopFile);
        Assert.Null(Create(nameof(ConfigurationReader.DesktopFile)).DesktopFile);
    }

    [Fact]
    public void PrimeCategory_Optional_DecodeOK()
    {
        Assert.Equal("Development", Create().PrimeCategory);
        Assert.Null(Create(nameof(ConfigurationReader.PrimeCategory)).PrimeCategory);
    }

    [Fact]
    public void IconFiles_Optional_DecodeOK()
    {
        var paths = Create().IconFiles;
        Assert.NotEmpty(paths);
        Assert.Contains("Assets/Icon.svg", paths);
        Assert.Contains("Assets/Icon.32x32.png", paths);

        Assert.Empty(Create(nameof(ConfigurationReader.IconFiles)).IconFiles);
    }

    [Fact]
    public void MetaFile_Optional_DecodeOK()
    {
        Assert.Equal("metainfo.xml", Create().MetaFile);
        Assert.Null(Create(nameof(ConfigurationReader.MetaFile)).MetaFile);
    }

    [Fact]
    public void DotnetProjectPath_Optional_DecodeOK()
    {
        Assert.Equal("HelloProject", Create().DotnetProjectPath);
        Assert.NotNull(Create(nameof(ConfigurationReader.DotnetProjectPath)).DotnetProjectPath);
    }

    [Fact]
    public void DotnetPublishArgs_Optional_DecodeOK()
    {
        Assert.Equal("--self-contained true", Create().DotnetPublishArgs);
        Assert.Null(Create(nameof(ConfigurationReader.DotnetPublishArgs)).DotnetPublishArgs);
    }

    [Fact]
    public void DotnetPostPublish_Optional_DecodeOK()
    {
        Assert.Equal("PostPublishCommand.sh", Create().DotnetPostPublish);
        Assert.Null(Create(nameof(ConfigurationReader.DotnetPostPublish)).DotnetPostPublish);
    }

    [Fact]
    public void DotnetPostPublishOnWindows_Optional_DecodeOK()
    {
        Assert.Equal("PostPublishCommandOnWindows.bat", Create().DotnetPostPublishOnWindows);
        Assert.Null(Create(nameof(ConfigurationReader.DotnetPostPublishOnWindows)).DotnetPostPublishOnWindows);
    }

    [Fact]
    public void AppImageArgs_Optional_DecodeOK()
    {
        Assert.Equal("-appargs", Create().AppImageArgs);
        Assert.Null(Create(nameof(ConfigurationReader.AppImageArgs)).AppImageArgs);
    }

    [Fact]
    public void AppImageVersionOutput_Bool_IsTrue()
    {
        Assert.True(Create().AppImageVersionOutput);
        Assert.False(Create(nameof(ConfigurationReader.AppImageVersionOutput)).AppImageVersionOutput);
    }


    [Fact]
    public void FlatpakPlatformRuntime_Mandatory_DecodeOK()
    {
        Assert.Equal("org.freedesktop.Platform", Create().FlatpakPlatformRuntime);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.FlatpakPlatformRuntime)));
    }

    [Fact]
    public void FlatpakPlatformSdk_Mandatory_DecodeOK()
    {
        Assert.Equal("org.freedesktop.Sdk", Create().FlatpakPlatformSdk);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.FlatpakPlatformSdk)));
    }

    [Fact]
    public void FlatpakPlatformVersion_Mandatory_DecodeOK()
    {
        Assert.Equal("18.00", Create().FlatpakPlatformVersion);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.FlatpakPlatformVersion)));
    }

    [Fact]
    public void FlatpakFinishArgs_Optional_DecodeOK()
    {
        var args = Create().FlatpakFinishArgs;
        Assert.NotEmpty(args);
        Assert.Contains("--socket=wayland", args);
        Assert.Contains("--share=network", args);
        Assert.Empty(Create(nameof(ConfigurationReader.FlatpakFinishArgs)).FlatpakFinishArgs);
    }

    [Fact]
    public void FlatpakBuilderArgs_Optional_DecodeOK()
    {
        Assert.Equal("-flatargs", Create().FlatpakBuilderArgs);
        Assert.Null(Create(nameof(ConfigurationReader.FlatpakBuilderArgs)).FlatpakBuilderArgs);
    }


    [Fact]
    public void RpmAutoReq_Bool_IsTrue()
    {
        Assert.True(Create().RpmAutoReq);
        Assert.False(Create(nameof(ConfigurationReader.RpmAutoReq)).RpmAutoReq);
    }

    [Fact]
    public void RpmAutoProv_Bool_IsFalse()
    {
        Assert.False(Create().RpmAutoProv);
        Assert.True(Create(nameof(ConfigurationReader.RpmAutoProv)).RpmAutoProv);
    }

    [Fact]
    public void RpmRequires_Optional_DecodeOK()
    {
        var args = Create().RpmRequires;

        Assert.NotEmpty(args);
        Assert.Contains("rpm-requires1", args);
        Assert.Contains("rpm-requires2", args);

        args = Create(nameof(ConfigurationReader.RpmRequires)).RpmRequires;
        Assert.Contains("libicu", args);
    }

    [Fact]
    public void DebianRecommends_Optional_DecodeOK()
    {
        var args = Create().DebianRecommends;

        Assert.NotEmpty(args);
        Assert.Contains("deb-depends1", args);
        Assert.Contains("deb-depends2", args);

        args = Create(nameof(ConfigurationReader.DebianRecommends)).DebianRecommends;
        Assert.Contains("libicu", args);
    }

    [Fact]
    public void SetupAdminInstall_Bool_IsTrue()
    {
        Assert.True(Create().SetupAdminInstall);
        Assert.False(Create(nameof(ConfigurationReader.SetupAdminInstall)).SetupAdminInstall);
    }

    [Fact]
    public void SetupCommandPrompt_Optional_DecodeOK()
    {
        Assert.Equal("Command Prompt", Create().SetupCommandPrompt);
        Assert.Null(Create(nameof(ConfigurationReader.SetupCommandPrompt)).SetupCommandPrompt);
    }

    [Fact]
    public void SetupMinWindowsVersion_Mandatory_DecodeOK()
    {
        Assert.Equal("6.9", Create().SetupMinWindowsVersion);
        Assert.Throws<ArgumentException>(() => Create(nameof(ConfigurationReader.SetupMinWindowsVersion)));
    }

    [Fact]
    public void SetupSignTool_Optional_DecodeOK()
    {
        Assert.Equal(DummyConf.ExpectSignTool, Create().SetupSignTool);
        Assert.Null(Create(nameof(ConfigurationReader.SetupSignTool)).SetupSignTool);
    }

    [Fact]
    public void SetupSuffixOutput_Optional_DecodeOK()
    {
        Assert.Equal("Setup", Create().SetupSuffixOutput);
        Assert.Null(Create(nameof(ConfigurationReader.SetupSuffixOutput)).SetupSuffixOutput);
    }

    [Fact]
    public void SetupVersionOutput_Bool_IsTrue()
    {
        Assert.True(Create().SetupVersionOutput);
        Assert.False(Create(nameof(ConfigurationReader.SetupVersionOutput)).SetupVersionOutput);
    }

    [Fact]
    public void ToString_VisualInspection()
    {
        Console.WriteLine(Create().ToString(DocStyles.Comments));
    }

    private static ConfigurationReader Create(string? omit = null)
    {
        var conf = new DummyConf(PackageKind.AppImage, omit);

        // Ensure can read its own output
        var content = conf.ToString(DocStyles.Comments).Split('\n');
        return new ConfigurationReader(conf.Arguments, content);
    }


}