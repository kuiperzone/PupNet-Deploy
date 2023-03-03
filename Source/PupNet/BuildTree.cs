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
/// Determines temporary build directory tree.
/// </summary>
public class BuildTree
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public BuildTree(ConfDecoder conf)
    {
        var kind = conf.Args.Kind;

        Root = Path.Combine(Global, $"{conf.AppId}-{conf.GetBuildArch()}-{conf.Args.Build}");
        Ops = new(Root);

        PublishBin = Path.Combine(Root, "Publish");
        PackTop = Path.Combine(Root, kind.ToString());

        // AppDir is AppImage convention - fine for others
        AppDir = Path.Combine(PackTop, "AppDir");
        AppBin = Path.Combine(AppDir, "usr", "bin");

        AppShare = Path.Combine(AppDir, "usr", "share");
        AppShareMeta = Path.Combine(AppShare, "metainfo");
        AppShareApplications = Path.Combine(AppShare, "applications");
        AppShareIcons = Path.Combine(AppShare, "icons");

        AppExecName = kind.IsWindows() ? $"{conf.AppBase}.exe" : conf.AppBase;

        // Defaults for standard linux install
        AppMetaName = conf.AppId + ".metainfo.xml";
        AppMetaPath = Path.Combine(AppShareMeta, AppMetaName);

        DesktopId = conf.AppId + ".desktop";
        DesktopPath = Path.Combine(AppShareApplications, DesktopId);

        AppInstall = Path.Combine(AppDir, "usr", "bin");
        AppExecPath = Path.Combine(AppInstall, AppExecName);
        LaunchExec = AppExecName;

        if (kind == PackKind.AppImage)
        {
            // Need ".appdata" extension
            // Also we are using AppId in name (not AppBase)
            AppMetaName = conf.AppId + ".appdata.xml";
            AppMetaPath = Path.Combine(AppShareMeta, AppMetaName);
            LaunchExec = Path.Combine("usr", "bin", AppExecName);
        }
        else
        if (kind == PackKind.Rpm || kind == PackKind.Deb)
        {
            // Here we put dotnet build files into /opt/AppId, rather than /usr/bin
            AppInstall = Path.Combine(AppDir, "opt", conf.AppId);
            AppExecPath = Path.Combine(AppInstall, AppExecName);
            LaunchExec = Path.Combine("/opt", conf.AppId, AppExecName);
        }
        else
        if (kind.IsWindows())
        {
            LaunchExec = AppExecName;
        }

        // Does not imply these are actually used
        FlatpakManifestPath = Path.Combine(PackTop, conf.AppId + ".yml");
        RpmSpecPath = Path.Combine(PackTop, conf.AppId + ".spec");
    }

    /// <summary>
    /// Global root.
    /// </summary>
    public static readonly string Global = Path.Combine(Path.GetTempPath(), $"{nameof(KuiperZone)}.{nameof(PupNet)}");

    /// <summary>
    /// Gets application project root.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Get file operations instance.
    /// </summary>
    public FileOps Ops { get; }

    /// <summary>
    /// Output of publish command, i.e. "${Root}/Publish".
    /// </summary>
    public string PublishBin { get; }

    /// <summary>
    /// Gets the packing directory, i.e. "${Root}/AppImage".
    /// </summary>
    public string PackTop { get; }

    /// <summary>
    /// Gets the app build directory, i.e. "${PackTop}/AppDir".
    /// </summary>
    public string AppDir { get; }

    /// <summary>
    /// Gets the app bin directory, ALWAYS "${AppDir}/usr/bin". We do NOT necessarily copy app here.
    /// See <see cref="AppInstall"/>.
    /// </summary>
    public string AppBin { get; }

    /// <summary>
    /// Gets the app share directory, i.e. "${AppDir}/usr/share".
    /// </summary>
    public string AppShare { get; }

    /// <summary>
    /// Gets the app metainfo directory, i.e. "${AppDir}/usr/share/metainfo".
    /// </summary>
    public string AppShareMeta { get; }

    /// <summary>
    /// Gets the build metainfo directory, i.e. "${AppDir}/usr/share/applications".
    /// </summary>
    public string AppShareApplications { get; }

    /// <summary>
    /// Gets the build icons directory, i.e. "${AppDir}/usr/share/icons".
    /// </summary>
    public string AppShareIcons { get; }

    /// <summary>
    /// Gets the app INSTALL directory, which may be either: "${AppDir}/usr/bin" or "${AppDir}/opt/AppId".
    /// </summary>
    public string AppInstall { get; }

    /// <summary>
    /// Gets the expected filename (not directory) for the desktop file. Always has value, but does not imply content exists.
    /// </summary>
    public string DesktopId { get; }

    /// <summary>
    /// Gets the full path to the desktop file. Always has value, but does not imply content exists.
    /// </summary>
    public string DesktopPath { get; }

    /// <summary>
    /// Gets the expected filename (not directory) for the AppStream data file. Does not imply it will exist.
    /// </summary>
    public string AppMetaName { get; }

    /// <summary>
    /// Gets the full path to the AppStream data file. Does not imply it will exist.
    /// </summary>
    public string AppMetaPath { get; }

    /// <summary>
    /// Gets the app executable filename , i.e. "AppBase[.exe]". No directory part.
    /// </summary>
    public string AppExecName { get; }

    /// <summary>
    /// Gets the app executable path , i.e. "${AppInstal}/AppBase[.exe]".
    /// </summary>
    public string AppExecPath { get; }

    /// <summary>
    /// Gets flatpak manifest path. Always has value, but does not imply content exists.
    /// </summary>
    public string FlatpakManifestPath { get; }

    /// <summary>
    /// Gets Rpm path. Always has value, but does not imply content exists.
    /// </summary>
    public string RpmSpecPath { get; }

    /// <summary>
    /// Gets the path to app when deployed, i.e.: "$/usr/bin/AppBase[.exe]" or "/opt/AppId/AppBase[.exe]" or just "AppBase[.exe]".
    /// </summary>
    public string LaunchExec { get; }

    /// <summary>
    /// Create directories tree.
    /// </summary>
    public void Create()
    {
        RemoveRoot();

        Ops.CreateDirectory(Root);
        Ops.CreateDirectory(PublishBin);
        Ops.CreateDirectory(PackTop);
        Ops.CreateDirectory(AppDir);
        Ops.CreateDirectory(AppBin);
        Ops.CreateDirectory(AppShare);
        Ops.CreateDirectory(AppShareMeta);
        Ops.CreateDirectory(AppShareApplications);
        Ops.CreateDirectory(AppShareIcons);
        Ops.CreateDirectory(AppInstall);
    }

    /// <summary>
    /// Removes <see cref="Root"/>.
    /// </summary>
    public void RemoveRoot()
    {
        Ops.RemoveDirectory(Root);
    }

    /// <summary>
    /// Overrides. Provides console output.
    /// </summary>
    public override string ToString()
    {
        return Root;
    }

}

