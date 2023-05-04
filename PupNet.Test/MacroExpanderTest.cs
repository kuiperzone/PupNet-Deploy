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

namespace KuiperZone.PupNet.Test;

public class MacroExpanderTest
{
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

        // Need to remove embedded macro in test string
        var test = host.Macros.Expand(sb.ToString()).Replace("${LINE3_VAR}", "LINE3_VAR");

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

        // Content escape but has XML <p>
        var desc = host.Macros.Expand("${APPSTREAM_DESCRIPTION_XML}", true);

        // Line1\n<Line2>\n\nLine3 has ${VAR}
        Assert.Equal("<p>Line1\n&lt;Line2&gt;</p>\n\n<p>Line3 has ${LINE3_VAR}</p>", desc);
    }

}