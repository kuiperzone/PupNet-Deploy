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
using System.Runtime.InteropServices;
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Accepts a configuration and assembles path and assets information. The build
/// process is run using the Run() method. Most path and content information is
/// public for test and inspection.
/// </summary>
public class BuildAssets
{
    private readonly static string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
            throw new InvalidOperationException("Failed to get EntryAssembly location");

    /// <summary>
    /// Constructor.
    /// </summary>
    public BuildAssets(ConfDecoder conf, BuildTree tree, BuildMacros macros)
    {
        Conf = conf;
        Tree = tree;
        Macros = macros;

        var kind = Conf.Args.Kind;

        var icons = Conf.Icons.Count != 0 ? Conf.Icons : DefaultIcons;
        SourceIcon = GetSourceIcon(kind, icons);

        if (SourceIcon != null)
        {
            if (kind == PackKind.AppImage)
            {
                DestIcon = Path.Combine(Tree.AppDir, Conf.AppId + Path.GetExtension(SourceIcon));
            }
            else
            {
                // DestIcon = MapSourceIconToBuild(SourceIcon);
            }
        }

        LinuxIcons = GetLinuxIconPaths(icons);

        if (kind.IsLinux())
        {
            AppMetaContent = Macros.Expand(ReadFileText(Conf.MetaInfo));

            if (string.IsNullOrEmpty(Conf.DesktopEntry))
            {
                DesktopContent = Macros.Expand(GetDesktopTemplate(Conf.IsTerminal));
            }
            else
            {
                var temp = ReadFileText(Conf.DesktopEntry);

                if (temp != null)
                {
                    DesktopContent = Macros.Expand(temp);

                    bool assert = true;

                    foreach (var line in temp.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.StartsWith("Exec") && line.Contains($"{{{MacroNames.DesktopExec}}}"))
                        {
                            // OK
                            assert = false;
                            break;
                        }
                    }

                    if (assert)
                    {
                        throw new ArgumentException($"{nameof(ConfDecoder.DesktopEntry)} must contain 'Exec=${{{MacroNames.DesktopExec}}}'");
                    }
                }
            }
        }

