// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-25
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
/// Dummy configuration with test values. The values are expected by unit tests,
/// so changing them will break the tests.
/// </summary>
public class DummyConf : ConfigurationReader
{
    public const string ExpectSignTool =
        "\"C:/Program Files (x86)/Windows Kits/10/bin/10.0.22621.0/x64/signtool.exe\" sign /f \"{#GetEnv('SigningCertificate')}\" /p \"{#GetEnv('SigningCertificatePassword')}\" /tr http://timestamp.sectigo.com /td sha256 /fd sha256 $f";

    public DummyConf()
        : base(new ArgumentReader(), Create())
    {
    }

    public DummyConf(ArgumentReader args)
        : base(args, Create())
    {
    }

    /// <summary>
    /// Creates demo of given kind. For unit test - if omit is not null, the property name given
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
        lines.Add($"{nameof(AppBaseName)} = 'HelloWorld'");
        lines.Add($"{nameof(AppFriendlyName)} = Hello World");
        lines.Add($"{nameof(AppId)} = \"com.example.helloworld\"");
        lines.Add($"{nameof(AppVersionRelease)} = 5.4.3[2]");
        lines.Add($"{nameof(PackageName)} = HelloWorld");
        lines.Add($"{nameof(AppShortSummary)} = Test <application> only");
        lines.Add($"{nameof(AppDescription)} = \n Para1-Line1\n<Para1-Line2>\n\n- Bullet1\n* Bullet2\nPara2-Line1 has ${{MACRO_VAR}}\n");
        lines.Add($"{nameof(AppLicenseId)} = LicenseRef-LICENSE");
        lines.Add($"{nameof(AppLicenseFile)} = LICENSE");
        lines.Add($"{nameof(AppChangeFile)} = CHANGELOG");

        lines.Add($"{nameof(PublisherName)} = Kuiper Zone");
        lines.Add($"{nameof(PublisherCopyright)} = Copyright Kuiper Zone");
        lines.Add($"{nameof(PublisherLinkName)} = example.com");
        lines.Add($"{nameof(PublisherLinkUrl)} = https://example.com");
        lines.Add($"{nameof(PublisherEmail)} = email@example.com");

        lines.Add($"{nameof(StartCommand)} = helloworld");
        lines.Add($"{nameof(DesktopNoDisplay)} = TRUE");
        lines.Add($"{nameof(DesktopTerminal)} = False");
        lines.Add($"{nameof(PrimeCategory)} = Development");
        lines.Add($"{nameof(DesktopFile)} = app.desktop");
        lines.Add($"{nameof(IconFiles)} = Assets/Icon.32x32.png; Assets/Icon.x48.png; Assets/Icon.64.png; Assets/Icon.ico; Assets/Icon.svg;");
        lines.Add($"{nameof(MetaFile)} = metainfo.xml");

        lines.Add($"{nameof(DotnetProjectPath)} = HelloProject");
        lines.Add($"{nameof(DotnetPublishArgs)} = --self-contained true");
        lines.Add($"{nameof(DotnetPostPublish)} = PostPublishCommand.sh");
        lines.Add($"{nameof(DotnetPostPublishOnWindows)} = PostPublishCommandOnWindows.bat");

        lines.Add($"{nameof(OutputDirectory)} = Deploy");

        lines.Add($"{nameof(AppImageArgs)} = -appargs");
        lines.Add($"{nameof(AppImageVersionOutput)} = true");
        lines.Add($"{nameof(AppImageRuntimePath)} = Runtimes/runtime-x86_64");

        lines.Add($"{nameof(FlatpakPlatformRuntime)} = org.freedesktop.Platform");
        lines.Add($"{nameof(FlatpakPlatformSdk)} = org.freedesktop.Sdk");
        lines.Add($"{nameof(FlatpakPlatformVersion)} = \"18.00\"");
        lines.Add($"{nameof(FlatpakFinishArgs)} = --socket=wayland;--socket=fallback-x11;--filesystem=host;--share=network");
        lines.Add($"{nameof(FlatpakBuilderArgs)} = -flatargs");

        lines.Add($"{nameof(RpmAutoReq)} = true");
        lines.Add($"{nameof(RpmAutoProv)} = false");
        lines.Add($"{nameof(RpmRequires)} = rpm-requires1;rpm-requires2");

        lines.Add($"{nameof(DebianRecommends)} = deb-depends1;deb-depends2");

        lines.Add($"{nameof(SetupAdminInstall)} = true");
        lines.Add($"{nameof(SetupCommandPrompt)} = Command Prompt");
        lines.Add($"{nameof(SetupMinWindowsVersion)} = 6.9");
        lines.Add($"{nameof(SetupSignTool)} = {ExpectSignTool}");
        lines.Add($"{nameof(SetupSuffixOutput)} = Setup");
        lines.Add($"{nameof(SetupVersionOutput)} = true");
        lines.Add($"{nameof(SetupUninstallScript)} = uninstall.bat");

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