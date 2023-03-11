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
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// A base class for package builds. Defines build directory structure.
/// </summary>
public abstract class BuildHost
{
    private IReadOnlyDictionary<MacroId, string> _macros;

    /// <summary>
    /// Constructor.
    /// </summary>
    public BuildHost(BuildRoot root)
    {
        PackageKind = conf.Args.Kind;
        Arguments = conf.Args;
        Configuration = conf;
        Tree = new PackageBuilder(Configuration, buildRootName);


        var icons = Configuration.Icons.Count != 0 ? Configuration.Icons : DefaultIcons;
        IconPaths = GetIconPaths(icons);
        SourceIcon = GetSourceIcon(PackageKind, icons);

        PublishCommands = GetPublishCommands();

        if (!PackageKind.IsWindows())
        {
            DesktopContent = ReadFile(Configuration.DesktopEntry);
            MetaInfoContent = ReadFile(Configuration.MetaInfo);
        }
    }

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
    /// Gets the packaging kind.
    /// </summary>
    public PackKind PackageKind { get; }

    /// <summary>
    /// Gets a reference to the arguments.
    /// </summary>
    public ArgumentReader Arguments { get; }

    /// <summary>
    /// Gets a reference to the configuration.
    /// </summary>
    public ConfigurationReader Configuration { get; }

    /// <summary>
    /// Get the build directory tree.
    /// </summary>
    public PackageBuilder Tree { get; }

    /// <summary>
    /// Gets available macros.
    /// </summary>
    public IReadOnlyDictionary<MacroId, string> Macros
    {
        get
        {
            if (_macros == null)
            {
                var dict = new SortedDictionary<MacroId, string>();

                dict.Add(MacroId.AppBaseName, Configuration.AppBaseName);
                dict.Add(MacroId.AppFriendlyName, Configuration.AppFriendlyName);
                dict.Add(MacroId.AppId, Configuration.AppId);
                dict.Add(MacroId.AppSummary, Configuration.AppSummary);
                dict.Add(MacroId.AppLicense, Configuration.AppLicense);
                dict.Add(MacroId.AppVendor, Configuration.AppVendor);
                dict.Add(MacroId.AppUrl, Configuration.AppUrl ?? "");

                dict.Add(MacroId.AppVersion, AppVersion);
                dict.Add(MacroId.PackRelease, PackRelease);
                dict.Add(MacroId.PackKind, PackageKind.ToString().ToLowerInvariant());
                dict.Add(MacroId.DotnetRuntime, DotnetRuntime);
                dict.Add(MacroId.BuildArch, BuildArch);
                dict.Add(MacroId.BuildTarget, BuildTarget);
                dict.Add(MacroId.OutputPath, Path.Combine(OutputDirectory, OutputName));
                dict.Add(MacroId.IsoDate, DateTime.UtcNow.ToString("yyyy-MM-dd"));

                dict.Add(MacroId.DesktopName, Path.GetFileName(DesktopBuildPath) ?? "");
                dict.Add(MacroId.MetaInfoName, Path.GetFileName(MetaInfoBuildPath) ?? "");
                dict.Add(MacroId.BuildRoot, Tree.BuildRoot);
                dict.Add(MacroId.BuildShare, Tree.BuildUsrShare ?? "");
                dict.Add(MacroId.PublishBin, PublishBin);
                dict.Add(MacroId.DesktopExec, DeployExecPath);

                _macros = dict;
            }

            return _macros;
        }
    }

    /// <summary>
    /// Gets the path of the "source" icon, i.e. the single icon considered to be the most generally suitable.
    /// On Linux this is the first SVG file encountered, or the largest PNG otherwise. On Windows, it is an ICO file.
    /// </summary>
    public string? SourceIcon { get; }

    /// <summary>
    /// A sequence of source icon paths (key) and their destinations (value) under <see cref="PackageBuilder.BuildShareIcons"/>.
    /// Defaults are used if the configuration supplies none. Empty on Windows.
    /// </summary>
    public IReadOnlyDictionary<string, string> IconPaths { get; }

    /// <summary>
    /// Gets the application executable filename (no directory part). I.e. "Configuration.AppBase[.exe]".
    /// </summary>
    public string AppExecName
    {
        get { return Arguments.IsWindowsRuntime() ? Configuration.AppBaseName + ".exe" : Configuration.AppBaseName; }
    }

