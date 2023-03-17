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

using System.Runtime.InteropServices;
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Handles Console interaction, publishes the dotnet application and creates and executes a concrete instance of
/// <see cref="PackageBuilder"/>.
/// </summary>
public class BuildHost
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public BuildHost(ArgumentReader args)
        : this(new ConfigurationReader(args))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public BuildHost(ConfigurationReader conf)
    {
        Arguments = conf.Arguments;
        Configuration = conf;
        Builder = new BuilderFactory().Create(Configuration);
        Macros = new MacrosExpander(Builder);

        var kind = Builder.Architecture.Kind;

        if (!kind.IsWindows())
        {
            var desktop = Configuration.ReadAssociatedFile(Configuration.DesktopFile);

            // Careful - filename may equal "NONE"
            if (Configuration.DesktopFile == null)
            {
                // Magic desktop file
                desktop = MetaTemplates.Desktop;
            }

            if (desktop != null && ((!desktop.Contains("Exec=") && !desktop.Contains("Exec ")) || !desktop.Contains(MacroId.DesktopExec.ToVar())))
            {
                Builder.WarningSink.Add($"WARNING. Desktop file does not contain Exec={MacroId.DesktopExec.ToVar()} line needed to accommodate multi-variant deployments");
            }

            ExpandedDesktop = Macros.Expand(desktop, Path.GetFileName(Configuration.DesktopFile));
            ExpandedMetaInfo = Macros.Expand(Configuration.ReadAssociatedFile(Configuration.MetaFile), true, Path.GetFileName(Configuration.MetaFile));

            if (ExpandedDesktop == null)
            {
                // App image can launch from standalone file
                if (string.IsNullOrEmpty(Configuration.StartCommand) && kind != DeployKind.AppImage)
                {
                    Builder.WarningSink.Add($"Note. No desktop file and no {nameof(Configuration.StartCommand)} is configured\n" +
                        "There will be no way to start the application on the target system - are you sure?");
                }
            }

            if (ExpandedMetaInfo == null)
            {
                Builder.WarningSink.Add("Note. AppStream metadata (.metainfo.xml) file not provided");
            }
        }

        if (Builder.Architecture.IsWindowsRuntime != kind.IsWindows(false))
        {
            Builder.WarningSink.Add($"WARNING. You are going to package a {Builder.Architecture.RuntimeId} runtime as {kind}\n"
                + "Is this really what you want to do?");
        }

        PublishCommands = Macros.Expand(GetPublishCommands(Builder), "dotnet publish");

        if (Arguments.IsRun && !Builder.SupportsRunOnBuild)
        {
            Builder.WarningSink.Add($"{Arguments.Kind} does not support post-build run (--{ArgumentReader.RunLongArg} ignored)");
        }

    }

    /// <summary>
    /// Gets a reference to the arguments.
    /// </summary>
    public ArgumentReader Arguments { get; }

    /// <summary>
    /// Gets a reference to the configuration.
    /// </summary>
    public ConfigurationReader Configuration { get; }

    /// <summary>
    /// Get the concrete package builder.
    /// </summary>
    public PackageBuilder Builder { get; }

    /// <summary>
    /// Get the macro expander.
    /// </summary>
    public MacrosExpander Macros { get; }

    /// <summary>
    /// Gets expanded desktop entry content.
    /// </summary>
    public string? ExpandedDesktop { get; }

    /// <summary>
    /// Gets expanded desktop entry content.
    /// </summary>
    public string? ExpandedMetaInfo { get; }

    /// <summary>
    /// Gets expanded publish commands, including post publish.
    /// </summary>
    public IReadOnlyCollection<string> PublishCommands { get; }

    /// <summary>
    /// Runs the build process. Returns true if complete, or false if cancelled.
    /// </summary>
    public bool Run()
    {
        Console.WriteLine(ToString());
        Console.WriteLine();

        if (Arguments.IsSkipYes || new ConfirmPrompt().Wait())
        {
            Builder.Create(ExpandedDesktop, ExpandedMetaInfo);

            foreach (var item in Macros.Dictionary)
            {
                // We set variable to be used by any executed processes
                Environment.SetEnvironmentVariable(item.Key.ToName(), item.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Building Project ...");
            Builder.Operations.Execute(PublishCommands);

            if (Arguments.IsVerbose)
            {
                Console.WriteLine();
                Console.WriteLine("BuildRoot:");

                foreach (var item in Builder.ListBuild(false))
                {
                    Console.WriteLine(item);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Building Package ...");
            Builder.BuildPackage();

            Console.WriteLine();
            Console.WriteLine("OUTPUT OK:");
            Console.WriteLine(Builder.OutputPath);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Overrides and returns pre-build action summary.
    /// </summary>
    public override string ToString()
    {
        return ToString(Arguments.IsVerbose);
    }

    /// <summary>
    /// Returns pre-build action summary.
    /// </summary>
    public string ToString(bool verbose)
    {
        var sb = new StringBuilder();

        AppendHeader(sb, "APPLICATION");
        AppendPair(sb, nameof(Configuration.AppBaseName), Configuration.AppBaseName);
        AppendPair(sb, nameof(Configuration.AppId), Configuration.AppId);
        AppendPair(sb, nameof(Builder.AppVersion), Builder.AppVersion);
        AppendPair(sb, nameof(Builder.PackRelease), Builder.PackRelease);

        AppendHeader(sb, "OUTPUT");
        AppendPair(sb, nameof(DeployKind), Arguments.Kind.ToString().ToLowerInvariant());
        AppendPair(sb, nameof(Arguments.Runtime), Arguments.Runtime);
        AppendPair(sb, nameof(Arguments.Arch), Arguments.Arch ?? $"Auto ({Builder.Architecture})");
        AppendPair(sb, nameof(Arguments.Build), Arguments.Build);
        AppendPair(sb, nameof(Builder.OutputName), Builder.OutputName);
        AppendPair(sb, nameof(Builder.OutputDirectory), Builder.OutputDirectory);

        if (verbose)
        {
            AppendSection(sb, "CONFIGURATION", Configuration.ToString(false));
        }

        AppendSection(sb, "DESKTOP", ExpandedDesktop);

        if (verbose)
        {
            var temp = new StringBuilder();

            foreach (var item in Builder.IconPaths)
            {
                temp.AppendLine(Path.GetRelativePath(Builder.AppRoot, item.Value));
            }

            if (Builder.DesktopPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Builder.AppRoot, Builder.DesktopPath));
            }

            if (Builder.MetaInfoPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Builder.AppRoot, Builder.MetaInfoPath));
            }

            AppendSection(sb, "ASSETS", temp.ToString().TrimEnd());
            AppendSection(sb, "METAINFO", ExpandedMetaInfo);
            AppendSection(sb, "MANIFEST", Builder.ManifestContent?.TrimEnd());
        }

        AppendSection(sb, "BUILD PROJECT", PublishCommands);
        AppendSection(sb, "BUILD PACKAGE", Builder.PackageCommands);
        AppendSection(sb, "WARNINGS", Builder.WarningSink);

        return sb.ToString().Trim();
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

    private static void AppendSection(StringBuilder sb, string title, ICollection<string> content)
    {
        AppendSection(sb, title, (IReadOnlyCollection<string>)content);
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

    private static IReadOnlyCollection<string> GetPublishCommands(PackageBuilder builder)
    {
        // Returns unexpanded
        var list = new List<string>();
        var conf = builder.Configuration;

        if (conf.DotnetProjectPath != ConfigurationReader.PathDisable)
        {
            var sb = new StringBuilder();

            if (conf.Arguments.Clean)
            {
                // Clean first
                sb.Append("dotnet clean");

                if (!string.IsNullOrEmpty(conf.DotnetProjectPath) && conf.DotnetProjectPath != ".")
                {
                    sb.Append($" \"{conf.DotnetProjectPath}\"");
                }

                list.Add(sb.ToString());
                sb.Clear();
            }

            // PUBLISH
            sb.Append("dotnet publish");
            var pa = conf.DotnetPublishArgs;

            if (!string.IsNullOrEmpty(conf.DotnetProjectPath) && conf.DotnetProjectPath != ".")
            {
                sb.Append($" \"{conf.DotnetProjectPath}\"");
            }

            if (pa != null)
            {
                if (pa.Contains("-o ") || pa.Contains("--output "))
                {
                    // Cannot be allowed
                    throw new ArgumentException($"The -o, --output option cannot be used in {nameof(conf.DotnetPublishArgs)}");
                }

                if (!string.IsNullOrEmpty(conf.Arguments.Runtime) && !pa.Contains("-r ") && !pa.Contains("--runtime "))
                {
                    sb.Append(" -r ");
                    sb.Append(conf.Arguments.Runtime);
                }

                if (!string.IsNullOrEmpty(conf.Arguments.Build) && !pa.Contains("-c ") && !pa.Contains("--configuration"))
                {
                    sb.Append(" -c ");
                    sb.Append(conf.Arguments.Build);
                }
            }

            if (!string.IsNullOrEmpty(conf.Arguments.Property))
            {
                sb.Append(" -");

                if (!conf.Arguments.Property.StartsWith("p:"))
                {
                    sb.Append("p:");
                }

                sb.Append(conf.Arguments.Property);
            }

            if (!string.IsNullOrEmpty(pa))
            {
                sb.Append(" ");
                sb.Append(pa);
            }

            sb.Append(" -o \"");
            sb.Append(builder.PublishBin);
            sb.Append("\"");

            list.Add(sb.ToString());
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            list.AddRange(conf.DotnetPostPublishOnWindows);
        }
        else
        {
            list.AddRange(conf.DotnetPostPublish);
        }

        return list;
    }
}

