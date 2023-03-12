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
using KuiperZone.Utility.Yaap;

namespace KuiperZone.PupNet;

/// <summary>
/// Declares and decodes arguments.
/// </summary>
public class ArgumentReader
{
    public const string KindShortArg = "k";
    public const string KindLongArg = "kind";

    public const string RidShortArg = "r";
    public const string RidLongArg = "runtime";

    public const string BuildShortArg = "c";
    public const string BuildLongArg = "build";

    public const string AppVersionShortArg = "v";
    public const string AppVersionLongArg = "app-version";

    public const string PropertyShortArg = "p";
    public const string PropertyLongArg = "property";

    public const string ArchShortArg = "a";
    public const string ArchLongArg = "arch";

    public const string OutputShortArg = "o";
    public const string OutputLongArg = "output";

    public const string RunShortArg = "u";
    public const string RunLongArg = "run";

    public const string VerboseLongArg = "verbose";

    public const string SkipYesShortArg = "y";
    public const string SkipYesLongArg = "skip-yes";

    public const string NewShortArg = "n";
    public const string NewLongArg = "new";

    public const string AboutLongArg = "about";
    public const string VersionLongArg = "version";

    public const string HelpShortArg = "h";
    public const string HelpLongArg = "help";


    /// <summary>
    /// The default runtime.
    /// </summary>
    public readonly static string DefaultRuntime;

    /// <summary>
    /// The default package kind.
    /// </summary>
    public readonly static PackKind DefaultKind;

    private string _string;

