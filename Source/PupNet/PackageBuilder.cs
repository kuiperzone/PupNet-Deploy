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

        // MACROS
        // Items below may use macros
        Macros = new(Tree);
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

            if (Macros.OutputKind == PackKind.AppImage)
            {
                // We need a bodge fix to get AppImage to pass validation.
                // In effect, we need two .desktop files. One at root, and one under applications.
                // See: https://github.com/AppImage/AppImageKit/issues/603
                var path = Path.Combine(Tree.AppDir, Conf.AppId + ".desktop");
                Tree.Ops.WriteFile(path, Assets.DesktopContent);
            }

            Tree.Ops.CopyFile(Assets.SourceIcon, Assets.DestIcon);
            Tree.Ops.WriteFile(Tree.AppMetaPath, Assets.AppMetaContent);

            foreach (var item in Assets.LinuxIcons)
            {
                Tree.Ops.CopyFile(item.Key, item.Value, true);
            }

            foreach (var item in Macros.Dictionary)
            {
                // We set variable to be used by any executed processes
                Environment.SetEnvironmentVariable(item.Key, item.Value);
            }

            foreach (var item in PublishCommands)
            {
                Console.WriteLine(item);
                Tree.Ops.Exec(item);
            }

            Console.WriteLine();
            Tree.Ops.AssertExists(Path.Combine(Tree.PublishBin, Tree.AppExecName));
            Tree.Ops.CopyDirectory(Tree.PublishBin, Tree.AppInstall);

            AddConditionalRunLink();

            // Specs after all files are assembled
            Tree.Ops.WriteFile(Tree.FlatpakManifestPath, Assets.FlatpakManifestContent);
            Tree.Ops.WriteFile(Tree.RpmSpecPath, Assets.GetRpmSpecContent(true));
            Tree.Ops.CreateDirectory(Macros.OutputDirectory);

            if (Args.IsVerbose)
            {
                Console.WriteLine();
                Console.WriteLine("Packaged Files:");

                foreach (var item in Tree.GetDirectoryContents(Tree.AppDir))
                {
                    Console.WriteLine(item);
                }
            }

            if (Args.Arch != null && Macros.OutputKind == PackKind.AppImage)
            {
                // Used by AppImage
                Environment.SetEnvironmentVariable("ARCH", Conf.Args.Arch);
            }

            foreach (var item in PackageCommands)
            {
                Console.WriteLine();
                Tree.Ops.Exec(item);
            }

            if (Macros.OutputKind == PackKind.Zip)
            {
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("OUTPUT: " + Path.Combine(Macros.OutputDirectory, Macros.OutputName));
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
        AppendPair(builder, nameof(Conf.AppBaseName), Conf.AppBaseName);
        AppendPair(builder, nameof(Conf.AppId), Conf.AppId);
        AppendPair(builder, nameof(Macros.AppVersion), Macros.AppVersion);
        AppendPair(builder, nameof(Macros.PackRelease), Macros.PackRelease);

        AppendHeader(builder, "OUTPUT");
        AppendPair(builder, nameof(Macros.OutputKind), Macros.OutputKind.ToString().ToLowerInvariant());
        AppendPair(builder, nameof(Macros.DotnetRuntime), Macros.DotnetRuntime);
        AppendPair(builder, nameof(Args.Arch), Args.Arch ?? $"Auto ({Macros.BuildArch})");
        AppendPair(builder, nameof(Macros.BuildTarget), Macros.BuildTarget);
        AppendPair(builder, nameof(Macros.OutputDirectory), Macros.OutputDirectory);
        AppendPair(builder, nameof(Macros.OutputName), Macros.OutputName);

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

            if (Macros.OutputKind == PackKind.Flatpak)
            {
                AppendHeader(builder, "FLATPAK MANIFEST");
                builder.AppendLine(Assets.FlatpakManifestContent);
            }

            if (Macros.OutputKind == PackKind.Rpm)
            {
                AppendHeader(builder, "RPM SPEC");
                builder.AppendLine(Assets.GetRpmSpecContent(false));
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

    private void AddConditionalRunLink()
    {
        if (Macros.OutputKind == PackKind.AppImage)
        {
            // IMPORTANT - Create AppRun link
            // ln -s {target} {link}
            var cmd = $"ln -s \"{Tree.LaunchExec}\" \"{Path.Combine(Tree.AppDir, "AppRun")}\"";
            Tree.Ops.Exec(cmd);
        }
        else
        if (!string.IsNullOrEmpty(Conf.StartCommand) && Tree.AppBin != Tree.AppInstall && Macros.OutputKind.IsLinux())
        {
            var path = Path.Combine(Tree.AppBin, Conf.StartCommand);

            // Note
            // Rpm and Deb etc only. These get installed to /opt, but put 'link file' in /usr/bin
            var script = $"#!/bin/sh\nexec {Tree.LaunchExec} \"$@\"";

            if (!File.Exists(path))
            {
                Tree.Ops.WriteFile(path, script);
                Tree.Ops.Exec($"chmod a+x {path}");
            }
        }
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
                builder.Append($"\"{Conf.DotnetProjectPath}\"");
            }

            if (!string.IsNullOrEmpty(Macros.DotnetRuntime) && !args.Contains("-r ") && !args.Contains("--runtime "))
            {
                builder.Append(" -r ");
                builder.Append(Macros.DotnetRuntime);
            }

            if (!string.IsNullOrEmpty(Args.Build) && !args.Contains("-c ") && !args.Contains("--configuration"))
            {
                builder.Append(" -c ");
                builder.Append(Args.Build);
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

            if (!string.IsNullOrEmpty(args))
            {
                builder.Append(" ");
                builder.Append(args);
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
        var output = Path.Combine(Macros.OutputDirectory, Macros.OutputName);

        if (Macros.OutputKind == PackKind.AppImage)
        {
            // Path to embedded
            if (BuildAssets.AppImageTool == null)
            {
                throw new InvalidOperationException($"{PackKind.AppImage} not supported on {ConfDecoder.GetOSArch()}");
            }

            list.Add($"{BuildAssets.AppImageTool} {Conf.AppImageArgs} \"{Tree.AppDir}\" \"{output}\"");

            if (Args.IsRun)
            {
                list.Add(output);
            }

            return list;
        }

        if (Macros.OutputKind == PackKind.Flatpak)
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

        if (Macros.OutputKind == PackKind.Rpm)
        {
            // https://stackoverflow.com/questions/2777737/how-to-set-the-rpmbuild-destination-folder
            var cmd = $"rpmbuild -bb \"{Tree.RpmSpecPath}\"";
            cmd += $" --define \"_topdir {Path.Combine(Tree.PackTop, "rpmbuild")}\" --buildroot=\"{Tree.AppDir}\"";
            cmd += $" --define \"_rpmdir {output}\" --define \"_build_id_links none\"";

            list.Add(cmd);
            return list;
        }

        if (Macros.OutputKind == PackKind.Deb)
        {

            return list;
        }

        if (Macros.OutputKind == PackKind.Zip)
        {
            return list;
        }

        throw new NotImplementedException($"Not implemented {Macros.OutputKind}");
    }

}