    /// <summary>
    /// Gets the application executable path , i.e. "${AppBin}/AppBase[.exe]".
    /// </summary>
    public string PublishExecPath
    {
        get { return Path.Combine(PublishBin, AppExecName); }
    }

    /// <summary>
    /// Gets the app bin directory, which may be either: "${Tree.UsrBin}" or "${Tree.BuildRoot}/opt/AppId".
    /// NOTE. This is where we must publish to.
    /// </summary>
    public abstract string PublishBin { get; }

    /// <summary>
    /// Gets the path to the runnable binary when deployed, i.e.: "/usr/bin/${AppExecName}" or "/opt/AppId/${AppExecName}".
    /// </summary>
    public abstract string DeployExecPath{ get; }

    /// <summary>
    /// Gets the command to publish the application, including any post-publish commands. May contain macros.
    /// </summary>
    public IReadOnlyCollection<string> PublishCommands { get; }

    /// <summary>
    /// Gets the desktop file contents. May contain macros. Null for windows.
    /// </summary>
    public string? DesktopContent { get; }

    /// <summary>
    /// Gets the destination build path for the desktop file file. Not used (null) for Windows.
    /// Default is "${Tree.ShareApplications}/${AppId}.desktop".
    /// </summary>
    public abstract string? DesktopBuildPath { get; }

    /// <summary>
    /// Gets the metainfo file contents. May contain macros.
    /// </summary>
    public string? MetaInfoContent { get; }

    /// <summary>
    /// Gets the destination build path for the metainfo file. Null for Windows.
    /// Default is "${Tree.ShareMeta}/${AppId}.metainfo.xml".
    /// </summary>
    public string? MetaInfoBuildPath
    {
        get
        {
            if (Tree.BuildShareMeta != null)
            {
                return Path.Combine(Tree.BuildShareMeta, Configuration.AppId) + ".metainfo.xml";
            }

            return null;
        }
    }

    /// <summary>
    /// Gets a package "manifest file". For RPM this is the SPEC file contents. For Flatpak, it is the manifest.
    /// May contain macros.
    /// </summary>
    public abstract string? ManifestContent { get; }

    /// <summary>
    /// Gets a sequence of commends needed to build the package. May contain macros.
    /// </summary>
    public abstract IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Gets multi-line summary string.
    /// </summary>
    public string GetSummary(BuildMacros macros)
    {
        return GetSummary(macros, Arguments.IsVerbose);
    }

    /// <summary>
    /// Gets multi-line summary string. Overload.
    /// </summary>
    public string GetSummary(BuildMacros macros, bool verbose)
    {
        var sb = new StringBuilder();

        AppendHeader(sb, "APPLICATION");
        AppendPair(sb, nameof(Configuration.AppBaseName), Configuration.AppBaseName);
        AppendPair(sb, nameof(Configuration.AppId), Configuration.AppId);
        AppendPair(sb, nameof(AppVersion), AppVersion);
        AppendPair(sb, nameof(PackRelease), PackRelease);

        AppendHeader(sb, "OUTPUT");
        AppendPair(sb, nameof(PackageKind), PackageKind.ToString().ToLowerInvariant());
        AppendPair(sb, nameof(DotnetRuntime), DotnetRuntime);
        AppendPair(sb, nameof(Arguments.Arch), Arguments.Arch ?? $"Auto ({BuildArch})");
        AppendPair(sb, nameof(BuildTarget), BuildTarget);
        AppendPair(sb, nameof(OutputDirectory), OutputDirectory);
        AppendPair(sb, nameof(OutputName), OutputName);

        if (verbose)
        {
            AppendSection(sb, "CONFIGURATION", Configuration.ToString(false));
        }

        AppendSection(sb, "DESKTOP", macros.Expand(DesktopContent));

        if (verbose)
        {
            var temp = new StringBuilder();

            if (DesktopBuildPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Tree.BuildRoot, DesktopBuildPath));
            }

