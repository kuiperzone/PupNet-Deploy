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
        list.Add($"Comment={MacroId.AppShortSummary.ToVar()}");
        list.Add($"Exec={MacroId.InstallExec.ToVar()}");
        list.Add($"TryExec={MacroId.InstallExec.ToVar()}");
        list.Add($"NoDisplay={MacroId.DesktopNoDisplay.ToVar()}");
        list.Add($"X-AppImage-Integrate={MacroId.DesktopIntegrate.ToVar()}");
        list.Add($"Terminal={MacroId.DesktopTerminal.ToVar()}");
        list.Add($"Categories={MacroId.PrimeCategory.ToVar()}");
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
        sb.AppendLine($"{Indent}<metadata_license>MIT</metadata_license>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<id>{MacroId.AppId.ToVar()}</id>");
        sb.AppendLine($"{Indent}<name>{MacroId.AppFriendlyName.ToVar()}</name>");
        sb.AppendLine($"{Indent}<summary>{MacroId.AppShortSummary.ToVar()}</summary>");
        sb.AppendLine($"{Indent}<developer_name>{MacroId.PublisherName.ToVar()}</developer_name>");
        sb.AppendLine($"{Indent}<url type=\"homepage\">{MacroId.PublisherLinkUrl.ToVar()}</url>");
        sb.AppendLine($"{Indent}<project_license>{MacroId.AppLicenseId.ToVar()}</project_license>");
        sb.AppendLine($"{Indent}<content_rating type=\"oars-1.1\" />");
        sb.AppendLine();
	    sb.AppendLine($"{Indent}<launchable type=\"desktop-id\">{MacroId.AppId.ToVar()}.desktop</launchable>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<description>");
        sb.AppendLine($"{IndentIndent}<p>REPLACE WITH YOUR OWN DESCRIPTION. This is a longer application description than the summary.");
        sb.AppendLine($"{IndentIndent}IMPORTANT: Many of the values in this file may be automated with macros. See --help for details.</p>");
        sb.AppendLine($"{Indent}</description>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<!-- Freedesktop Categories -->");
        sb.AppendLine($"{Indent}<categories>");
        sb.AppendLine($"{IndentIndent}<category>{MacroId.PrimeCategory.ToVar()}</category>");
        sb.AppendLine($"{Indent}</categories>");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<!-- Uncomment to provide keywords");
        sb.AppendLine($"{Indent}<keywords>");
        sb.AppendLine($"{IndentIndent}<keyword translate=\"no\">IDE</keyword>");
        sb.AppendLine($"{IndentIndent}<keyword>development</keyword>");
        sb.AppendLine($"{IndentIndent}<keyword>programming</keyword>");
        sb.AppendLine($"{IndentIndent}<keyword xml:lang=\"de\">entwicklung</keyword>");
        sb.AppendLine($"{IndentIndent}<keyword xml:lang=\"de\">programmierung</keyword>");
        sb.AppendLine($"{Indent}</keywords>");
        sb.AppendLine($"{Indent}-->");
        sb.AppendLine();
        sb.AppendLine($"{Indent}<!-- Uncomment to provide screenshots");
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

