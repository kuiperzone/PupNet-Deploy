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

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;

namespace KuiperZone.PupNet;

/// <summary>
/// Wraps expected file operations with desired console output behavior.
/// This is mainly for convenience and aesthetic/information purposes.
/// </summary>
public class FileOps(string? root = null)
{
    /// <summary>
    /// Gets the root directory for operations. This is used for display purposes only where directory path is
    /// removed from the displayed path.
    /// </summary>
    public string? Root { get; } = root;

    /// <summary>
    /// Gets or sets whether to display commands and path information. Default is true.
    /// </summary>
    public bool ShowCommands { get; set; } = true;

    /// <summary>
    /// Gets a list of files currently under dir, including sub-paths.
    /// Output paths are relative to given directory. Does not pick up symlinks.
    /// </summary>
    public static string[] ListFiles(string dir, string filter = "*")
    {
        var opts = new EnumerationOptions();
        opts.RecurseSubdirectories = true;
        opts.ReturnSpecialDirectories = false;
        opts.IgnoreInaccessible = true;
        opts.MaxRecursionDepth = 20;

        var files = Directory.GetFiles(dir, filter, System.IO.SearchOption.AllDirectories);

        for (int n = 0; n < files.Length; ++n)
        {
            files[n] = Path.GetRelativePath(dir, files[n]);
        }

        return files;
    }

    /// <summary>
    /// Asserts file exist. Does nothing if file is null.
    /// </summary>
    public void AssertExists(string? filepath)
    {
        if (filepath != null)
        {
            Write("Exists?: ", filepath);

            if (!File.Exists(filepath))
            {
                WriteLine(" ... FAILED");
                throw new FileNotFoundException("File not found " + filepath);
            }

            WriteLine(" ... OK");
        }
    }

    /// <summary>
    /// Ensures directory exists. Does nothing if dir is null.
    /// </summary>
    public void CreateDirectory(string? dir)
    {
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            try
            {
                Write("Create Directory: ", dir);
                Directory.CreateDirectory(dir);
                WriteLine(" ... OK");
            }
            catch
            {
                WriteLine(" ... FAILED");
                throw;
            }
        }
    }

    /// <summary>
    /// Ensures directory is deleted (recursive). Does nothing if dir is null.
    /// </summary>
    public void RemoveDirectory(string? dir)
    {
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            try
            {
                Write("Remove: ", dir);
                Directory.Delete(dir, true);
                WriteLine(" ... OK");
            }
            catch
            {
                WriteLine(" ... FAILED");
                throw;
            }
        }
    }


    /// <summary>
    /// Copies directory. Does not create destination. Does nothing if either value is null.
    /// </summary>
    public void CopyDirectory(string? src, string? dst)
    {
        if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(dst))
        {
            try
            {
                Write("Populate: ", dst);

                if (!Directory.Exists(dst))
                {
                    throw new DirectoryNotFoundException("Directory not found " + dst);
                }

                FileSystem.CopyDirectory(src, dst);
                WriteLine(" ... OK");
            }
            catch
            {
                WriteLine(" ... FAILED");
                throw;
            }
        }

    }

    /// <summary>
    /// Copies single single file. Does nothing if either value is null.
    /// </summary>
    public void CopyFile(string? src, string? dst, bool ensureDirectory = false)
    {
        if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(dst))
        {
            if (ensureDirectory)
            {
                CreateDirectory(Path.GetDirectoryName(dst));
            }

            try
            {
                Write("Create File: ", dst);
                File.Copy(src, dst, true);
                WriteLine(" ... OK");
            }
            catch
            {
                WriteLine(" ... FAILED");
                throw;
            }
        }
    }

    /// <summary>
    /// Writes file content. Does nothing if either value is null.
    /// </summary>
    public void WriteFile(string? path, string? content, bool replace = false)
    {
        if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(content) && (replace || !File.Exists(path)))
        {
            try
            {
                Write("Create File: ", path);
                File.WriteAllText(path, content);
                WriteLine(" ... OK");
            }
            catch
            {
                WriteLine(" ... FAILED");
                throw;
            }
        }
    }

    /// <summary>
    /// Zips the directory and writes to output.
    /// </summary>
    public void Zip(string? directory, string? output)
    {
        if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(output))
        {
            try
            {
                if (File.Exists(output))
                {
                    File.Delete(output);
                }

                Write("Zip: ", directory);
                ZipFile.CreateFromDirectory(directory, output, CompressionLevel.Optimal, false);
                WriteLine(" ... OK");
            }
            catch
            {
                WriteLine(" ... FAILED");
                throw;
            }
        }
    }

    /// <summary>
    /// Runs the command.
    /// </summary>
    public int Execute(string command, bool throwNonZeroExit = true)
    {
        string? args = null;
        int idx = command.IndexOf(' ');

        if (idx > 0)
        {
            args = command.Substring(idx + 1).Trim();
            command = command.Substring(0, idx).Trim();
        }

        return Execute(command, args, throwNonZeroExit);
    }

    /// <summary>
    /// Runs the command with separate arguments.
    /// </summary>
    public int Execute(string command, string? args, bool throwNonZeroExit = true)
    {
        bool redirect = false;
        string orig = command.ToLowerInvariant();

        if (orig == "rem" || orig == "::" || orig == "#")
        {
            // Ignore commands which look like comments
            return 0;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            redirect = true;

            if (orig != "cmd" && orig != "cmd.exe")
            {
                // Fix up dos command
                args = $"/C {command} {args}";
                command = "cmd";
            }
        }

        if (orig != "echo")
        {
            WriteLine($"{command} {args}");
        }

        var info = new ProcessStartInfo
        {
            Arguments = args,
            CreateNoWindow = true,
            FileName = command,
            RedirectStandardOutput = redirect,
            RedirectStandardError = redirect,
            UseShellExecute = false,
        };

        using var proc = Process.Start(info) ??
            throw new InvalidOperationException($"{command} failed");

        if (redirect)
        {
            Write(proc.StandardOutput.ReadToEnd());
            Write(proc.StandardError.ReadToEnd());
        }

        proc.WaitForExit();

        if (throwNonZeroExit && proc.ExitCode != 0)
        {
            throw new InvalidOperationException($"{command} returned non-zero exit code {proc.ExitCode}");
        }

        return proc.ExitCode;
    }

    /// <summary>
    /// Runs the commands. Does nothing if empty.
    /// </summary>
    public int Execute(IEnumerable<string> commands, bool throwNonZeroExit = true)
    {
        bool more = false;

        foreach (var item in commands)
        {
            if (more)
            {
                WriteLine(null);
            }

            more = true;
            int rslt = Execute(item, throwNonZeroExit);

            if (rslt != 0)
            {
                return rslt;
            }
        }

        return 0;
    }

    private void Write(string? prefix, string? path = null)
    {
        if (ShowCommands)
        {
            Console.Write(prefix);

            if (path != null && Root != null)
            {
                path = Path.GetRelativePath(Root, path);
            }

            Console.Write(path);
        }
    }

    private void WriteLine(string? prefix, string? path = null)
    {
        if (ShowCommands)
        {
            Write(prefix, path);
            Console.WriteLine();
        }
    }

}

