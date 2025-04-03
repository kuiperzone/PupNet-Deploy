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

namespace KuiperZone.PupNet;

/// <summary>
/// Reads and decodes the configuration file. Changes the working directory to that of the config file.
/// </summary>
public class ConfigurationReader
{
    /// <summary>
    /// Explicitly no path.
    /// </summary>
    public const string PathDisable = "NONE";

    /// <summary>
    /// Default constructor. No arguments. Test or demo only.
    /// </summary>
    public ConfigurationReader(bool examples = false, string? metabase = null)
        : this(PackageKind.AppImage, examples, metabase)
    {
    }

    /// <summary>
    /// Constructor with kind using for test or demo only.
    /// </summary>
    public ConfigurationReader(PackageKind kind, bool examples, string? metabase = null)
    {
        // Used default options. Useful in unit test
        // generating example reference information.
        Arguments = new($"-{ArgumentReader.KindShortArg} {kind}");
        Reader = new();
        LocalDirectory = "";
        PackageName = AppBaseName;
        DotnetProjectPath = "";
        PupnetVersion = Program.Version;

        if (examples)
        {
            AppDescription = Array.Empty<string>();
            AppLicenseFile = "LICENSE.txt";
            AppChangeFile = "CHANGELOG.txt";

            PublisherCopyright = "Copyright (C) Acme Ltd 2024";
            PublisherLinkName = "Project Page";
            PublisherLinkUrl = "https://example.net";
            PublisherEmail = "contact@example.net";
            DesktopFile = "Deploy/app.desktop";
            PrimeCategory = "Utility";
            StartCommand = "helloworld";
            MetaFile = "Deploy/app.metainfo.xml";
            IconFiles = new string[] { "app.ico", "app.svg", "app.16x16.png", "app.32x32.png", "app.64x64.png" };
            DotnetProjectPath = "Source";
            DotnetPostPublish = "post-publish.sh";
            DotnetPostPublishOnWindows = "post-publish.bat";
            PackageName = "HelloWorld";
            AppImageArgs = "--sign";
            FlatpakBuilderArgs = "--gpg-keys=FILE";
            SetupGroupName = "HelloWorld Demo";
            SetupCommandPrompt = "Command Prompt";
            SetupSuffixOutput = "Setup";
            SetupVersionOutput = true;
            SetupUninstallScript = "uninstall.bat";
        }

        if (!string.IsNullOrEmpty(metabase))
        {
            DesktopFile = metabase + Program.DesktopExt;
            MetaFile = metabase + Program.MetaExt;
        }
    }

    /// <summary>
    /// Constructor with supplied content. For unit test only.
    /// </summary>
    public ConfigurationReader(ArgumentReader args, string[] content, bool assertPaths = false)
        : this(args, new IniReader(content), assertPaths)
    {
    }

    /// <summary>
    /// Production constructor where the file path is either given as args.Value or a single file in the current
    /// working directory. If assertPaths is true, it ensures that all reference files exist and populates
    /// property values with fully qualified names. Where relative paths are given, these are treated as relative
    /// to this file location.
    /// </summary>
    public ConfigurationReader(ArgumentReader args, bool assertPaths = true)
        : this(args, new IniReader(GetConfOrDefault(args.Value)), assertPaths)
    {
    }

    /// <summary>
    /// Production constructor in which the conf file path is given explicitly.
    /// </summary>
    public ConfigurationReader(ArgumentReader args, string confPath, bool assertPaths = true)
        : this(args, new IniReader(confPath), assertPaths)
    {
    }

