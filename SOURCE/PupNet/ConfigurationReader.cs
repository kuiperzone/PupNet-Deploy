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
/// Reads and decodes the configuration file. Changes the working directory to that of the config file.
/// </summary>
public class ConfigurationReader
{
    [Flags]
    private enum ValueFlags
    {
        None = 0x00,
        Multi = 0x01,
        Safe = 0x02,
        SafeNoSpace = Safe | 0x04,
        StrictSafe = SafeNoSpace | 0x08,
        Path = Safe | 0x10,
        AssertPath = Path | 0x20,
        PathWithDisable = Path | 0x40,
        AssertPathWithDisable = AssertPath | PathWithDisable,
    };

    /// <summary>
    /// Explicitly no path.
    /// </summary>
    public const string PathDisable = "NONE";

    /// <summary>
    /// Default constructor. No arguments. Test or demo only.
    /// </summary>
    public ConfigurationReader(string? metabase = null)
        : this(DeployKind.AppImage, metabase)
    {
    }

    /// <summary>
    /// Constructor with kind. No other arguments. Test or demo only.
    /// </summary>
    public ConfigurationReader(DeployKind kind, string? metabase = null)
    {
        Arguments = new($"-{ArgumentReader.KindShortArg} {kind}");
        Reader = new();
        LocalDirectory = "";
        PackageName = AppBaseName;

        if (!string.IsNullOrEmpty(metabase))
        {
            DesktopFile = metabase + NewKind.Desktop.GetFileExt();
            MetaFile = metabase + NewKind.Meta.GetFileExt();
        }
    }

    /// <summary>
    /// Production constructor. Reads from file.
    /// </summary>
    public ConfigurationReader(ArgumentReader args, bool assertPaths = true)
        : this(args, new IniReader(AssertConfPath(args.Value, assertPaths)), assertPaths)
    {
    }

    /// <summary>
    /// Constructor with content. For unit test.
    /// </summary>
    public ConfigurationReader(ArgumentReader args, string[] content, bool assertPaths = false)
        : this(args, new IniReader(content), assertPaths)
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
        VersionRelease = GetMandatory(nameof(VersionRelease), ValueFlags.SafeNoSpace);
        ShortSummary = GetMandatory(nameof(ShortSummary), ValueFlags.None);
        LicenseId = GetMandatory(nameof(LicenseId), ValueFlags.Safe);
        LicenseFile = GetOptional(nameof(LicenseFile), ValueFlags.AssertPathWithDisable);

        VendorName = GetMandatory(nameof(VendorName), ValueFlags.Safe);
        VendorCopyright = GetOptional(nameof(VendorCopyright), ValueFlags.Safe);
        VendorUrl = GetOptional(nameof(VendorUrl), ValueFlags.Safe);
        VendorEmail = GetOptional(nameof(VendorEmail), ValueFlags.None);

        StartCommand = GetOptional(nameof(StartCommand), ValueFlags.StrictSafe);
        IsTerminalApp = GetBool(nameof(IsTerminalApp));
        DesktopFile = GetOptional(nameof(DesktopFile), ValueFlags.AssertPathWithDisable);
        PrimeCategory = GetOptional(nameof(PrimeCategory), ValueFlags.StrictSafe);
        IconFiles = GetCollection(nameof(IconFiles), ValueFlags.AssertPath);
        MetaFile = GetOptional(nameof(MetaFile), ValueFlags.AssertPathWithDisable);

        DotnetProjectPath = GetOptional(nameof(DotnetProjectPath), ValueFlags.AssertPathWithDisable);
        DotnetPublishArgs = GetOptional(nameof(DotnetPublishArgs), ValueFlags.None);
        DotnetPostPublish = GetOptional(nameof(DotnetPostPublish), ValueFlags.AssertPath);
        DotnetPostPublishOnWindows = GetOptional(nameof(DotnetPostPublishOnWindows), ValueFlags.AssertPath);

        PackageName = GetOptional(nameof(PackageName), ValueFlags.StrictSafe) ?? AppBaseName;
        OutputDirectory = GetOptional(nameof(OutputDirectory), ValueFlags.Path) ?? LocalDirectory;
        OutputVersion = GetBool(nameof(OutputVersion));

        AppImageArgs = GetOptional(nameof(AppImageArgs), ValueFlags.None);

