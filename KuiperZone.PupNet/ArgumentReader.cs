// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-25
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

using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Declares and decodes arguments.
/// </summary>
public class ArgumentReader
{
    public const char KindShortArg = 'k';
    public const string KindLongArg = "kind";

    public const char RidShortArg = 'r';
    public const string RidLongArg = "runtime";

    public const char BuildShortArg = 'c';
    public const string BuildLongArg = "build";

    public const char ProjectShortArg = 'j';
    public const string ProjectLongArg = "project";

    public const char CleanShortArg = 'e';
    public const string CleanLongArg = "clean";

    public const char VersionReleaseShortArg = 'v';
    public const string VersionReleaseLongArg = "app-version";

    public const char PropertyShortArg = 'p';
    public const string PropertyLongArg = "property";

    public const string ArchLongArg = "arch";

    public const char OutputShortArg = 'o';
    public const string OutputLongArg = "output";

    public const char RunShortArg = 'u';
    public const string RunLongArg = "run";

    public const string VerboseLongArg = "verbose";

    public const char SkipYesShortArg = 'y';
    public const string SkipYesLongArg = "skip-yes";

    public const char NewShortArg = 'n';
    public const string NewLongArg = "new";

    public const string UpgradeConfLongArg = "upgrade-conf";

    public const string VersionLongArg = "version";

    public const char HelpShortArg = 'h';
    public const string HelpLongArg = "help";

    // New files
    public const string NewConfValue = "conf";
    public const string NewDesktopValue = "desktop";
    public const string NewMetaValue = "meta";
    public const string NewAllValue = "all";
    public const string NewAllowedSequence = $"conf|desktop|meta|all";

    private readonly string _string;

