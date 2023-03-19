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

namespace KuiperZone.PupNet;

/// <summary>
/// Defines expandable macros.
/// </summary>
public enum MacroId
{
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
    IsTerminalApp,
    PrimeCategory,

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
            case MacroId.IsTerminalApp: return "IS_TERMINAL_APP";
            case MacroId.PrimeCategory: return "PRIME_CATEGORY";

            // Others
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
            case MacroId.AppBaseName: return $"{nameof(ConfigurationReader.AppBaseName)} value from conf file";
            case MacroId.AppFriendlyName: return $"{nameof(ConfigurationReader.AppFriendlyName)} value from conf file";
            case MacroId.AppId: return $"{nameof(ConfigurationReader.AppId)} value from conf file";
            case MacroId.AppShortSummary: return $"{nameof(ConfigurationReader.AppShortSummary)} value from conf file";
            case MacroId.AppLicenseId: return $"{nameof(ConfigurationReader.AppLicenseId)} value from conf file";

            case MacroId.PublisherName: return $"{nameof(ConfigurationReader.PublisherName)} value from conf file";
            case MacroId.PublisherCopyright: return $"{nameof(ConfigurationReader.PublisherCopyright)} value from conf file";
            case MacroId.PublisherLinkName: return $"{nameof(ConfigurationReader.PublisherLinkName)} value from conf file";
            case MacroId.PublisherLinkUrl: return $"{nameof(ConfigurationReader.PublisherLinkUrl)} value from conf file";
            case MacroId.PublisherEmail: return $"{nameof(ConfigurationReader.PublisherEmail)} value from conf file";

            case MacroId.IsTerminalApp: return $"{nameof(ConfigurationReader.IsTerminalApp)} value from conf file";
            case MacroId.PrimeCategory: return $"{nameof(ConfigurationReader.PrimeCategory)} value from conf file";

            case MacroId.AppVersion: return "Application version, exluding package-release extension";
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

}