        FlatpakPlatformRuntime = GetMandatory(nameof(FlatpakPlatformRuntime), ValueFlags.StrictSafe);
        FlatpakPlatformSdk = GetMandatory(nameof(FlatpakPlatformSdk), ValueFlags.StrictSafe);
        FlatpakPlatformVersion = GetMandatory(nameof(FlatpakPlatformVersion), ValueFlags.StrictSafe);
        FlatpakBuilderArgs = GetOptional(nameof(FlatpakBuilderArgs), ValueFlags.None);
        FlatpakFinishArgs = GetCollection(nameof(FlatpakFinishArgs), ValueFlags.None, "=", "--");

        SetupSignTool = GetOptional(nameof(SetupSignTool), ValueFlags.None);
        SetupMinWindowsVersion = GetMandatory(nameof(SetupMinWindowsVersion), ValueFlags.StrictSafe);
    }

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
    public string VersionRelease { get; } = "1.0.0[1]";
    public string ShortSummary { get; } = "A HelloWorld application";
    public string LicenseId { get; } = "LicenseRef-Proprietary";
    public string? LicenseFile { get; }

    public string VendorName { get; } = "HelloWorld Team";
    public string? VendorCopyright = "Copyright (C) HelloWorld Team 1970";
    public string? VendorUrl { get; } = "https://example.net";
    public string? VendorEmail { get; } = "helloworld@example.net";

    public string? StartCommand { get; }
    public bool IsTerminalApp { get; } = true;
    public string? DesktopFile { get; }
    public string? PrimeCategory { get; }
    public string? MetaFile { get; }
    public IReadOnlyCollection<string> IconFiles { get; } = Array.Empty<string>();

    public string? DotnetProjectPath { get; }
    public string? DotnetPublishArgs { get; } = $"-p:Version={MacroId.AppVersion.ToVar()} --self-contained true -p:DebugType=None -p:DebugSymbols=false";
    public string? DotnetPostPublish { get; }
    public string? DotnetPostPublishOnWindows { get; }

    public string PackageName { get; }
    public string OutputDirectory { get; } = "Deploy/bin";
    public bool OutputVersion { get; } = false;

    public string? AppImageArgs { get; }

    public string FlatpakPlatformRuntime { get; } = "org.freedesktop.Platform";
    public string FlatpakPlatformSdk { get; } = "org.freedesktop.Sdk";
    public string FlatpakPlatformVersion { get; } = "22.08";
    public IReadOnlyCollection<string> FlatpakFinishArgs { get; } = new string[]
        { "--socket=wayland", "--socket=x11", "--filesystem=host", "--share=network" };
    public string? FlatpakBuilderArgs { get; }

    public string? SetupSignTool { get; }
    public string SetupMinWindowsVersion { get; } = "10";

    /// <summary>
    /// Reads file associated with this configuration and returns the text. Returns null if path is null or
    /// equals <see cref="PathDisable"/>. If path is unqualified, then relative to <see cref="LocalDirectory"/>.
    /// Throws if file not exist and <see cref="AssertPaths"/> is true.
    /// </summary>
    public string? ReadAssociatedFile(string? path)
    {
        if (path != null && path != ConfigurationReader.PathDisable && (AssertPaths || File.Exists(path)))
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
    /// Overrides and writes contents with comments.
    /// </summary>
    public override string ToString()
    {
        return ToString(true);
    }

    /// <summary>
    /// Writes contents with comments if verbose.
    /// </summary>
    public string ToString(bool verbose)
    {
        // Convenience
        var breaker1 = new string('#', 80);
        var breaker2 = new string('#', 40);
        string macHelp = $"'{Program.CommandName} --{ArgumentReader.HelpLongArg} macro'";

        var sb = new StringBuilder();

        // Conditional reference
        var c = verbose ? sb : null;

        c?.AppendLine(breaker1);
        c?.AppendLine($"# THIS IS A {Program.ProductName.ToUpperInvariant()} CONF FILE");
        c?.AppendLine($"# {Program.ProjectUrl}");
        c?.AppendLine(breaker1);

        c?.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# APP PREAMBLE");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine("# Mandatory application base name. This MUST BE the base name of the main executable");
        c?.AppendLine("# file. It should NOT include any directory part or extension, i.e. do not append");
        c?.AppendLine("# '.exe' or '.dll'. It should not contain spaces or invalid filename characters.");
        c?.AppendLine("# Example: HelloWorld");
        sb.AppendLine(GetHelpNameValue(nameof(AppBaseName), AppBaseName));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application friendly name. Example: Hello World");
        sb.AppendLine(GetHelpNameValue(nameof(AppFriendlyName), AppFriendlyName));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application ID in reverse DNS form. Example: net.example.helloworld");
        sb.AppendLine(GetHelpNameValue(nameof(AppId), AppId));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application version and package release of form: 'VERSION[RELEASE]'. Use");
        c?.AppendLine($"# optional square brackets to denote package release, i.e. '1.2.3[1]'. Release refers to");
        c?.AppendLine($"# a change to the deployment package, rather the application. If release part is absent");
        c?.AppendLine($"# (i.e. '1.2.3'), the release value defaults to '1'. Note that the version-release value");
        c?.AppendLine($"# given here may be overidden from the command line.");
        sb.AppendLine(GetHelpNameValue(nameof(VersionRelease), VersionRelease));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory single line application description. Example: Yet another Hello World application.");
        sb.AppendLine(GetHelpNameValue(nameof(ShortSummary), ShortSummary));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application license name. This should be one of the recognised SPDX license");
        c?.AppendLine($"# identifiers, such as: 'MIT', 'GPL-3.0-or-later' or 'Apache-2.0'. For a properietary or");
        c?.AppendLine($"# custom license, use 'LicenseRef-Proprietary' or 'LicenseRef-LICENSE'.");
        sb.AppendLine(GetHelpNameValue(nameof(LicenseId), LicenseId));

        c?.AppendLine();
        c?.AppendLine($"# Optional path to a copyright/license text file. If provided, it will be packaged with the");
        c?.AppendLine($"# application and identified to package builder where supported. Example: Copyright.txt");
        sb.AppendLine(GetHelpNameValue(nameof(LicenseFile), LicenseFile));

        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# VENDOR");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Mandatory vendor, group or creator. Example: Acme Ltd, or HelloWorld Team");
        sb.AppendLine(GetHelpNameValue(nameof(VendorName), VendorName));

        c?.AppendLine();
        c?.AppendLine($"# Optional copyright statement. Example: Copyright (C) HelloWorld Team 1970");
        sb.AppendLine(GetHelpNameValue(nameof(VendorCopyright), VendorCopyright));

        c?.AppendLine();
        c?.AppendLine($"# Optional vendor or application homepage URL. Example: https://example.net");
        sb.AppendLine(GetHelpNameValue(nameof(VendorUrl), VendorUrl));

        c?.AppendLine();
        c?.AppendLine($"# Vendor or maintainer email contact. Although optional, some packages (such as Debian)");
        c?.AppendLine($"# require it and will fail unless provided. Example: <hello> helloworld@example.net");
        sb.AppendLine(GetHelpNameValue(nameof(VendorEmail), VendorEmail));

        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# DESKTOP INTEGRATION");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Optional command name to start the application from the terminal. If, for example, {nameof(AppBaseName)}");
        c?.AppendLine($"# is 'HelloWorld', the value here may be set to the same or a lower-case 'helloworld' variant.");
        c?.AppendLine($"# It must not contain spaces or invalid filename characters. If empty, the application will");
        c?.AppendLine($"# not be in the path and cannot be started from the command line. This value is not supported");
        c?.AppendLine($"# for {nameof(DeployKind.AppImage)} and {nameof(DeployKind.Flatpak)} and will be ignored. Default is empty.");
        sb.AppendLine(GetHelpNameValue(nameof(StartCommand), StartCommand));

        c?.AppendLine();
        c?.AppendLine($"# Boolean (true or false) which indicates whether the application runs in the terminal, rather");
        c?.AppendLine($"# than provides a GUI. It is used only to populate the 'Terminal' field of the .desktop file.");
        sb.AppendLine(GetHelpNameValue(nameof(IsTerminalApp), IsTerminalApp));

        c?.AppendLine();
        c?.AppendLine($"# Optional path to a Linux desktop file (ignored for Windows). If empty (default), one will be");
        c?.AppendLine($"# generated automatically from the information in this file. Supplying a custom file, however,");
        c?.AppendLine($"# allows for mime-types and internationalisation. If supplied, the file MUST contain the line:");
        c?.AppendLine($"# 'Exec={MacroId.DesktopExec.ToVar()}' in order to use the correct install location. Other macros may be");
        c?.AppendLine($"# used to help automate the content. If required that no desktop be installed, set value to: '{PathDisable}.");
        c?.AppendLine($"# Note. The contents of the files may use macro variables. Use {macHelp} for reference.");
        c?.AppendLine($"# See: https://specifications.freedesktop.org/desktop-entry-spec/desktop-entry-spec-latest.html");
        sb.AppendLine(GetHelpNameValue(nameof(DesktopFile), DesktopFile));

        c?.AppendLine();
        c?.AppendLine($"# Optional category for the application. The value should be one of the recognised Freedesktop");
        c?.AppendLine($"# top-level categories, such as: Audio, Development, Game, Office, Utility etc. Only a single value");
        c?.AppendLine($"# should be provided here which will be used, where supported, to populate metadata. The default");
        c?.AppendLine($"# is empty. See: https://specifications.freedesktop.org/menu-spec/latest/apa.html");
        c?.AppendLine($"# Example: Utility");
        sb.AppendLine(GetHelpNameValue(nameof(PrimeCategory), PrimeCategory));

        c?.AppendLine();
        c?.AppendLine($"# Optional icon file paths. The value may include multiple filenames separated with semicolon or");
        c?.AppendLine($"# given in multi-line form. Valid types are SVG, PNG and ICO (ICO ignored on Linux). Note that the");
        c?.AppendLine($"# inclusion of a scalable SVG is preferable on Linux, whereas PNGs must be one of the standard");
        c?.AppendLine($"# sizes and MUST include the size in the filename in the form: name.32x32.png' or 'name.32.png'.");
        c?.AppendLine($"# Example: Assets/app.svg;Assets/app.24x24.png;Assets/app.32x32.png;Assets/app.ico");
        sb.AppendLine(GetHelpNameValue(nameof(IconFiles), IconFiles, true));

        c?.AppendLine();
        c?.AppendLine($"# Path to AppStream metadata file. It is optional, but recommended as it is used by software centers.");
        c?.AppendLine($"# Note. The contents of the files may use macro variables. Use {macHelp} for reference.");
        c?.AppendLine($"# See: https://docs.appimage.org/packaging-guide/optional/appstream.html");
        c?.AppendLine($"# Example: Deploy/app.metainfo.xml.");
        sb.AppendLine(GetHelpNameValue(nameof(MetaFile), MetaFile));

        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# DOTNET PUBLISH");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Optional path relative to this file in which to find the dotnet project (.csproj)");
        c?.AppendLine($"# or solution (.sln) file, or the directory containing it. If empty (default), a single");
        c?.AppendLine($"# project or solution file is expected under the same directory as this file.");
        c?.AppendLine($"# IMPORTANT. If set to '{PathDisable}', dotnet publish is disabled (not called).");
        c?.AppendLine($"# Instead, only {nameof(DotnetPostPublish)} is called. Example: Source/MyProject");
        sb.AppendLine(GetHelpNameValue(nameof(DotnetProjectPath), DotnetProjectPath));

        c?.AppendLine();
        c?.AppendLine($"# Optional arguments supplied to 'dotnet publish'. Do NOT include '-r' (runtime), app version,");
        c?.AppendLine($"# or '-c' (configuration) here as they will be added (i.e. via {nameof(VersionRelease)}).");
        c?.AppendLine($"# Typically you want as a minimum: '-p:Version={MacroId.AppVersion.ToVar()} --self-contained true'. Additional");
        c?.AppendLine($"# useful arguments include: '-p:DebugType=None -p:DebugSymbols=false -p:PublishSingleFile=true");
        c?.AppendLine($"# -p:PublishReadyToRun=true -p:PublishTrimmed=true -p:TrimMode=link'.");
        c?.AppendLine($"# Note. This value may use macro variables. Use {macHelp} for reference.");
        c?.AppendLine($"# See: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish");
        sb.AppendLine(GetHelpNameValue(nameof(DotnetPublishArgs), DotnetPublishArgs));

        c?.AppendLine();
        c?.AppendLine($"# Post-publish (or standalone build) commands on Linux (ignored on Windows). Multiple commands");
        c?.AppendLine($"# may be specified, separated by semicolon or given in multi-line form. They are called after");
        c?.AppendLine($"# dotnet publish, but before the final output is built. These could, for example, copy additional");
        c?.AppendLine($"# files into the build directory given by {MacroId.PublishBin.ToVar()}. The working directory will be the");
        c?.AppendLine($"# location of this file. This value is optional, but becomes mandatory if {nameof(DotnetProjectPath)} equals '{PathDisable}'.");
        c?.AppendLine($"# Note. This value may use macro variables. Additionally, scripts may use these as environment variables.");
        c?.AppendLine($"# Use {macHelp} for reference.");
        sb.AppendLine(GetHelpNameValue(nameof(DotnetPostPublish), DotnetPostPublish));

        c?.AppendLine();
        c?.AppendLine($"# Post-publish (or standalone build) commands on Windows (ignored on Linux). This should perform");
        c?.AppendLine($"# the equivalent operations, as required, as {nameof(DotnetPostPublish)}, but using DOS commands and batch scripts.");
        c?.AppendLine($"# Multiple commands may be specified, separated by semicolon or given in multi-line form. Note. This value");
        c?.AppendLine($"# may use macro variables. Additionally, scripts may use these as environment variables.");
        c?.AppendLine($"# Use {macHelp} for reference.");
        sb.AppendLine(GetHelpNameValue(nameof(DotnetPostPublishOnWindows), DotnetPostPublishOnWindows));

        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# PACKAGE OUTPUT");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Optional package name (excludes version etc.). If empty, defaults to {nameof(AppBaseName)}.");
        c?.AppendLine($"# However, it is used not only to specify the base output filename, but to identify the application");
        c?.AppendLine($"# in .deb and .rpm packages. You may wish, therefore, to ensure that the value represents a");
        c?.AppendLine($"# unique name, such as the {nameof(AppId)}. Naming requirements for this are strict and must");
        c?.AppendLine($"# contain only alpha-numeric and '-', '+' and '.' characters. Example: HelloWorld");
        sb.AppendLine(GetHelpNameValue(nameof(PackageName), PackageName));

        c?.AppendLine();
        c?.AppendLine($"# Output directory, or subdirectory relative to this file. It will be created if it does not");
        c?.AppendLine($"# exist and will contain the final deploy output files. If empty, it defaults to the location");
        c?.AppendLine($"# of this file. Default: Deploy/bin");
        sb.AppendLine(GetHelpNameValue(nameof(OutputDirectory), OutputDirectory));

        c?.AppendLine();
        c?.AppendLine($"# Boolean (true or false) which sets whether to include the application version in the filename");
        c?.AppendLine($"# of the package (i.e. 'HelloWorld-1.2.3-x86_64.AppImage'). It is ignored if the output");
        c?.AppendLine($"# filename is specified at command line.");
        sb.AppendLine(GetHelpNameValue(nameof(OutputVersion), OutputVersion));

        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# APPIMAGE OPTIONS");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Additional arguments for use with appimagetool. Useful for signing. Default is empty.");
        c?.AppendLine($"# See appimagetool --help. Example: --sign");
        sb.AppendLine(GetHelpNameValue(nameof(AppImageArgs), AppImageArgs));


        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# FLATPAK OPTIONS");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# The runtime platform. Invariably for .NET (inc. Avalonia), this should be");
        c?.AppendLine($"# 'org.freedesktop.Platform'.");
        c?.AppendLine($"# Refer: https://docs.flatpak.org/en/latest/available-runtimes.html");
        sb.AppendLine(GetHelpNameValue(nameof(FlatpakPlatformRuntime), FlatpakPlatformRuntime));

        c?.AppendLine();
        c?.AppendLine($"# The platform SDK. Invariably for .NET (inc. Avalonia applications) this should");
        c?.AppendLine($"# be 'org.freedesktop.Sdk'. The SDK must be installed on the build system.");
        sb.AppendLine(GetHelpNameValue(nameof(FlatpakPlatformSdk), FlatpakPlatformSdk));

        c?.AppendLine();
        c?.AppendLine($"# The platform runtime version. The latest available version may change periodically.");
        c?.AppendLine($"# Refer to Flatpak documentation. Example: 22.08");
        sb.AppendLine(GetHelpNameValue(nameof(FlatpakPlatformVersion), FlatpakPlatformVersion));

        c?.AppendLine();
        c?.AppendLine($"# Flatpak manifest 'finish-args' sandbox permissions. Optional, but if empty, the");
        c?.AppendLine($"# application will have extremely limited access to the host environment. This");
        c?.AppendLine($"# option may be used to grant required application permissions. Values here should");
        c?.AppendLine($"# be prefixed with '--' and separated by semicolon or given in multi-line form.");
        c?.AppendLine($"# Example: --socket=wayland;--socket=x11;--filesystem=host;--share=network");
        c?.AppendLine($"# Refer: https://docs.flatpak.org/en/latest/sandbox-permissions.html");
        sb.AppendLine(GetHelpNameValue(nameof(FlatpakFinishArgs), FlatpakFinishArgs, true));

        c?.AppendLine();
        c?.AppendLine($"# Additional arguments for use with flatpak-builder. Useful for signing. Default is empty.");
        c?.AppendLine($"# See flatpak-builder --help. Example: --gpg-keys=FILE");
        sb.AppendLine(GetHelpNameValue(nameof(FlatpakBuilderArgs), FlatpakBuilderArgs));


        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# WINDOWS SETUP OPTIONS");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Optional name and parameters of the Sign Tool to be used to digitally sign: the installer, ");
        c?.AppendLine($"# uninstaller, and contained exe and dll files. If empty, files will not be signed.");
        c?.AppendLine($"# See 'SignTool' parameter in: https://jrsoftware.org/ishelp/");
        sb.AppendLine(GetHelpNameValue(nameof(SetupSignTool), SetupSignTool));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory value which specifies minimum version of Windows that your software runs on.");
        c?.AppendLine($"# Windows 8 = 6.2, Windows 10/11 = 10. Default: 10.");
        c?.AppendLine($"# See 'MinVersion' parameter in: https://jrsoftware.org/ishelp/");
        sb.AppendLine(GetHelpNameValue(nameof(SetupMinWindowsVersion), SetupMinWindowsVersion));

        return sb.ToString().Trim();
    }

    private static string GetHelpNameValue(string name, string? value)
    {
        return $"{name} = {value}";
    }

    private static string GetHelpNameValue(string name, bool value)
    {
        return $"{name} = {value.ToString().ToLowerInvariant()}";
    }

    private static string GetHelpNameValue(string name, IReadOnlyCollection<string> values, bool multi = false)
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
            return sb.ToString();
        }

        sb.Append(string.Join(';', values));
        return sb.ToString();
    }

    private static string AssertConfPath(string? path, bool assert)
    {
        if (string.IsNullOrEmpty(path))
        {
            if (assert)
            {
                throw new ArgumentException($"Specify .conf file (directory does not contain exactly one file).");
            }

            // Dummy only (not in production)
            return "file.conf";
        }

        return path;
    }

    private IReadOnlyCollection<string> GetCollection(string name, ValueFlags flags, string? mustContain = null, string? mustStart = null)
    {
        var value = GetOptional(name, flags | ValueFlags.Multi);

        if (value != null)
        {
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

        return Array.Empty<string>();
    }

    private bool GetBool(string name)
    {
        var value = GetMandatory(name, ValueFlags.None).Trim();

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

        if (!flags.HasFlag(ValueFlags.Multi) && value.Contains('\n'))
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
                if (flags.HasFlag(ValueFlags.SafeNoSpace) && char.IsWhiteSpace(c))
                {
                    throw new ArgumentException($"{name} must not contain spaces or non-printing characters");
                }

                if (c < ' ' || Invalid.Contains(c))
                {
                    if (c != '\n' || !flags.HasFlag(ValueFlags.Multi))
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

                /*
                // Don't do this - too strict for many fields
                // Leave it to Debian to fail if this is a problem
                if (char.IsDigit(value[0]))
                {
                    throw new ArgumentException($"Configuration {name} cannot start with a numeric digit");
                }
                */

                foreach (var c in value)
                {
                    // Not force lower case, but we will convert as needed
                    if (c != '-' && c != '+' && c != '.' && !char.IsAsciiLetterOrDigit(c))
                    {
                        if (c != '\n' || !flags.HasFlag(ValueFlags.Multi))
                        {
                            throw new ArgumentException($"Configuration {name} contains invalid characters");
                        }
                    }
                }
            }
        }

        // Cannot assert paths on a multi -- see GetCollection()
        return flags.HasFlag(ValueFlags.Multi) ? value : AssertPathFlags(name, value, flags);
    }

    private string AssertPathFlags(string name, string value, ValueFlags flags)
    {
        if (flags.HasFlag(ValueFlags.Path) && AssertPaths)
        {
            if (flags.HasFlag(ValueFlags.PathWithDisable) && value.Equals(PathDisable, StringComparison.OrdinalIgnoreCase))
            {
                // Disabled - no path
                return PathDisable;
            }

            if (!string.IsNullOrEmpty(LocalDirectory) && !Path.IsPathFullyQualified(value))
            {
                value = Path.Combine(LocalDirectory, value);
            }

            if (flags.HasFlag(ValueFlags.AssertPath) && !Path.Exists(value))
            {
                throw new FileNotFoundException($"Configuration {name} path not found {value}");
            }
        }

        return value;
    }
}
