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

using System.Reflection;

namespace KuiperZone.PupNet;

/// <summary>
/// A base class for package build operations. It defines a temporary build directory structure under which the
/// application is to be published by dotnet, along with other assets such a desktop and AppStream metadata files
/// and icons. The subclass is to define package specific values and operations by overriding key members.
/// </summary>
public abstract class PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public PackageBuilder(ConfigurationReader conf, PackKind kind, string buildRootName = "AppDir")
    {
        PackKind = kind;
        Arguments = conf.Arguments;
        Configuration = conf;
        IsWindowsPackage = PackKind.IsWindows();

        AppVersion = SplitVersion(conf.AppVersionRelease, out string temp);
        PackRelease = temp;

        OutputDirectory = GetOutputDirectory(Configuration);
        OutputName = GetOutputName(Configuration, PackKind, AppVersion, PackRelease);

        Root = Path.Combine(GlobalRoot, $"{conf.AppId}-{conf.GetBuildArch()}-{conf.Arguments.Build}-{PackKind}");
        BuildRoot = Path.Combine(Root, buildRootName);
        Operations = new(Root);

        IconPaths = GetShareIconPaths(Configuration.Icons);

        if (IconPaths.Count == 0)
        {
            // Always has some linux icons
            IconPaths = GetShareIconPaths(DefaultIcons);
        }

        IconSource = GetSourceIcon(PackKind, IconPaths.Keys);
    }

    /// <summary>
    /// Global temporary directory.
    /// </summary>
    public static readonly string GlobalRoot = Path.Combine(Path.GetTempPath(), $"{nameof(KuiperZone)}.{nameof(PupNet)}");

    /// <summary>
    /// Gets the EntryAssembly directory.
    /// </summary>
    public readonly static string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
        throw new InvalidOperationException("Failed to get EntryAssembly location");

    /// <summary>
    /// Known and accepted PNG icon sizes.
    /// </summary>
    public static IReadOnlyCollection<int> StandardIconSizes = new List<int>(new int[] { 16, 24, 32, 48, 64, 96, 128, 256 });

    /// <summary>
    /// Gets default source icons.
    /// </summary>
    public static IReadOnlyCollection<string> DefaultIcons { get; } = GetDefaultIcons();

    /// <summary>
    /// Gets the output package kind.
    /// </summary>
    public PackKind PackKind { get; }

    /// <summary>
    /// Gets a reference to the arguments.
    /// </summary>
    public ArgumentReader Arguments { get; }

    /// <summary>
    /// Gets a reference to the configuration.
    /// </summary>
    public ConfigurationReader Configuration { get; }

    /// <summary>
    /// Gets a "file operations" instance.
    /// </summary>
    public FileOps Operations { get; }

    /// <summary>
    /// Gets whether output is for Windows.
    /// </summary>
    public bool IsWindowsPackage { get; }

    /// <summary>
    /// Gets the application version. This is the configured version, excluding any Release suffix.
    /// </summary>
    public string AppVersion { get; }

    /// <summary>
    /// Gets the package release.
    /// </summary>
    public string PackRelease { get; }

    /// <summary>
    /// Gets output directory.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Gets output filename.
    /// </summary>
    public string OutputName { get; }

    /// <summary>
    /// Gets output file path.
    /// </summary>
    public string OutputPath
    {
        get { return Path.Combine(OutputDirectory, OutputName); }
    }

    /// <summary>
    /// Gets the package root for this build instance. This will be a temporary top level build directory.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Gets the app root directory, i.e. "${Root}/AppDir".
    /// </summary>
    public string BuildRoot { get; }

    /// <summary>
    /// Gets the application executable filename (no directory part). I.e. "Configuration.AppBase[.exe]".
    /// </summary>
    public string AppExecName
    {
        get { return Arguments.IsWindowsRuntime() ? Configuration.AppBaseName + ".exe" : Configuration.AppBaseName; }
    }

    /// <summary>
    /// Gets the build usr/bin directory, "${BuildRoot}/usr/bin". We do not necessarily publish here.
    /// See <see cref="AppBin"/>. Returns null if <see cref="IsWindowsPackage"/> is true.
    /// </summary>
    public string? BuildUsrBin
    {
        get { return IsWindowsPackage ? null : Path.Combine(BuildRoot, "usr", "bin"); }
    }

    /// <summary>
    /// Gets the build share directory, i.e. "${BuildRoot}/usr/share". Returns null if <see cref="IsWindowsPackage"/> is true.
    /// </summary>
    public string? BuildUsrShare
    {
        get { return IsWindowsPackage ? null : Path.Combine(BuildRoot, "usr", "share"); }
    }

    /// <summary>
    /// Gets the app metainfo directory, i.e. "${BuildRoot}/usr/share/metainfo". Returns null if <see cref="IsWindowsPackage"/> is true.
    /// </summary>
    public string? BuildShareMeta
    {
        get { return IsWindowsPackage ? null : Path.Combine(BuildRoot, "usr", "share", "metainfo"); }
    }

    /// <summary>
    /// Gets the build metainfo directory, i.e. "${BuildRoot}/usr/share/applications". Returns null if <see cref="IsWindowsPackage"/> is true.
    /// </summary>
    public string? BuildShareApplications
    {
        get { return IsWindowsPackage ? null : Path.Combine(BuildRoot, "usr", "share", "applications"); }
    }

    /// <summary>
    /// Gets the build icons directory, i.e. "${BuildRoot}/usr/share/icons". Returns null if <see cref="IsWindowsPackage"/> is true.
    /// </summary>
    public string? BuildShareIcons
    {
        get { return IsWindowsPackage ? null : Path.Combine(BuildRoot, "usr", "share", "icons"); }
    }

    /// <summary>
    /// Gets the desktop build file path. AppImage needs to override, otherwise default is standard under
    /// <see cref="BuildShareApplications"/>. Returns null if <see cref="IsWindowsPackage"/> is true.
    /// </summary>
    public virtual string? DesktopPath
    {
        get
        {
            if (BuildShareApplications != null)
            {
                return Path.Combine(BuildShareApplications, Configuration.AppId + ".desktop");
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the AppStream build file path. AppImage needs to override, otherwise default is standard under
    /// <see cref="BuildShareMeta"/>. Returns null if <see cref="IsWindowsPackage"/> is true.
    /// </summary>
    public virtual string? MetaInfoPath
    {
        get
        {
            if (BuildShareMeta != null)
            {
                return Path.Combine(BuildShareMeta, Configuration.AppId + ".metainfo.xml");
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the source path of the "prime" icon, i.e. the single icon considered to be the most generally suitable.
    /// On Linux this is the first SVG file encountered, or the largest PNG otherwise. On Windows, it is an ICO file.
    /// </summary>
    public string? IconSource { get; }

    /// <summary>
    /// A sequence of source icon paths (key) and their destinations (value) under <see cref="PackageBuilder.BuildShareIcons"/>.
    /// Defaults are used if the configuration supplies none. Empty on Windows.
    /// </summary>
    public IReadOnlyDictionary<string, string> IconPaths { get; }

    /// <summary>
    /// Gets the path to the runnable binary when deployed, i.e. the path we use in the desktop file for the Exec
    /// field. Typically: "/usr/bin/${AppExecName}" or "/opt/AppId/${AppExecName}".
    /// </summary>
    public abstract string DesktopExec{ get; }

    /// <summary>
    /// Gets the application bin directory to which the dotnet (or C++) build must publish to. It must be under
    /// <see cref="BuildRoot"/> and may typically be equal to <see cref="BuildUsrBin"/>, or "${BuildRoot}/opt/AppId".
    /// </summary>
    public abstract string PublishBin { get; }

    /// <summary>
    /// Gets the manifest file path to which <see cref="ManifestContent"/> will be written. If null, no file is saved.
    /// </summary>
    public abstract string? ManifestPath { get; }

    /// <summary>
    /// Gets the "manifest file" specific to the package kind. For RPM, this is the "Spec file" content.
    /// For Flatpak, it is the "manifest". It must not contain macros. It may be null if not used.
    /// </summary>
    public abstract string? ManifestContent { get; }

    /// <summary>
    /// Gets a sequence of commends needed to build the package. It must not contain macros.
    /// </summary>
    public abstract IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Gets whether package supports run after build without installation.
    /// </summary>
    public abstract bool SupportsRunOnBuild { get; }

    /// <summary>
    /// Create directories tree. It will be called at the start of the build process to create all build directories
    /// and populate them with standard assets. It does not populate the application binary. The base implementation
    /// writes the "desktop" and "metainfo" (expanded) content to locations under <see cref="DesktopPath"/> and
    /// <see cref="MetaInfoPath"/> respectively. It does nothing for these for Windows packages or if the respective
    /// string is null or empty. It copies <see cref="IconPaths"/> to their respective destinations, and writes
    /// <see cref="ManifestContent"/> to <see cref="ManifestPath"/>. Finally, it ensures that <see cref="OutputDirectory"/>
    /// exists. It should be overridden to perform additional tasks, but subclass should call this base method first.
    /// </summary>
    public virtual void Create(string? desktop, string? metainfo)
    {
        RemoveRoot();
        Operations.CreateDirectory(Root);
        Operations.CreateDirectory(BuildRoot);
        Operations.CreateDirectory(BuildUsrBin);
        Operations.CreateDirectory(BuildUsrShare);
        Operations.CreateDirectory(BuildShareIcons);
        Operations.CreateDirectory(BuildShareApplications);
        Operations.CreateDirectory(BuildShareMeta);
        Operations.CreateDirectory(PublishBin);

        if (!IsWindowsPackage)
        {
            // Non-Windows only
            Operations.WriteFile(DesktopPath, desktop);
            Operations.WriteFile(MetaInfoPath, metainfo);

            foreach (var item in IconPaths)
            {
                Operations.CopyFile(item.Key, item.Value, true);
            }
        }

        Operations.WriteFile(ManifestPath, ManifestContent);
        Operations.CreateDirectory(OutputDirectory);
    }

    /// <summary>
    /// Removes <see cref="Root"/>.
    /// </summary>
    public void RemoveRoot()
    {
        Operations.RemoveDirectory(Root);
    }

    /// <summary>
    /// Gets all files under <see cref="BuildRoot"/>. Note results are prefixed with "/" on non-windows platforms.
    /// </summary>
    public IReadOnlyCollection<string> ListBuild(string filter = "*")
    {
        if (Directory.Exists(BuildRoot))
        {
            var files = FileOps.ListFiles(BuildRoot, filter);

            if (!IsWindowsPackage)
            {
                for (int n = 0; n < files.Length; ++n)
                {
                    if (!files[n].StartsWith('/'))
                    {
                        files[n] = "/" + files[n];
                    }
                }
            }

            return files;
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Builds the package. This method is called after <see cref="Create"/> and after <see cref="PublishBin"/>
    /// has been populated with the application. The base implementation ensures that the expected runnable binary
    /// exists and calls <see cref="FileOps.Execute(string)"/> against each item in <see cref="PackageCommands"/>.
    /// It may be overridden to perform additional or other operations.
    /// </summary>
    public virtual void BuildPackage()
    {
        // Must exist
        Operations.AssertExists(Path.Combine(PublishBin, AppExecName));
        Operations.Execute(PackageCommands);
    }

    /// <summary>
    /// Overrides. Provides console output.
    /// </summary>
    public override string ToString()
    {
        return Root;
    }

    private static string SplitVersion(string version, out string release)
    {
        release = "1";

        if (!string.IsNullOrEmpty(version))
        {
            int p0 = version.IndexOf("[");
            var len = version.IndexOf("]") - p0 - 1;

            if (p0 > 0 && len > 0)
            {
                var temp = version.Substring(p0 + 1, len).Trim();
                version = version.Substring(0, p0).Trim();

                if (temp.Length != 0)
                {
                    release = temp;
                }
            }
        }

        return version;
    }

    private static string GetOutputDirectory(ConfigurationReader conf)
    {
        var output = Path.GetDirectoryName(conf.Arguments.Output);

        if (output != null)
        {
            if (Path.IsPathFullyQualified(output))
            {
                return output;
            }

            return Path.Combine(conf.OutputDirectory, output);
        }

        return conf.OutputDirectory;
    }

    private static string GetOutputName(ConfigurationReader conf, PackKind kind, string version, string release)
    {
        var output = Path.GetFileName(conf.Arguments.Output);

        if (output != null)
        {
            return output;
        }

        output = conf.AppBaseName;

        if (conf.OutputVersion && !string.IsNullOrEmpty(version))
        {
            output += $"-{version}-{release}";
        }

        output += $".{conf.GetBuildArch()}";

        if (kind == PackKind.AppImage)
        {
            return output + ".AppImage";
        }

        if (kind == PackKind.WinSetup)
        {
            return output + ".exe";
        }

        return output + "." + kind.ToString().ToLowerInvariant();
    }

    private static IReadOnlyCollection<string> GetDefaultIcons()
    {
        // Default icon in assembly directory
        var list = new List<string>();

        // Leave windows icon out - leave InnoSetup to assign own
        // list.Add(Path.Combine(AssemblyDirectory, "app.icon"));

        list.Add(Path.Combine(AssemblyDirectory, "app.svg"));
        list.Add(Path.Combine(AssemblyDirectory, "app.16x16.png"));
        list.Add(Path.Combine(AssemblyDirectory, "app.24x24.png"));
        list.Add(Path.Combine(AssemblyDirectory, "app.32x32.png"));
        list.Add(Path.Combine(AssemblyDirectory, "app.48x48.png"));
        list.Add(Path.Combine(AssemblyDirectory, "app.64x64.png"));

        return list;
    }

    private static int GetStandardPngSize(string filename)
    {
        // Where filename = name.32.png, or name.32x32.png
        var ext = Path.GetExtension(filename);

        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            // Loose any directory
            filename = Path.GetFileName(filename);

            // Interior extension, i.e. the value
            ext = Path.GetExtension(Path.GetFileNameWithoutExtension(filename));

            // Accept "64x64" but key off first value
            int pos = ext.IndexOf('x', StringComparison.OrdinalIgnoreCase);

            if (pos > 0)
            {
                ext = ext.Substring(1, pos - 1);
            }

            if (int.TryParse(ext, out int size) && StandardIconSizes.Contains(size))
            {
                return size;
            }

            var sizes = string.Join(',', StandardIconSizes);
            throw new ArgumentException($"Icon {filename} must be of form 'name.size.png', where size = {sizes} only");
        }

        return 0;
    }

    private static string? GetSourceIcon(PackKind kind, IEnumerable<string> paths)
    {
        int max = 0;
        string? rslt = null;

        foreach (var item in paths)
        {
            var ext = Path.GetExtension(item).ToLowerInvariant();

            if (kind.IsWindows() && ext == ".ico")
            {
                // Only need this
                return item;
            }

            if (!kind.IsWindows() && ext == ".svg")
            {
                // Or this for non-windows
                return item;
            }

            // Get biggest PNG
            int size = GetStandardPngSize(item);

            if (size > max)
            {
                max = size;
                rslt = item;
            }
        }

        return rslt;
    }

    private string? MapSourceIconToSharePath(string sourcePath)
    {
        if (BuildShareIcons != null)
        {
            var ext = Path.GetExtension(sourcePath).ToLowerInvariant();

            if (ext.Equals(".svg", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(BuildShareIcons, "hicolor", "scalable", "apps", Configuration.AppId) + ".svg";
            }

            int size = GetStandardPngSize(sourcePath);

            if (size > 0)
            {
                return Path.Combine(BuildShareIcons, "hicolor", $"{size}x{size}", "apps", Configuration.AppId) + ".png";
            }
        }

        return null;
    }

    private IReadOnlyDictionary<string, string> GetShareIconPaths(IReadOnlyCollection<string> sources)
    {
        // Empty on windows
        var dict = new Dictionary<string, string>();

        if (BuildShareIcons != null)
        {
            foreach (var item in sources)
            {
                var dest = MapSourceIconToSharePath(item);

                if (dest != null)
                {
                    dict.TryAdd(item, dest);
                }
            }
        }

        return dict;
    }

}