    /// <summary>
    /// Default constructor. Values are defaults only.
    /// </summary>
    public ArgumentReader()
        : this(new ArgumentParser(""))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public ArgumentReader(string[] args)
        : this(new ArgumentParser(args))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public ArgumentReader(string args)
        : this(new ArgumentParser(args))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public ArgumentReader(ArgumentParser args)
    {
        _string = args.ToString();
        Parser = args;

        Value = args.FirstValueOrNull;

        NewFile = args.GetOrDefault(NewShortArg, NewLongArg, null)?.ToLowerInvariant();

        Arch = args.GetOrDefault(ArchLongArg, null);
        Runtime = args.GetOrDefault(RidShortArg, RidLongArg, RuntimeConverter.DefaultRuntime);
        Build = args.GetOrDefault(BuildShortArg, BuildLongArg, "Release");
        Project = args.GetOrDefault(ProjectShortArg, ProjectLongArg, null);
        VersionRelease = args.GetOrDefault(VersionReleaseShortArg, VersionReleaseLongArg, null);
        Clean = args.GetOrDefault(CleanShortArg, CleanLongArg, false);
        IsVerbose = args.GetOrDefault(VerboseLongArg, false);
        IsUpgradeConf = args.GetOrDefault(UpgradeConfLongArg, false);
        IsSkipYes = args.GetOrDefault(SkipYesShortArg, SkipYesLongArg, false) || GetEnvironmentFlag("CI");

        if (NewFile == null)
        {
            Kind = AssertEnum<PackageKind>(KindShortArg, KindLongArg,
                args.GetOrDefault(KindShortArg, KindLongArg, new RuntimeConverter(Runtime).DefaultPackage.ToString()));

            Property = args.GetOrDefault(PropertyShortArg, PropertyLongArg, null);
            Output = args.GetOrDefault(OutputShortArg, OutputLongArg, null);
            IsRun = args.GetOrDefault(RunShortArg, RunLongArg, false);

            ShowVersion = args.GetOrDefault(VersionLongArg, false);
            ShowHelp = args.GetOrDefault(HelpShortArg, HelpLongArg, null)?.ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets internal parser instance.
    /// </summary>
    public ArgumentParser Parser { get; }

    /// <summary>
    /// Gets the unknown arg value. Typically file name.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Gets new conf filename.
    /// </summary>
    public string? NewFile { get; }

    /// <summary>
    /// Gets the dotnet runtime-id.
    /// </summary>
    public string Runtime { get; }

    /// <summary>
    /// Gets the target build configuration (Release or Debug).
    /// </summary>
    public string Build { get; }

    /// <summary>
    /// Gets the project path override.
    /// </summary>
    public string? Project { get; }

    /// <summary>
    /// Gets whether to call dotnet clean prior to publish.
    /// </summary>
    public bool Clean { get; }

    /// <summary>
    /// Gets the package kinds.
    /// </summary>
    public PackageKind Kind { get; }

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public string? VersionRelease { get; }

    /// <summary>
    /// Gets the property string.
    /// </summary>
    public string? Property { get; }

    /// <summary>
    /// Gets the architecture. Explicit only.
    /// </summary>
    public string? Arch { get; }

    /// <summary>
    /// Gets the output string.
    /// </summary>
    public string? Output { get; }

    /// <summary>
    /// Gets whether to run.
    /// </summary>
    public bool IsRun { get; }

    /// <summary>
    /// Gets whether verbose output.
    /// </summary>
    public bool IsVerbose { get; }

    /// <summary>
    /// Gets whether to update configuration.
    /// </summary>
    public bool IsUpgradeConf { get; }

    /// <summary>
    /// Gets whether to skip yes. Also set to true if "CI" environment variable is true.
    /// See: https://docs.gitlab.com/ee/ci/variables/predefined_variables.html
    /// </summary>
    public bool IsSkipYes { get; }

    /// <summary>
    /// Gets whether to show version only.
    /// </summary>
    public bool ShowVersion { get; }

    /// <summary>
    /// Gets whether to show help. "args", "macros" or "conf".
    /// </summary>
    public string? ShowHelp { get; }

    /// <summary>
    /// Returns a command help string.
    /// </summary>
    public static string GetHelperText()
    {
        var indent = "  ";

        var sb = new StringBuilder();

        sb.AppendLine("USAGE:");
        sb.AppendLine($"{indent}{Program.CommandName} [<file{Program.ConfExt}>] [--option-n value-n]");
        sb.AppendLine();
        sb.AppendLine("Example:");
        sb.AppendLine($"{indent}{Program.CommandName} app.{Program.ConfExt} -{SkipYesShortArg} -{RidShortArg} linux-arm64");
        sb.AppendLine();
        sb.AppendLine($"Always give {Program.ConfExt} file first. If {Program.ConfExt} file is omitted, the default is the one in the working directory.");

        sb.AppendLine();
        sb.AppendLine("Build Options:");

        sb.AppendLine($"{indent}-{KindShortArg}, --{KindLongArg} <{string.Join('|', Enum.GetNames<PackageKind>()).ToLowerInvariant()}>");
        sb.AppendLine($"{indent}Package output kind. If omitted, one is chosen according to the runtime ({PackageKind.AppImage} on linux).");
        sb.AppendLine($"{indent}Example: {Program.CommandName} HelloWorld -{KindShortArg} {PackageKind.Flatpak}");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{RidShortArg}, --{RidLongArg} <linux-x64|linux-arm64|win-x64...>");
        sb.AppendLine($"{indent}Dotnet publish runtime identifier. Default: {RuntimeConverter.DefaultRuntime}.");
        sb.AppendLine($"{indent}Valid examples include: 'linux-x64', 'linux-arm64' and 'win-x64' etc.");
        sb.AppendLine($"{indent}See: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{BuildShortArg}, --{BuildLongArg} <Release|Debug>");
        sb.AppendLine($"{indent}Optional build target (or 'Configuration' in dotnet terminology).");
        sb.AppendLine($"{indent}Value should be 'Release' or 'Debug' only. Default: Release.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{ProjectShortArg}, --{ProjectLongArg} <csproj path>");
        sb.AppendLine($"{indent}Optional path to the .csproj file or directory containing it. Overrides {nameof(ConfigurationReader.DotnetProjectPath)}");
        sb.AppendLine($"{indent}in the conf file.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{CleanShortArg}, --{CleanLongArg}");
        sb.AppendLine($"{indent}Call 'dotnet clean' prior to 'dotnet publish'. Default: false.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{VersionReleaseShortArg}, --{VersionReleaseLongArg} <version[release]>");
        sb.AppendLine($"{indent}Specifies application version-release in form 'version[release]', where value in square");
        sb.AppendLine($"{indent}brackets is package release. Overrides {nameof(ConfigurationReader.AppVersionRelease)} in conf file.");
        sb.AppendLine($"{indent}Example: 1.2.3[1].");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{PropertyShortArg}, --{PropertyLongArg} <name=value>");
        sb.AppendLine($"{indent}Specifies a property to be supplied to dotnet publish command. Do not use for app versioning.");
        sb.AppendLine($"{indent}Separate multiple values with comma. Example: -{PropertyShortArg} DefineConstants=TRACE,DEBUG");
        sb.AppendLine();
        sb.AppendLine($"{indent}--{ArchLongArg} <value>");
        sb.AppendLine($"{indent}Force target architecture, i.e. as 'x86_64', 'amd64' or 'aarch64' etc. Note that this is");
        sb.AppendLine($"{indent}not normally necessary as, in most cases, the architecture is defined by the dotnet runtime-id");
        sb.AppendLine($"{indent}and will be successfully detected automatically. However, in the event of a problem, the value");
        sb.AppendLine($"{indent}explicitly supplied here will be used to override. It should be provided in the form");
        sb.AppendLine($"{indent}expected by the underlying package builder (i.e. rpmbuild, appimagetool or InnoSetup etc.).");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{OutputShortArg}, --{OutputLongArg} <filename>");
        sb.AppendLine($"{indent}Force package output filename. Normally this is derived from parameters in the configuration.");
        sb.AppendLine($"{indent}This value will be used to override. Example: -{OutputShortArg} AppName.AppImage");
        sb.AppendLine();
        sb.AppendLine($"{indent}--{VerboseLongArg}");
        sb.AppendLine($"{indent}Indicates verbose output when building. It is used also with --{NewLongArg} option.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{RunShortArg}, --{RunLongArg}");
        sb.AppendLine($"{indent}Performs a test run of the application after successful build (where supported).");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{SkipYesShortArg}, --{SkipYesLongArg}");
        sb.AppendLine($"{indent}Skips confirmation prompts (assumes yes).");

        sb.AppendLine();
        sb.AppendLine("Other Options:");

        sb.AppendLine();
        sb.AppendLine($"{indent}-{NewShortArg}, --{NewLongArg} <{NewAllowedSequence}> [--{VerboseLongArg}] [--{SkipYesLongArg}]");
        sb.AppendLine($"{indent}Creates a new empty conf file or associated file (i.e. desktop of metadata) for a new project.");
        sb.AppendLine($"{indent}A base file name may optionally be given. If --{VerboseLongArg} is used, a configuration file with");
        sb.AppendLine($"{indent}documentation comments is generated. Use 'all' to generate a full set of configuration assets.");
        sb.AppendLine($"{indent}Example: {Program.CommandName} HelloWorld -{NewShortArg} {NewAllValue} --{VerboseLongArg}");
        sb.AppendLine();
        sb.AppendLine($"{indent}--{UpgradeConfLongArg} [--{VerboseLongArg}] [--{SkipYesLongArg}]");
        sb.AppendLine($"{indent}Upgrades supplied {Program.ConfExt} file to latest version parameters. For example, if the");
        sb.AppendLine($"{indent}conf file was created with program version 1.1 and new parameters where added in version");
        sb.AppendLine($"{indent}1.2, this command will upgrade the file by adding new parameters with default values.");
        sb.AppendLine($"{indent}If --{VerboseLongArg} is used, a configuration file with documentation comments is generated.");
        sb.AppendLine($"{indent}Example: {Program.CommandName} file{Program.ConfExt} --{UpgradeConfLongArg} --{VerboseLongArg}");

        sb.AppendLine();
        sb.AppendLine($"{indent}-{HelpShortArg}, --{HelpLongArg} <args|macro|conf>");
        sb.AppendLine($"{indent}Show help information. Optional value specifies what kind of information to display.");
        sb.AppendLine($"{indent}Default is 'args'. Example: {Program.CommandName} -{HelpShortArg} macro");
        sb.AppendLine();
        sb.AppendLine($"{indent}--{VersionLongArg}");
        sb.AppendLine($"{indent}Show version and associated information.");

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override string ToString()
    {
        return _string;
    }

    private static T AssertEnum<T>(char sname, string lname, string value) where T : struct, Enum
    {
        // Provides more error information
        if (Enum.TryParse<T>(value, true, out T rslt))
        {
            return rslt;
        }

        throw new ArgumentException($"Invalid or absent value for -{sname}, --{lname}\n" +
            "Use one of: " + string.Join(',', Enum.GetValues<T>()));
    }

    private static bool GetEnvironmentFlag(string name)
    {
        try
        {
            return name.Length != 0 && Environment.GetEnvironmentVariable(name)?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch
        {
            return false;
        }
    }

}
