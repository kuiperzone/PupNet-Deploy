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
        Macros = new BuildMacros(Builder);

        var warnings = new List<string>();

        if (!Builder.IsWindowsPackage)
        {
            ExpandedDesktop = Macros.Expand(Configuration.ReadFile(Configuration.DesktopEntry), warnings, Path.GetFileName(Configuration.DesktopEntry));
            ExpandedMetaInfo = Macros.Expand(Configuration.ReadFile(Configuration.MetaInfo), warnings, Path.GetFileName(Configuration.MetaInfo));

            if (ExpandedDesktop == null)
            {
                warnings.Add("Installation does not provide a desktop file");
            }

            if (ExpandedMetaInfo == null)
            {
                warnings.Add("Installation does not provide AppStream metadata");
            }
        }

        PublishCommands = Macros.Expand(GetPublishCommands(Builder), warnings, "dotnet publish");

        if (Arguments.IsRun && !Builder.SupportsRunOnBuild)
        {
            warnings.Add($"{Builder.PackKind} does not support the post-build run option");
        }

        Warnings = warnings;
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
    public BuildMacros Macros { get; }

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
    /// Gets any warning pre-build.
    /// </summary>
    public IReadOnlyCollection<string> Warnings { get; }

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
            Console.WriteLine("Build Project");
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
            Console.WriteLine("Build Package");
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
        AppendPair(sb, nameof(Builder.PackKind), Builder.PackKind.ToString().ToLowerInvariant());
        AppendPair(sb, nameof(Arguments.Runtime), Arguments.Runtime);
        AppendPair(sb, nameof(Arguments.Arch), Arguments.Arch ?? $"Auto ({Configuration.GetBuildArch()})");
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
                temp.AppendLine(Path.GetRelativePath(Builder.BuildRoot, item.Value));
            }

            if (Builder.DesktopPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Builder.BuildRoot, Builder.DesktopPath));
            }

            if (Builder.MetaInfoPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Builder.BuildRoot, Builder.MetaInfoPath));
            }

            AppendSection(sb, "ASSETS", temp.ToString().TrimEnd());
            AppendSection(sb, "METAINFO", ExpandedMetaInfo);
            AppendSection(sb, "MANIFEST", Builder.ManifestContent);
        }

        AppendSection(sb, "BUILD PROJECT", PublishCommands);
        AppendSection(sb, "BUILD PACKAGE", Builder.PackageCommands);
        AppendSection(sb, "WARNINGS", Warnings);

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

        if (conf.DotnetProjectPath != ConfigurationReader.PathNone)
        {
            var sb = new StringBuilder("dotnet publish");
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

        list.AddRange(conf.DotnetPostPublish);
        return list;
    }
}

