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

using System.Xml.Linq;

namespace KuiperZone.PupNet.Test;

public class AppStreamTest
{
    [Fact]
    public void MetaTemplate_ExpandsToValidXML()
    {
        var host = new BuildHost(new DummyConf(PackageKind.AppImage));

        // Must be escaped
        var test = host.Macros.Expand(MetaTemplates.MetaInfo, true);

        // Console.WriteLine("+++++");
        // Console.WriteLine(test);
        // Console.WriteLine("+++++");

        XDocument.Parse(test);
    }

    [Fact]
    public void MetaTemplate_NoAppDescription_ExpandsToValidXML()
    {
        var host = new BuildHost(new DummyConf(PackageKind.AppImage, nameof(ConfigurationReader.AppDescription)));
        var test = host.Macros.Expand(MetaTemplates.MetaInfo, true);

        XDocument.Parse(test);
    }

}