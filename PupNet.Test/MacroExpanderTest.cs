// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-24
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

namespace KuiperZone.PupNet.Test;

public class MacroExpanderTest
{
    [Fact]
    public void AppDescriptionToXml_HandlesEmpty()
    {
        Assert.Empty(MacroExpander.AppDescriptionToXml(Array.Empty<string>()));
        Assert.Empty(MacroExpander.AppDescriptionToXml(new string[]{"", "  ", ""}));
    }

    [Fact]
    public void AppDescriptionToXml_HandlesParagraphsAndLists()
    {
        var value = new List<string>();
        value.Add("Para1 Line1\n");
        value.Add("Para1 Line2\n");
        value.Add("Para1 Line3");
        value.Add("");
        value.Add("Para2 Line1 with escapes <>");
        value.Add("");
        value.Add("Para3 Line1");
        value.Add("");
        value.Add("- List 1-1");
        value.Add("- List 1-2");
        value.Add("");
        value.Add("Para4 Line1");
        value.Add("* List 2-1");
        value.Add("* List 2-2");
        value.Add("Para5 Line1");

        var exp = new StringBuilder();
        exp.Append("<p>Para1 Line1\n");
        exp.Append("Para1 Line2\n");
        exp.Append("Para1 Line3</p>\n");
        exp.Append("\n");
        exp.Append("<p>Para2 Line1 with escapes &lt;&gt;</p>\n");
        exp.Append("\n");
        exp.Append("<p>Para3 Line1</p>\n");
        exp.Append("\n");
        exp.Append("<ul>\n");
        exp.Append("<li>List 1-1</li>\n");
        exp.Append("<li>List 1-2</li>\n");
        exp.Append("</ul>\n");
        exp.Append("\n");
        exp.Append("<p>Para4 Line1</p>\n");
        exp.Append("\n");
        exp.Append("<ul>\n");
        exp.Append("<li>List 2-1</li>\n");
        exp.Append("<li>List 2-2</li>\n");
        exp.Append("</ul>\n");
        exp.Append("\n");
        exp.Append("<p>Para5 Line1</p>");

        var html = MacroExpander.AppDescriptionToXml(value);

        // Console.WriteLine("===========================");
        // Console.WriteLine(html);
        // Console.WriteLine("===========================");

        Assert.Equal(exp.ToString(), html);

        // Append trailing list
        value.Add("");
        value.Add("+ List 3-1");

        exp.Append("\n");
        exp.Append("\n");
        exp.Append("<ul>\n");
        exp.Append("<li>List 3-1</li>\n");
        exp.Append("</ul>");

        html = MacroExpander.AppDescriptionToXml(value);
        Assert.Equal(exp.ToString(), html);
    }

    [Fact]
    public void Expand_EnsureNoMacroOmitted()
    {
        // Use factory to create one
        var host = new BuildHost(new DummyConf(PackageKind.Zip));

        var sb = new StringBuilder();

        foreach (var item in Enum.GetValues<MacroId>())
        {
            sb.AppendLine(item.ToVar());
        }

        // Need to remove known embedded macro in test string put there by DummyConf
        var test = host.Macros.Expand(sb.ToString()).Replace("${MACRO_VAR}", "MACRO_VAR");

        // Expect no remaining macros
        Console.WriteLine(test);
        Assert.DoesNotContain("${", test);
    }

    [Fact]
    public void Expand_EscapeXMLCharacters()
    {
        var host = new BuildHost(new DummyConf(PackageKind.Zip));

        // Has XML chars
        var summary = host.Macros.Expand("${APP_SHORT_SUMMARY}", true);
        Assert.Equal("Test &lt;application&gt; only", summary);
    }

    [Fact]
    public void Expand_DoesNotRecurse()
    {
        var host = new BuildHost(new DummyConf(PackageKind.Zip));

        // Content we expect ${MACRO_VAR} by DummyConf
        var desc = host.Macros.Expand("${APPSTREAM_DESCRIPTION_XML}", true);
        Assert.Contains("${MACRO_VAR}", desc);
    }

}