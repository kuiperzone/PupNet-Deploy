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
        list.Add($"StartupWMClass={MacroId.AppBaseName.ToVar()}");
        list.Add($"Comment={MacroId.AppShortSummary.ToVar()}");
        list.Add($"Exec={MacroId.InstallExec.ToVar()}");
        list.Add($"TryExec={MacroId.InstallExec.ToVar()}");
        list.Add($"NoDisplay={MacroId.DesktopNoDisplay.ToVar()}");
        list.Add($"Terminal={MacroId.DesktopTerminal.ToVar()}");
        list.Add($"Categories={MacroId.PrimeCategory.ToVar()};");
        list.Add($"X-AppImage-Name={MacroId.AppId}");
        list.Add($"X-AppImage-Version={MacroId.AppVersion}");
        list.Add($"X-AppImage-Arch={MacroId.PackageArch}");
        list.Add($"MimeType=");
        list.Add($"Keywords=");

        return string.Join('\n', list);
    }

    private static string GetMetaInfoTemplate(bool comments = true)
    {
        const string IndentX1 = "    ";
        const string IndentX2 = IndentX1 + "    ";
        const string IndentX3 = IndentX2 + "    ";
        const string IndentX4 = IndentX3 + "    ";
        const string IndentX5 = IndentX4 + "    ";

        var sb = new StringBuilder();
        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<component type=\"desktop-application\">");
        sb.AppendLine($"{IndentX1}<metadata_license>MIT</metadata_license>");
        sb.AppendLine();

        if (comments)
        {
            sb.AppendLine($"{IndentX1}<!-- Note the use of macros to automate most (but not all) content below -->");
        }

        sb.AppendLine($"{IndentX1}<id>{MacroId.AppId.ToVar()}</id>");
        sb.AppendLine($"{IndentX1}<name>{MacroId.AppFriendlyName.ToVar()}</name>");
        sb.AppendLine($"{IndentX1}<summary>{MacroId.AppShortSummary.ToVar()}</summary>");
        sb.AppendLine($"{IndentX1}<url type=\"homepage\">{MacroId.PublisherLinkUrl.ToVar()}</url>");
        sb.AppendLine($"{IndentX1}<project_license>{MacroId.AppLicenseId.ToVar()}</project_license>");
        sb.AppendLine($"{IndentX1}<content_rating type=\"oars-1.1\" />");

        sb.AppendLine();
        sb.AppendLine($"{IndentX1}<developer id=\"{MacroId.PublisherId}\">");
        sb.AppendLine($"{IndentX2}<name>{MacroId.PublisherName.ToVar()}</name>");
        sb.AppendLine($"{IndentX1}</developer>");

        sb.AppendLine();
        sb.AppendLine($"{IndentX1}<launchable type=\"desktop-id\">{MacroId.AppId.ToVar()}.desktop</launchable>");

        sb.AppendLine();
        sb.AppendLine($"{IndentX1}<description>");

        if (comments)
        {
            sb.AppendLine($"{IndentX2}<!-- See {nameof(ConfigurationReader.AppDescription)} in configuration -->");
        }

        sb.AppendLine($"{IndentX2}{MacroId.AppStreamDescriptionXml.ToVar()}");

        if (comments)
        {
            sb.AppendLine($"{IndentX2}<!--");
            sb.AppendLine($"{IndentX2}<p>This is a longer application description in the default (English) language");
            sb.AppendLine($"{IndentX2}which may span several short paragraphs. You may either specify it yourself");
            sb.AppendLine($"{IndentX2}directly here, or have it populated automatically using the macro above.</p>");
            sb.AppendLine($"{IndentX2}<p>IMPORTANT. If you wish to provide your own (default) description here, delete the");
            sb.AppendLine($"{IndentX2}macro directly above. In either case, you may wish to provide other language");
            sb.AppendLine($"{IndentX2}descriptions here also.</p>");
            sb.AppendLine($"{IndentX2}-->");
        }

        sb.AppendLine($"{IndentX1}</description>");

        sb.AppendLine();
        sb.AppendLine($"{IndentX1}<!-- Freedesktop Categories -->");
        sb.AppendLine($"{IndentX1}<categories>");
        sb.AppendLine($"{IndentX2}<category>{MacroId.PrimeCategory.ToVar()}</category>");
        sb.AppendLine($"{IndentX1}</categories>");
        sb.AppendLine();

        if (comments)
        {
            sb.AppendLine($"{IndentX1}<!-- Uncomment to provide keywords");
            sb.AppendLine($"{IndentX1}<keywords>");
            sb.AppendLine($"{IndentX2}<keyword translate=\"no\">IDE</keyword>");
            sb.AppendLine($"{IndentX2}<keyword>development</keyword>");
            sb.AppendLine($"{IndentX2}<keyword>programming</keyword>");
            sb.AppendLine($"{IndentX2}<keyword xml:lang=\"de\">entwicklung</keyword>");
            sb.AppendLine($"{IndentX2}<keyword xml:lang=\"de\">programmierung</keyword>");
            sb.AppendLine($"{IndentX1}</keywords>");
            sb.AppendLine($"{IndentX1}-->");
            sb.AppendLine();
            sb.AppendLine($"{IndentX1}<!-- Uncomment to provide screenshots");
            sb.AppendLine($"{IndentX1}<screenshots>");
            sb.AppendLine($"{IndentX2}<screenshot type=\"default\">");
            sb.AppendLine($"{IndentX3}<image>https://i.postimg.cc/0jc8xxxC/Hello-Computer.png</image>");
            sb.AppendLine($"{IndentX2}</screenshot>");
            sb.AppendLine($"{IndentX1}</screenshots>");
            sb.AppendLine($"{IndentX1}-->");
            sb.AppendLine();
        }

        sb.AppendLine($"{IndentX1}<releases>");

        if (comments)
        {
            sb.AppendLine($"{IndentX2}<!-- See {nameof(ConfigurationReader.AppChangeFile)} in configuration -->");
        }

        sb.AppendLine($"{IndentX2}{MacroId.AppStreamChangelogXml.ToVar()}");

        if (comments)
        {
            sb.AppendLine($"{IndentX2}<!-- Or, uncomment below and delete macro directly above to specify changes yourself");
            sb.AppendLine($"{IndentX2}<release version=\"1.0.0\" date=\"2023-05-04\">");
            sb.AppendLine($"{IndentX3}<description>");
            sb.AppendLine($"{IndentX4}<ul>");
            sb.AppendLine($"{IndentX5}<li>Added feature 1</li>");
            sb.AppendLine($"{IndentX5}<li>Added feature 2</li>");
            sb.AppendLine($"{IndentX4}</ul>");
            sb.AppendLine($"{IndentX3}</description>");
            sb.AppendLine($"{IndentX2}</release>");
            sb.AppendLine($"{IndentX2}-->");
        }

        sb.AppendLine($"{IndentX1}</releases>");
        sb.AppendLine();
        sb.AppendLine($"</component>");

        return sb.ToString();
    }

}

