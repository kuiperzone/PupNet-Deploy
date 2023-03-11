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

public class DummyConf : ConfigurationReader
{
    public DummyConf(PackKind kind, string? omit = null)
        : base(new ArgumentReader("-k " + kind), Create(omit))
    {
    }

    public DummyConf(ArgumentReader args)
        : base(args, Create())
    {
    }

    private static string[] Create(string? omit = null)
    {
        var lines = new List<string>();

        // Quote variations
        lines.Add($"{nameof(ConfigurationReader.AppBaseName)} = 'HelloWorld'");
        lines.Add($"{nameof(ConfigurationReader.AppFriendlyName)} = Hello World");
        lines.Add($"{nameof(ConfigurationReader.AppId)} = \"net.example.helloword\"");
        lines.Add($"{nameof(ConfigurationReader.AppSummary)} = Test application only");
        lines.Add($"{nameof(ConfigurationReader.AppVendor)} = KuiperZone");
        lines.Add($"{nameof(ConfigurationReader.AppUrl)} = https://kuiper.zone");
        lines.Add($"{nameof(ConfigurationReader.AppVersionRelease)} = 5.4.3[3]");
        lines.Add($"{nameof(ConfigurationReader.AppLicense)} = LicenseRef-LICENSE");

        lines.Add($"{nameof(ConfigurationReader.StartCommand)} = helloworld");
        lines.Add($"{nameof(ConfigurationReader.IsTerminal)} = true");
        lines.Add($"{nameof(ConfigurationReader.DesktopEntry)} = app.desktop");
        lines.Add($"{nameof(ConfigurationReader.Icons)} = Assets/Icon.32x32.png; Assets/Icon.64x64.png; Assets/Icon.ico; Assets/Icon.svg;");
        lines.Add($"{nameof(ConfigurationReader.MetaInfo)} = metainfo.xml");

        lines.Add($"{nameof(ConfigurationReader.DotnetProjectPath)} = HelloProject");
        lines.Add($"{nameof(ConfigurationReader.DotnetPublishArgs)} = --self-contained true");
        lines.Add($"{nameof(ConfigurationReader.DotnetPostPublish)} = PostPublishCommand.sh");

        lines.Add($"{nameof(ConfigurationReader.OutputDirectory)} = Deploy");
        lines.Add($"{nameof(ConfigurationReader.OutputVersion)} = true");

        lines.Add($"{nameof(ConfigurationReader.AppImageArgs)} = -appargs");

        lines.Add($"{nameof(ConfigurationReader.FlatpakPlatformRuntime)} = org.freedesktop.Platform");
        lines.Add($"{nameof(ConfigurationReader.FlatpakPlatformSdk)} = org.freedesktop.Sdk");
        lines.Add($"{nameof(ConfigurationReader.FlatpakPlatformVersion)} = \"18.00\"");
        lines.Add($"{nameof(ConfigurationReader.FlatpakFinishArgs)} = --socket=wayland;--socket=fallback-x11;--filesystem=host;--share=network");
        lines.Add($"{nameof(ConfigurationReader.FlatpakBuilderArgs)} = -flatargs");

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