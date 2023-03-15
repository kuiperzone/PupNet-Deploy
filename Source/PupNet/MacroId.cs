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
    ShortSummary,
    LicenseId,
    VendorName,
    VendorCopyright,
    VendorUrl,
    VendorEmail,
    IsTerminalApp,

    AppVersion,
    DotnetRuntime,
    BuildTarget,
    BuildDate,
    BuildYear,
    BuildRoot,
    BuildShare,
    PublishBin,
    DesktopExec,
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
            // Conf only
            case MacroId.AppBaseName: return "APP_BASE_NAME";
            case MacroId.AppFriendlyName: return "APP_FRIENDLY_NAME";
            case MacroId.AppId: return "APP_ID";
            case MacroId.ShortSummary: return "SHORT_SUMMARY";
            case MacroId.LicenseId: return "LICENSE_ID";
            case MacroId.VendorName: return "VENDOR_NAME";
            case MacroId.VendorCopyright: return "VENDOR_COPYRIGHT";
            case MacroId.VendorUrl: return "VENDOR_URL";
            case MacroId.VendorEmail: return "VENDOR_EMAIL";
            case MacroId.IsTerminalApp: return "IS_TERMINAL_APP";

            // Derived
            case MacroId.AppVersion: return "APP_VERSION";
            case MacroId.DotnetRuntime: return "DOTNET_RUNTIME";
            case MacroId.BuildTarget: return "BUILD_TARGET";
            case MacroId.BuildDate: return "BUILD_DATE";
            case MacroId.BuildYear: return "BUILD_YEAR";
            case MacroId.BuildRoot: return "BUILD_ROOT";
            case MacroId.BuildShare: return "BUILD_SHARE";
            case MacroId.PublishBin: return "PUBLISH_BIN";
            case MacroId.DesktopExec: return "DESKTOP_EXEC";

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
            case MacroId.ShortSummary: return $"{nameof(ConfigurationReader.ShortSummary)} value from conf file";
            case MacroId.LicenseId: return $"{nameof(ConfigurationReader.LicenseId)} value from conf file";
            case MacroId.VendorName: return $"{nameof(ConfigurationReader.VendorName)} value from conf file";
            case MacroId.VendorCopyright: return $"{nameof(ConfigurationReader.VendorCopyright)} value from conf file";
            case MacroId.VendorUrl: return $"{nameof(ConfigurationReader.VendorUrl)} value from conf file";
            case MacroId.VendorEmail: return $"{nameof(ConfigurationReader.VendorName)} value from conf file";
            case MacroId.IsTerminalApp: return $"{nameof(ConfigurationReader.IsTerminalApp)} value from conf file";

            case MacroId.AppVersion: return "Application version, exluding package-release extension.";
            case MacroId.DotnetRuntime: return "Dotnet publish runtime identifier used (RID)";
            case MacroId.BuildTarget: return "Release or Debug (Release unless explicitly specified)";
            case MacroId.BuildDate: return "Date in ISO 'yyyy-MM-dd' format";
            case MacroId.BuildYear: return "Current year as 'yyyy'";
            case MacroId.BuildRoot: return "Root of the temporary directory used to build the application";
            case MacroId.BuildShare: return $"The Linux 'share' directory under {nameof(MacroId.BuildRoot)} (maybe empty)";
            case MacroId.PublishBin: return "Directory for dotnet publish output (i.e. application binary directory)";
            case MacroId.DesktopExec: return "Path to executable on target system (variable according to package kind)";

            default: throw new ArgumentException("Unknown macro " + id);
        }
    }

}
