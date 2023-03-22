// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-23
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

using System.Reflection;
using System.Runtime.InteropServices;
using KuiperZone.PupNet.Builders;

namespace KuiperZone.PupNet;

internal class Program
{
    /// <summary>
    /// Gets the program name.
    /// </summary>
    public const string CommandName = "pupnet";

    /// <summary>
    /// Gets the conf file extension.
    /// </summary>
    public const string ConfExt = ".pupnet.conf";

    /// <summary>
    /// Gets the desktop file extension.
    /// </summary>
    public const string DesktopExt = ".desktop";

    /// <summary>
    /// Gets the AppStream metadata extension.
    /// </summary>
    public const string MetaExt = ".metainfo.xml";

    /// <summary>
    /// Gets the program product name.
    /// </summary>
    public const string ProductName = "PupNet Deploy";

    /// <summary>
    /// Gets the program product name.
    /// </summary>
    public const string Copyright = "Copyright © Andy Thomas 2022-23";

    /// <summary>
    /// Gets the project URL.
    /// </summary>
    public const string ProjectUrl = "https://github.com/kuiperzone/PupNet-Deploy";

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public static string Version;

    static Program()
    {
        Version =  Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ??
            throw new InvalidOperationException("Failed to get Assembly version");
    }

    internal static int Main(string[] args)
    {
        try
        {
            var decoder = new ArgumentReader(args);

            if (decoder.NewFile != null)
            {
                CreateNewFiles(decoder);
                return 0;
            }

            if (decoder.ShowVersion)
            {
                Console.WriteLine($"{ProductName} {Version}");
                Console.WriteLine($"{Copyright}");
                Console.WriteLine($"{ProjectUrl}");
                Console.WriteLine($"System: {RuntimeConverter.SystemOS} {RuntimeInformation.OSArchitecture}");

                Console.WriteLine();
                Console.WriteLine($"{ProductName} is free software: you can redistribute it and/or modify it under");
                Console.WriteLine($"the terms of the GNU Affero General Public License as published by the Free Software");
                Console.WriteLine($"Foundation, either version 3 of the License, or (at your option) any later version.");

                Console.WriteLine();
                Console.WriteLine($"{ProductName} is distributed in the hope that it will be useful, but WITHOUT");
                Console.WriteLine($"ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS");
                Console.WriteLine($"FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.");

                if (AppImageBuilder.AppImageTool != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Third-party Tools:");
                    Console.WriteLine();
                    Console.WriteLine($"AppImageKit: {AppImageBuilder.AppImageVersion}");
                    Console.WriteLine("Copyright (C) 2004-20 Simon Peter");
                }

                Console.WriteLine();

                return 0;
            }

            if (decoder.ShowHelp != null)
            {
                Console.WriteLine($"{ProductName} {Version}");
                Console.WriteLine($"See also: {ProjectUrl}");
                Console.WriteLine();

                if (decoder.ShowHelp == "macro" || decoder.ShowHelp == "macros")
                {
                    Console.WriteLine("MACROS:");

                    Console.WriteLine("Macro variables may be used with the following configuration items:");

                    Console.WriteLine($"{nameof(ConfigurationReader.DesktopFile)}, {nameof(ConfigurationReader.MetaFile)}, {nameof(ConfigurationReader.DotnetPublishArgs)}, " +
                    $"{nameof(ConfigurationReader.DotnetPostPublish)} and {nameof(ConfigurationReader.DotnetPostPublishOnWindows)}.");

                    Console.WriteLine();
                    Console.WriteLine("IMPORTANT: Always use the ${MACRO_NAME} form, and not $MACRO_NAME.");
                    Console.WriteLine();
                    Console.WriteLine(new MacrosExpander().ToString(true));
                    Console.WriteLine();
                    return 0;
                }

                if (decoder.ShowHelp == "conf")
                {
                    Console.WriteLine(new ConfigurationReader(true).ToString(DocStyles.Reference));
                    Console.WriteLine();
                    return 0;
                }

                Console.WriteLine(ArgumentReader.GetHelperText());
                Console.WriteLine();
                return 0;
            }

            if (decoder.Parser.GetOrDefault("update-conf", false))
            {
                // Undocumented feature. Internal use
                var conf = new ConfigurationReader(decoder, false);
                var path = conf.Reader.Filepath;

                var ops = new FileOps();
                ops.CopyFile(path, path + ".old");

                var style = decoder.IsVerbose ? DocStyles.Comments : DocStyles.NoComments;
                ops.WriteFile(path, conf.ToString(style), true);
                return 0;
            }

            // BUILD AND RUN
            Console.WriteLine($"{ProductName} {Version}");
            Console.WriteLine($"Configuration: {decoder.Value ?? "[None]"}");
            Console.WriteLine();

            new BuildHost(decoder).Run();

            Console.WriteLine();
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine();
            Console.WriteLine("FAILED");

#if DEBUG
            Console.WriteLine(e);
#else
            Console.WriteLine(e.Message);
#endif
            Console.WriteLine();
            return 1;
        }
    }

