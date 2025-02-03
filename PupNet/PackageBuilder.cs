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

using System.Reflection;
using System.Runtime.InteropServices;

namespace KuiperZone.PupNet;

/// <summary>
/// A base class for package build operations. It defines a working build directory structure under which the
/// application is to be published by dotnet, along with other assets such a desktop file (Linux), AppStream metadata
/// and icons (think 'AppDir' for AppImage, or buildroot for RPM). It is Linux centric, where on Windows or non-Linux
/// unused properties are ignored (may be null). The subclass is to define package specific values and operations by
/// overriding key members, and implement the build operation. Note that many of properties of this class are public
/// purely to make them accessible to <see cref="BuildHost"/> owner class so they can be presented to the user.
/// When the user confirms, the build process commences which is a 3 stage process. First, <see cref="Create"/>
/// is called to create the directory structure. Next "dotnet publish" (and/or a custom post-build operation) is
/// called to build the project and populate the <see cref="BuildAppBin"/> directory. Finally, <see cref="BuildPackage"/>
/// is called to perform the package build.
/// </summary>
public abstract class PackageBuilder
{
    /// <summary>
    /// AppRoot leaf name.
    /// </summary>
    protected const string AppRootName = "AppDir";

    /// <summary>
    /// Constructor.
    /// </summary>
    public PackageBuilder(ConfigurationReader conf, PackageKind kind)
    {
        Kind = kind;
        Arguments = conf.Arguments;
        Configuration = conf;
        Runtime = new RuntimeConverter(Arguments.Runtime);
        IsLinuxExclusive = Kind.TargetsLinux(true);
        IsWindowsExclusive = Kind.TargetsWindows(true);
        IsOsxExclusive = Kind.TargetsOsx(true);

        // Important - Architecture is tailored for third-party builder
        AppVersion = SplitVersion(conf.Arguments.VersionRelease ?? conf.AppVersionRelease, out string temp);
        PackageRelease = temp;

        OutputDirectory = GetOutputDirectory(Configuration);
        Root = Path.Combine(GlobalRoot, $"{conf.AppId}-{Runtime}-{conf.Arguments.Build}-{kind}");
        BuildRoot = Path.Combine(Root, AppRootName);
        Operations = new(Root);

        IconPaths = GetShareIconPaths(Configuration.IconFiles);

        if (IconPaths.Count == 0)
        {
            // Fallback to embedded icons on Linux
            // Should always empty on non-linux systems
            IconPaths = GetShareIconPaths(Configuration.DesktopTerminal ? DefaultTerminalIcons : DefaultGuiIcons);
        }

        // Should be ico on Windows, or SVG or PNG on linux
        PrimaryIcon = GetSourceIcon(kind, Configuration.IconFiles);

        // It can be null on Windows.
        // Fallback to embedded icons on Linux
        PrimaryIcon ??= GetSourceIcon(kind, Configuration.DesktopTerminal ? DefaultTerminalIcons : DefaultGuiIcons);

        // Ignore fact file might not exist if AssertPaths if false (test only)
        if (Configuration.AssertPaths || File.Exists(Configuration.AppChangeFile))
        {
            ChangeLog = new(Configuration.AppChangeFile);
        }
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
    public static IReadOnlyCollection<int> StandardIconSizes { get; } =
        new List<int>(new int[] { 16, 24, 32, 48, 64, 96, 128, 256, 512, 1024 });

    /// <summary>
    /// Gets default GUI icons.
    /// </summary>
    public static IReadOnlyCollection<string> DefaultGuiIcons { get; } = GetDefaultIcons(false);

    /// <summary>
    /// Gets default GUI icons.
    /// </summary>
    public static IReadOnlyCollection<string> DefaultTerminalIcons { get; } = GetDefaultIcons(true);

    /// <summary>
    /// Gets the package kind.
    /// </summary>
    public PackageKind Kind { get; }

    /// <summary>
    /// Gets a reference to the arguments.
    /// </summary>
    public ArgumentReader Arguments { get; }

    /// <summary>
    /// Gets a reference to the configuration.
    /// </summary>
    public ConfigurationReader Configuration { get; }

    /// <summary>
    /// Gets the parsed changelog.
    /// </summary>
    public ChangeParser ChangeLog { get; } = new();

    /// <summary>
    /// Gets a "file operations" instance.
    /// </summary>
    public FileOps Operations { get; }

    /// <summary>
    /// Gets a thing that provides dotnet runtime information.
    /// </summary>
    public RuntimeConverter Runtime { get; }

    /// <summary>
    /// Gets the architecture string tailed to suit the package builder.
    /// Subclass to override to set appropriate value based on <see cref="RuntimeConverter.RuntimeArch"/>.
    /// If user supplies <see cref="Arguments.Arch"/>, this value should be used verbatim.
    /// </summary>
    public abstract string Architecture { get; }

    /// <summary>
    /// Collects warning messages.
    /// </summary>
    public ICollection<string> WarningSink { get; } = new List<string>();

    /// <summary>
    /// Gets whether output is for Linux exclusively. This will be true for AppImage, and
    /// false for Zip and Setup.
    /// </summary>
    public bool IsLinuxExclusive { get; }

    /// <summary>
    /// Gets whether output is for Windows exclusively. This will be true for Setup, and
    /// false for Zip and AppImage.
    /// </summary>
    public bool IsWindowsExclusive { get; }

    /// <summary>
    /// Gets whether output is for OSX exclusively. Currently always false.
    /// </summary>
    public bool IsOsxExclusive { get; }

    /// <summary>
    /// Gets the application version. This is the configured version, excluding any Release suffix.
    /// </summary>
    public string AppVersion { get; }

    /// <summary>
    /// Gets the package release.
    /// </summary>
    public string PackageRelease { get; }

    /// <summary>
    /// Gets output directory of the final deployable package.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Gets output filename of the final deployable package.
    /// Note. For certain builders (namely RPM), this may be a directory which then contains the output.
    /// </summary>
    public abstract string OutputName { get; }

    /// <summary>
    /// Gets output file path "OutputDirectory/OutputName".
    /// </summary>
    public string OutputPath
    {
        get { return Path.Combine(OutputDirectory, OutputName); }
    }

    /// <summary>
    /// Gets the application executable filename (no directory part). I.e. "Configuration.AppBase[.exe]".
    /// </summary>
    public string AppExecName
    {
        get { return Runtime.IsWindowsRuntime ? Configuration.AppBaseName + ".exe" : Configuration.AppBaseName; }
    }

    /// <summary>
    /// Gets the package root for this build instance. This will be a temporary top level build directory.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Gets the app root directory, i.e. "${Root}/AppDir". This is equivalent to "AppDir" in AppImage terminology,
    /// or "buildroot" in RPM terminology. Always populated.
    /// </summary>
    public string BuildRoot { get; }

    /// <summary>
    /// Gets the build 'usr' directory, i.e. "{BuildRoot}/usr",  We do not necessarily 'dotnet publish' here, and
    /// it may be distinct from <see cref="BuildAppBin"/>. Returns null for Windows packages.
    /// </summary>
    public string? BuildUsr
    {
        get
        {
            if (IsLinuxExclusive || IsOsxExclusive)
            {
                // Note. We have the option in the future of using "{BuildRoot}/usr/local"
                return $"{BuildRoot}/usr";
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the build usr/bin directory, "{BuildRoot}/usr/bin". We do not necessarily 'dotnet publish' here, and
    /// it may be distinct from <see cref="BuildAppBin"/>. Returns null for Windows packages.
    /// </summary>
    public string? BuildUsrBin
    {
        get
        {
            var usr = BuildUsr;
            return usr != null ? $"{BuildUsr}/bin" : null;
        }
    }

    /// <summary>
    /// Gets the build share directory, i.e. "{BuildRoot}/usr/share". Returns null for Windows packages.
    /// </summary>
    public string? BuildUsrShare
    {
        get
        {
            var usr = BuildUsr;
            return usr != null ? $"{BuildUsr}/share" : null;
        }
    }

    /// <summary>
    /// Gets the app metainfo directory, i.e. "{BuildRoot}/usr/share/metainfo". Returns null if <see cref="IsLinuxExclusive"/> is false.
    /// </summary>
    public string? BuildShareMeta
    {
        get
        {
            if (IsLinuxExclusive)
            {
                var usr = BuildUsr;
                return usr != null ? $"{BuildUsr}/share/metainfo" : null;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the build metainfo directory, i.e. "{BuildRoot}/usr/share/applications". Returns null if <see cref="IsLinuxExclusive"/> is false.
    /// </summary>
    public string? BuildShareApplications
    {
        get
        {
            if (IsLinuxExclusive)
            {
                var usr = BuildUsr;
                return usr != null ? $"{BuildUsr}/share/applications" : null;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the build icons directory, i.e. "{BuildRoot}/usr/share/icons". Returns null if <see cref="IsLinuxExclusive"/> is false.
    /// </summary>
    public string? BuildShareIcons
    {
        get
        {
            if (IsLinuxExclusive)
            {
                var usr = BuildUsr;
                return usr != null ? $"{BuildUsr}/share/icons" : null;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the desktop build file path. Returns null if <see cref="BuildShareApplications"/> is null.
    /// </summary>
    public string? DesktopBuildPath
    {
        get
        {
            if (BuildShareApplications != null)
            {
                return $"{BuildShareApplications}/{Configuration.AppId}.desktop";
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the AppStream metadata build file path. AppImage needs to override, otherwise default is standard under
    /// <see cref="BuildShareMeta"/>. Returns null if <see cref="BuildShareMeta"/> is null.
    /// </summary>
    public virtual string? MetaBuildPath
    {
        get
        {
            if (BuildShareMeta != null)
            {
                return $"{BuildShareMeta}/{Configuration.AppId}.metainfo.xml";
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the source path of the "prime" icon, i.e. the single icon considered to be the most generally suitable.
    /// On Linux this is the first SVG file encountered, or the largest PNG otherwise. On Windows, it is the first ICO
    /// file encountered. It is a full path to the source icon on build.
    /// </summary>
    public string? PrimaryIcon { get; }

    /// <summary>
    /// A sequence of source icon paths (key) and their install destinations (value) under
    /// <see cref="PackageBuilder.BuildShareIcons"/>. Default generic icons are provided if the configuration supplies
    /// none. Always empty on Windows.
    /// </summary>
    public IReadOnlyDictionary<string, string> IconPaths { get; }

    /// <summary>
    /// Gets the application bin directory to which the dotnet build must publish to (or the C++ make output).
    /// It should always be under <see cref="BuildRoot"/>. For some packages on Linux, it may be equal to
    /// <see cref="BuildUsrBin"/>, but not necessarily. For RPM and Deb, it is expected to be "{BuildRoot}/opt/{AppId}".
    /// Always populated.
    /// </summary>
    public abstract string BuildAppBin { get; }

    /// <summary>
    /// Gets the path to the application executable on target system (not the build system).
    /// See also <see cref="BuildAppBin"/>.
    /// </summary>
    public string InstallExec
    {
        get { return Path.Combine(InstallBin, AppExecName);  }
    }

    /// <summary>
    /// Gets the path to the application directory on target system (not the build system). On Linux, typically:
    /// "/usr/bin" or "/opt/AppId". See also <see cref="BuildAppBin"/>.
    /// </summary>
    public abstract string InstallBin { get; }

    /// <summary>
    /// Gets the "manifest content" specific to the package kind provided for display purposes. For RPM, this is the
    /// "Spec file" content. For Flatpak, it is the "manifest". For deb, it is the "control file". It must not contain
    /// macros. It may be null if not used.
    /// </summary>
    public abstract string? ManifestContent { get; set; }

    /// <summary>
    /// Gets the manifest file path to which <see cref="ManifestContent"/> will be written.
    /// Concrete subclass may override to specify custom location. If null, no file is written.
    /// </summary>
    public abstract string? ManifestBuildPath { get; }

    /// <summary>
    /// Gets the destination path of the LICENSE file in the build directory. This will cause
    /// <see cref="ConfigurationReader.AppLicenseFile"/> to be copied into <see cref="BinBin"/>.
    /// Null if no license specified.
    /// </summary>
    public string? LicenseBuildPath
    {
        get
        {
            if (Configuration.AppLicenseFile != null)
            {
                return Path.Combine(BuildAppBin, Path.GetFileName(Configuration.AppLicenseFile));
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the destination path of the readme/changelog file in the build directory. This will cause
    /// <see cref="ConfigurationReader.AppChangeFile"/> to be copied into <see cref="BinBin"/>.
    /// Null if no license specified.
    /// </summary>
    public string? ChangeBuildPath
    {
        get
        {
            if (Configuration.AppChangeFile != null)
            {
                return Path.Combine(BuildAppBin, Path.GetFileName(Configuration.AppChangeFile));
            }

            return null;
        }
    }

    /// <summary>
    /// Gets a sequence of commends needed to build the package. It must not contain macros.
    /// </summary>
    public abstract IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Gets whether package supports <see cref="ConfigurationReader.StartCommand">.
    /// </summary>
    public abstract bool SupportsStartCommand { get; }

    /// <summary>
    /// Gets whether package supports run after build without installation.
    /// </summary>
    public abstract bool SupportsPostRun { get; }

    /// <summary>
    /// Create directories tree. It will be called at the start of the build process to create all build directories
    /// and populate them with standard assets. It does not populate the application binary. The base implementation
    /// writes the "desktop" and "metainfo" (expanded) content to locations under <see cref="DesktopBuildPath"/> and
    /// <see cref="MetaBuildPath"/> respectively. It does nothing for these for Windows packages or if the respective
    /// string is null or empty. It copies <see cref="IconPaths"/> to their respective destinations, and writes
    /// <see cref="ManifestContent"/> to <see cref="ManifestBuildPath"/>. Finally, it ensures that <see cref="OutputDirectory"/>
    /// exists. It should be overridden to perform additional tasks, but subclass should call this base method first.
    /// It throws any Exception of failure.
    /// </summary>
    public virtual void Create(string? desktop, string? metainfo)
    {
        WarningSink.Clear();

        RemoveRoot();
        Operations.CreateDirectory(Root);
        Operations.CreateDirectory(BuildRoot);
        Operations.CreateDirectory(BuildUsrBin);
        Operations.CreateDirectory(BuildUsrShare);
        Operations.CreateDirectory(BuildShareIcons);
        Operations.CreateDirectory(BuildShareApplications);
        Operations.CreateDirectory(BuildShareMeta);
        Operations.CreateDirectory(BuildAppBin);

        // For example, debian needs subdirectories for these files.
        // Calls do nothing if respective property is null or directory exists.
        Operations.CreateDirectory(Path.GetDirectoryName(ManifestBuildPath));

        if (IsLinuxExclusive)
        {
            Operations.WriteFile(DesktopBuildPath, desktop);
            Operations.WriteFile(MetaBuildPath, metainfo);

            foreach (var item in IconPaths)
            {
                Operations.CopyFile(item.Key, item.Value, true);
            }
        }

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
    /// Gets all files under <see cref="BuildRoot"/>. Note results are prefixed with "/" on non-windows platforms if rooted.
    /// </summary>
    public IReadOnlyCollection<string> ListBuild(bool sysrooted)
    {
        if (Directory.Exists(BuildRoot))
        {
            var files = FileOps.ListFiles(BuildRoot, "*");

            if (sysrooted)
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
    /// Builds the package. This method is called after <see cref="Create"/> and after <see cref="BuildAppBin"/>
    /// has been populated with the application. The base implementation ensures that the expected runnable binary
    /// exists and calls <see cref="FileOps.Execute(string)"/> against each item in <see cref="PackageCommands"/>.
    /// It may be overridden to perform additional or other operations. It throws any Exception of failure.
    /// </summary>
    public virtual void BuildPackage()
    {
        // Must exist
        var appPath = Path.Combine(BuildAppBin, AppExecName);
        Operations.AssertExists(appPath);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // A bit of a hack for .NET6
            // Need to ensure that all files have read universal read permission,
            // and that main has execute permission. .NET6 does not seem to necessarily give these.
            var hold = Operations.ShowCommands;

            try
            {
                Operations.ShowCommands = false;
                Operations.Execute($"chmod a+rx \"{appPath}\"");

                foreach (var item in FileOps.ListFiles(BuildAppBin))
                {
                    Operations.Execute($"chmod a+r \"{Path.Combine(BuildAppBin, item)}\"");
                }
            }
            finally
            {
                Operations.ShowCommands = hold;
            }
        }

        // Copy associated files into bin directory
        // These must come before we call ManifestContent
        // We don't want to replace on these if they exist
        if (LicenseBuildPath != null)
        {

            var content = Configuration.ReadAssociatedFile(Configuration.AppLicenseFile);
            Operations.WriteFile(LicenseBuildPath, content);
        }

        if (ChangeBuildPath != null)
        {

            var content = Configuration.ReadAssociatedFile(Configuration.AppChangeFile);
            Operations.WriteFile(ChangeBuildPath, content);
        }

        // Write manifest just before build, as ManifestContents is virtual and
        // may change after Create() and dotnet publish in some deployments.
        Operations.WriteFile(ManifestBuildPath, ManifestContent);

        Operations.Execute(PackageCommands);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override string ToString()
    {
        return Root;
    }

    /// <summary>
    /// Accessible by subclass. Derives "standard" output name. Ext to include leading ".".
    /// Returns Configuration.Arguments.Output if not null.
    /// </summary>
    protected string GetOutputName(bool version, string? suffix, string arch, string ext)
    {
        var output = Configuration.Arguments.Output;
        var name = Path.GetFileName(output);

        if (!string.IsNullOrEmpty(name) && !Directory.Exists(output))
        {
            return name;
        }

        name = Configuration.PackageName + suffix;

        if (version)
        {
            name += $"-{AppVersion}-{PackageRelease}";
        }

        return $"{name}.{arch}{ext}";
    }

    /// <summary>
    /// Overload.
    /// </summary>
    protected string GetOutputName(bool version, string arch, string ext)
    {
        return GetOutputName(version, null, arch, ext);
    }

    private static string GetOutputDirectory(ConfigurationReader conf)
    {
        var path = conf.Arguments.Output;

        if (path != null)
        {
            if (!Path.IsPathFullyQualified(path))
            {
                path = Path.Combine(conf.OutputDirectory, path);
            }

            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return Path.GetDirectoryName(path) ?? conf.OutputDirectory;
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

    private static IReadOnlyCollection<string> GetDefaultIcons(bool terminal)
    {
        // Default icon in assembly directory
        var list = new List<string>();

        string name = terminal ? "terminal" : "generic";

        // Leave windows icon out - leave InnoSetup to assign own
        // list.Add(Path.Combine(AssemblyDirectory, $"{name}.ico"));

        list.Add(Path.Combine(AssemblyDirectory, $"{name}.16x16.png"));
        list.Add(Path.Combine(AssemblyDirectory, $"{name}.24x24.png"));
        list.Add(Path.Combine(AssemblyDirectory, $"{name}.32x32.png"));
        list.Add(Path.Combine(AssemblyDirectory, $"{name}.48x48.png"));
        list.Add(Path.Combine(AssemblyDirectory, $"{name}.64x64.png"));
        list.Add(Path.Combine(AssemblyDirectory, $"{name}.96x96.png"));
        list.Add(Path.Combine(AssemblyDirectory, $"{name}.128x128.png"));
        list.Add(Path.Combine(AssemblyDirectory, $"{name}.256x256.png"));

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

            // Interior extension, i.e. the size (here may be ".x32", ".32" or "32x32"
            ext = Path.GetExtension(Path.GetFileNameWithoutExtension(filename)).Trim('.');

            int pos = ext.IndexOf('x', StringComparison.OrdinalIgnoreCase);

            if (pos > -1 && pos < ext.Length)
            {
                ext = ext.Substring(pos + 1);
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

    private static string? GetSourceIcon(PackageKind kind, IEnumerable<string> paths)
    {
        int max = 0;
        string? rslt = null;

        foreach (var item in paths)
        {
            var ext = Path.GetExtension(item).ToLowerInvariant();

            if (kind.TargetsWindows() && ext == ".ico")
            {
                // Only need this
                return item;
            }

            if (!kind.TargetsWindows())
            {
                // For non-windows
                if (ext == ".svg")
                {
                    // Preferred
                    return item;
                }

                // Or biggest PNG
                int size = GetStandardPngSize(item);

                if (size > max)
                {
                    max = size;
                    rslt = item;
                }

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