        if (kind == PackKind.Flatpak)
        {
            FlatpakManifestContent = GetFlatpakManifestContent();
        }
    }

    /// <summary>
    /// Gets a desktop template.
    /// </summary>
    public static string GetDesktopTemplate(bool terminal)
    {
        var list = new List<string>();
        list.Add("[Desktop Entry]");
        list.Add($"Type=Application");
        list.Add($"Name=${{{MacroNames.AppFriendlyName}}}");
        list.Add($"Icon=${{{MacroNames.AppId}}}");
        list.Add($"Comment=${{{MacroNames.AppSummary}}}");
        list.Add($"Exec=${{{MacroNames.DesktopExec}}}");
        list.Add($"TryExec=${{{MacroNames.DesktopExec}}}");
        list.Add($"Terminal={terminal.ToString().ToLowerInvariant()}");
        list.Add($"Categories=Utility");
        list.Add($"MimeType=");
        list.Add($"Keywords=");

        return string.Join('\n', list);
    }

    /// <summary>
    /// Gets an AppStream metadata template.
    /// </summary>
    public static string AppMetaTemplate { get; } = GetAppMetaTemplate();

    /// <summary>
    /// Known and accepted PNG icon sizes.
    /// </summary>
    public static IReadOnlyCollection<int> StandardIconSizes = new List<int>(new int[] { 16, 24, 32, 48, 64, 96, 128, 256 });

    /// <summary>
    /// Gets default source icons.
    /// </summary>
    public static IReadOnlyCollection<string> DefaultIcons { get; } = GetDefaultIcons();

    /// <summary>
    /// Gets full path to embedded appimagetool. Null if architecture not supported.
    /// </summary>
    public static string? AppImageTool { get; } = GetAppImageTool();

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public ConfDecoder Conf { get; }

    /// <summary>
    /// Gets the build tree.
    /// </summary>
    public BuildTree Tree { get; }

    /// <summary>
    /// Gets the macros tree.
    /// </summary>
    public BuildMacros Macros { get; }

    /// <summary>
    /// Key is source, value is build destination. Includes .svg and .png only.
    /// </summary>
    public IReadOnlyDictionary<string, string> LinuxIcons;

    /// <summary>
    /// Single icon source path. Only used for AppImage and WinSetup.
    /// </summary>
    public string? SourceIcon { get; }

    /// <summary>
    /// Single icon destination path. Only used for AppImage and WinSetup.
    /// </summary>
    public string? DestIcon { get; }

    public string? DesktopContent { get; }
    public string? AppMetaContent { get; }
    public string? FlatpakManifestContent { get; }

    /// <summary>
    /// Gets the RPM file content. Returns null if not rpm. Supply only true only when files assembled in AppRoot.
    /// </summary>
    public string? GetRpmSpecContent(bool includeFiles)
    {
        if (Conf.Args.Kind == PackKind.Rpm)
        {
            // We don't actually need install, build sections.
            var sb = new StringBuilder();
            var dict = Macros.Dictionary;

            sb.AppendLine($"Name: {Conf.AppBaseName}");
            sb.AppendLine($"Version: {Macros.AppVersion}");
            sb.AppendLine($"Release: {Macros.PackRelease}");
            sb.AppendLine($"BuildArch: {Macros.BuildArch}");
            sb.AppendLine($"Summary: {Conf.AppSummary}");
            sb.AppendLine($"License: {Conf.AppLicense}");
            sb.AppendLine($"Vendor: {Conf.AppVendor}");

            if (!string.IsNullOrEmpty(Conf.AppUrl))
            {
                sb.AppendLine($"Url: {Conf.AppUrl}");
            }

            // We expect dotnet "--self-contained true" to provide ALL dependencies in single directory
            // https://rpm-list.redhat.narkive.com/KqUzv7C1/using-nodeps-with-rpmbuild-is-it-possible
            sb.AppendLine();
            sb.AppendLine("AutoReqProv: no");

            if (DesktopContent != null || AppMetaContent != null)
            {
                sb.AppendLine("BuildRequires: libappstream-glib");
                sb.AppendLine();
                sb.AppendLine("%check");

                if (DesktopContent != null)
                {
                    sb.AppendLine("desktop-file-validate %{buildroot}/%{_datadir}/applications/*.desktop");
                }

                if (AppMetaContent != null)
                {
                    sb.AppendLine($"appstream-util validate-relax --nonet %{{buildroot}}%{{_metainfodir}}/{Tree.AppMetaName}");
                }
            }

            // Description is mandatory, but just repeat summary
            sb.AppendLine();
            sb.AppendLine("%description");
            sb.AppendLine(Conf.AppSummary);

            sb.AppendLine();
            sb.AppendLine("%files");

            if (includeFiles)
            {
                foreach (var item in Tree.GetDirectoryContents(Tree.AppDir))
                {
                    if (item.Length != 0)
                    {
                        if (!item.StartsWith('/'))
                        {
                            sb.Append('/');
                        }

                        sb.AppendLine(item);
                    }
                }
            }
            else
            {
                // Placeholder only
                sb.Append("[FILES]");
            }

            return sb.ToString();
        }

        return null;
    }

    private static IReadOnlyCollection<string> GetDefaultIcons()
    {
        // Default icon in assembly directory
        var list = new List<string>();

        list.Add(Path.Combine(AssemblyDirectory, "app.svg"));
        list.Add(Path.Combine(AssemblyDirectory, "app.icon"));
        list.Add(Path.Combine(AssemblyDirectory, "app.16x16.png"));
        list.Add(Path.Combine(AssemblyDirectory, "app.24x24.png"));
        list.Add(Path.Combine(AssemblyDirectory, "app.32x32.png"));
        list.Add(Path.Combine(AssemblyDirectory, "app.48x48.png"));

        return list;
    }

    private static string? GetAppImageTool()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return Path.Combine(AssemblyDirectory, "appimagetool-x86_64.AppImage");
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return Path.Combine(AssemblyDirectory, "appimagetool-aarch64.AppImage");
        }

        return null;
    }

    private static string GetAppMetaTemplate()
    {
        const string Indent = "    ";
        const string IndentIndent = "        ";
        const string IndentIndentIndent = "            ";

        var sb = new StringBuilder();
        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<component type=\"desktop-application\">");
        sb.AppendLine($"{Indent}<id>${{{MacroNames.AppId}}}</id>");
        sb.AppendLine($"{Indent}<metadata_license>MIT</metadata_license>");
        sb.AppendLine($"{Indent}<project_license>${{{MacroNames.AppLicense}}}</project_license>");
        sb.AppendLine($"{Indent}<content_rating type=\"oars-1.1\" />");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<name>${{{MacroNames.AppFriendlyName}}}</name>");
        sb.AppendLine($"{Indent}<summary>${{{MacroNames.AppSummary}}}</summary>");
        sb.AppendLine($"{Indent}<developer_name>${{{MacroNames.AppVendor}}}</developer_name>");
        sb.AppendLine($"{Indent}<url type=\"homepage\">${{{MacroNames.AppUrl}}}</url>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<description>");
        sb.AppendLine($"{IndentIndent}<p>REPLACE THIS WITH YOUR OWN. This is a longer application description.");
        sb.AppendLine($"{IndentIndent}IMPORTANT: In this file, you can use supported macros. See the --help output for details.</p>");
        sb.AppendLine($"{Indent}</description>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<releases>");
        sb.AppendLine($"{IndentIndent}<release version=\"${{{MacroNames.AppVersion}}}\" date=\"${{{MacroNames.IsoDate}}}\">");
        sb.AppendLine($"{IndentIndentIndent}<description><p>The latest release.</p></description>");
        sb.AppendLine($"{IndentIndent}</release>");
        sb.AppendLine($"{Indent}</releases>");
        sb.AppendLine();

        sb.AppendLine($"{Indent}<!-- Uncomment and provide screenshot");
        sb.AppendLine($"{Indent}<screenshots>");
        sb.AppendLine($"{IndentIndent}<screenshot type=\"default\">");
        sb.AppendLine($"{IndentIndentIndent}<image>https://i.postimg.cc/0jc8xxxC/Hello-Computer.png</image>");
        sb.AppendLine($"{IndentIndent}</screenshot>");
        sb.AppendLine($"{Indent}</screenshots>");
        sb.AppendLine($"{Indent}-->");

        sb.AppendLine($"{Indent}<!-- Do not change the ID -->");
	    sb.AppendLine($"{Indent}<launchable type=\"desktop-id\">${{{MacroNames.DesktopName}}}</launchable>");

        sb.AppendLine($"</component>");

        return sb.ToString();

        /* Needed?
        sb.AppendLine();
        sb.AppendLine($"{Indent}<provides>");
        sb.AppendLine($"{Indent}<!-- Do not change the ID -->");
		sb.AppendLine($"{IndentIndent}<id>${{{MacroNames.DesktopId}}}</id>");
	    sb.AppendLine($"{Indent}</provides>");
        */
    }

    private static int GetStandardPngSize(string filename)
    {
        // Where filename = name.32.png, or name.32x32.png
        var ext = Path.GetExtension(filename);

        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            // Loose any directory
            filename = Path.GetFileName(filename);

            // Interior extension, i.e. the value
            ext = Path.GetExtension(Path.GetFileNameWithoutExtension(filename));

            // Accept "64x64" but key off first value
            int pos = ext.IndexOf('x', StringComparison.OrdinalIgnoreCase);

            if (pos > 0)
            {
                ext = ext.Substring(1, pos - 1);
            }

            if (int.TryParse(ext, out int size) && StandardIconSizes.Contains(size))
            {
                return size;
            }

            var sizes = string.Join(',', StandardIconSizes);
            throw new ArgumentException($"Icon {filename} must be of form 'name.size.png', where size = {sizes} only");
        }

        return 0;
    }

    private static string? GetSourceIcon(PackKind kind, IReadOnlyCollection<string> paths)
    {
        int max = 0;
        string? rslt = null;

        foreach (var item in paths)
        {
            var ext = Path.GetExtension(item).ToLowerInvariant();

            if (kind == PackKind.WinSetup && ext == ".ico")
            {
                // Only need this
                return item;
            }

            if (kind == PackKind.AppImage)
            {
                if (ext == ".svg")
                {
                    return item;
                }

                // Get biggest PNG
                int size = GetStandardPngSize(item);

                if (size > max)
                {
                    max = size;
                    rslt = item;
                }
            }
        }

        return rslt;
    }

    private string? MapSourceIconToBuild(string sourcePath)
    {
        var ext = Path.GetExtension(sourcePath).ToLowerInvariant();

        if (ext.Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(Tree.AppShareIcons, "hicolor", "scalable", "apps", Conf.AppId) + ".svg";
        }

        int size = GetStandardPngSize(sourcePath);

        if (size > 0)
        {
            return Path.Combine(Tree.AppShareIcons, "hicolor", $"{size}x{size}", "apps", Conf.AppId) + ".png";
        }

        return null;
    }

    private IReadOnlyDictionary<string, string> GetLinuxIconPaths(IReadOnlyCollection<string> sources)
    {
        // Empty on windows
        var dict = new Dictionary<string, string>();

        if (Conf.Args.Kind.IsLinux())
        {
            foreach (var item in sources)
            {
                var dest = MapSourceIconToBuild(item);

                if (dest != null)
                {
                    dict.TryAdd(item, dest);
                }
            }
        }

        return dict;
    }

    private string? ReadFileText(string? path)
    {
        if (path != null && !path.Equals(ConfDecoder.PathNone, StringComparison.OrdinalIgnoreCase) &&
            (Conf.AssertFiles || File.Exists(path)))
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

    private string GetFlatpakManifestContent()
    {
        var sb = new StringBuilder();

        // NOTE. Yaml file must be saved next to AppDir directory
        sb.AppendLine($"app-id: {Conf.AppId}");
        sb.AppendLine($"runtime: {Conf.FlatpakPlatformRuntime}");
        sb.AppendLine($"runtime-version: '{Conf.FlatpakPlatformVersion}'");
        sb.AppendLine($"sdk: {Conf.FlatpakPlatformSdk}");
        sb.AppendLine($"command: {Conf.AppBaseName}");
        sb.AppendLine($"modules:");
        sb.AppendLine($"  - name: {Conf.AppId}");
        sb.AppendLine($"    buildsystem: simple");
        sb.AppendLine($"    build-commands:");
        sb.AppendLine($"      - mkdir -p /app/bin");
        sb.AppendLine($"      - cp -rn bin/* /app/bin");
        sb.AppendLine($"      - mkdir -p /app/share");
        sb.AppendLine($"      - cp -rn share/* /app/share");
        sb.AppendLine($"    sources:");
        sb.AppendLine($"      - type: dir");
        sb.AppendLine($"        path: AppDir/usr/");

        if (Conf.FlatpakFinishArgs.Count != 0)
        {
            sb.AppendLine($"finish-args:");

            foreach (var item in Conf.FlatpakFinishArgs)
            {
                sb.Append("  - ");
                sb.AppendLine(item);
            }
        }

        return sb.ToString().TrimEnd();
    }

}