    /// <summary>
    /// Static constructor.
    /// </summary>
    static ArgumentReader()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            DefaultKind = PackKind.AppImage;
        }
        else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DefaultKind = PackKind.WinSetup;
        }
        else
        {
            DefaultKind = PackKind.Zip;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                DefaultRuntime = "win-arm64";
            }
            else
            {
                DefaultRuntime = "win-x64";
            }
        }
        else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            DefaultRuntime = "osx-x64";
        }
        else
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            DefaultRuntime = "linux-arm64";
        }
        else
        {
            DefaultRuntime = "linux-x64";
        }
    }

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

        Value = args.Value;
        New = args.GetOrDefault(NewShortArg, NewLongArg, NewKind.None);
        Runtime = args.GetOrDefault(RidShortArg, RidLongArg, DefaultRuntime);
        Build = args.GetOrDefault(BuildShortArg, BuildLongArg, "Release");

        if (New == NewKind.None)
        {
            Value = GetDefaultValuePath(Value);
            Kind = AssertKind(args.GetOrDefault(KindShortArg, KindLongArg, DefaultKind));
            AppVersion = args.GetOrDefault(AppVersionShortArg, AppVersionLongArg, null);

            // Currently not implemented
            // The '=' in "-p DefineConstants=TESTFLAG" causes a problem for ArgumentParser
            // Property = args.GetOrDefault(PropertyShortArg, PropertyLongArg, null);

            Arch = args.GetOrDefault(ArchShortArg, ArchLongArg, null);
            Output = args.GetOrDefault(OutputShortArg, OutputLongArg, null);
            IsRun = args.GetOrDefault(RunShortArg, RunLongArg, false);
            IsVerbose = args.GetOrDefault(VerboseLongArg, false);
            IsSkipYes = args.GetOrDefault(SkipYesShortArg, SkipYesLongArg, false);

            ShowAbout = args.GetOrDefault(AboutLongArg, false);
            ShowVersion = args.GetOrDefault(VersionLongArg, false);
            ShowHelp = args.GetOrDefault(HelpShortArg, HelpLongArg, false);
        }
    }

    /// <summary>
    /// Gets the unknown arg value. Typically file name.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Gets new conf filename.
    /// </summary>
    public NewKind New { get; }

    /// <summary>
    /// Gets the dotnet runtime-id.
    /// </summary>
    public string Runtime { get; }

    /// <summary>
    /// Gets the target build configuration (Release or Debug).
    /// </summary>
    public string Build { get; }

    /// <summary>
    /// Gets the package kinds.
    /// </summary>
    public PackKind Kind { get; }

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public string? AppVersion { get; }

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
    /// Gets whether to skip yes.
    /// </summary>
    public bool IsSkipYes { get; }

    /// <summary>
    /// Gets whether to show about information.
    /// </summary>
    public bool ShowAbout { get; }

    /// <summary>
    /// Gets whether to show version only.
    /// </summary>
    public bool ShowVersion { get; }

    /// <summary>
    /// Gets whether to show help.
    /// </summary>
    public bool ShowHelp { get; }

    /// <summary>
    /// Returns a command help string.
    /// </summary>
    public static string GetHelperText()
    {
        var indent = "  ";

        var sb = new StringBuilder();

        sb.AppendLine("Usage:");
        sb.AppendLine($"{indent}{Program.CommandName} [file.conf] [-option-n value-n]");
        sb.AppendLine();
        sb.AppendLine("Example:");
        sb.AppendLine($"{indent}{Program.CommandName} file.conf -{SkipYesShortArg} -{RidShortArg} linux-arm64");
        sb.AppendLine();
        sb.AppendLine($"If conf file is omitted, one in the working directory will be selected.");

        sb.AppendLine();
        sb.AppendLine("Build Options:");

        sb.AppendLine($"{indent}-{KindShortArg}, --{KindLongArg} value");
        sb.AppendLine($"{indent}Package output kind. Default is {DefaultKind}");
        sb.AppendLine($"{indent}Value must be one of: {string.Join(",", Enum.GetNames<PackKind>())}");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{RidShortArg}, --{RidLongArg} value");
        sb.AppendLine($"{indent}Dotnet publish runtime identifier. Default: {DefaultRuntime}.");
        sb.AppendLine($"{indent}Valid examples include: 'linux-x64' and 'linux-arm64'.");
        sb.AppendLine($"{indent}See: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{BuildShortArg}, --{BuildLongArg} value");
        sb.AppendLine($"{indent}Optional build target (or 'Configuration' is dotnet terminology).");
        sb.AppendLine($"{indent}Value should be 'Release' or 'Debug'. Default: Release.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{AppVersionShortArg}, --{AppVersionLongArg} value");
        sb.AppendLine($"{indent}Specifies application version-release in form 'VERSION[RELEASE]', where value in square");
        sb.AppendLine($"{indent}brackets is package release. Overrides {nameof(ConfigurationReader.AppVersionRelease)} in conf file.");
        sb.AppendLine($"{indent}Example: 1.2.3[1].");

        /*
        DISABLED
        sb.AppendLine();
        sb.AppendLine($"{indent}-{PropertyShortArg}, --{PropertyLongArg} value");
        sb.AppendLine($"{indent}Specifies a property to be supplied to dotnet publish command. Do not use for");
        sb.AppendLine($"{indent}app versioning. Example: -{PropertyShortArg} DefineConstants=TRACE;DEBUG");
        */
        sb.AppendLine();
        sb.AppendLine($"{indent}-{ArchShortArg}, --{ArchLongArg} value");
        sb.AppendLine($"{indent}Force target architecture, i.e. as 'x86_64' or 'aarch64'. Note this is optional and");
        sb.AppendLine($"{indent}not normally necessary as, in most cases, the architecture is defined by the dotnet");
        sb.AppendLine($"{indent}runtime-id and detected automatically. However, in the event of a problem, the value");
        sb.AppendLine($"{indent}may be supplied explicitly.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{OutputShortArg}, --{OutputLongArg} value");
        sb.AppendLine($"{indent}Package output filename. If omitted, the output name is derived from the application");
        sb.AppendLine($"{indent}name, version and architecture. Example: -{OutputShortArg} AppName.AppImage");
        sb.AppendLine();
        sb.AppendLine($"{indent}--{VerboseLongArg} [flag]");
        sb.AppendLine($"{indent}Indicates verbose output.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{RunShortArg}, --{RunLongArg} [flag]");
        sb.AppendLine($"{indent}Performs a test run of the application after successful build.");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{SkipYesShortArg}, --{SkipYesLongArg} [flag]");
        sb.AppendLine($"{indent}Skips confirmation prompts (assumes yes).");

        sb.AppendLine();
        sb.AppendLine("Other Options:");

        sb.AppendLine();
        sb.AppendLine($"{indent}-{NewShortArg}, --{NewLongArg} [value]");
        sb.AppendLine($"{indent}Creates a new empty conf or asset file for new project. A base file name may optionally");
        sb.AppendLine($"{indent}be given. Valid values are : {NewKind.Conf}, {NewKind.Desktop}, {NewKind.Meta} and {NewKind.All}.");
        sb.AppendLine($"{indent}Example: {Program.CommandName} basename -{NewShortArg} {NewKind.All}");
        sb.AppendLine();
        sb.AppendLine($"{indent}-{HelpShortArg}, --{HelpLongArg} [flag]");
        sb.AppendLine($"{indent}Show help information.");
        sb.AppendLine();
        sb.AppendLine($"{indent}--{VersionLongArg} [flag]");
        sb.AppendLine($"{indent}Show version information.");
        sb.AppendLine();
        sb.AppendLine($"{indent}--{AboutLongArg} [flag]");
        sb.Append($"{indent}Show about information.");

        return sb.ToString();
    }

    /// <summary>
    /// Returns true if the RID appears to be windows.
    /// </summary>
    public bool IsWindowsRuntime()
    {
        return !string.IsNullOrEmpty(Runtime) && Runtime.StartsWith("win", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override string ToString()
    {
        return _string;
    }

    private static string? GetDefaultValuePath(string? path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            return path;
        }

        var files = Directory.GetFiles("./", "*.conf", SearchOption.TopDirectoryOnly);
        return files.Length == 1 ? files[0] : null;
    }

    private static PackKind AssertKind(PackKind pack)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && pack.IsLinux())
        {
            return pack;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && pack.IsWindows())
        {
            return pack;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && pack.IsOsx())
        {
            return pack;
        }

        throw new ArgumentException($"Package kind {pack} cannot be built on this platform");
    }

}
