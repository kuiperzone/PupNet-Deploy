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

using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace KuiperZone.PupNet;

/// <summary>
/// Creates a concrete instance of <see cref="PackageBuilder"/>, and handles the Console interaction with the user.
/// </summary>
public class BuildHost
{
    private static readonly string DotnetHost = GetDotnetHost();

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
        Macros = new MacroExpander(Builder);

        if (!Configuration.AppId.Contains('.'))
        {
            // AppId must have a '.'
            Builder.WarningSink.Add($"WARNING. Configuration item {nameof(Configuration.AppId)} should be in reverse DNS form, i.e. 'net.example.appname'");
        }

        if (!string.IsNullOrEmpty(Configuration.AppChangeFile) && Builder.ChangeLog.Items.Count == 0)
        {
            Builder.WarningSink.Add($"WARNING. Configuration supplied {nameof(Configuration.AppChangeFile)}, but content does not contain version information in recognised format");
        }

        if (Configuration.PublisherLinkUrl != null && !Configuration.PublisherLinkUrl.Contains("://") && !Configuration.PublisherLinkUrl.Contains('.'))
        {
            // AppId must have a '://' and '.'
            Builder.WarningSink.Add($"WARNING. Configuration item {nameof(Configuration.PublisherLinkUrl)} doesn't look like a valid URL (a valid example is: http://example.net)");
        }

        if (Builder.BuildShareApplications != null)
        {
            // Magic desktop file - always have one
            var desktop = Configuration.ReadAssociatedFile(Configuration.DesktopFile) ?? MetaTemplates.Desktop;

            // Not fool-proof but a fair check
            bool hasExec = desktop.Contains("Exec=") || desktop.Contains("Exec ");
            bool hasInstall = desktop.Contains(MacroId.InstallBin.ToVar()) || desktop.Contains(MacroId.InstallExec.ToVar());

            if (!hasExec || !hasInstall)
            {
                Builder.WarningSink.Add($"WARNING. Desktop file does not contain line needed to accommodate multi-variant deployments: 'Exec={MacroId.InstallExec.ToVar()}' or 'Exec={MacroId.InstallBin.ToVar()}/app-name'");
            }

            ExpandedDesktop = Macros.Expand(desktop, Path.GetFileName(Configuration.DesktopFile));
        }

        if (Builder.BuildShareMeta != null)
        {
            ExpandedMetaInfo = Macros.Expand(Configuration.ReadAssociatedFile(Configuration.MetaFile), true, Path.GetFileName(Configuration.MetaFile));

            if (ExpandedDesktop == null)
            {
                // AppImage can launch from standalone file, desktop not required
                if (string.IsNullOrEmpty(Configuration.StartCommand) && Builder.Kind != PackageKind.AppImage && Builder.Kind != PackageKind.Zip)
                {
                    Builder.WarningSink.Add($"Note. No desktop file or {nameof(Configuration.StartCommand)} is configured\n" +
                        "There will be no way to start the application once installed - are you sure?");
                }
            }

            if (ExpandedMetaInfo == null)
            {
                Builder.WarningSink.Add("Note. AppStream metadata (.metainfo.xml) file not provided");
            }
            else
            {
                try
                {
                    XDocument.Parse(ExpandedMetaInfo);
                }
                catch (Exception e)
                {
                    Builder.WarningSink.Add($"CRITICAL. AppStream metadata {Path.GetFileName(Configuration.MetaFile)} is not valid XML. {e.Message}");
                }
            }
        }

        if (Configuration.DotnetProjectPath == ConfigurationReader.PathDisable)
        {
            var name = nameof(Configuration.DotnetPostPublish);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                name = nameof(Configuration.DotnetPostPublishOnWindows);
            }