            if (MetaInfoBuildPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Tree.BuildRoot, MetaInfoBuildPath));
            }

            foreach (var item in IconPaths)
            {
                temp.AppendLine(Path.GetRelativePath(Tree.BuildRoot, item.Value));
            }

            AppendSection(sb, "ASSETS", temp.ToString());

            AppendSection(sb, "METAINFO", macros.Expand(MetaInfoContent));
            AppendSection(sb, "MANIFEST", macros.Expand(ManifestContent));
        }

        AppendSection(sb, "PROJECT BUILD", macros.Expand(PublishCommands));
        AppendSection(sb, "PACKAGE BUILD", macros.Expand(PackageCommands));

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Overrides
    /// </summary>
    public override string ToString()
    {
        return Tree.ToString();
    }

    private static void AppendHeader(StringBuilder sb, string title, bool spacer = true)
    {
        if (sb.Length != 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine(new string('=', 40));
        sb.AppendLine(title);
        sb.AppendLine(new string('=', 40));

        if (spacer)
        {
            sb.AppendLine();
        }
    }

    private static void AppendPair(StringBuilder sb, string name, string? value)
    {
        sb.Append(name);
        sb.Append(": ");
        sb.AppendLine(value);
    }

    private static void AppendSection(StringBuilder sb, string title, IReadOnlyCollection<string> content)
    {
        if (content.Count != 0)
        {
            AppendHeader(sb, title, false);

            foreach (var item in content)
            {
                sb.AppendLine();
                sb.AppendLine(item);
            }
        }
    }

    private static void AppendSection(StringBuilder sb, string title, string? content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            AppendHeader(sb, title);
            sb.AppendLine(content);
        }
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


    private string? ReadFile(string? path)
    {
        if (path != null && !path.Equals(ConfigurationReader.PathNone, StringComparison.OrdinalIgnoreCase) &&
            (Configuration.AssertFiles || File.Exists(path)))
        {
            var content = File.ReadAllText(path).Trim().ReplaceLineEndings("\n");

            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("File is empty " + path);
            }

            return content;
        }

        return null;
    }

    private string? ReadOrJoin(IReadOnlyCollection<string> value)
    {
        if (value.Count == 0)
        {
            return null;
        }

        if (value.Count == 1)
        {
            foreach (var item in value)
            {
                if (item.Length != 0)
                {
                    return ReadFile(item);
                }
            }
        }

        return string.Join('\n', value);
    }

    private string? GetDesktopContent()
    {
        return null;
    }

    private string? GetMetaInfoContent()
    {
        return null;
    }

    private IReadOnlyCollection<string> GetPublishCommands()
    {
        var list = new List<string>();

        if (Configuration.DotnetProjectPath != ConfigurationReader.PathNone)
        {
            var builder = new StringBuilder("dotnet publish");

            var args = Configuration.DotnetPublishArgs;

            if (!string.IsNullOrEmpty(Configuration.DotnetProjectPath) && Configuration.DotnetProjectPath != ".")
            {
                builder.Append(" ");
                builder.Append($"\"{Configuration.DotnetProjectPath}\"");
            }

            if (args != null)
            {
                if (args.Contains("-o ") || args.Contains("--output "))
                {
                    // Cannot be allowed
                    throw new ArgumentException($"The -o, --output option cannot be used in {nameof(Configuration.DotnetPublishArgs)}");
                }

                if (!string.IsNullOrEmpty(Arguments.Runtime) && !args.Contains("-r ") && !args.Contains("--runtime "))
                {
                    builder.Append(" -r ");
                    builder.Append(Arguments.Runtime);
                }

                if (!string.IsNullOrEmpty(Arguments.Build) && !args.Contains("-c ") && !args.Contains("--configuration"))
                {
                    builder.Append(" -c ");
                    builder.Append(Arguments.Build);
                }
            }

            if (!string.IsNullOrEmpty(Arguments.Property))
            {
                builder.Append(" -");

                if (!Arguments.Property.StartsWith("p:"))
                {
                    builder.Append("p:");
                }

                builder.Append(Arguments.Property);
            }

            if (!string.IsNullOrEmpty(args))
            {
                builder.Append(" ");
                builder.Append(args);
            }

            builder.Append(" -o \"");
            builder.Append(PublishBin);
            builder.Append("\"");

            list.Add(builder.ToString());
        }

        list.AddRange(Configuration.DotnetPostPublish);

        return list;
    }
}