    private ConfigurationReader(ArgumentReader args, IniReader reader, bool assertPaths)
    {
        Arguments = args;
        Reader = reader;
        AssertPaths = assertPaths;
        LocalDirectory = assertPaths ? Path.GetDirectoryName(reader.Filepath)! : "";

        AppBaseName = GetMandatory(nameof(AppBaseName), ValueFlags.SafeNoSpace);
        AppFriendlyName = GetMandatory(nameof(AppFriendlyName), ValueFlags.Safe);
        AppId = GetMandatory(nameof(AppId), ValueFlags.StrictSafe);
        AppVersionRelease = GetMandatory(nameof(AppVersionRelease), ValueFlags.SafeNoSpace);
        AppShortSummary = GetMandatory(nameof(AppShortSummary), ValueFlags.None);
        AppDescription = GetCollection(nameof(AppDescription), ValueFlags.MultiText);
        AppLicenseId = GetMandatory(nameof(AppLicenseId), ValueFlags.Safe);
        AppLicenseFile = GetOptional(nameof(AppLicenseFile), ValueFlags.AssertPath);
        AppChangeFile = GetOptional(nameof(AppChangeFile), ValueFlags.AssertPath);

        PublisherName = GetMandatory(nameof(PublisherName), ValueFlags.Safe);
        PublisherCopyright = GetOptional(nameof(PublisherCopyright), ValueFlags.Safe);
        PublisherLinkName = GetOptional(nameof(PublisherLinkName), ValueFlags.Safe);
        PublisherLinkUrl = GetOptional(nameof(PublisherLinkUrl), ValueFlags.Safe);
        PublisherEmail = GetOptional(nameof(PublisherEmail), ValueFlags.None);

        DesktopNoDisplay = GetBool(nameof(DesktopNoDisplay), DesktopNoDisplay);
        DesktopTerminal = GetBool(nameof(DesktopTerminal), DesktopTerminal);
        DesktopFile = GetOptional(nameof(DesktopFile), ValueFlags.AssertPathWithDisable);
        StartCommand = GetOptional(nameof(StartCommand), ValueFlags.StrictSafe);
        PrimeCategory = GetOptional(nameof(PrimeCategory), ValueFlags.StrictSafe);
        MetaFile = GetOptional(nameof(MetaFile), ValueFlags.AssertPathWithDisable);
        IconFiles = GetCollection(nameof(IconFiles), ValueFlags.AssertPath);

        if (args.Project != null)
        {
            DotnetProjectPath = AssertPathFlags(nameof(DotnetProjectPath), args.Project, ValueFlags.AssertPathWithDisable);
        }
        else
        {
            DotnetProjectPath = GetOptional(nameof(DotnetProjectPath), ValueFlags.AssertPathWithDisable) ?? LocalDirectory;
        }

        DotnetPublishArgs = GetOptional(nameof(DotnetPublishArgs), ValueFlags.None);
        DotnetPostPublish = GetOptional(nameof(DotnetPostPublish), ValueFlags.AssertPath);
        DotnetPostPublishOnWindows = GetOptional(nameof(DotnetPostPublishOnWindows), ValueFlags.AssertPath);

        PackageName = GetOptional(nameof(PackageName), ValueFlags.StrictSafe) ?? AppBaseName;
        OutputDirectory = GetOptional(nameof(OutputDirectory), ValueFlags.Path) ?? LocalDirectory;

        AppImageArgs = GetOptional(nameof(AppImageArgs), ValueFlags.None);
        AppImageVersionOutput = GetBool(nameof(AppImageVersionOutput), AppImageVersionOutput);

        RpmAutoReq = GetBool(nameof(RpmAutoReq), RpmAutoReq);
        RpmAutoProv = GetBool(nameof(RpmAutoProv), RpmAutoProv);
        RpmGroup = GetOptional(nameof(RpmGroup), ValueFlags.None);
        RpmPre = GetCollection(nameof(RpmPre), ValueFlags.MultiText);
        RpmPost = GetCollection(nameof(RpmPost), ValueFlags.MultiText);
        RpmPreUn = GetCollection(nameof(RpmPreUn), ValueFlags.MultiText);
        RpmPostUn = GetCollection(nameof(RpmPostUn), ValueFlags.MultiText);
        RpmRequires = GetCollection(nameof(RpmRequires), RpmRequires, ValueFlags.SafeNoSpace);

        DebianRecommends = GetCollection(nameof(DebianRecommends), DebianRecommends, ValueFlags.SafeNoSpace);
        DebianPreInst = GetCollection(nameof(DebianPreInst), DebianPreInst, ValueFlags.MultiText);
        DebianPostInst = GetCollection(nameof(DebianPostInst), DebianPostInst, ValueFlags.MultiText);
        DebianPreRm = GetCollection(nameof(DebianPreRm), DebianPreRm, ValueFlags.MultiText);
        DebianPostRm = GetCollection(nameof(DebianPostRm), DebianPostRm, ValueFlags.MultiText);

        FlatpakPlatformRuntime = GetMandatory(nameof(FlatpakPlatformRuntime), ValueFlags.StrictSafe);
        FlatpakPlatformSdk = GetMandatory(nameof(FlatpakPlatformSdk), ValueFlags.StrictSafe);
        FlatpakPlatformVersion = GetMandatory(nameof(FlatpakPlatformVersion), ValueFlags.StrictSafe);
        FlatpakBuilderArgs = GetOptional(nameof(FlatpakBuilderArgs), ValueFlags.None);
        FlatpakFinishArgs = GetCollection(nameof(FlatpakFinishArgs), ValueFlags.None, "=", "--");

        SetupGroupName = GetOptional(nameof(SetupGroupName), ValueFlags.Safe);
        SetupAdminInstall = GetBool(nameof(SetupAdminInstall), SetupAdminInstall);
        SetupCommandPrompt = GetOptional(nameof(SetupCommandPrompt), ValueFlags.Safe);
        SetupMinWindowsVersion = GetMandatory(nameof(SetupMinWindowsVersion), ValueFlags.StrictSafe);
        SetupSignTool = GetOptional(nameof(SetupSignTool), ValueFlags.None);
        SetupSuffixOutput = GetOptional(nameof(SetupSuffixOutput), ValueFlags.SafeNoSpace);
        SetupVersionOutput = GetBool(nameof(SetupVersionOutput), SetupVersionOutput);
        SetupUninstallScript = GetOptional(nameof(SetupUninstallScript), ValueFlags.None);

        // Not actually a key-value, but a comment
        PupnetVersion = ExtractVersion(reader.ToString());
    }

    [Flags]
    private enum ValueFlags
    {
        None = 0x0000,
        MultiItems = 0x0001,
        MultiText = MultiItems | 0x0002,
        Safe = 0x0004,
        SafeNoSpace = Safe | 0x0008,
        StrictSafe = SafeNoSpace | 0x0010,
        Path = Safe | 0x0020,
        AssertPath = Path | 0x0040,
        PathWithDisable = Path | 0x0080,
        AssertPathWithDisable = AssertPath | PathWithDisable,
    };

    /// <summary>
    /// Gets the underlying ini values.
    /// </summary>
    public IniReader Reader { get; }

    /// <summary>
    /// Gets a reference to the arguments used on construction.
    /// </summary>
    public ArgumentReader Arguments { get; }

    /// <summary>
    /// Gets whether to assert paths exist. Only false for unit testing.
    /// </summary>
    public bool AssertPaths { get; }

    /// <summary>
    /// Gets the configuration file local directory. Empty if <see cref="AssertPaths"/> is false.
    /// </summary>
    public string LocalDirectory { get; }

    public string AppBaseName { get; } = "HelloWorld";
    public string AppFriendlyName { get; } = "Hello World";
    public string AppId { get; } = "net.example.helloworld";
    public string AppVersionRelease { get; } = "1.0.0[1]";
    public string AppShortSummary { get; } = "A HelloWorld application";
    public IReadOnlyCollection<string> AppDescription { get; } = new string[]
        { "HelloWorld is a really friendly application.", "It says \"Hello\" and supports an additional \"Goodbye\" feature.", "", "It is the only app you'll ever need." };
    public string AppLicenseId { get; } = "LicenseRef-Proprietary";
    public string? AppLicenseFile { get; }
    public string? AppChangeFile { get; }