            Builder.WarningSink.Add($"CRITICAL. Configuration of {name} is mandatory where {nameof(Configuration.DotnetProjectPath)} = {ConfigurationReader.PathDisable}");
        }

        if (Builder.Runtime.IsArchUncertain)
        {
            Builder.WarningSink.Add($"WARNING. Package architecture is uncertain for the runtime {Builder.Runtime}\n" +
                $"Use the argument --{ArgumentReader.ArchLongArg} to specify.");
        }

        if ((Builder.Runtime.IsLinuxRuntime && !Builder.Kind.TargetsLinux()) ||
            (Builder.Runtime.IsWindowsRuntime && !Builder.Kind.TargetsWindows()) ||
            (Builder.Runtime.IsOsxRuntime && !Builder.Kind.TargetsOsx()))
        {
            Builder.WarningSink.Add($"WARNING. You are going to package the runtime '{Builder.Runtime.RuntimeId}' as {Builder.Kind}\n" +
                "Are you sure?");
        }

        if (!Builder.Kind.CanBuildOnSystem())
        {
            Builder.WarningSink.Add($"CRITICAL. Building {Builder.Kind} packages is not supported on {RuntimeConverter.SystemOS} development systems");
        }

        PublishCommands = Macros.Expand(GetPublishCommands(Builder), nameof(PublishCommands));

        if (Arguments.IsRun && !Builder.SupportsPostRun)
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
    public MacroExpander Macros { get; }

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
                Console.WriteLine("Deploy Files:");

                foreach (var item in Builder.ListBuild(false))
                {
                    Console.WriteLine(item);
                }
            }

            if (Builder.Kind == PackageKind.Setup)
            {
                if (Directory.GetFiles(Builder.BuildAppBin, "*.dll").Length == 0)
                {
                    if (Builder.ManifestContent != null)
                    {
                        Console.WriteLine("No dll files found");
                        StringReader reader = new StringReader(Builder.ManifestContent);
                        string line = reader.ReadLine();
                        var sb = new StringBuilder();
                        while (line != null)
                        {
                            if (line.Contains($"Source: \"{Builder.BuildAppBin}\\*.dll\"; DestDir: \"{{app}}\""))
                            {
                                Console.WriteLine("Skipping dll line: " + line);    
                            }
                            else
                            {
                                sb.AppendLine(line);
                            }
                            line = reader.ReadLine();
                        }
                        Builder.ManifestContent = sb.ToString().TrimEnd();    
                    }
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

        AppendHeader(sb, $"APPLICATION: {Configuration.AppBaseName} {Builder.AppVersion} [{Builder.PackageRelease}]");
        AppendPair(sb, nameof(Configuration.AppBaseName), Configuration.AppBaseName);
        AppendPair(sb, nameof(Configuration.AppId), Configuration.AppId);
        AppendPair(sb, nameof(Builder.AppVersion), Builder.AppVersion);
        AppendPair(sb, nameof(Builder.PackageRelease), Builder.PackageRelease);

        if (Builder.SupportsStartCommand)
        {
            AppendPair(sb, nameof(Configuration.StartCommand), Configuration.StartCommand ?? "[None]");
        }
        else
        {
            AppendPair(sb, nameof(Configuration.StartCommand), "[Not Supported]");
        }

        if (Builder.Kind == PackageKind.Setup)
        {
            AppendPair(sb, nameof(Configuration.SetupCommandPrompt), Configuration.SetupCommandPrompt ?? "[None]");
        }

        AppendHeader(sb, $"OUTPUT: {Arguments.Kind.ToString().ToUpperInvariant()}");
        AppendPair(sb, nameof(PackageKind), Arguments.Kind.ToString());
        AppendPair(sb, nameof(Arguments.Runtime), Arguments.Runtime);
        AppendPair(sb, nameof(Arguments.Arch), Arguments.Arch ?? $"Auto ({Builder.Architecture})");
        AppendPair(sb, nameof(Arguments.Build), Arguments.Build);
        AppendPair(sb, nameof(Builder.OutputName), Builder.OutputName);
        AppendPair(sb, nameof(Builder.OutputDirectory), Builder.OutputDirectory);

        if (verbose)
        {
            AppendSection(sb, $"CONFIGURATION: {Path.GetFileName(Configuration.Reader.Filepath)}", Configuration.ToString(DocStyles.NoComments));
        }

        AppendSection(sb, $"DESKTOP: {Path.GetFileName(Configuration.DesktopFile)}", ExpandedDesktop);

        if (verbose)
        {
            AppendSection(sb, $"CHANGELOG: {Path.GetFileName(Configuration.AppChangeFile)}", Builder.ChangeLog.ToString());

            var temp = new StringBuilder();

            foreach (var item in Builder.IconPaths)
            {
                // IconPaths are fully qualified - make appear relative to root
                // for info otherwise the full path could be long and cryptic
                temp.AppendLine(Path.GetRelativePath(Builder.BuildRoot, item.Value));
            }

            if (Builder.DesktopBuildPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Builder.BuildRoot, Builder.DesktopBuildPath));
            }

            if (Builder.MetaBuildPath != null)
            {
                temp.AppendLine(Path.GetRelativePath(Builder.BuildRoot, Builder.MetaBuildPath));
            }

            AppendSection(sb, "DEPLOY ASSETS", temp.ToString().TrimEnd(), true);
            AppendSection(sb, $"METAINFO: {Path.GetFileName(Configuration.MetaFile)}", ExpandedMetaInfo);
            AppendSection(sb, $"MANIFEST: {Path.GetFileName(Builder.ManifestBuildPath)}", Builder.ManifestContent?.TrimEnd());

            AppendSection(sb, "MACROS", Macros.ToString(false, false), true);
            sb.AppendLine();
            sb.AppendLine("NB. Macros with XML content are not listed above.");
        }

        string? proj = Path.GetFileName(Configuration.DotnetProjectPath);

        if (!string.IsNullOrEmpty(proj))
        {
            proj = ": " + proj;
        }

        AppendSection(sb, $"BUILD PROJECT{proj}", PublishCommands);

        AppendSection(sb, $"BUILD PACKAGE: {Builder.OutputName}", Builder.PackageCommands);
        AppendSection(sb, "ISSUES", Builder.WarningSink, true);

        return sb.ToString().Trim();
    }

    private static void AppendHeader(StringBuilder sb, string title, bool spacer = true)
    {
        if (sb.Length != 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine(new string('=', 60));
        sb.AppendLine(title);
        sb.AppendLine(new string('=', 60));

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

    private static void AppendSection(StringBuilder sb, string title, ICollection<string> content, bool alwaysShow = false)
    {
        AppendSection(sb, title, (IReadOnlyCollection<string>)content, alwaysShow);
    }

    private static void AppendSection(StringBuilder sb, string title, IReadOnlyCollection<string> content, bool alwaysShow = false)
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
        else
        if (alwaysShow)
        {
            AppendHeader(sb, title);
            sb.AppendLine("NONE");
        }
    }

    private static void AppendSection(StringBuilder sb, string title, string? content, bool alwaysShow = false)
    {
        if (alwaysShow || !string.IsNullOrEmpty(content))
        {
            AppendHeader(sb, title);
            sb.AppendLine(!string.IsNullOrEmpty(content) ? content : "[NONE]");
        }
    }

    private static string GetDotnetHost()
    {
        // Locate dotnet
        // https://github.com/dotnet/docs/blob/main/docs/core/tools/dotnet-environment-variables.md#dotnet_host_path
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

        if (string.IsNullOrEmpty(dotnet))
        {
            // In path (default)
            return "dotnet";
        }

        return dotnet;
    }

    private static List<string> GetPublishCommands(PackageBuilder builder)
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
                sb.Append($"{DotnetHost} clean");

                if (!string.IsNullOrEmpty(conf.DotnetProjectPath) && conf.DotnetProjectPath != ".")
                {
                    sb.Append($" \"{conf.DotnetProjectPath}\"");
                }

                list.Add(sb.ToString());
                sb.Clear();
            }

            // PUBLISH
            sb.Append($"{DotnetHost} publish");
            var pa = conf.DotnetPublishArgs;

            if (!string.IsNullOrEmpty(conf.DotnetProjectPath) && conf.DotnetProjectPath != ".")
            {
                sb.Append($" \"{conf.DotnetProjectPath}\"");
            }

            if (pa != null)
            {
                // Cannot allow
                if (pa.Contains("-o ") || pa.Contains("--output "))
                {
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
                sb.Append(' ');
                var prop = conf.Arguments.Property;
                bool quoted = prop.Contains('"');

                if (!quoted)
                {
                    // See: https://github.com/dotnet/sdk/issues/9562
                    sb.Append('"');
                    prop = conf.Arguments.Property.Replace(",", "%2C");
                }

                sb.Append('-');

                if (!conf.Arguments.Property.StartsWith("p:"))
                {
                    sb.Append("p:");
                }

                sb.Append(prop);

                if (!quoted)
                {
                    sb.Append('"');
                }
            }

            if (!string.IsNullOrEmpty(pa))
            {
                sb.Append(' ');
                sb.Append(pa);
            }

            sb.Append(" -o \"");
            sb.Append(builder.BuildAppBin);
            sb.Append('"');

            list.Add(sb.ToString());
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (conf.DotnetPostPublishOnWindows != null)
            {
                list.Add(conf.DotnetPostPublishOnWindows);
            }
        }
        else
        if (conf.DotnetPostPublish != null)
        {
            list.Add(conf.DotnetPostPublish);
        }

        return list;
    }
}

