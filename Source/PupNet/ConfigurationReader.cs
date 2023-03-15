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

using System.Diagnostics.CodeAnalysis;
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
    public const string PathNone = "NONE";

    /// <summary>
    /// Default constructor. No arguments. Test or demo only.
    /// </summary>
    public ConfigurationReader(string? metabase = null)
        : this(PackKind.AppImage, metabase)
    {
    }

    /// <summary>
    /// Constructor with kind. No other arguments. Test or demo only.
    /// </summary>
    public ConfigurationReader(PackKind kind, string? metabase = null)
    {
        Arguments = new($"-{ArgumentReader.KindShortArg} {kind}");
        Reader = new();
        LocalDirectory = "";

        if (!string.IsNullOrEmpty(metabase))
        {
            DesktopFile = metabase + NewKind.Desktop.GetFileExt();
            MetaFile = metabase + NewKind.Meta.GetFileExt();
        }
    }

    /// <summary>
    /// Constructor. Reads from file.
    /// </summary>
    public ConfigurationReader(ArgumentReader args, bool assertFiles = true)
        : this(args, new IniReader(AssertConfPath(args.Value, assertFiles)), assertFiles)
    {
    }

    /// <summary>
    /// Constructor with content. For unit test.
    /// </summary>
    public ConfigurationReader(ArgumentReader args, string[] content, bool assertFiles = false)
        : this(args, new IniReader(content), assertFiles)
    {
    }

    private ConfigurationReader(ArgumentReader args, IniReader reader, bool assertFiles)
    {
        Arguments = args;
        Reader = reader;
        AssertFiles = assertFiles;
        LocalDirectory = assertFiles ? Path.GetDirectoryName(reader.Filepath)! : "";

        AppBaseName = GetStrict(nameof(AppBaseName));
        AppFriendlyName = GetMandatory(nameof(AppFriendlyName));
        AppId = GetStrict(nameof(AppId), true);
        VersionRelease = GetStrict(nameof(VersionRelease));
        PackageName = GetStrict(nameof(PackageName), true);
        ShortSummary = GetMandatory(nameof(ShortSummary));
        LicenseId = GetMandatory(nameof(LicenseId));

        VendorName = GetMandatory(nameof(VendorName));
        VendorCopyright = GetOptional(nameof(VendorCopyright));
        VendorUrl = GetOptional(nameof(VendorUrl));
        VendorEmail = GetOptional(nameof(VendorEmail));

        StartCommand = GetOptional(nameof(StartCommand));
        IsTerminalApp = GetBool(nameof(IsTerminalApp));
        DesktopFile = AssertAbsolutePath(GetOptional(nameof(DesktopFile)), true);
        IconFiles = AssertAbsolutePaths(GetMultiCollection(nameof(IconFiles)));
        MetaFile = AssertAbsolutePath(GetOptional(nameof(MetaFile)), false);

        DotnetProjectPath = AssertAbsolutePath(GetOptional(nameof(DotnetProjectPath)), true);
        DotnetPublishArgs = GetOptional(nameof(DotnetPublishArgs));
        DotnetPostPublish = GetMultiCollection(nameof(DotnetPostPublish));

        OutputDirectory = AssertAbsolutePath(GetOptional(nameof(OutputDirectory)), false) ?? LocalDirectory;
        OutputVersion = GetBool(nameof(OutputVersion));

        AppImageArgs = GetOptional(nameof(AppImageArgs));

        FlatpakPlatformRuntime = GetMandatory(nameof(FlatpakPlatformRuntime));
        FlatpakPlatformSdk = GetMandatory(nameof(FlatpakPlatformSdk));
        FlatpakPlatformVersion = GetMandatory(nameof(FlatpakPlatformVersion));
        FlatpakBuilderArgs = GetOptional(nameof(FlatpakBuilderArgs));
        FlatpakFinishArgs = GetMultiCollection(nameof(FlatpakFinishArgs), "=", "--");

        SetupSignTool = GetOptional(nameof(SetupSignTool));
        SetupMinWindowsVersion = GetStrict(nameof(SetupMinWindowsVersion), true);

        // Additional validation
        if (!AppId.Contains('.'))
        {
            // AppId must have a '.'
            throw new ArgumentException($"{nameof(AppId)} must be in reverse DNS form, i.e. 'net.example.appname'");
        }

        if (DotnetProjectPath == PathNone)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (DotnetPostPublish.Count == 0)
                {
                    throw new ArgumentException($"{nameof(DotnetPostPublish)} is mandatory where {nameof(DotnetProjectPath)} = {PathNone}");
                }
            }
        }
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
    /// Gets whether to assert files exist. Only false for unit testing.
    /// </summary>
    public bool AssertFiles { get; }

    /// <summary>
    /// Gets the configuration file local directory. Empty if <see cref="AssertFiles"/> is false.
    /// </summary>
    public string LocalDirectory { get; }

    public string AppBaseName { get; } = "HelloWorld";
    public string AppFriendlyName { get; } = "Hello World";
    public string AppId { get; } = "net.example.helloworld";
    public string VersionRelease { get; } = "1.0.0[1]";
    public string PackageName { get; } = "HelloWorld";
    public string ShortSummary { get; } = "A HelloWorld application";
    public string LicenseId { get; } = "LicenseRef-Proprietary";

    public string VendorName { get; } = "HelloWorld Team";
    public string? VendorCopyright = "Copyright (C) HelloWorld Team 1970";
    public string? VendorUrl { get; } = "https://example.net";
    public string? VendorEmail { get; } = "helloworld@example.net";

    public string? StartCommand { get; }
    public bool IsTerminalApp { get; } = true;
    public string? DesktopFile { get; }
    public string? MetaFile { get; }
    public IReadOnlyCollection<string> IconFiles { get; } = Array.Empty<string>();

    public string? DotnetProjectPath { get; }
    public string? DotnetPublishArgs { get; } = $"-p:Version={MacroId.AppVersion.ToVar()} --self-contained true -p:DebugType=None -p:DebugSymbols=false";
    public IReadOnlyCollection<string> DotnetPostPublish { get; } = Array.Empty<string>();

    public string OutputDirectory { get; } = "Deploy";
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
    /// Reads text file. Returns null if path is null or equals <see cref="PathNone"/>.
    /// If path is unqualified, then relative to <see cref="LocalDirectory"/>.
    /// Throws if file not exist and <see cref="AssertFiles"/> is true.
    /// </summary>
    public string? ReadFile(string? path)
    {
        path = AssertAbsolutePath(path, true);

        if (path != null && path != ConfigurationReader.PathNone && (AssertFiles || File.Exists(path)))
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
        var breaker1 = new string('#', 80);
        var breaker2 = new string('#', 40);

        var sb = new StringBuilder();

        // Conditional reference
        var c = verbose ? sb : null;

        sb.AppendLine(breaker1);
        sb.AppendLine($"# THIS IS A {Program.ProductName.ToUpperInvariant()} CONF FILE");
        sb.AppendLine($"# {Program.ProjectUrl}");
        sb.AppendLine(breaker1);

        sb.AppendLine();
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
        c?.AppendLine($"# Mandatory package name (excludes version etc.). It must contain only alpha-numeric and");
        c?.AppendLine($"# the '-' characters. It will be converted to lowercase for RPM and Debian. Example: helloworld");
        sb.AppendLine(GetHelpNameValue(nameof(PackageName), PackageName));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory single line application description. Example: Yet another Hello World application.");
        sb.AppendLine(GetHelpNameValue(nameof(ShortSummary), ShortSummary));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application license name. This should be one of the recognised SPDX license");
        c?.AppendLine($"# identifiers, such as: 'MIT', 'GPL-3.0-or-later' or 'Apache-2.0'. For a properietary or");
        c?.AppendLine($"# custom license, use 'LicenseRef-Proprietary' or 'LicenseRef-LICENSE'.");
        sb.AppendLine(GetHelpNameValue(nameof(LicenseId), LicenseId));

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
        c?.AppendLine($"# Vendor or maintainer email contact. Although optional, some package deployments (such as");
        c?.AppendLine($"# Debian) require it and will fail unless provided. Example: <hello> helloworld@example.net");
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
        c?.AppendLine($"# for {nameof(PackKind.AppImage)} and {nameof(PackKind.Flatpak)} and will be ignored. Default is empty.");
        sb.AppendLine(GetHelpNameValue(nameof(StartCommand), StartCommand));

        c?.AppendLine();
        c?.AppendLine($"# Boolean (true or false) which indicates whether the application runs in the terminal, rather");
        c?.AppendLine($"# than provides a GUI. It is used only to populate the 'Terminal' field of the .desktop file.");
        sb.AppendLine(GetHelpNameValue(nameof(IsTerminalApp), IsTerminalApp));

        c?.AppendLine();
        c?.AppendLine($"# Optional path to a Linux desktop file (ignored for Windows). If empty (default), one will be");
        c?.AppendLine($"# generated automatically from the information in this file. A file name may be supplied instead");
        c?.AppendLine($"# to provide for mime-types and internationalisation. If supplied, the file MUST contain the line:");
        c?.AppendLine($"# 'Exec={MacroId.DesktopExec.ToVar()}' in order to use the correct install location. Other macros may be");
        c?.AppendLine($"# used to help automate the content. If required that no desktop be installed, set value to: '{PathNone}.");
        c?.AppendLine($"# Reference1: https://www.baeldung.com/linux/desktop-entry-files");
        c?.AppendLine($"# Reference2: https://specifications.freedesktop.org/desktop-entry-spec/desktop-entry-spec-latest.html");
        sb.AppendLine(GetHelpNameValue(nameof(DesktopFile), DesktopFile));

        c?.AppendLine();
        c?.AppendLine($"# Optional icon file paths. The value may include multiple filenames separated with semicolon or");
        c?.AppendLine($"# given in multi-line form. Valid types are SVG, PNG and ICO (ICO ignored on Linux). Note that the");
        c?.AppendLine($"# inclusion of a scalable SVG is preferable on Linux, whereas PNGs must be one of the standard");
        c?.AppendLine($"# sizes and MUST include the size in the filename in the form: name.32x32.png' or 'name.32.png'.");
        c?.AppendLine($"# Example: Assets/app.svg;Assets/app.24x24.png;Assets/app.32x32.png;Assets/app.ico");
        sb.AppendLine(GetHelpNameValue(nameof(IconFiles), IconFiles, true));

        c?.AppendLine();
        c?.AppendLine($"# Path to AppStream metadata file. It is optional, but recommended as it is used by software centers.");
        c?.AppendLine($"# The file content may embed supported macros such as, such as {MacroId.AppFriendlyName.ToVar()} and {MacroId.AppId.ToVar()} etc.");
        c?.AppendLine($"# to assist in automating many fields. Refer: https://docs.appimage.org/packaging-guide/optional/appstream.html");
        c?.AppendLine($"# Example: Assets/app.metainfo.xml.");
        sb.AppendLine(GetHelpNameValue(nameof(MetaFile), MetaFile));

        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# DOTNET PUBLISH");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Optional path relative to this file in which to find the dotnet project (.csproj)");
        c?.AppendLine($"# or solution (.sln) file, or the directory containing it. If empty (default), a single");
        c?.AppendLine($"# project or solution file is expected under the same directory as this file.");
        c?.AppendLine($"# IMPORTANT. If set to '{PathNone}', dotnet publish is disabled (not called).");
        c?.AppendLine($"# Instead, only {nameof(DotnetPostPublish)} is called. Example: Source/MyProject");
        sb.AppendLine(GetHelpNameValue(nameof(DotnetProjectPath), DotnetProjectPath));

        c?.AppendLine();
        c?.AppendLine($"# Optional arguments supplied to 'dotnet publish'. Do NOT include '-r' (runtime), app version,");
        c?.AppendLine($"# or '-c' (configuration) here as they will be added (i.e. via {nameof(VersionRelease)}).");
        c?.AppendLine($"# Typically you want as a minimum: '-p:Version={MacroId.AppVersion.ToVar()} --self-contained true'. Additional");
        c?.AppendLine($"# useful arguments include: '-p:DebugType=None -p:DebugSymbols=false -p:PublishSingleFile=true");
        c?.AppendLine($"# -p:PublishTrimmed=true -p:TrimMode=link'.");
        c?.AppendLine($"# Refer: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish");
        sb.AppendLine(GetHelpNameValue(nameof(DotnetPublishArgs), DotnetPublishArgs));

        c?.AppendLine();
        c?.AppendLine($"# Post-publish (or standalone build) commands on Linux (ignored on Windows). Multiple commands");
        c?.AppendLine($"# may be specifed, separated by semicolon or given in multi-line form. They are called after");
        c?.AppendLine($"# dotnet publish, but before the final output is built. This could, for example, copy additional");
        c?.AppendLine($"# files into the build directory. The working directory will be the location of this file.");
        c?.AppendLine($"# This value is optional, but becomes mandatory if {nameof(DotnetProjectPath)} equals '{PathNone}'.");
        sb.AppendLine(GetHelpNameValue(nameof(DotnetPostPublish), DotnetPostPublish));


        sb.AppendLine();
        c?.AppendLine(breaker2);
        sb.AppendLine("# PACKAGE OUTPUT");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Output directory, or subdirectory relative to this file. It will be created if it does not");
        c?.AppendLine($"# exist and will contain the final package output files. If empty, it defaults to the location");
        c?.AppendLine($"# of this file. Default: Deploy");
        sb.AppendLine(GetHelpNameValue(nameof(OutputDirectory), OutputDirectory));

        c?.AppendLine();
        c?.AppendLine($"# Boolean (true or false) which sets whether to include the application version in the filename");
        c?.AppendLine($"# of package (i.e. 'HelloWorld-1.2.3-x86_64.AppImage'). It is ignored if the output filename");
        c?.AppendLine($"# the output  is specified at command line.");
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
        c?.AppendLine($"# Mandatory values which specifies minimum version of Windows that your software runs on.");
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

    private static string? AssertConfValue(string name, string? value, bool multi, int maxlen = int.MaxValue)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (!multi && value.Contains('\n'))
            {
                throw new ArgumentException($"Single line value only for {name}");
            }

            // Not currently restricting on length
            if (value.Length > maxlen)
            {
                throw new ArgumentException($"Value too long for {name}");
            }

            return value;
        }

        return null;
    }

    [return: NotNullIfNotNull("value")]
    private string? AssertStrictValue(string name, string? value, bool alphaNumOnly)
    {
        if (!string.IsNullOrEmpty(value))
        {
            const string Invalid = "\\/:*?\"<>|";

            foreach (var c in value)
            {
                if (c <= ' ')
                {
                    throw new ArgumentException($"{name} must not contain spaces or non-printing characters");
                }

                if (Invalid.Contains(c) || (alphaNumOnly && c != '-' && c != '.' && !Char.IsAsciiLetterOrDigit(c)))
                {
                    throw new ArgumentException($"{name} contains invalid characters");
                }
            }

            return value;
        }

        return null;
    }

    private string? AssertAbsolutePath(string? path, bool allowNone)
    {
        if (!string.IsNullOrEmpty(path))
        {
            if (allowNone && path.Equals(PathNone, StringComparison.OrdinalIgnoreCase))
            {
                // Disabled - no path
                return PathNone;
            }

            if (!string.IsNullOrEmpty(LocalDirectory) && !Path.IsPathFullyQualified(path))
            {
                path = Path.Combine(LocalDirectory, path);
            }

            if (AssertFiles && !Path.Exists(path))
            {
                throw new FileNotFoundException($"File path not found {path}");
            }

            return path;
        }

        return null;
    }

    private IReadOnlyCollection<string> AssertAbsolutePaths(IReadOnlyCollection<string> paths)
    {
        if (paths.Count != 0)
        {
            var list = new List<string>();

            foreach (var item in paths)
            {
                list.Add(AssertAbsolutePath(item, false) ?? "");
            }

            return list;
        }

        return Array.Empty<string>();
    }

    private IReadOnlyCollection<string> GetMultiCollection(string name, string? mustContain = null, string? mustStart = null)
    {
        var value = GetOptional(name, true);

        if (value != null)
        {
            value = value.ReplaceLineEndings("\n");

            char sep = value.Contains('\n') ? '\n' : ';';
            var list = value.Split(sep, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(mustStart) && !item.StartsWith(mustStart))
                {
                    throw new ArgumentException($"{name} items must start with {mustStart}");
                }

                if (!string.IsNullOrEmpty(mustContain) && !item.Contains(mustContain))
                {
                    throw new ArgumentException($"{name} items must contain {mustContain}");
                }
            }

            return list;
        }

        return Array.Empty<string>();
    }

    private string? GetOptional(string name, bool multi = false)
    {
        Reader.Values.TryGetValue(name, out string? value);
        return AssertConfValue(name, value, multi);
    }

    private string GetMandatory(string name, bool multi = false)
    {
        Reader.Values.TryGetValue(name, out string? value);

        return AssertConfValue(name, value, multi) ??
            throw new ArgumentException($"Mandatory value required for {name}");
    }

    private string GetStrict(string name, bool alphanum = false)
    {
        return AssertStrictValue(name, GetMandatory(name, false), alphanum);
    }

    private bool GetBool(string name)
    {
        var value = GetMandatory(name);

        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new ArgumentException($"Use 'true' or 'false' only in conf for {name}");
    }

}
