// -----------------------------------------------------------------------------
// PROJECT   : Pubpak
// COPYRIGHT : Andy Thomas (C) 2022-23
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/Pubpak
//
// Pubpak is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// Pubpak is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with Pubpak. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

namespace KuiperZone.Pubpak.Test;

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
        lines.Add("AppBase = 'helloworld'");
        lines.Add("AppName = Hello World");
        lines.Add("AppId = \"net.example.helloword\"");
        lines.Add("AppSummary = Test application only");
        lines.Add("AppVendor = KuiperZone");
        lines.Add("AppUrl = https://kuiper.zone");

        lines.Add("AppVersionRelease = 5.4.3[3]");
        lines.Add("AppLicense = LicenseRef-LICENSE");
        lines.Add("AppIcons = Assets/Icon.32x32.png; Assets/Icon.64x64.png; Assets/Icon.ico; Assets/Icon.svg;");
        lines.Add("AppDataPath = metainfo.xml");

        lines.Add("DesktopCategory = Utility;Programming");
        lines.Add("DesktopTerminal = true");
        lines.Add("DesktopMimeType = image/x-foo");

        lines.Add("DotnetProjectPath = HelloProject");
        lines.Add("DotnetPublishArgs = --self-contained true");
        lines.Add("DotnetPostPublish = PostPublishCommand.sh");

        lines.Add("OutputDirectory = Deploy");
        lines.Add("OutputVersion = true");

        lines.Add("AppImageCommand = appimagetool");
        lines.Add("AppImageArgs = -appargs");

        lines.Add("FlatpakPlatformRuntime = org.freedesktop.Platform");
        lines.Add("FlatpakPlatformSdk = org.freedesktop.Sdk");
        lines.Add("FlatpakPlatformVersion = \"18.00\"");
        lines.Add("FlatpakFinishArgs = --socket=wayland;--socket=fallback-x11;--filesystem=host;--share=network");
        lines.Add("FlatpakBuilderArgs = -flatargs");

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