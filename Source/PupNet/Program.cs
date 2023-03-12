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

using System.Reflection;

namespace KuiperZone.PupNet;

internal class Program
{
    /// <summary>
    /// Gets the program name.
    /// </summary>
    public const string CommandName = "PupNet";

    /// <summary>
    /// Gets the program product name.
    /// </summary>
    public const string ProductName = "PupNet";

    /// <summary>
    /// Gets the program product name.
    /// </summary>
    public const string Copyright = "Copyright © Andy Thomas 2022-23";

    /// <summary>
    /// Gets the project URL.
    /// </summary>
    public const string ProjectUrl = "https://github.com/kuiperzone/PupNet";

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

            if (decoder.New != NewKind.None)
            {
                CreateNewFiles(decoder);
                return 0;
            }

            if (decoder.ShowVersion)
            {
                Console.WriteLine($"{ProductName} {Version}");
                return 0;
            }

            if (decoder.ShowAbout)
            {
                Console.WriteLine($"{ProductName} {Version}");
                Console.WriteLine($"{Copyright}");
                Console.WriteLine($"{ProjectUrl}");

                Console.WriteLine();
                Console.WriteLine($"{ProductName} is free software: you can redistribute it and/or modify it under");
                Console.WriteLine($"the terms of the GNU General Public License as published by the Free Software");
                Console.WriteLine($"Foundation, either version 3 of the License, or (at your option) any later version.");

                Console.WriteLine();
                Console.WriteLine($"{ProductName} is distributed in the hope that it will be useful, but WITHOUT");
                Console.WriteLine($"ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS");
                Console.WriteLine($"FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.");

                Console.WriteLine();
                Console.WriteLine("AppImageKit:");
                Console.WriteLine("Copyright (c) 2004-20 Simon Peter");

                if (AppImageBuilder.AppImageTool != null)
                {
                    try
                    {
                        var ops = new FileOps();
                        ops.ShowCommands = false;
                        ops.Execute($"{AppImageBuilder.AppImageTool} --version");
                    }
                    catch
                    {
                    }
                }

                Console.WriteLine();
                return 0;
            }

            if (decoder.ShowHelp)
            {
                Console.WriteLine($"{ProductName} {Version}");
                Console.WriteLine();
                Console.WriteLine(ArgumentReader.GetHelperText());

                Console.WriteLine();
                Console.WriteLine("Macro Reference:");
                Console.WriteLine("The following macros (with example values) are supported. See online help.");
                Console.WriteLine();
                Console.WriteLine(new BuildMacros().ToString());
                Console.WriteLine();

                return 0;
            }

            // BUILD AND RUN
            Console.WriteLine($"{ProductName} {Version}");
            Console.WriteLine($"Conf File: {decoder.Value ?? "[None]"}");
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
        bool all = args.New == NewKind.All;

        if (all || args.New == NewKind.Conf)
        {
            CreateNewSingleFile(NewKind.Conf, args);
        }

        if (all || args.New == NewKind.Desktop)
        {
            CreateNewSingleFile(NewKind.Desktop, args);
        }

        if (all || args.New == NewKind.Meta)
        {
            CreateNewSingleFile(NewKind.Meta, args);
        }
    }

    private static void CreateNewSingleFile(NewKind kind, ArgumentReader args)
    {
        var path = GetNewPath(kind, args.Value);
        var name = Path.GetFileName(path);

        if (!File.Exists(path) || new ConfirmPrompt($"{name} exists. Replace?").Wait())
        {
            var fop = new FileOps();
            string? baseName = null;

            if (args.New == NewKind.All)
            {
                // Link conf to expected other meta files
                baseName = Path.GetFileNameWithoutExtension(GetNewPath(NewKind.Desktop, args.Value));
            }

            switch (kind)
            {
                case NewKind.Conf:
                    fop.WriteFile(path, new ConfigurationReader(baseName).ToString());
                    break;
                case NewKind.Desktop:
                    fop.WriteFile(path, MetaTemplates.Desktop);
                    break;
                case NewKind.Meta:
                    fop.WriteFile(path, MetaTemplates.MetaInfo);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid {kind} value");
            }
        }
    }

    private static string GetNewPath(NewKind kind, string? baseName)
    {
        var ext = kind.GetFileExt();
        var def = (kind == NewKind.Conf ? CommandName : "app") + ext;

        if (!string.IsNullOrEmpty(baseName))
        {
            if (baseName.EndsWith('/') || baseName.EndsWith('\\'))
            {
                return Path.Combine(baseName, def);
            }

            if (!baseName.EndsWith(ext))
            {
                return baseName + ext;
            }

            return baseName;
        }

        return def;
    }
}
