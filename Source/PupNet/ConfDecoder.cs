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
public class ConfDecoder
{
    /// <summary>
    /// Explicitly no path.
    /// </summary>
    public const string PathNone = "NONE";

    /// <summary>
    /// Default constructor. No arguments.
    /// </summary>
    public ConfDecoder()
    {
        Args = new();
        Reader = new();
        ConfDirectory = "";
    }

    /// <summary>
    /// Constructor. Reads from file.
    /// </summary>
    public ConfDecoder(ArgDecoder args, bool assertFiles = true)
        : this(args, new IniReader(AssertConfPath(args.Value, assertFiles)), assertFiles)
    {
    }

    /// <summary>
    /// Constructor with content. For unit test.
    /// </summary>
    public ConfDecoder(ArgDecoder args, string[] content, bool assertFiles = false)
        : this(args, new IniReader(content), assertFiles)
    {
    }

    private ConfDecoder(ArgDecoder args, IniReader reader, bool assertFiles)
    {
        Args = args;
        Reader = reader;
        AssertFiles = assertFiles;
        ConfDirectory = assertFiles ? Path.GetDirectoryName(reader.Filepath)! : "";

        AppBase = GetMandatory(nameof(AppBase));
        AppName = GetMandatory(nameof(AppName));
        AppId = GetMandatory(nameof(AppId));
        AppVersionRelease = GetMandatory(nameof(AppVersionRelease));

        AppSummary = GetMandatory(nameof(AppSummary));
        AppLicense = GetMandatory(nameof(AppLicense));
        AppVendor = GetMandatory(nameof(AppVendor));
        AppUrl = GetOptional(nameof(AppUrl));

        CommandName = GetOptional(nameof(CommandName));
        IsTerminal = GetBool(nameof(IsTerminal));
        DesktopEntry = AssertAbsolutePath(ConfDirectory, GetOptional(nameof(DesktopEntry)), true);
        Icons = AssertAbsolutePath(ConfDirectory, GetConfMultiline(nameof(Icons), true));
        MetaInfo = AssertAbsolutePath(ConfDirectory, GetOptional(nameof(MetaInfo)), false);

        DotnetProjectPath = AssertAbsolutePath(ConfDirectory, GetOptional(nameof(DotnetProjectPath)), true);
        DotnetPublishArgs = GetOptional(nameof(DotnetPublishArgs));
        DotnetPostPublish = GetConfMultiline(nameof(DotnetPostPublish), true);

        OutputDirectory = AssertAbsolutePath(ConfDirectory, GetOptional(nameof(OutputDirectory)), false) ?? ConfDirectory;
        OutputVersion = GetBool(nameof(OutputVersion));

        AppImageArgs = GetOptional(nameof(AppImageArgs));

        FlatpakPlatformRuntime = GetMandatory(nameof(FlatpakPlatformRuntime));
        FlatpakPlatformSdk = GetMandatory(nameof(FlatpakPlatformSdk));
        FlatpakPlatformVersion = GetMandatory(nameof(FlatpakPlatformVersion));
        FlatpakBuilderArgs = GetOptional(nameof(FlatpakBuilderArgs));
        FlatpakFinishArgs = GetConfMultiline(nameof(FlatpakFinishArgs), true, "=", "--");

        AssertOK();
    }

    public ArgDecoder Args { get; }
    public IniReader Reader { get; }
    public bool AssertFiles { get; }

    /// <summary>
    /// Gets the conf file directory. Empty if <see cref="AssertFiles"/> is false.
    /// </summary>
    public string ConfDirectory { get; }

    public string AppBase { get; } = "HelloWorld";
    public string AppName { get; } = "Hello World";
    public string AppId { get; } = "net.example.helloworld";
    public string AppVersionRelease { get; } = "1.0.0[1]";
    public string AppSummary { get; } = "A HelloWorld application";
    public string AppLicense { get; } = "LicenseRef-Proprietary";
    public string AppVendor { get; } = "Acme Ltd";
    public string? AppUrl { get; } = "https://example.net";

    public string? CommandName { get; }
    public bool IsTerminal { get; } = true;
    public string? DesktopEntry { get; }
    public string? MetaInfo { get; }
    public IReadOnlyCollection<string> Icons { get; } = Array.Empty<string>();

