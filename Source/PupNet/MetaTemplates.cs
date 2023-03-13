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

using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Static templates for desktop and AppStream metainfo files.
/// </summary>
public static class MetaTemplates
{
    /// <summary>
    /// Gets the desktop file template.
    /// </summary>
    public static string Desktop { get; } = GetDesktopTemplate();

    /// <summary>
    /// Gets the AppStream metadata template. Contains macro variables.
    /// </summary>
    public static string MetaInfo { get; } = GetMetaInfoTemplate();

    private static string GetDesktopTemplate()
    {
        var list = new List<string>();
        list.Add("[Desktop Entry]");
        list.Add($"Type=Application");
        list.Add($"Name={MacroId.AppFriendlyName.ToVar()}");
        list.Add($"Icon={MacroId.AppId.ToVar()}");
        list.Add($"Comment={MacroId.AppSummary.ToVar()}");
        list.Add($"Exec={MacroId.DesktopExec.ToVar()}");
        list.Add($"TryExec={MacroId.DesktopExec.ToVar()}");
        list.Add($"Terminal=true");
        list.Add($"Categories=Utility");
        list.Add($"MimeType=");
        list.Add($"Keywords=");

        return string.Join('\n', list);
    }

    private static string GetMetaInfoTemplate()
    {
        const string Indent = "    ";
        const string IndentIndent = "        ";
        const string IndentIndentIndent = "            ";

        var sb = new StringBuilder();
        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<component type=\"desktop-application\">");
        sb.AppendLine($"{Indent}<id>{MacroId.AppId.ToVar()}</id>");
        sb.AppendLine($"{Indent}<metadata_license>MIT</metadata_license>");
        sb.AppendLine($"{Indent}<project_license>{MacroId.AppLicense.ToVar()}</project_license>");
        sb.AppendLine($"{Indent}<content_rating type=\"oars-1.1\" />");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<name>{MacroId.AppFriendlyName.ToVar()}</name>");
        sb.AppendLine($"{Indent}<summary>{MacroId.AppSummary.ToVar()}</summary>");
        sb.AppendLine($"{Indent}<developer_name>{MacroId.AppVendor.ToVar()}</developer_name>");
        sb.AppendLine($"{Indent}<url type=\"homepage\">{MacroId.AppUrl.ToVar()}</url>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<!-- Do not change the ID -->");
	    sb.AppendLine($"{Indent}<launchable type=\"desktop-id\">{MacroId.AppId.ToVar()}.desktop</launchable>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<description>");
        sb.AppendLine($"{IndentIndent}<p>REPLACE THIS WITH YOUR OWN. This is a longer application description.");
        sb.AppendLine($"{IndentIndent}IMPORTANT: In this file, you can use supported macros. See the --help output for details.</p>");
        sb.AppendLine($"{Indent}</description>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<!-- Uncomment and provide screenshot");
        sb.AppendLine($"{Indent}<screenshots>");
        sb.AppendLine($"{IndentIndent}<screenshot type=\"default\">");
        sb.AppendLine($"{IndentIndentIndent}<image>https://i.postimg.cc/0jc8xxxC/Hello-Computer.png</image>");
        sb.AppendLine($"{IndentIndent}</screenshot>");
        sb.AppendLine($"{Indent}</screenshots>");
        sb.AppendLine($"{Indent}-->");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<releases>");
        sb.AppendLine($"{IndentIndent}<release version=\"{MacroId.AppVersion.ToVar()}\" date=\"{MacroId.BuildDate.ToVar()}\">");
        sb.AppendLine($"{IndentIndentIndent}<description><p>The latest release.</p></description>");
        sb.AppendLine($"{IndentIndent}</release>");
        sb.AppendLine($"{Indent}</releases>");
        sb.AppendLine();
        sb.AppendLine($"</component>");

        return sb.ToString();
    }

}

