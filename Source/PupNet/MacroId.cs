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
    AppSummary,
    AppLicense,
    AppVendor,
    AppUrl,

    AppVersion,
    DotnetRuntime,
    BuildArch,
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
            case MacroId.AppSummary: return "APP_SUMMARY";
            case MacroId.AppLicense: return "APP_LICENSE";
            case MacroId.AppVendor: return "APP_VENDOR";
            case MacroId.AppUrl: return "APP_URL";

            // Build static
            case MacroId.AppVersion: return "APP_VERSION";
            case MacroId.DotnetRuntime: return "DOTNET_RUNTIME";
            case MacroId.BuildArch: return "BUILD_ARCH";
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
}