    public string? DotnetProjectPath { get; }
    public string? DotnetPublishArgs { get; } = $"-p:Version=${{{MacroNames.AppVersion}}} --self-contained true -p:DebugType=None -p:DebugSymbols=false";
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

    /// <summary>
    /// Gets this system architecture as string in common use with packagers.
    /// </summary>
    public static string GetOSArch()
    {
        // Get from OS, but map to expected names
        var arch = RuntimeInformation.OSArchitecture;

        if (arch == Architecture.X64)
        {
            return "x86_64";
        }

        if (arch == Architecture.Arm64)
        {
            return "aarch64";
        }

        if (arch == Architecture.Ppc64le)
        {
            return "powerpc";
        }

        if (arch == Architecture.X86)
        {
            return "x86";
        }

        return arch.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Gets the target build architecture. Derived from args and config.
    /// </summary>
    public string GetBuildArch()
    {
        if (Args.Arch != null)
        {
            // Provided explicitly
            return Args.Arch;
        }

        if (DotnetProjectPath != ConfDecoder.PathNone)
        {
            // Map from dotnet RID
            return GetRuntimeArch(Args.Runtime);
        }

        return GetOSArch();
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

        c?.AppendLine(breaker1);
        c?.AppendLine($"# THIS IS A {Program.ProductName.ToUpperInvariant()} CONF FILE");
        c?.AppendLine($"# {Program.ProductName} Homepage: {Program.ProjectUrl}");
        c?.AppendLine(breaker1);

        c?.AppendLine();
        c?.AppendLine(breaker2);
        c?.AppendLine("# APP PREAMBLE");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine("# Mandatory application base name. This MUST BE the base name of the main executable");
        c?.AppendLine("# file. It should NOT include any directory part or extension, i.e. do not append");
        c?.AppendLine("# '.exe' or '.dll'. It should not contain a space character. Example: HelloWorld");
        sb.AppendLine(GetHelpNameValue(nameof(AppBase), AppBase));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application friendly name. Example: Hello World");
        sb.AppendLine(GetHelpNameValue(nameof(AppName), AppName));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application ID in reverse DNS form. Example: net.example.helloworld");
        sb.AppendLine(GetHelpNameValue(nameof(AppId), AppId));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application version and package release of form: 'VERSION[RELEASE]'.");
        c?.AppendLine($"# Use optional square brackets to denote package release, i.e. '1.2.3[1]'. If release is");
        c?.AppendLine($"# absent (i.e. '1.2.3') the release value defaults to '1'. Note that the value given here");
        c?.AppendLine($"# may be overidden from the command line.");
        sb.AppendLine(GetHelpNameValue(nameof(AppVersionRelease), AppVersionRelease));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory single line application description. Example: A Hello World application");
        sb.AppendLine(GetHelpNameValue(nameof(AppSummary), AppSummary));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application license name. Ideally, this should be one of the recognised");
        c?.AppendLine($"# SPDX license identifiers, such as: 'MIT', 'GPL-3.0-or-later' or 'Apache-2.0'. For");
        c?.AppendLine($"# a properietary or custom license, use 'LicenseRef-Proprietary' or 'LicenseRef-LICENSE'.");
        sb.AppendLine(GetHelpNameValue(nameof(AppLicense), AppLicense));

        c?.AppendLine();
        c?.AppendLine($"# Mandatory application vendor, group or creator. Example: Acme Ltd");
        sb.AppendLine(GetHelpNameValue(nameof(AppVendor), AppVendor));

        c?.AppendLine();
        c?.AppendLine($"# Optional application or vendor URL. Example: https://example.net");
        sb.AppendLine(GetHelpNameValue(nameof(AppUrl), AppUrl));


        c?.AppendLine();
        c?.AppendLine(breaker2);
        c?.AppendLine("# INTEGRATION");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Optional command name to launch the application from the terminal. For example, if");
        c?.AppendLine($"# {nameof(AppBase)} equals 'HelloWorld', the value here may be set to the same or a lower-case");
        c?.AppendLine($"# 'helloworld' variant. If empty, the application name will not be in the path and cannont be");
        c?.AppendLine($"# started from the command line. This value is not supported and ignored for {nameof(PackKind.AppImage)}");
        c?.AppendLine($"# and {nameof(PackKind.Flatpak)} and will be ignored. Default is empty.");
        sb.AppendLine(GetHelpNameValue(nameof(CommandName), CommandName));

        c?.AppendLine();
        c?.AppendLine($"# Flag (use 'true' or 'false') which indicates whether the application runs in the terminal.");
        sb.AppendLine(GetHelpNameValue(nameof(IsTerminal), IsTerminal));

        c?.AppendLine();
        c?.AppendLine($"# Optional path to a Linux desktop file (ignored for Windows). If empty (default), one will be");
        c?.AppendLine($"# generated automatically from known application information. A file may be supplied instead to");
        c?.AppendLine($"# provide for mime-types and internationalisation. If supplied, the file MUST contain the line:");
        c?.AppendLine($"# 'Exec=${{{MacroNames.LaunchExec}}}' in order to use the correct install location. Other macros");
        c?.AppendLine($"# may be used to help automate some content, and include: ${{{MacroNames.AppName}}}, ${{{MacroNames.AppId}}},");
        c?.AppendLine($"# ${{{MacroNames.AppSummary}}} etc. If required that no desktop be installed, set value to: '{PathNone}'");
        c?.AppendLine($"# Reference1: https://www.baeldung.com/linux/desktop-entry-files");
        c?.AppendLine($"# Reference2: https://specifications.freedesktop.org/desktop-entry-spec/desktop-entry-spec-latest.html");
        sb.AppendLine(GetHelpNameValue(nameof(DesktopEntry), DesktopEntry));

        c?.AppendLine();
        c?.AppendLine($"# Optional icon paths. The value may include multiple filenames separated with semicolon or");
        c?.AppendLine($"# given in multi-line form. Valid types are SVG, PNG and ICO (ignored on Linux). Note that the");
        c?.AppendLine($"# inclusion of a scalable SVG is preferable on Linux, whereas PNGs must be one of the standard");
        c?.AppendLine($"# sizes and MUST include the size in the filename in the form: 'name.32.png' or name.32x32.png'.");
        c?.AppendLine($"# Example: Assets/app.svg;Assets/app.24x24.png;Assets/app.32x32.png;Assets/app.ico");
        sb.AppendLine(GetHelpNameValue(nameof(Icons), Icons, true));

        c?.AppendLine();
        c?.AppendLine($"# Path to AppStream metadata file. It is optional, but recommended as it is used by software centers.");
        c?.AppendLine($"# The file content may embed supported macros such as, such as '${{APP_NAME}}' and '${{APP_ID}}' etc.");
        c?.AppendLine($"# to assist in automating fields. Refer: https://docs.appimage.org/packaging-guide/optional/appstream.html");
        c?.AppendLine($"# Example: Assets/metainfo.xml.");
        sb.AppendLine(GetHelpNameValue(nameof(MetaInfo), MetaInfo));

        c?.AppendLine();
        c?.AppendLine(breaker2);
        c?.AppendLine("# DOTNET PUBLISH");
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
        c?.AppendLine($"# or '-c' (configuration) here as they will be added (i.e. via {nameof(AppVersionRelease)}).");
        c?.AppendLine($"# Typically you want as a minimum: '-p:Version=${{{MacroNames.AppVersion}}} --self-contained true'. Additional");
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


        c?.AppendLine();
        c?.AppendLine(breaker2);
        c?.AppendLine("# PACKAGE OUTPUT");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Output directory, or subdirectory relative to this file. It will be created if it does not");
        c?.AppendLine($"# exist and will contain the final package output files. If empty, it defaults to the location");
        c?.AppendLine($"# of this file. Default: Deploy");
        sb.AppendLine(GetHelpNameValue(nameof(OutputDirectory), OutputDirectory));

        c?.AppendLine();
        c?.AppendLine($"# Boolean which sets whether to include the application version in the filename of the output");
        c?.AppendLine($"# package (i.e. 'HelloWorld-1.2.3-x86_64.AppImage'). It is ignored if the output filename");
        c?.AppendLine($"# is specified at command line.");
        sb.AppendLine(GetHelpNameValue(nameof(OutputVersion), OutputVersion));

        c?.AppendLine();
        c?.AppendLine(breaker2);
        c?.AppendLine("# APPIMAGE OPTIONS");
        c?.AppendLine(breaker2);

        c?.AppendLine();
        c?.AppendLine($"# Additional arguments for use with appimagetool. Useful for signing. Default is empty.");
        c?.AppendLine($"# See appimagetool --help. Example: --sign");
        sb.AppendLine(GetHelpNameValue(nameof(AppImageArgs), AppImageArgs));


        c?.AppendLine();
        c?.AppendLine(breaker2);
        c?.AppendLine("# FLATPAK OPTIONS");
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
        c?.AppendLine($"# Permissive example: --socket=wayland;--socket=x11;--filesystem=host;--share=network");
        c?.AppendLine($"# Less permissive: --socket=wayland;--socket=x11;--filesystem=home");
        c?.AppendLine($"# Refer: https://docs.flatpak.org/en/latest/sandbox-permissions.html");
        sb.AppendLine(GetHelpNameValue(nameof(FlatpakFinishArgs), FlatpakFinishArgs, true));

        c?.AppendLine();
        c?.AppendLine($"# Additional arguments for use with flatpak-builder. Useful for signing. Default is empty.");
        c?.AppendLine($"# See flatpak-builder --help. Example: --gpg-keys=FILE");
        sb.AppendLine(GetHelpNameValue(nameof(FlatpakBuilderArgs), FlatpakBuilderArgs));

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
                throw new ArgumentException($"Specify .conf file to use (directory does not contain exactly one file).");
            }

            // Dummy only (not in production)
            return "file.conf";
        }

