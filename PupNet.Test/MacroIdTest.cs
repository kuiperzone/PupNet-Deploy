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

public class MacroIdTest
{
    [Fact]
    public void ToName_Regression_AllHaveExpectedNames()
    {
        // REGRESSION - it would be bad form to change names once the
        // software is in the wild, as it break existing configurations
        Assert.Equal("APP_BASE_NAME", MacroId.AppBaseName.ToName());
        Assert.Equal("APP_FRIENDLY_NAME", MacroId.AppFriendlyName.ToName());
        Assert.Equal("APP_ID", MacroId.AppId.ToName());
        Assert.Equal("APP_SHORT_SUMMARY", MacroId.AppShortSummary.ToName());
        Assert.Equal("APP_LICENSE_ID", MacroId.AppLicenseId.ToName());
        Assert.Equal("PUBLISHER_NAME", MacroId.PublisherName.ToName());
        Assert.Equal("PUBLISHER_COPYRIGHT", MacroId.PublisherCopyright.ToName());
        Assert.Equal("PUBLISHER_LINK_NAME", MacroId.PublisherLinkName.ToName());
        Assert.Equal("PUBLISHER_LINK_URL", MacroId.PublisherLinkUrl.ToName());
        Assert.Equal("PUBLISHER_EMAIL", MacroId.PublisherEmail.ToName());
        Assert.Equal("DESKTOP_NODISPLAY", MacroId.DesktopNoDisplay.ToName());
        Assert.Equal("DESKTOP_INTEGRATE", MacroId.DesktopIntegrate.ToName());
        Assert.Equal("DESKTOP_TERMINAL", MacroId.DesktopTerminal.ToName());
        Assert.Equal("PRIME_CATEGORY", MacroId.PrimeCategory.ToName());

        // Others
        Assert.Equal("APPSTREAM_DESCRIPTION_XML", MacroId.AppStreamDescriptionXml.ToName());
        Assert.Equal("APPSTREAM_CHANGELOG_XML", MacroId.AppStreamChangelogXml.ToName());
        Assert.Equal("APP_VERSION", MacroId.AppVersion.ToName());
        Assert.Equal("DEPLOY_KIND", MacroId.DeployKind.ToName());
        Assert.Equal("DOTNET_RUNTIME", MacroId.DotnetRuntime.ToName());
        Assert.Equal("BUILD_ARCH", MacroId.BuildArch.ToName());
        Assert.Equal("BUILD_TARGET", MacroId.BuildTarget.ToName());
        Assert.Equal("BUILD_DATE", MacroId.BuildDate.ToName());
        Assert.Equal("BUILD_YEAR", MacroId.BuildYear.ToName());
        Assert.Equal("BUILD_ROOT", MacroId.BuildRoot.ToName());
        Assert.Equal("BUILD_SHARE", MacroId.BuildShare.ToName());
        Assert.Equal("BUILD_APP_BIN", MacroId.BuildAppBin.ToName());

        // Install locations
        Assert.Equal("INSTALL_BIN", MacroId.InstallBin.ToName());
        Assert.Equal("INSTALL_EXEC", MacroId.InstallExec.ToName());

        // Make sure we've not missed any
        foreach (var item in Enum.GetValues<MacroId>())
        {
            Console.WriteLine(item);
            Assert.NotEmpty(item.ToName());
        }
    }

    [Fact]
    public void ToHint_AllHaveHints()
    {
        foreach (var item in Enum.GetValues<MacroId>())
        {
            Console.WriteLine(item);
            Assert.NotEmpty(item.ToHint());
        }
    }
}