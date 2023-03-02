// -----------------------------------------------------------------------------
// PROJECT   : Pubpak
// COPYRIGHT : Andy Thomas (C) 2022-23
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/Pubpak
//
// Pubpak is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// Pubpak is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with Pubpak. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

using System.Reflection;

namespace KuiperZone.Pubpak;

internal class Program
{
    /// <summary>
    /// Gets the program name.
    /// </summary>
    public const string CommandName = "pubpak";

    /// <summary>
    /// Gets the program product name.
    /// </summary>
    public const string ProductName = "PUBPAK for .NET";

    /// <summary>
    /// Gets the program product name.
    /// </summary>
    public const string Copyright = "Copyright © Andy Thomas 2022-23";

    /// <summary>
    /// Gets the project URL.
    /// </summary>
    public const string ProjectUrl = "https://github.com/kuiperzone/Pubpak";

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
            var decoder = new ArgDecoder(args);

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
                return 0;
            }

            if (decoder.ShowHelp)
            {
                Console.WriteLine($"{ProductName} {Version}");
                Console.WriteLine();
                Console.WriteLine(ArgDecoder.GetHelperText());
                Console.WriteLine();
                return 0;
            }

            // BUILD AND RUN
            Console.WriteLine($"{ProductName} {Version}");
            Console.WriteLine($"Conf File: {decoder.Value ?? "[None]"}");
            Console.WriteLine();

            new PackageBuilder(decoder).Run();
            return 0;
        }
        catch (Exception e)
        {
#if DEBUG
            Console.WriteLine(e);
#else
            Console.WriteLine(e.Message);
#endif
            Console.WriteLine();
            return 1;
        }
    }

    private static void CreateNewFiles(ArgDecoder args)
    {
        if (args.New == NewKind.Conf || args.New == NewKind.All)
        {
            CreateNewSingleFile(NewKind.Conf, args.Value);
        }

        if (args.New == NewKind.Desktop || args.New == NewKind.All)
        {
            CreateNewSingleFile(NewKind.Desktop, args.Value);
        }

        if (args.New == NewKind.Meta || args.New == NewKind.All)
        {
            CreateNewSingleFile(NewKind.Meta, args.Value);
        }
    }

    private static void CreateNewSingleFile(NewKind kind, string? bas)
    {
        var path = GetNewPath(kind, bas);
        var name = Path.GetFileName(path);

        if (!File.Exists(path) || new ConfirmPrompt($"{name} exists. Replace?").Wait())
        {
            var fop = new FileOps();

            switch (kind)
            {
                case NewKind.Conf:
                    fop.WriteFile(path, new ConfDecoder().ToString());
                    break;
                case NewKind.Desktop:
                    fop.WriteFile(path, BuildAssets.DesktopTemplate);
                    break;
                case NewKind.Meta:
                    fop.WriteFile(path, BuildAssets.AppMetaTemplate);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid {kind} value");
            }
        }
    }

    private static string GetNewPath(NewKind kind, string? bas)
    {
        var ext = kind.GetFileExt();
        var def = (kind == NewKind.Conf ? CommandName : "app") + ext;

        if (!string.IsNullOrEmpty(bas))
        {
            if (Directory.Exists(bas))
            {
                return Path.Combine(bas, def);
            }

            if (!def.EndsWith(ext))
            {
                return bas + kind.GetFileExt();
            }

            return bas;
        }

        return def;
    }
}