        return path;
    }

    private static string GetRuntimeArch(string runtime)
    {
        if (runtime.EndsWith("-x64", StringComparison.InvariantCultureIgnoreCase))
        {
            return "x86_64";
        }

        if (runtime.EndsWith("-arm64", StringComparison.InvariantCultureIgnoreCase))
        {
            return "aarch64";
        }

        if (runtime.EndsWith("-arm", StringComparison.InvariantCultureIgnoreCase))
        {
            return "arm";
        }

        if (runtime.EndsWith("-x86", StringComparison.InvariantCultureIgnoreCase))
        {
            return "x86";
        }

        return runtime;
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

    private void AssertOK()
    {
        // Validate
        if (AppBase.Contains(' '))
        {
            throw new ArgumentException($"{nameof(AppBase)} must not contain space characters");
        }

        if (CommandName != null && CommandName.Contains(' '))
        {
            throw new ArgumentException($"{nameof(CommandName)} must not contain space characters");
        }

        if (!AppId.Contains('.') || AppId.Contains(' '))
        {
            throw new ArgumentException($"{nameof(AppId)} must be in reverse DNS form, i.e. 'net.example.appname'");
        }

        if (Args.Kind.IsLinux() && !Args.Kind.IsWindows())
        {
            if (DotnetProjectPath == PathNone && DotnetPostPublish.Count == 0)
            {
                throw new ArgumentException($"{nameof(DotnetPostPublish)} is mandatory where {nameof(DotnetProjectPath)} = {PathNone}");
            }
        }
    }

    private string? AssertAbsolutePath(string? source, string? path, bool allowNone)
    {
        if (!string.IsNullOrEmpty(path))
        {
            if (allowNone && path.Equals(PathNone, StringComparison.OrdinalIgnoreCase))
            {
                // Disabled - no path
                return PathNone;
            }

            if (!string.IsNullOrEmpty(source) && !Path.IsPathFullyQualified(path))
            {
                path = Path.Combine(source, path);
            }

            if (AssertFiles && !Path.Exists(path))
            {
                throw new FileNotFoundException($"File path not found {path}");
            }

            return path;
        }

        return null;
    }

    private IReadOnlyCollection<string> AssertAbsolutePath(string? source, IReadOnlyCollection<string> paths)
    {
        if (paths.Count != 0)
        {
            var list = new List<string>();

            foreach (var item in paths)
            {
                list.Add(AssertAbsolutePath(source, item, false) ?? "");
            }

            return list;
        }

        return Array.Empty<string>();
    }

    private IReadOnlyCollection<string> GetConfMultiline(string name, bool allowSemiColon, string? mustContain = null, string? mustStart = null)
    {
        var value = GetOptional(name, true);

        if (value != null)
        {
            char sep = '\n';
            value = value.ReplaceLineEndings("\n");

            if (allowSemiColon && !value.Contains('\n'))
            {
                sep = ';';
            }

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
