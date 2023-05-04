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

/// <summary>
/// Dummy configuration with test values. Used in unit tests.
/// </summary>
public class DummyConf : ConfigurationReader
{
    public DummyConf()
        : base(new ArgumentReader(), Create())
    {
    }

    public DummyConf(ArgumentReader args)
        : base(args, Create())
    {
    }

    /// <summary>
    /// Creates demo of given kind. If omit is not null, the property name given
    /// will be removed from the content, leaving the value to fall back to its default.
    /// </summary>
    public DummyConf(PackageKind kind, string? omit = null)
        : base(new ArgumentReader("-k " + kind), Create(omit))
    {
    }

    private static string[] Create(string? omit = null)
    {
        var lines = new List<string>();

        // Quote variations
        lines.Add($"{nameof(ConfigurationReader.AppBaseName)} = 'HelloWorld'");
        lines.Add($"{nameof(ConfigurationReader.AppFriendlyName)} = Hello World");
        lines.Add($"{nameof(ConfigurationReader.AppId)} = \"net.example.helloworld\"");
        lines.Add($"{nameof(ConfigurationReader.AppVersionRelease)} = 5.4.3[2]");
        lines.Add($"{nameof(ConfigurationReader.PackageName)} = HelloWorld");
        lines.Add($"{nameof(ConfigurationReader.AppShortSummary)} = Test <application> only");
        lines.Add($"{nameof(ConfigurationReader.AppDescription)} = \n Line1\n<Line2>\n\n  Line3 has ${{LINE3_VAR}}\n");
        lines.Add($"{nameof(ConfigurationReader.AppLicenseId)} = LicenseRef-LICENSE");
        lines.Add($"{nameof(ConfigurationReader.AppLicenseFile)} = LICENSE");
        lines.Add($"{nameof(ConfigurationReader.AppChangeFile)} = CHANGELOG");

        lines.Add($"{nameof(ConfigurationReader.PublisherName)} = Kuiper Zone");
        lines.Add($"{nameof(ConfigurationReader.PublisherCopyright)} = Copyright Kuiper Zone");
        lines.Add($"{nameof(ConfigurationReader.PublisherLinkName)} = kuiper.zone");
        lines.Add($"{nameof(ConfigurationReader.PublisherLinkUrl)} = https://kuiper.zone");
        lines.Add($"{nameof(ConfigurationReader.PublisherEmail)} = email@example.net");

        lines.Add($"{nameof(ConfigurationReader.StartCommand)} = helloworld");
        lines.Add($"{nameof(ConfigurationReader.DesktopNoDisplay)} = TRUE");
        lines.Add($"{nameof(ConfigurationReader.DesktopTerminal)} = False");
        lines.Add($"{nameof(ConfigurationReader.PrimeCategory)} = Development");
        lines.Add($"{nameof(ConfigurationReader.DesktopFile)} = app.desktop");
        lines.Add($"{nameof(ConfigurationReader.IconFiles)} = Assets/Icon.32x32.png; Assets/Icon.64x64.png; Assets/Icon.ico; Assets/Icon.svg;");
        lines.Add($"{nameof(ConfigurationReader.MetaFile)} = metainfo.xml");

        lines.Add($"{nameof(ConfigurationReader.DotnetProjectPath)} = HelloProject");
        lines.Add($"{nameof(ConfigurationReader.DotnetPublishArgs)} = --self-contained true");
        lines.Add($"{nameof(ConfigurationReader.DotnetPostPublish)} = PostPublishCommand.sh");
        lines.Add($"{nameof(ConfigurationReader.DotnetPostPublishOnWindows)} = PostPublishCommandOnWindows.bat");

        lines.Add($"{nameof(ConfigurationReader.OutputDirectory)} = Deploy");

        lines.Add($"{nameof(ConfigurationReader.AppImageArgs)} = -appargs");
        lines.Add($"{nameof(ConfigurationReader.AppImageVersionOutput)} = true");

        lines.Add($"{nameof(ConfigurationReader.FlatpakPlatformRuntime)} = org.freedesktop.Platform");
        lines.Add($"{nameof(ConfigurationReader.FlatpakPlatformSdk)} = org.freedesktop.Sdk");
        lines.Add($"{nameof(ConfigurationReader.FlatpakPlatformVersion)} = \"18.00\"");
        lines.Add($"{nameof(ConfigurationReader.FlatpakFinishArgs)} = --socket=wayland;--socket=fallback-x11;--filesystem=host;--share=network");
        lines.Add($"{nameof(ConfigurationReader.FlatpakBuilderArgs)} = -flatargs");

        lines.Add($"{nameof(ConfigurationReader.RpmAutoReq)} = true");
        lines.Add($"{nameof(ConfigurationReader.RpmAutoProv)} = false");
        lines.Add($"{nameof(ConfigurationReader.RpmRequires)} = rpm-requires1;rpm-requires2");

        lines.Add($"{nameof(ConfigurationReader.DebianRecommends)} = deb-depends1;deb-depends2");

        lines.Add($"{nameof(ConfigurationReader.SetupAdminInstall)} = true");
        lines.Add($"{nameof(ConfigurationReader.SetupCommandPrompt)} = Command Prompt");
        lines.Add($"{nameof(ConfigurationReader.SetupMinWindowsVersion)} = 6.9");
        lines.Add($"{nameof(ConfigurationReader.SetupSignTool)} = signtool.exe");
        lines.Add($"{nameof(ConfigurationReader.SetupSuffixOutput)} = Setup");
        lines.Add($"{nameof(ConfigurationReader.SetupVersionOutput)} = true");

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