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
/// Accepts a configuration and assembles path and assets information. The build
/// process is run using the Run() method. Most path and content information is
/// public for test and inspection.
/// </summary>
public class PackageBuilder
{
    private readonly static string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
            throw new InvalidOperationException("Failed to get EntryAssembly location");

    /// <summary>
    /// Constructor.
    /// </summary>
    public PackageBuilder(ArgDecoder args)
        : this(new ConfDecoder(args))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public PackageBuilder(ConfDecoder conf)
    {
        Conf = conf;
        Args = conf.Args;
        Tree = new(conf);

        OutputKind = Args.Kind;
        BuildTarget = Args.Build;
        BuildArch = Conf.GetBuildArch();
        DotnetRuntime = Args.Runtime;
        AppVersion = SplitVersion(conf.AppVersionRelease, out string temp);
        PackRelease = temp;
        OutputDirectory = GetOutputDirectory();
        OutputName = GetOutputName(OutputKind);

        // MACROS
        // Items below may use macros
        Macros = new(this);
        Assets = new(Conf, Tree, Macros);
        PublishCommands = GetPublishCommands(Macros);
        PackageCommands = GetPackageCommands();
    }

    /// <summary>
    /// Gets configuration.
    /// </summary>
    public ArgDecoder Args { get; }

    /// <summary>
    /// Gets configuration.
    /// </summary>
    public ConfDecoder Conf { get; }

    /// <summary>
    /// Gets the directory build tree.
    /// </summary>
    public BuildTree Tree { get; }

    /// <summary>
    /// Gets package output kinds.
    /// </summary>
    public PackKind OutputKind { get; }

    /// <summary>
    /// Release or Debug.
    /// </summary>
    public string BuildTarget { get; }

    /// <summary>
    /// Target arch.
    /// </summary>
    public string BuildArch { get; }

    /// <summary>
    /// Gets the dotnet runtime.
    /// </summary>
    public string DotnetRuntime { get; }

    /// <summary>
    /// Gets the application version. This is the configured version, excluding any Release suffix.
    /// </summary>
    public string? AppVersion { get; }

    /// <summary>
    /// Gets the package release. This is the suffix of the configured version.
    /// </summary>
    public string PackRelease { get; } = "1";

    /// <summary>
    /// Gets output directory.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Gets output filename.
    /// </summary>
    public string OutputName { get; }

    /// <summary>
    /// Gets a dictionary of macros.
    /// </summary>
    public BuildMacros Macros { get; }

    /// <summary>
    /// Gets runtime assets.
    /// </summary>
    public BuildAssets Assets { get; }

    /// <summary>
    /// Gets publish commands.
    /// </summary>
    public IReadOnlyCollection<string> PublishCommands { get; }

    /// <summary>
    /// Gets the package commands.
    /// </summary>
    public IReadOnlyCollection<string> PackageCommands { get; }

