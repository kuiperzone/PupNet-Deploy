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

namespace KuiperZone.PupNet.Test;

public class IniReaderTest
{
    [Fact]
    public void NoQuoteValue_DecodesOK()
    {
        Assert.Equal("Hello World", Create().Values["NoQuote"]);
    }

    [Fact]
    public void SingleQuoteValue_DecodesOK()
    {
        // No trim inside quote
        Assert.Equal(" Hello World ", Create().Values["SingleQuote"]);
    }

    [Fact]
    public void DoubleQuoteValue_DecodesOK()
    {
        // No trim inside quote
        Assert.Equal(" Hello World ", Create().Values["DoubleQuote"]);
    }

    [Fact]
    public void MultiLineValue_DecodesOK()
    {
        // Expect this to trim inside quote
        Assert.Equal("Hello World", Create().Values["MultiLine1"]);

        // Concatenates
        var sb = new StringBuilder();
        sb.AppendLine("Hello World");
        sb.Append("Hello World");
        Assert.Equal(sb.ToString(), Create().Values["MultiLine2"]);

        sb.Clear();
        sb.AppendLine("Hello World");
        sb.AppendLine("Hello World");
        sb.AppendLine("Hello World");
        sb.AppendLine();
        sb.AppendLine("Hello World");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append("Hello World");
        Assert.Equal(sb.ToString(), Create().Values["MultiLine3"]);
    }

    [Fact]
    public void SyntaxError_ThrowsArgumentException()
    {
        var lines = new List<string>();
        lines.Add("Key = Hello World ");
        lines.Add("No equals ");
        Assert.Throws<ArgumentException>(() => new IniReader(lines.ToArray()));

        lines.Clear();
        lines.Add($"MultiLine1 = {IniReader.StartMultiQuote} Hello World");
        lines.Add("No Termination");
        Assert.Throws<ArgumentException>(() => new IniReader(lines.ToArray()));
    }

    private static IniReader Create()
    {
        var lines = new List<string>();

        // Quote variations
        lines.Add("NoQuote = Hello World ");
        lines.Add("SingleQuote = ' Hello World '");
        lines.Add("DoubleQuote = \" Hello World \"");

        lines.Add($" #Comment");

        lines.Add($"MultiLine1 = {IniReader.StartMultiQuote} Hello World {IniReader.EndMultiQuote}");

        lines.Add($"MultiLine2 = {IniReader.StartMultiQuote} Hello World");
        lines.Add($"Hello World {IniReader.EndMultiQuote}");

        lines.Add($"MultiLine3 = {IniReader.StartMultiQuote} Hello World");
        lines.Add($"Hello World");
        lines.Add($"Hello World ");
        lines.Add($"");
        lines.Add($"Hello World ");
        lines.Add($"");
        lines.Add($"");
        lines.Add($"Hello World ");
        lines.Add($" {IniReader.EndMultiQuote}");

        return new IniReader(lines.ToArray());
    }

    private static void Remove(List<string> list, string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            name += " ";

            for (int n = 0; n < list.Count; ++n)
            {
                if (list[n].StartsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    list.RemoveAt(n);
                    return;
                }
            }
        }

    }

}