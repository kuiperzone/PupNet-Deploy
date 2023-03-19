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
        var host = new BuildHost(new DummyConf(PackageKind.AppImage));

        var sb = new StringBuilder();

        foreach (var item in Enum.GetValues<MacroId>())
        {
            sb.AppendLine(item.ToVar());
        }

        var test = host.Macros.Expand(sb.ToString());

        // Expect no remaining macros
        Console.WriteLine(test);
        Assert.DoesNotContain("${", test);
    }

    [Fact]
    public void Expand_EscapeXMLCharacters()
    {
        var host = new BuildHost(new DummyConf(PackageKind.AppImage));

        // Has XML chars
        var summary = host.Macros.Expand("${APP_SHORT_SUMMARY}", true);
        Assert.Equal("Test &lt;application&gt; only", summary);
    }

}