    private static void CreateNewFiles(ArgumentReader args)
    {
        bool ok = false;
        bool all = args.NewFile == ArgumentReader.NewAllValue;

        if (all || args.NewFile == ArgumentReader.NewConfValue || args.NewFile == "true")
        {
            ok = true;
            CreateNewSingleFile(ArgumentReader.NewConfValue, args);
        }

        if (all || args.NewFile == ArgumentReader.NewDesktopValue)
        {
            ok = true;
            CreateNewSingleFile(ArgumentReader.NewDesktopValue, args);
        }

        if (all || args.NewFile == ArgumentReader.NewMetaValue)
        {
            ok = true;
            CreateNewSingleFile(ArgumentReader.NewMetaValue, args);
        }

        if (!ok)
        {
            throw new InvalidOperationException($"Invalid -{ArgumentReader.NewShortArg} or --{ArgumentReader.NewLongArg} value {args.NewFile}");
        }
    }

    private static void CreateNewSingleFile(string newKind, ArgumentReader args)
    {
        var path = GetNewPath(newKind, args.Value);
        var name = Path.GetFileName(path);

        if (!File.Exists(path) || new ConfirmPrompt($"{name} exists. Replace?").Wait())
        {
            var fop = new FileOps();
            string? baseName = null;

            if (newKind == ArgumentReader.NewAllValue)
            {
                // Link conf to expected other meta files
                baseName = Path.GetFileNameWithoutExtension(GetNewPath(newKind, args.Value));
            }

            switch (newKind)
            {
                case ArgumentReader.NewConfValue:
                    var style = args.IsVerbose ? DocStyles.Comments : DocStyles.NoComments;
                    fop.WriteFile(path, new ConfigurationReader(false, baseName).ToString(style), true);
                    break;
                case ArgumentReader.NewDesktopValue:
                    fop.WriteFile(path, MetaTemplates.Desktop, true);
                    break;
                case ArgumentReader.NewMetaValue:
                    fop.WriteFile(path, MetaTemplates.MetaInfo, true);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid {newKind} value");
            }
        }
    }

    private static string GetNewPath(string newKind, string? baseName)
    {
        const string Default = "app";
        string ext = ConfExt;

        switch (newKind)
        {
            case ArgumentReader.NewDesktopValue:
                ext = DesktopExt;
                break;
            case ArgumentReader.NewMetaValue:
                ext = MetaExt;
                break;
        }

        if (!string.IsNullOrEmpty(baseName))
        {
            if (baseName.EndsWith('/') || baseName.EndsWith('\\'))
            {
                // Provided a directory
                return Path.Combine(baseName, Default);
            }

            if (!baseName.EndsWith(ext))
            {
                return baseName + ext;
            }

            return baseName;
        }

        return Default + ext;
    }
}