    public void Run()
    {
        Console.WriteLine(ToString());
        Console.WriteLine();

        if (Conf.Args.IsSkipYes || new ConfirmPrompt().Wait())
        {
            Console.WriteLine();

            Tree.Create();
            Console.WriteLine();

            // Conditional writes
            Tree.Ops.WriteFile(Tree.DesktopPath, Assets.DesktopContent);

            if (OutputKind == PackKind.AppImage)
            {
                // We need a bodge fix to get AppImage to pass validation.
                // In effect, we need two .desktop files. One at root, and one under applications.
                // See: https://github.com/AppImage/AppImageKit/issues/603
                var path = Path.Combine(Tree.AppDir, Conf.AppId + ".desktop");
                Tree.Ops.WriteFile(path, Assets.DesktopContent);
            }

            Tree.Ops.CopyFile(Assets.SourceIcon, Assets.DestIcon);
            Tree.Ops.WriteFile(Tree.AppMetaPath, Assets.AppMetaContent);
            Tree.Ops.WriteFile(Tree.FlatpakManifestPath, Assets.FlatpakManifestContent);

            foreach (var item in Assets.LinuxIcons)
            {
                Tree.Ops.CopyFile(item.Key, item.Value, true);
            }

            foreach (var item in Macros.Dictionary)
            {
                // We set variable to be used by any executed processes
                Environment.SetEnvironmentVariable(item.Key, item.Value);
            }

            if (Args.Arch != null && OutputKind == PackKind.AppImage)
            {
                // Used by AppImage
                Environment.SetEnvironmentVariable("ARCH", Conf.Args.Arch);
            }

            foreach (var item in PublishCommands)
            {
                Console.WriteLine();
                Tree.Ops.Exec(item);
            }

            Console.WriteLine();
            Tree.Ops.AssertExists(Path.Combine(Tree.PublishBin, Tree.AppExecName));
            Tree.Ops.CopyDirectory(Tree.PublishBin, Tree.AppInstall);

            if (OutputKind == PackKind.Rpm)
            {
                Tree.Ops.WriteFile(Tree.RpmSpecPath, Assets.GetRpmSpecContent(GetAppDirFiles()));
            }

            Tree.Ops.CreateDirectory(OutputDirectory);

            foreach (var item in PackageCommands)
            {
                Console.WriteLine();
                Tree.Ops.Exec(item);
            }

            if (OutputKind == PackKind.Zip)
            {
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Overrides. Provides console output.
    /// </summary>
    public override string ToString()
    {
        return ToString(Args.IsVerbose);
    }

    /// <summary>
    /// Provides console output.
    /// </summary>
    public string ToString(bool verbose)
    {
        var builder = new StringBuilder();

        AppendHeader(builder, "APPLICATION");
        AppendPair(builder, nameof(Conf.AppBase), Conf.AppBase);
        AppendPair(builder, nameof(Conf.AppId), Conf.AppId);
        AppendPair(builder, nameof(AppVersion), AppVersion);
        AppendPair(builder, nameof(PackRelease), PackRelease);

        AppendHeader(builder, "OUTPUT");
        AppendPair(builder, nameof(OutputKind), OutputKind.ToString().ToLowerInvariant());
        AppendPair(builder, nameof(DotnetRuntime), DotnetRuntime);
        AppendPair(builder, nameof(Args.Arch), Args.Arch ?? $"Auto ({BuildArch})");
        AppendPair(builder, nameof(BuildTarget), BuildTarget);
        AppendPair(builder, nameof(OutputDirectory), OutputDirectory);
        AppendPair(builder, nameof(OutputName), OutputName);

        if (verbose)
        {
            AppendHeader(builder, "CONF");
            builder.AppendLine(Conf.ToString(false));
        }

        if (Assets.DesktopContent != null)
        {
            AppendHeader(builder, "DESKTOP");
            builder.AppendLine(Assets.DesktopContent);
        }

        if (verbose)
        {
            AppendHeader(builder, "META PATHS");
            builder.AppendLine(Path.GetRelativePath(Tree.PackTop, Tree.DesktopPath));
            builder.AppendLine(Path.GetRelativePath(Tree.PackTop, Tree.AppMetaPath));

            foreach (var item in Assets.LinuxIcons)
            {
                builder.AppendLine(Path.GetRelativePath(Tree.PackTop, item.Value));
            }

            if (Assets.AppMetaContent != null)
            {
                AppendHeader(builder, "APP METADATA");
                builder.AppendLine(Assets.AppMetaContent);
            }

            if (OutputKind == PackKind.Flatpak)
            {
                AppendHeader(builder, "FLATPAK MANIFEST");
                builder.AppendLine(Assets.FlatpakManifestContent);
            }

            if (OutputKind == PackKind.Rpm)
            {
                AppendHeader(builder, "RPM SPEC");
                builder.AppendLine(Assets.GetRpmSpecContent());
            }

            AppendHeader(builder, "MACROS");
            builder.AppendLine(Macros.ToString());
        }

        AppendHeader(builder, "PROJECT BUILD", false);

        foreach (var item in PublishCommands)
        {
            builder.AppendLine();
            builder.AppendLine(item);
        }

        AppendHeader(builder, "PACKAGE BUILD", false);

        foreach (var item in PackageCommands)
        {
            builder.AppendLine();
            builder.AppendLine(item);
        }

        return builder.ToString().Trim();
    }

    private static void AppendHeader(StringBuilder builder, string title, bool spacer = true)
    {
        if (builder.Length != 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine(new string('=', 40));
        builder.AppendLine(title);
        builder.AppendLine(new string('=', 40));

        if (spacer)
        {
            builder.AppendLine();
        }
    }

    private static void AppendPair(StringBuilder builder, string name, string? value)
    {
        builder.Append(name);
        builder.Append(": ");
        builder.AppendLine(value);
    }

    private static string? SplitVersion(string? version, out string release)
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

    private string[] GetAppDirFiles()
    {
        var opts = new EnumerationOptions();
        opts.RecurseSubdirectories = true;
        opts.ReturnSpecialDirectories = false;
        opts.IgnoreInaccessible = true;

        var files = Directory.GetFiles(Tree.AppDir, "*", SearchOption.AllDirectories);

        for (int n = 0; n < files.Length; ++n)
        {
            files[n] = Path.GetRelativePath(Tree.AppDir, files[n]);
        }

        return files;
    }

    private string GetOutputDirectory()
    {
        var argDir = Path.GetDirectoryName(Args.Output);

        if (argDir != null)
        {
            if (Path.IsPathFullyQualified(argDir))
            {
                return argDir;
            }

            return Path.Combine(Conf.OutputDirectory, argDir);
        }

        return Conf.OutputDirectory;
    }

    private string GetOutputName(PackKind kind)
    {
        var output = Path.GetFileName(Args.Output);

        if (output != null)
        {
            return output;
        }

        output = Conf.AppBase;

        if (Conf.OutputVersion && !string.IsNullOrEmpty(AppVersion))
        {
            output += $"-{AppVersion}-{PackRelease}";
        }

        output += $".{BuildArch}";

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

    private List<string> GetPublishCommands(BuildMacros macros)
    {
        var list = new List<string>();

        if (Conf.DotnetProjectPath != ConfDecoder.PathNone)
        {
            var args = macros.Expand(Conf.DotnetPublishArgs) ?? "";

            if (args.Contains("-o ") || args.Contains("--output "))
            {
                // Cannot be allowed
                throw new ArgumentException($"Option -o, --output cannot be specified in {nameof(Conf.DotnetProjectPath)}");
            }

            var builder = new StringBuilder("dotnet publish");

            if (!string.IsNullOrEmpty(Conf.DotnetProjectPath) && Conf.DotnetProjectPath != ".")
            {
                builder.Append(" ");
                builder.Append(Conf.DotnetProjectPath);
            }

            if (!string.IsNullOrEmpty(DotnetRuntime) && !args.Contains("-r ") && !args.Contains("--runtime "))
            {
                builder.Append(" -r ");
                builder.Append(DotnetRuntime);
            }

            if (!string.IsNullOrEmpty(Args.Build) && !args.Contains("-c ") && !args.Contains("--configuration"))
            {
                builder.Append(" -c ");
                builder.Append(Args.Build);
            }

            if (!string.IsNullOrEmpty(AppVersion) && !args.Contains("-p:Version") && !args.Contains("--property:Version"))
            {
                builder.Append(" -p:Version=");
                builder.Append(AppVersion);
            }

            if (!string.IsNullOrEmpty(Args.Property))
            {
                builder.Append(" -");

                if (!Args.Property.StartsWith("p:"))
                {
                    builder.Append("p:");
                }

                builder.Append(Args.Property);
            }

            if (!string.IsNullOrEmpty(Conf.DotnetPublishArgs))
            {
                builder.Append(" ");
                builder.Append(Conf.DotnetPublishArgs);
            }

            builder.Append(" -o \"");
            builder.Append(Tree.PublishBin);
            builder.Append("\"");

            list.Add(builder.ToString());
        }

        foreach (var item in Macros.Expand(Conf.DotnetPostPublish))
        {
            list.Add(item);
        }

        return list;
    }

    private List<string> GetPackageCommands()
    {
        var list = new List<string>();
        var output = Path.Combine(OutputDirectory, OutputName);

        if (OutputKind == PackKind.AppImage)
        {
            // IMPORTANT - Create AppRun link
            // ln -s {target} {link}
            list.Add($"ln -s \"{Tree.LaunchExec}\" \"{Path.Combine(Tree.AppDir, "AppRun")}\"");

            list.Add($"{Conf.AppImageCommand} {Conf.AppImageArgs} \"{Tree.AppDir}\" \"{output}\"");

            if (Args.IsRun)
            {
                list.Add(output);
            }

            return list;
        }

        if (OutputKind == PackKind.Flatpak)
        {
            var temp = Path.Combine(Tree.PackTop, "build");
            var state = Path.Combine(Tree.PackTop, "state");
            var repo = Path.Combine(Tree.PackTop, "repo");

            var cmd = $"flatpak-builder {Conf.FlatpakBuilderArgs}";

            if (Args.Arch != null)
            {
                // Explicit only (otherwise leave it to utility to determine)
                cmd += $" --arch ${Args.Arch}";
            }

            cmd += $" --repo=\"{repo}\" --force-clean \"{temp}\" --state-dir \"{state}\" \"{Tree.FlatpakManifestPath}\"";
            list.Add(cmd);

            list.Add($"flatpak build-bundle \"{repo}\" \"{output}\" {Conf.AppId}");

            return list;
        }

        if (OutputKind == PackKind.Rpm)
        {
            // --nobuild
            // --nocheck
            // --noprep
            // --noclean
            // --buildroot=DIRECTORY
            // https://stackoverflow.com/questions/2777737/how-to-set-the-rpmbuild-destination-folder
            var cmd = $"rpmbuild -bb --nocheck --noprep --verbose \"{Tree.RpmSpecPath}\"";
            cmd += $" --define \"_topdir {Path.Combine(Tree.PackTop, "rpmbuild")}\" --buildroot=\"{Tree.AppDir}\"";
            cmd += $" --define \"_rpmdir {output}\" --define \"_build_id_links none\"";

            list.Add(cmd);
            return list;
        }

        if (OutputKind == PackKind.Zip)
        {
            return list;
        }

        throw new NotImplementedException($"Not implemented {OutputKind}");
    }

}


