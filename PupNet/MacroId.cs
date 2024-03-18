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

namespace KuiperZone.PupNet;

/// <summary>
/// Defines expandable macros.
/// </summary>
public enum MacroId
{
    LocalDirectory,
    AppBaseName,
    AppFriendlyName,
    AppId,
    AppShortSummary,
    AppLicenseId,
    PublisherName,
    PublisherCopyright,
    PublisherLinkName,
    PublisherLinkUrl,
    PublisherEmail,
    DesktopNoDisplay,
    DesktopIntegrate,
    DesktopTerminal,
    PrimeCategory,

    AppStreamDescriptionXml,
    AppStreamChangelogXml,
    AppVersion,
    PackageRelease,
    DeployKind,
    DotnetRuntime,
    BuildArch,
    BuildTarget,
    BuildDate,
    BuildYear,
    BuildRoot,
    BuildShare,
    BuildAppBin,

    InstallBin,
    InstallExec,
}

/// <summary>
/// Extension methods.
/// </summary>
public static class MacroIdExtension
{
    /// <summary>
    /// Converts to name string (i.e. "APP_BASE_NAME").
    /// </summary>
    public static string ToName(this MacroId id)
    {
        // Do not change names as will break configs out in the wild
        switch (id)
        {
            // Direct from config
            case MacroId.LocalDirectory: return "LOCAL_DIRECTORY";
            case MacroId.AppBaseName: return "APP_BASE_NAME";
            case MacroId.AppFriendlyName: return "APP_FRIENDLY_NAME";
            case MacroId.AppId: return "APP_ID";
            case MacroId.AppShortSummary: return "APP_SHORT_SUMMARY";
            case MacroId.AppLicenseId: return "APP_LICENSE_ID";
            case MacroId.PublisherName: return "PUBLISHER_NAME";
            case MacroId.PublisherCopyright: return "PUBLISHER_COPYRIGHT";
            case MacroId.PublisherLinkName: return "PUBLISHER_LINK_NAME";
            case MacroId.PublisherLinkUrl: return "PUBLISHER_LINK_URL";
            case MacroId.PublisherEmail: return "PUBLISHER_EMAIL";
            case MacroId.DesktopNoDisplay: return "DESKTOP_NODISPLAY";
            case MacroId.DesktopIntegrate: return "DESKTOP_INTEGRATE";
            case MacroId.DesktopTerminal: return "DESKTOP_TERMINAL";
            case MacroId.PrimeCategory: return "PRIME_CATEGORY";

            // Others
            case MacroId.AppStreamDescriptionXml: return "APPSTREAM_DESCRIPTION_XML";
            case MacroId.AppStreamChangelogXml: return "APPSTREAM_CHANGELOG_XML";
            case MacroId.AppVersion: return "APP_VERSION";
            case MacroId.PackageRelease: return "PACKAGE_RELEASE";
            case MacroId.DeployKind: return "DEPLOY_KIND";
            case MacroId.DotnetRuntime: return "DOTNET_RUNTIME";
            case MacroId.BuildArch: return "BUILD_ARCH";
            case MacroId.BuildTarget: return "BUILD_TARGET";
            case MacroId.BuildDate: return "BUILD_DATE";
            case MacroId.BuildYear: return "BUILD_YEAR";
            case MacroId.BuildRoot: return "BUILD_ROOT";
            case MacroId.BuildShare: return "BUILD_SHARE";
            case MacroId.BuildAppBin: return "BUILD_APP_BIN";

            // Install locations
            case MacroId.InstallBin: return "INSTALL_BIN";
            case MacroId.InstallExec: return "INSTALL_EXEC";

            default: throw new ArgumentException("Unknown macro " + id);
        }
    }

    /// <summary>
    /// Returns true if the macro value may contain XML and/or other key names.
    /// </summary>
    public static bool ContainsXml(this MacroId id)
    {
        return id == MacroId.AppStreamDescriptionXml || id == MacroId.AppStreamChangelogXml;
    }

    /// <summary>
    /// Converts to variable string (i.e. "${APP_BASE_NAME}").
    /// </summary>
    public static string ToVar(this MacroId id)
    {
        return "${" + ToName(id) + "}";
    }

    public static string ToHint(this MacroId id)
    {
        switch (id)
        {
            case MacroId.LocalDirectory: return $"The pupnet.conf file directory";
            case MacroId.AppBaseName: return GetConfHelp(nameof(ConfigurationReader.AppBaseName));
            case MacroId.AppFriendlyName: return GetConfHelp(nameof(ConfigurationReader.AppFriendlyName));
            case MacroId.AppId: return GetConfHelp(nameof(ConfigurationReader.AppId));
            case MacroId.AppShortSummary: return GetConfHelp(nameof(ConfigurationReader.AppShortSummary));
            case MacroId.AppLicenseId: return GetConfHelp(nameof(ConfigurationReader.AppLicenseId));

            case MacroId.PublisherName: return GetConfHelp(nameof(ConfigurationReader.PublisherName));
            case MacroId.PublisherCopyright: return GetConfHelp(nameof(ConfigurationReader.PublisherCopyright));
            case MacroId.PublisherLinkName: return GetConfHelp(nameof(ConfigurationReader.PublisherLinkName));
            case MacroId.PublisherLinkUrl: return GetConfHelp(nameof(ConfigurationReader.PublisherLinkUrl));
            case MacroId.PublisherEmail: return GetConfHelp(nameof(ConfigurationReader.PublisherEmail));

            case MacroId.DesktopNoDisplay: return GetConfHelp(nameof(ConfigurationReader.DesktopNoDisplay));
            case MacroId.DesktopTerminal: return GetConfHelp(nameof(ConfigurationReader.DesktopTerminal));
            case MacroId.PrimeCategory: return GetConfHelp(nameof(ConfigurationReader.PrimeCategory));

            case MacroId.DesktopIntegrate: return $"Gives the logical not of {MacroId.DesktopNoDisplay.ToVar()}";

            case MacroId.AppStreamDescriptionXml: return "AppStream application description XML (use within the <description> element only)";
            case MacroId.AppStreamChangelogXml: return "AppStream changelog XML content (use within the <releases> element only)";
            case MacroId.AppVersion: return "Application version, excluding package-release extension";
            case MacroId.PackageRelease: return "Package release version";
            case MacroId.DeployKind: return "Deployment output kind: appimage, flatpak, rpm, deb, setup, zip";
            case MacroId.DotnetRuntime: return "Dotnet publish runtime identifier used (RID)";

            case MacroId.BuildArch: return "Build architecture: x64, arm64, arm or x86 (may differ from package output notation)";
            case MacroId.BuildTarget: return "Release or Debug (Release unless explicitly specified)";
            case MacroId.BuildDate: return "Build date in 'yyyy-MM-dd' format";
            case MacroId.BuildYear: return "Build year as 'yyyy'";
            case MacroId.BuildRoot: return "Root of the temporary application build directory";
            case MacroId.BuildShare: return $"Linux 'usr/share' build directory under {nameof(MacroId.BuildRoot)} (empty for some deployments)";
            case MacroId.BuildAppBin: return "Application build directory (i.e. the output of dotnet publish or C++ make)";

            case MacroId.InstallBin: return "Path to application directory on target system (not the build system)";
            case MacroId.InstallExec: return "Path to application executable on target system (not the build system)";

            default: throw new ArgumentException("Unknown macro " + id);
        }
    }

    private static string GetConfHelp(string name)
    {
        return $"Gives the {name} value from the pupnet.conf file";
    }
}
