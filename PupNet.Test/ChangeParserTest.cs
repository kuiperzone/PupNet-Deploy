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

namespace KuiperZone.PupNet.Test;

public class ChangeParserTest
{
    private readonly static IReadOnlyList<string> _source;

    static ChangeParserTest()
    {
        var temp = new List<string>();

        temp.Add("Test readme");
        temp.Add("");
        temp.Add("+ 1.0.1;Title1;contact@example.com;2023-05-01");
        temp.Add("");
        temp.Add("- Change1-a");
        temp.Add("");
        temp.Add("- Change1-b");
        temp.Add("  second line");
        temp.Add("- Change1-c");
        temp.Add("");
        temp.Add("Some description (ignored)");
        temp.Add("- Other change is ignored");
        temp.Add("");
        temp.Add("+1.0.2 ; Title2 ; 2023-05-02");
        temp.Add("- Change2");
        temp.Add("");
        temp.Add("Some description (ignored)");

        _source = temp;
    }

    [Fact]
    public void Constructor_ParsesCorrectly()
    {
        var changes = new ChangeParser(_source);

        var exp = new List<ChangeItem>();
        exp.Add(new ChangeItem("1.0.1", new DateTime(2023, 05, 01)));
        exp.Add(new ChangeItem("Change1-a"));
        exp.Add(new ChangeItem("Change1-b second line"));
        exp.Add(new ChangeItem("Change1-c"));
        exp.Add(new ChangeItem("1.0.2", new DateTime(2023, 05, 02)));
        exp.Add(new ChangeItem("Change2"));

        Assert.Equal(exp, changes.Items);
    }

    [Fact]
    public void ToString_Text()
    {
        var changes = new ChangeParser(_source);

        var exp = "+ 1.0.1;2023-05-01\n" +
            "- Change1-a\n" +
            "- Change1-b second line\n" +
            "- Change1-c\n" +
            "\n" +
            "+ 1.0.2;2023-05-02\n" +
            "- Change2";

        Assert.Equal(exp, changes.ToString());
    }

    [Fact]
    public void ToString_Html()
    {
        var changes = new ChangeParser(_source);

        var exp = "<release version=\"1.0.1\" date=\"2023-05-01\"><description><ul>\n" +
            "<li>Change1-a</li>\n" +
            "<li>Change1-b second line</li>\n" +
            "<li>Change1-c</li>\n" +
            "</ul></description></release>\n" +
            "\n" +
            "<release version=\"1.0.2\" date=\"2023-05-02\"><description><ul>\n" +
            "<li>Change2</li>\n" +
            "</ul></description></release>";

        Assert.Equal(exp, changes.ToString(true));
    }

}

