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

using System;
using System.IO;
using System.Reflection;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(nameof(HelloWorld));
            Console.WriteLine("Version: {0}", GetVersion());
#if DEBUG
            Console.WriteLine("Build: DEBUG");
#else
            Console.WriteLine("Build: RELEASE");
#endif
            Console.WriteLine();

            // Ensure arguments are passed
            Console.WriteLine("Args: {0}", string.Join(", ", args));

            // Working directory
            Console.WriteLine("GetCurrentDirectory: {0}", Directory.GetCurrentDirectory());

            // Executable directory
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine("AppDomain.CurrentDomain.BaseDirectory: {0}", AppDomain.CurrentDomain.BaseDirectory);

            // Executable path (warning IL3000 for "single-file" is expected)
            Console.WriteLine("Assembly.GetEntryAssembly().Location: {0}", Assembly.GetEntryAssembly()?.Location ?? "null");
            Console.WriteLine();

            // Look for sub-directory created by DotnetPostPublish - ensures custom content is packaged
            dir = Path.Combine(dir, "subdir");
            Console.WriteLine("Packaged subdir exists: " + Directory.Exists(dir));
            Console.WriteLine("Packaged subdir/file.test exists: " + File.Exists(Path.Combine(dir, "file.test")));
            Console.WriteLine("If the above is 'True', it indicates the PostPublish build script was run during the build");
            Console.WriteLine();

#if BUILDFLAG
            // Test for passing property from command line
            // NOTE. In some case, you might have to do a clean first
            Console.WriteLine("BUILDFLAG constant defined OK");
            Console.WriteLine("The build was called with --property:DefineConstants=BUILDFLAG");
#else
            Console.WriteLine("BUILDFLAG NOT defined");
            Console.WriteLine("Build package with publish arg: --property:DefineConstants=BUILDFLAG");
#endif

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to finish");
            Console.ReadKey(false);
            Console.WriteLine();
        }

        private static string GetVersion()
        {
            try
            {
                // Wasn't expecting this to work for:
                // -p:PublishSingleFile=true
                // But it seems to work OK
                var ea = Assembly.GetEntryAssembly();

                if (ea != null)
                {
                    var ver = ea.GetName().Version;

                    if (ver != null)
                    {
                        return ver.ToString();
                    }

                    throw new Exception($"{ver} is null");
                }

                throw new Exception($"{ea} is null");
            }
#if DEBUG
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
#else
            catch
            {
            }
#endif

            // Fallback
            return "Unknown";
        }
    }

}
