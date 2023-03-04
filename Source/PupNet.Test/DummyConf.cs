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

namespace KuiperZone.PupNet.Test;

public class DummyConf : ConfDecoder
{
    public DummyConf(PackKind kind, string? omit = null)
        : base(new ArgDecoder("-k " + kind), Create(omit))
    {
    }

    public DummyConf(ArgDecoder args)
        : base(args, Create())
    {
    }

    private static string[] Create(string? omit = null)
    {
        var lines = new List<string>();

        // Quote variations
        lines.Add($"{nameof(ConfDecoder.AppBase)} = 'HelloWorld'");
        lines.Add($"{nameof(ConfDecoder.AppName)} = Hello World");
        lines.Add($"{nameof(ConfDecoder.AppId)} = \"net.example.helloword\"");
        lines.Add($"{nameof(ConfDecoder.AppSummary)} = Test application only");
        lines.Add($"{nameof(ConfDecoder.AppVendor)} = KuiperZone");
        lines.Add($"{nameof(ConfDecoder.AppUrl)} = https://kuiper.zone");
        lines.Add($"{nameof(ConfDecoder.AppVersionRelease)} = 5.4.3[3]");
        lines.Add($"{nameof(ConfDecoder.AppLicense)} = LicenseRef-LICENSE");

        lines.Add($"{nameof(ConfDecoder.CommandName)} = helloworld");
        lines.Add($"{nameof(ConfDecoder.IsTerminal)} = true");
        lines.Add($"{nameof(ConfDecoder.DesktopEntry)} = app.desktop");
        lines.Add($"{nameof(ConfDecoder.Icons)} = Assets/Icon.32x32.png; Assets/Icon.64x64.png; Assets/Icon.ico; Assets/Icon.svg;");
        lines.Add($"{nameof(ConfDecoder.MetaInfo)} = metainfo.xml");

        lines.Add($"{nameof(ConfDecoder.DotnetProjectPath)} = HelloProject");
        lines.Add($"{nameof(ConfDecoder.DotnetPublishArgs)} = --self-contained true");
        lines.Add($"{nameof(ConfDecoder.DotnetPostPublish)} = PostPublishCommand.sh");

        lines.Add($"{nameof(ConfDecoder.OutputDirectory)} = Deploy");
        lines.Add($"{nameof(ConfDecoder.OutputVersion)} = true");

        lines.Add($"{nameof(ConfDecoder.AppImageArgs)} = -appargs");

        lines.Add($"{nameof(ConfDecoder.FlatpakPlatformRuntime)} = org.freedesktop.Platform");
        lines.Add($"{nameof(ConfDecoder.FlatpakPlatformSdk)} = org.freedesktop.Sdk");
        lines.Add($"{nameof(ConfDecoder.FlatpakPlatformVersion)} = \"18.00\"");
        lines.Add($"{nameof(ConfDecoder.FlatpakFinishArgs)} = --socket=wayland;--socket=fallback-x11;--filesystem=host;--share=network");
        lines.Add($"{nameof(ConfDecoder.FlatpakBuilderArgs)} = -flatargs");

        Remove(lines, omit);

        return lines.ToArray();
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