    public string PublisherName { get; } = "The Hello World Team";
    public string? PublisherCopyright { get; } = "Copyright (C) Hello World Team 2024";
    public string? PublisherLinkName { get; } = "Home Page";
    public string? PublisherLinkUrl { get; }
    public string? PublisherEmail { get; }

    public bool DesktopNoDisplay { get; }
    public bool DesktopTerminal { get; } = true;
    public string? DesktopFile { get; }
    public string? StartCommand { get; }
    public string? PrimeCategory { get; }
    public string? MetaFile { get; }
    public IReadOnlyCollection<string> IconFiles { get; } = Array.Empty<string>();

    public string DotnetProjectPath { get; }
    public string? DotnetPublishArgs { get; } = $"-p:Version={MacroId.AppVersion.ToVar()} --self-contained true -p:DebugType=None -p:DebugSymbols=false";
    public string? DotnetPostPublish { get; }
    public string? DotnetPostPublishOnWindows { get; }

    public string PackageName { get; }
    public string OutputDirectory { get; } = "Deploy/OUT";

    public string? AppImageArgs { get; }
    public bool AppImageVersionOutput { get; }

    public string FlatpakPlatformRuntime { get; } = "org.freedesktop.Platform";
    public string FlatpakPlatformSdk { get; } = "org.freedesktop.Sdk";
    public string FlatpakPlatformVersion { get; } = "23.08";
    public IReadOnlyCollection<string> FlatpakFinishArgs { get; } = new string[]
        { "--socket=wayland", "--socket=x11", "--filesystem=host", "--share=network" };
    public string? FlatpakBuilderArgs { get; }

    public bool RpmAutoReq { get; } = false;
    public bool RpmAutoProv { get; } = true;
    public string? RpmGroup { get; }

    public IReadOnlyCollection<string> RpmPre { get; } = Array.Empty<string>();
    public IReadOnlyCollection<string> RpmPost { get; } = Array.Empty<string>();
    public IReadOnlyCollection<string> RpmPreUn { get; } = Array.Empty<string>();
    public IReadOnlyCollection<string> RpmPostUn { get; } = Array.Empty<string>(); 
    
    public IReadOnlyCollection<string> RpmRequires { get; } = new string[]
        { "krb5-libs", "libicu", "openssl-libs", "zlib" };

    public IReadOnlyCollection<string> DebianRecommends { get; } = new string[]
        { "libc6", "libgcc1", "libgcc-s1", "libgssapi-krb5-2", "libicu", "libssl", "libstdc++6", "libunwind", "zlib1g" };
    
    public IReadOnlyCollection<string> DebianPreInst { get; } = Array.Empty<string>();
    public IReadOnlyCollection<string> DebianPostInst { get; } = Array.Empty<string>();
    public IReadOnlyCollection<string> DebianPreRm { get; } = Array.Empty<string>();
    public IReadOnlyCollection<string> DebianPostRm { get; } = Array.Empty<string>(); 

    public string? SetupGroupName { get; }
    public bool SetupAdminInstall { get; }
    public string? SetupCommandPrompt { get; }
    public string SetupMinWindowsVersion { get; } = "10";
    public string? SetupSignTool { get; }
    public string? SetupSuffixOutput { get; }
    public bool SetupVersionOutput { get; }
    public string? SetupUninstallScript { get; }

    public string? PupnetVersion { get; }

    /// <summary>
    /// Ensures that the result is a valid pupnet.conf file. The result is 'path' if the value is not null or empty.
    /// Otherwise, if null, the method looks in the current working directory for a single 'pupnet.conf' file.
    /// If not file is found, it throws.
    /// </summary>
    /// <exception cref="FileNotFoundException"/>
    public static string GetConfOrDefault(string? path)
    {
        var dir = "./";

        if (!string.IsNullOrEmpty(path))
        {
            if (!Directory.Exists(path))
            {
                return path;
            }

            // Exists as a directory
            dir = path;
        }

        var files = Directory.GetFiles(dir, "*" + Program.ConfExt, SearchOption.TopDirectoryOnly);

        if (files.Length == 1)
        {
            return files[0];
        }

        throw new FileNotFoundException($"Specify {Program.ConfExt} file (otherwise directory must contain exactly one file with {Program.ConfExt} extension)");
    }

    /// <summary>
    /// Reads file associated with this configuration and returns the text. Returns null if path is null or
    /// equals <see cref="PathDisable"/>. If path is unqualified, then relative to <see cref="LocalDirectory"/>.
    /// Throws if file not exist and <see cref="AssertPaths"/> is true.
    /// </summary>
    public string? ReadAssociatedFile(string? path)
    {
        if (path != null && path != PathDisable && (AssertPaths || File.Exists(path)))
        {
            // Force linux style
            var content = File.ReadAllText(path).Trim().ReplaceLineEndings("\n");

            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("File is empty " + path);
            }

            return content;
        }

        return null;
    }

    /// <summary>
    /// Overrides and writes contents without comments.
    /// </summary>
    public override string ToString()
    {
        return ToString(DocStyles.NoComments);
    }

    /// <summary>
    /// Writes contents with comments if verbose.
    /// </summary>
    public string ToString(DocStyles style)
    {
        // Convenience
        string macroHelp = $"'{Program.CommandName} --{ArgumentReader.HelpLongArg} macro'";

        var sb = new StringBuilder();

        if (style != DocStyles.Reference)
        {
            sb.Append(CreateBreaker($"{Program.ProductName.ToUpperInvariant()}: {Program.Version}", style, true));

            if (style == DocStyles.NoComments)
            {
                sb.AppendLine($"# Use: '{Program.CommandName} --{ArgumentReader.HelpLongArg} conf' for information.");
            }
        }

        sb.Append(CreateBreaker("APP PREAMBLE", style));

        sb.Append(CreateHelpField(nameof(AppBaseName), AppBaseName, style,
                $"Mandatory application base name. This MUST BE the base name of the main executable file. It should NOT",
                $"include any directory part or extension, i.e. do not append '.exe' or '.dll'. It should not contain",
                $"spaces or invalid filename characters."));

        sb.Append(CreateHelpField(nameof(AppFriendlyName), AppFriendlyName, style,
                $"Mandatory application friendly name."));

        sb.Append(CreateHelpField(nameof(AppId), AppId, style,
                $"Mandatory application ID in reverse DNS form. This should stay constant for lifetime of the software."));

        sb.Append(CreateHelpField(nameof(AppVersionRelease), AppVersionRelease, style,
                $"Mandatory application version and package release of form: 'VERSION[RELEASE]'. Use optional square",
                $"brackets to denote package release, i.e. '1.2.3[1]'. Release refers to a change to the deployment",
                $"package, rather the application. If release part is absent (i.e. '1.2.3'), the release value defaults",
                $"to '1'. Note that the version-release value given here may be overridden from the command line."));

        sb.Append(CreateHelpField(nameof(AppShortSummary), AppShortSummary, style,
                $"Mandatory single line application summary text in default (English) language."));

        sb.Append(CreateHelpField(nameof(AppDescription), AppDescription, true, style,
                $"Multi-line (surround with triple \"\"\" quotes) application description which provides longer explanation",
                $"than {nameof(AppShortSummary)} in default language. Optional but it is recommended to specify this. Text",
                $"separated by an empty line will be treated as separate paragraphs. Avoid complex formatting, and do not",
                $"use HTML or markdown, other than list items begining with \"* \", \"+ \" or \"- \". This content is",
                $"used by package builders where supported, including RPM and DEB, and is used to populate the",
                $"{MacroId.AppStreamDescriptionXml.ToVar()} element used within AppStream metadata."));

        sb.Append(CreateHelpField(nameof(AppLicenseId), AppLicenseId, style,
                $"Mandatory application license ID. This should be one of the recognized SPDX license",
                $"identifiers, such as: 'MIT', 'GPL-3.0-or-later' or 'Apache-2.0'. For a proprietary or",
                $"custom license, use 'LicenseRef-Proprietary' or 'LicenseRef-LICENSE'."));

        sb.Append(CreateHelpField(nameof(AppLicenseFile), AppLicenseFile, style,
                $"Optional path to application copyright/license text file. If provided, it will be packaged with the",
                $"application and used with package builders where supported."));

        sb.Append(CreateHelpField(nameof(AppChangeFile), AppChangeFile, style,
                $"Optional path to application changelog file. IMPORTANT. If given, this file should contain version",
                $"information in a predefined format. Namely, it should contain one or more version headings of form:",
                $"'+ VERSION;DATE', under which are to be listed change items of form: '- Change description'. Formatted",
                $"information will be parsed and used to expand the {MacroId.AppStreamChangelogXml.ToVar()} macro used",
                $"for AppStream metadata (superfluous text is ignored, so the file may also contain README information).",
                $"The given file will also be packaged with the application verbatim. See: {Program.ProjectUrl}."));


        sb.Append(CreateBreaker("PUBLISHER", style));

        sb.Append(CreateHelpField(nameof(PublisherName), PublisherName, style,
                $"Mandatory publisher, group or creator."));

        sb.Append(CreateHelpField(nameof(PublisherCopyright), PublisherCopyright, style,
                $"Optional copyright statement."));

        sb.Append(CreateHelpField(nameof(PublisherLinkName), PublisherLinkName, style,
                $"Optional publisher or application web-link name. Note that Windows {nameof(PackageKind.Setup)} packages",
                $"require both {nameof(PublisherLinkName)} and {nameof(PublisherLinkUrl)} in order to include the link as",
                $"an item in program menu entries. Do not modify name, as may leave old entries in updated installations."));

        sb.Append(CreateHelpField(nameof(PublisherLinkUrl), PublisherLinkUrl, style,
                $"Publisher or application web-link URL. Although optional, it should be considered mandatory if using",
                $"{nameof(MetaFile)}"));

        sb.Append(CreateHelpField(nameof(PublisherEmail), PublisherEmail, style,
                $"Publisher or maintainer email contact. Although optional, some package builders (i.e. DEB) require it",
                $"and may warn or fail unless provided."));



        sb.Append(CreateBreaker("DESKTOP INTEGRATION", style));

        sb.Append(CreateHelpField(nameof(DesktopNoDisplay), DesktopNoDisplay, style,
                $"Boolean (true or false) which indicates whether the application is hidden on the desktop. It is used to",
                $"populate the 'NoDisplay' field of the .desktop file. The default is false. Setting to true will also",
                $"cause the main application start menu entry to be omitted for Windows {nameof(PackageKind.Setup)}."));

        sb.Append(CreateHelpField(nameof(DesktopTerminal), DesktopTerminal, style,
                $"Boolean (true or false) which indicates whether the application runs in the terminal, rather than",
                $"providing a GUI. It is used to populate the 'Terminal' field of the .desktop file."));

        sb.Append(CreateHelpField(nameof(DesktopFile), DesktopFile, style,
                $"Optional path to a Linux desktop file. If empty (default), one will be generated automatically from",
                $"the information in this file. Supplying a custom file, however, allows for mime-types and",
                $"internationalisation. If supplied, the file MUST contain the line: 'Exec={MacroId.InstallExec.ToVar()}'",
                $"in order to use the correct install location. Other macros may be used to help automate the content.",
                $"Note. {Program.ProductName} can generate you a desktop file. Use --help and {macroHelp} for reference.",
                $"See: https://specifications.freedesktop.org/desktop-entry-spec/desktop-entry-spec-latest.html"));

        sb.Append(CreateHelpField(nameof(StartCommand), StartCommand, style,
                $"Optional command name to start the application from the terminal. If, for example, {nameof(AppBaseName)} is",
                $"'Zone.Kuiper.HelloWorld', the value here may be set to a simpler and/or lower-case variant such as",
                $"'helloworld'. It must not contain spaces or invalid filename characters. Do not add any extension such",
                $"as '.exe'. If empty, the application will not be in the path and cannot be started from the command line.",
                $"For Windows {nameof(PackageKind.Setup)} packages, see also {nameof(SetupCommandPrompt)}. {nameof(StartCommand)} is not",
                $"supported for all packages kinds (i.e. Flatpak). Default is empty (none)."));

        sb.Append(CreateHelpField(nameof(PrimeCategory), PrimeCategory, style,
                $"Optional category for the application. The value should be one of the recognized Freedesktop top-level",
                $"categories, such as: Audio, Development, Game, Office, Utility etc. Only a single value should be",
                $"provided here which will be used, where supported, to populate metadata. The default is empty.",
                $"See: https://specifications.freedesktop.org/menu-spec/latest/apa.html"));

        sb.Append(CreateHelpField(nameof(MetaFile), MetaFile, style,
                $"Path to AppStream metadata file. It is optional, but recommended as it is used by software centers.",
                $"Note. The contents of the files may use macro variables. Use {macroHelp} for reference.",
                $"See: https://docs.appimage.org/packaging-guide/optional/appstream.html"));

        sb.Append(CreateHelpField(nameof(IconFiles), IconFiles, true, style,
                $"Optional icon file paths. The value may include multiple filenames separated with semicolon or given",
                $"in multi-line form. Valid types are SVG, PNG and ICO (ICO ignored on Linux). Note that the inclusion",
                $"of a scalable SVG is preferable on Linux, whereas PNGs must be one of the standard sizes and MUST",
                $"include the size in the filename in the form: name.32x32.png' or 'name.32.png'."));



        sb.Append(CreateBreaker("DOTNET PUBLISH", style));

        sb.Append(CreateHelpField(nameof(DotnetProjectPath), DotnetProjectPath, style,
                $"Optional path relative to this file in which to find the dotnet project (.csproj) file, or the",
                $"directory containing it. If empty (default), a single project file is expected under the same",
                $"directory as this file. IMPORTANT. If set to '{PathDisable}', dotnet publish is disabled",
                $"(i.e. not called). Instead, only {nameof(DotnetPostPublish)} is called."));

        sb.Append(CreateHelpField(nameof(DotnetPublishArgs), DotnetPublishArgs, style,
                $"Optional arguments supplied to 'dotnet publish'. Do NOT include '-r' (runtime), or '-c' (configuration)",
                $"here as they will be added according to command line arguments. Typically you want as a minimum:",
                $"'-p:Version={MacroId.AppVersion.ToVar()} --self-contained true'. Additional useful arguments include:",
                $"'-p:DebugType=None -p:DebugSymbols=false -p:PublishSingleFile=true -p:PublishReadyToRun=true",
                $"-p:PublishTrimmed=true -p:TrimMode=link'. Note. This value may use macro variables. Use {macroHelp}",
                $"for reference. See: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish"));

        sb.Append(CreateHelpField(nameof(DotnetPostPublish), DotnetPostPublish, style,
                $"Post-publish (or standalone build) command on Linux (ignored on Windows). It is called after dotnet",
                $"publish, but before the final output is built. This could, for example, be a script which copies",
                $"additional files into the build directory given by {MacroId.BuildAppBin.ToVar()}. The working directory will be",
                $"the location of this file. This value is optional, but becomes mandatory if {nameof(DotnetProjectPath)} equals",
                $"'{PathDisable}'. Note. This value may use macro variables. Additionally, scripts may use these as environment",
                $"variables. Use {macroHelp} for reference."));

        sb.Append(CreateHelpField(nameof(DotnetPostPublishOnWindows), DotnetPostPublishOnWindows, style,
                $"Post-publish (or standalone build) command on Windows (ignored on Linux). This should perform",
                $"the equivalent operation, as required, as {nameof(DotnetPostPublish)}, but using DOS commands and batch",
                $"scripts. Multiple commands may be specified, separated by semicolon or given in multi-line form.",
                $"Note. This value may use macro variables. Additionally, scripts may use these as environment",
                $"variables. Use {macroHelp} for reference."));



        sb.Append(CreateBreaker("PACKAGE OUTPUT", style));

        sb.Append(CreateHelpField(nameof(PackageName), PackageName, style,
                $"Optional package name (excludes version etc.). If empty, defaults to {nameof(AppBaseName)}. However, it is",
                $"used not only to specify the base output filename, but to identify the application in DEB and RPM",
                $"packages. You may wish, therefore, to ensure that the value represents a unique name. Naming",
                $"requirements are strict and must contain only alpha-numeric and '-', '+' and '.' characters."));

        sb.Append(CreateHelpField(nameof(OutputDirectory), OutputDirectory, style,
                $"Output directory, or subdirectory relative to this file. It will be created if it does not exist and",
                $"will contain the final deploy output files. If empty, it defaults to the location of this file."));



        sb.Append(CreateBreaker("APPIMAGE OPTIONS", style));

        sb.Append(CreateHelpField(nameof(AppImageArgs), AppImageArgs, style,
                $"Additional arguments for use with appimagetool. Useful for signing. Default is empty."));

        sb.Append(CreateHelpField(nameof(AppImageVersionOutput), AppImageVersionOutput, style,
                $"Boolean (true or false) which sets whether to include the application version in the AppImage filename,",
                $"i.e. 'HelloWorld-1.2.3-x86_64.AppImage'. Default is false. It is ignored if the output filename is",
                $"specified at command line."));



        sb.Append(CreateBreaker("FLATPAK OPTIONS", style));

        sb.Append(CreateHelpField(nameof(FlatpakPlatformRuntime), FlatpakPlatformRuntime, style,
                $"The runtime platform. Invariably for .NET (inc. Avalonia), this should be 'org.freedesktop.Platform'.",
                $"Refer: https://docs.flatpak.org/en/latest/available-runtimes.html"));

        sb.Append(CreateHelpField(nameof(FlatpakPlatformSdk), FlatpakPlatformSdk, style,
                $"The platform SDK. Invariably for .NET (inc. Avalonia applications) this should be 'org.freedesktop.Sdk'.",
                $"The SDK must be installed on the build system."));

        sb.Append(CreateHelpField(nameof(FlatpakPlatformVersion), FlatpakPlatformVersion, style,
                $"The platform runtime version. The latest available version may change periodically.",
                $"Refer to Flatpak documentation."));

        sb.Append(CreateHelpField(nameof(FlatpakFinishArgs), FlatpakFinishArgs, true, style,
                $"Flatpak manifest 'finish-args' sandbox permissions. Optional, but if empty, the application will have",
                $"extremely limited access to the host environment. This option may be used to grant required",
                $"application permissions. Values here should be prefixed with '--' and separated by semicolon or given",
                $"in multi-line form. Refer: https://docs.flatpak.org/en/latest/sandbox-permissions.html"));

        sb.Append(CreateHelpField(nameof(FlatpakBuilderArgs), FlatpakBuilderArgs, style,
                $"Additional arguments for use with flatpak-builder. Useful for signing. Default is empty.",
                $"See flatpak-builder --help."));



        sb.Append(CreateBreaker("RPM OPTIONS", style));

        sb.Append(CreateHelpField(nameof(RpmAutoReq), RpmAutoReq, style,
                $"Boolean (true or false) which specifies whether to build the RPM package with 'AutoReq' equal to yes or no.",
                $"For dotnet application, the value should typically be false, but see RpmRequires below.",
                $"Refer: https://rpm-software-management.github.io/rpm/manual/spec.html"));

        sb.Append(CreateHelpField(nameof(RpmAutoProv), RpmAutoProv, style,
                $"Boolean (true or false) which specifies whether to build the RPM package with 'AutoProv' equal to yes or no.",
                $"Refer: https://rpm-software-management.github.io/rpm/manual/spec.html"));

        sb.Append(CreateHelpField(nameof(RpmGroup), RpmGroup, style,
                $"The specified group must be in the list of groups known to RPM. This list is located in the file /usr/lib/rpm/GROUPS",
                $"which is part of the rpm package."));

        sb.Append(CreateHelpField(nameof(RpmPre), RpmPre, true, style, 
            "The script is executed before the package is installed into the system."));
        
        sb.Append(CreateHelpField(nameof(RpmPost), RpmPost, true, style, 
            "The script is executed after the package is installed into the system."));
        
        sb.Append(CreateHelpField(nameof(RpmPreUn), RpmPreUn, true, style, 
            "The script is executed before the package is removed from the system."));
        
        sb.Append(CreateHelpField(nameof(RpmPostUn), RpmPostUn, true, style, 
            "The script is executed after the package is removed from the system."));
            
        sb.Append(CreateHelpField(nameof(RpmRequires), RpmRequires, true, style,
                $"Optional list of RPM dependencies. The list may include multiple values separated with semicolon or given",
                $"in multi-line form. If empty, a self-contained dotnet package will successfully run on many (but not all)",
                $"Linux distros. In some cases, it will be necessary to explicitly specify additional dependencies.",
                $"Default values are recommended for use with dotnet and RPM packages at the time of writing.",
                $"For updated information, see: https://learn.microsoft.com/en-us/dotnet/core/install/linux-rhel#dependencies"));

        sb.Append(CreateBreaker("DEBIAN OPTIONS", style));
        
        sb.Append(CreateHelpField(nameof(DebianRecommends), DebianRecommends, true, style,
                $"Optional list of Debian dependencies. The list may include multiple values separated with semicolon or given",
                $"in multi-line form. If empty, a self-contained dotnet package will successfully run on many (but not all)",
                $"Linux distros. In some cases, it will be necessary to explicitly specify additional dependencies.",
                $"Default values are recommended for use with dotnet and Debian packages at the time of writing.",
                $"For updated information, see: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#dependencies"));

        sb.Append(CreateHelpField(nameof(DebianPreInst), DebianPreInst, true, style, 
            "The script is executed before the package is installed into the system."));
        
        sb.Append(CreateHelpField(nameof(DebianPostInst), DebianPostInst, true, style, 
            "The script is executed after the package is installed into the system."));
        
        sb.Append(CreateHelpField(nameof(DebianPreRm), DebianPreRm, true, style, 
            "The script is executed before the package is removed from the system."));
        
        sb.Append(CreateHelpField(nameof(DebianPostRm), DebianPostRm, true, style, 
            "The script is executed after the package is removed from the system."));

        sb.Append(CreateBreaker("WINDOWS SETUP OPTIONS", style));

        sb.Append(CreateHelpField(nameof(SetupGroupName), SetupGroupName, style,
                $"Optional application group name used as the Start Menu folder and install directory under Program Files.",
                $"Specifically, it is used to define the InnoSetup DefaultGroupName and DefaultDirName parameters.",
                $"If empty (default), suitable values are used based on your application.", 
                $"See: https://jrsoftware.org/ishelp/index.php?topic=setup_defaultgroupname"));

        sb.Append(CreateHelpField(nameof(SetupAdminInstall), SetupAdminInstall, style,
                $"Boolean (true or false) which specifies whether the application is to be installed in administrative",
                $"mode, or per-user. Default is false. See: https://jrsoftware.org/ishelp/topic_admininstallmode.htm"));

        sb.Append(CreateHelpField(nameof(SetupCommandPrompt), SetupCommandPrompt, style,
                $"Optional command prompt title. The Windows installer will NOT add your application to the path. However,",
                $"if your package contains a command-line utility, setting this value will ensure that a 'Command Prompt'",
                $"program menu entry is added (with this title) which, when launched, will open a dedicated command",
                $"window with your application directory in its path. Default is empty. See also {nameof(StartCommand)}."));

        sb.Append(CreateHelpField(nameof(SetupMinWindowsVersion), SetupMinWindowsVersion, style,
                $"Mandatory value which specifies minimum version of Windows that your software runs on. Windows 8 = 6.2,",
                $"Windows 10/11 = 10. Default: 10. See: https://jrsoftware.org/ishelp/topic_setup_minversion.htm"));

        sb.Append(CreateHelpField(nameof(SetupSignTool), SetupSignTool, style,
                $"Optional name and parameters of the Sign Tool to be used to digitally sign: the installer,",
                $"uninstaller, and contained exe and dll files. If empty, files will not be signed.",
                $"See: https://jrsoftware.org/ishelp/topic_setup_signtool.htm"));

        sb.Append(CreateHelpField(nameof(SetupSuffixOutput), SetupSuffixOutput, style,
                $"Optional suffix for the installer output filename. The default is empty, but you may wish set it to:",
                $"'Setup' or similar. This, for example, will output a file of name: HelloWorldSetup-x86_64.exe",
                $"Ignored if the output filename is specified at command line."));

        sb.Append(CreateHelpField(nameof(SetupVersionOutput), SetupVersionOutput, style,
                $"Boolean (true or false) which sets whether to include the application version in the setup filename,",
                $"i.e. 'HelloWorld-1.2.3-x86_64.exe'. Default is false. Ignored if the output filename is specified",
                $"at command line."));
        
        sb.Append(CreateHelpField(nameof(SetupUninstallScript), SetupUninstallScript, style,
            $"Optional name of a script to run before uninstall.",
            $"This is script file relative to the directory of the application and must have a default file association.",
            $"This binds to the `[UninstallRun]` section of InnoSetup.",
            $"From this script, you may want to run your application, which is very useful if ",
            $"your application has non-static configurable storage locations for data.",
            $"Example: uninstall.bat"));

        return sb.ToString().Trim().ReplaceLineEndings("\n");
    }

    private static string? ExtractVersion(string content)
    {
        // This is written in header of ToString() method above
        string prefix = "# " + Program.ProductName.ToUpperInvariant();

        int p0 = content.IndexOf(prefix);

        if (p0 > -1)
        {
            p0 += prefix.Length;
            int p1 = content.IndexOf('\n', p0);

            if (p1 > p0)
            {
                // 012345678901234567890123
                // # PUPNET DEPLOY: 1.2.0n
                var version = content.Substring(p0, p1 - p0).Trim().TrimStart(' ', '=', ':');

                if (version.Length > 0 && version.Length < 16)
                {
                    return version;
                }
            }
        }

        return null;
    }

    private static string CreateBreaker(string title, DocStyles style, bool major = false)
    {
        var sb = new StringBuilder();

        sb.AppendLine();

        if (style != DocStyles.NoComments)
        {
            var b = new string('#', major ? 80 : 40);

            sb.AppendLine(b);
            sb.Append("# ");
            sb.AppendLine(title);

            sb.AppendLine(b);

            return sb.ToString();
        }

        sb.Append("# ");
        sb.AppendLine(title);
        return sb.ToString();
    }

    private static string CreateHelpField(string name, string? value, DocStyles style, params string[] help)
    {
        var pair = $"{name} = {value}";
        return CreateHelpFieldCore(name, pair, style, help);
    }

    private static string CreateHelpField(string name, bool value, DocStyles style, params string[] help)
    {
        var pair = $"{name} = {value.ToString().ToLowerInvariant()}";
        return CreateHelpFieldCore(name, pair, style, help);
    }

    private static string CreateHelpField(string name, IReadOnlyCollection<string> values, bool multi, DocStyles style, params string[] help)
    {
        var sb = new StringBuilder(name);
        sb.Append(" = ");

        if (multi && values.Count != 0)
        {
            sb.AppendLine(IniReader.StartMultiQuote);

            foreach (var item in values)
            {
                sb.Append("    ");
                sb.AppendLine(item);
            }

            sb.Append(IniReader.EndMultiQuote);
        }
        else
        {
            sb.Append(string.Join(';', values));
        }

        return CreateHelpFieldCore(name, sb.ToString(), style, help);
    }

    private static string CreateHelpFieldCore(string name, string pair, DocStyles style, IEnumerable<string> help)
    {
        var sb = new StringBuilder(256);

        if (style == DocStyles.Reference)
        {
            sb.AppendLine();
            sb.Append("** ");
            sb.Append(name);
            sb.AppendLine(" **");

            foreach (var item in help)
            {
                sb.AppendLine(item);
            }

            if (!pair.EndsWith('=') && !pair.EndsWith("= "))
            {
                sb.Append("Example: ");
                sb.AppendLine(pair);
            }

            return sb.ToString();
        }

        if (style == DocStyles.Comments)
        {
            sb.AppendLine();

            foreach (var item in help)
            {
                sb.Append("# ");
                sb.AppendLine(item);
            }

            sb.AppendLine(pair);
            return sb.ToString();
        }

        sb.AppendLine(pair);
        return sb.ToString();
    }

    private IReadOnlyCollection<string> GetCollection(string name, ValueFlags flags, string? mustContain = null, string? mustStart = null)
    {
        return GetCollection(name, Array.Empty<string>(), flags, mustContain, mustStart);
    }

    private IReadOnlyCollection<string> GetCollection(string name, IReadOnlyCollection<string> def, ValueFlags flags, string? mustContain = null, string? mustStart = null)
    {
        flags |= ValueFlags.MultiItems;
        var value = GetOptional(name, flags);

        if (value == null && Reader.Values.ContainsKey(name))
        {
            // Intentionally empty rather than default
            return Array.Empty<string>();
        }

        if (value != null)
        {
            if (flags.HasFlag(ValueFlags.MultiText))
            {
                // Accepts only '\n' separator
                // Removes first and last empty, and consecutive empties
                bool lastEmpty = true;
                var list = new List<string>(value.Split('\n', StringSplitOptions.TrimEntries));

                for (int n = 0; n < list.Count; ++n)
                {
                    bool currentEmpty = string.IsNullOrEmpty(list[n]);

                    if (currentEmpty && (lastEmpty || n == list.Count - 1))
                    {
                        list.RemoveAt(n--);
                    }

                    lastEmpty = currentEmpty;
                }

                return list;
            }
            else
            {
                // Accepts either '\n' or ';' separators. Always removes empty.
                char sep = value.Contains('\n') ? '\n' : ';';
                var list = new List<string>(value.Split(sep, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

                for (int n = 0; n < list.Count; ++n)
                {
                    // Does nothing if not Path flag
                    list[n] = AssertPathFlags(name, list[n], flags);

                    if (!string.IsNullOrEmpty(mustStart) && !list[n].StartsWith(mustStart))
                    {
                        throw new ArgumentException($"{name} items must start with {mustStart}");
                    }

                    if (!string.IsNullOrEmpty(mustContain) && !list[n].Contains(mustContain))
                    {
                        throw new ArgumentException($"{name} items must contain {mustContain}");
                    }
                }

                return list;
            }
        }

        return def;
    }

    private bool GetBool(string name, bool def)
    {
        var value = GetOptional(name, ValueFlags.None)?.Trim();

        if (value == null)
        {
            return def;
        }

        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new ArgumentException($"Use 'true' or 'false' only for Configuration {name}");
    }

    private string GetMandatory(string name, ValueFlags flags)
    {
        return GetOptional(name, flags) ??
            throw new ArgumentException($"Mandatory value required for Configuration {name}");
    }

    private string? GetOptional(string name, ValueFlags flags)
    {
        Reader.Values.TryGetValue(name, out string? value);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!flags.HasFlag(ValueFlags.MultiItems) && value.Contains('\n'))
        {
            throw new ArgumentException($"Use single line only for Configuration {name}");
        }

        if (value.Contains('\t'))
        {
            // Never allow
            throw new ArgumentException($"Configuration {name} contains illegal tab character");
        }

        if (flags.HasFlag(ValueFlags.Safe))
        {
            // File path safe
            const string Invalid = "*?\"<>|";

            foreach (var c in value)
            {
                if (flags.HasFlag(ValueFlags.SafeNoSpace) && char.IsWhiteSpace(c) && c != '\n' && c != '\r')
                {
                    throw new ArgumentException($"{name} must not contain spaces or non-printing characters");
                }

                if (c < ' ' || Invalid.Contains(c))
                {
                    if (c != '\n' || !flags.HasFlag(ValueFlags.MultiItems))
                    {
                        throw new ArgumentException($"Configuration {name} contains invalid characters");
                    }
                }
            }

            if (flags.HasFlag(ValueFlags.StrictSafe))
            {
                // Strict rules for debian package name. RPM is similar, but less strict. Follow these:
                // Package names (both source and binary, see Package) must consist only of lower case letters (a-z),
                // digits (0-9), plus (+) and minus (-) signs, and periods (.). They must be at least two characters
                // long and must start with an alphanumeric character.
                if (value.Length < 2)
                {
                    throw new ArgumentException($"Configuration {name} must contain at least 2 characters");
                }

                foreach (var c in value)
                {
                    // Not force lower case, but we will convert as needed
                    if (c != '-' && c != '+' && c != '.' && (c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9'))
                    {
                        if (c != '\n' || !flags.HasFlag(ValueFlags.MultiItems))
                        {
                            throw new ArgumentException($"Configuration {name} contains invalid characters");
                        }
                    }
                }
            }
        }

        // Cannot assert paths on a multi -- see GetCollection()
        return flags.HasFlag(ValueFlags.MultiItems) ? value : AssertPathFlags(name, value, flags);
    }

    private string AssertPathFlags(string name, string value, ValueFlags flags)
    {
        if (flags.HasFlag(ValueFlags.Path))
        {
            if (flags.HasFlag(ValueFlags.PathWithDisable) && value.Equals(PathDisable, StringComparison.OrdinalIgnoreCase))
            {
                // Disabled - no path
                return PathDisable;
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Switch to linux separator
                value = value.Replace('\\', '/');
            }

            if (AssertPaths)
            {
                if (!string.IsNullOrEmpty(LocalDirectory) && !Path.IsPathFullyQualified(value))
                {
                    value = Path.Combine(LocalDirectory, value);
                }

                if (flags.HasFlag(ValueFlags.AssertPath) && !File.Exists(value) && !Directory.Exists(value))
                {
                    throw new FileNotFoundException($"Configuration {name} path not found {value}");
                }
            }
        }

        return value;
    }
}
