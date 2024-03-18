// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-24
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

using System.Runtime.InteropServices;

namespace KuiperZone.PupNet.Test;

/// <summary>
/// This goes a little beyond the scope of unit test, and performs an "integration test" against <see cref="BuildHost"/>.
/// To do this, we must have installed all the third-party builder-tools and we will actually produce a dummy package
/// output for each test. Although not ideal, it is necessary as testing for each package output kind prior to releasing
/// the software is otherwise an intensive and error prone exercise. Here, we run the tests only in RELEASE builds only
/// as each test execution blocks. It is recommended to run the tests with "dotnet test -c Release" ON MOST PLATFORMS
/// prior to releasing a new version of pupnet. NOTE. Cannot be used with Flatpak or Windows Setup (damn!).
/// </summary>
public class BuildHostIntegrationTest
{
    [Fact]
    public void BuildZip_EnsureBuildSucceedsAndOutputExists()
    {
        // We can always test zip
        Assert_BuildPackage(PackageKind.Zip, false);
        Assert_BuildPackage(PackageKind.Zip, true);
    }


#if !DEBUG
    [Fact]
    public void BuildThirdParty_EnsureBuildSucceedsAndOutputExists()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert_BuildPackage(PackageKind.AppImage, false);
            Assert_BuildPackage(PackageKind.AppImage, true);

            Assert_BuildPackage(PackageKind.Deb, false);
            Assert_BuildPackage(PackageKind.Deb, true);

            Assert_BuildPackage(PackageKind.Rpm, false);
            Assert_BuildPackage(PackageKind.Rpm, true);
        }
    }
#endif

    private void Assert_BuildPackage(PackageKind kind, bool complete, bool assertOutput = true)
    {
        string? metadata = null;
        string? manifest = null;
        var conf = new TestConfiguration(kind, complete);

        try
        {
            var host = new BuildHost(conf);

            // Must create build outside of BuildHost.Run()
            host.Builder.Create(host.ExpandedDesktop, host.ExpandedMetaInfo);
            metadata = host.ExpandedMetaInfo;
            manifest = host.Builder.ManifestContent;

            // Regression test - test for correct icon
            if (host.Builder.IsLinuxExclusive)
            {
                // If we do not define icon (i.e complete == false),
                // we still get default icon on Linux
                Assert.NotNull(host.Builder.PrimaryIcon);

                if (complete)
                {
                    // Icon provided in config below
                    // Always chooses SVG if one is provided over PNG
                    Assert.EndsWith("Icon.svg", host.Builder.PrimaryIcon);
                }
                else
                {
                    // Default icon is built into application.
                    // We have set DesktopTerminal to true below, so expect "terminal" icon
                    // We will accept either SVG or max size PNG
                    bool hasSvg = host.Builder.PrimaryIcon.Contains("terminal.svg");;
                    bool hasPng = host.Builder.PrimaryIcon.Contains("terminal.256x256");
                    Assert.True(hasSvg || hasPng);
                }
            }

            if (host.Builder.IsOsxExclusive)
            {
                // Test here
            }

            if (host.Builder.IsWindowsExclusive)
            {
                // Windows? We are not declaring Windows icon currently - must be null
                Assert.Null(host.Builder.PrimaryIcon);
            }


            // We do NOT call dotnet publish, and must create dummy app file, otherwise build will fail.
            var appPath = Path.Combine(host.Builder.BuildAppBin, host.Builder.AppExecName);
            File.WriteAllText(appPath, "Dummy app binary");

            host.Builder.BuildPackage();

            if (assertOutput)
            {
                // Check both file and directory (RPM output a directory)
                Assert.True(File.Exists(host.Builder.OutputPath) || Directory.Exists(host.Builder.OutputPath));
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("==============================");
            Console.WriteLine($"BUILD FAILED: {kind}, {complete}");
            Console.WriteLine("==============================");
            Console.WriteLine(e);
            Console.WriteLine();

            // For debug if fails
            Console.WriteLine("==============================");
            Console.WriteLine($"CONFIGURATION: {kind}, {complete}");
            Console.WriteLine("==============================");
            Console.WriteLine(conf.ToString());
            Console.WriteLine();

            Console.WriteLine("==============================");
            Console.WriteLine($"METADATA: {kind}, {complete}");
            Console.WriteLine("==============================");
            Console.WriteLine(metadata ?? "[NONE]");
            Console.WriteLine();

            Console.WriteLine("==============================");
            Console.WriteLine($"MANIFEST: {kind}, {complete}");
            Console.WriteLine("==============================");
            Console.WriteLine(manifest ?? "[NONE]");

            throw;
        }
        finally
        {
            Directory.Delete(conf.OutputDirectory, true);
        }
    }

    private class TestConfiguration : ConfigurationReader
    {
        public TestConfiguration(PackageKind kind, bool complete)
            : base(new ArgumentReader($"-k {kind} -y --verbose"), Create(complete))
        {
            // NB. We need to skip prompt above
        }

        private static string[] Create(bool complete)
        {
            // Use unique temporary directory for everything
            var workDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(workDir);

            var lines = new List<string>();

            // The idea here is to include only the minimal set of configuration we expect to build successfully
            lines.Add($"{nameof(AppBaseName)} = 'HelloWorld'");
            lines.Add($"{nameof(AppFriendlyName)} = Hello World");
            lines.Add($"{nameof(AppId)} = \"net.example.helloworld\"");
            lines.Add($"{nameof(AppVersionRelease)} = 5.4.3[2]");
            lines.Add($"{nameof(AppShortSummary)} = Test <application> only");
            lines.Add($"{nameof(AppLicenseId)} = LicenseRef-LICENSE");

            lines.Add($"{nameof(PublisherName)} = Kuiper Zone");
            lines.Add($"{nameof(PublisherLinkUrl)} = https://kuiper.zone");
            lines.Add($"{nameof(PublisherEmail)} = email@example.net");

            lines.Add($"{nameof(DesktopTerminal)} = true");

            lines.Add($"{nameof(OutputDirectory)} = {workDir}");

            // IMPORTANT - SDK must be installed
            lines.Add($"{nameof(FlatpakPlatformRuntime)} = org.freedesktop.Platform");
            lines.Add($"{nameof(FlatpakPlatformSdk)} = org.freedesktop.Sdk");
            lines.Add($"{nameof(FlatpakPlatformVersion)} = \"22.00\"");
            lines.Add($"{nameof(SetupMinWindowsVersion)} = 10");

            // Always include metafile We actually need to create the file here
            var metapath = Path.Combine(workDir, "app.metainfo.xml");
            File.WriteAllText(metapath, MetaTemplates.MetaInfo);
            lines.Add($"{nameof(MetaFile)} = {metapath}");

            if (complete)
            {
                // Here we add extended configuration we consider optional extras
                lines.Add($"{nameof(PackageName)} = HelloWorld");
                lines.Add($"{nameof(AppDescription)} = Test description\n\n* bullet1\n- bullet2\nLine2");
                lines.Add($"{nameof(PublisherCopyright)} = Copyright Kuiper Zone");
                lines.Add($"{nameof(PublisherLinkName)} = kuiper.zone");

                lines.Add($"{nameof(DesktopNoDisplay)} = false");
                lines.Add($"{nameof(PrimeCategory)} = Development");
                lines.Add($"{nameof(AppImageVersionOutput)} = true");
                lines.Add($"{nameof(FlatpakFinishArgs)} = --socket=wayland;--socket=fallback-x11;--filesystem=host;--share=network");

                lines.Add($"{nameof(RpmAutoReq)} = true");
                lines.Add($"{nameof(RpmAutoProv)} = false");

                lines.Add($"{nameof(SetupVersionOutput)} = true");
                lines.Add($"{nameof(SetupCommandPrompt)} = Command Prompt");

                // Need to create dummy icons in order to get the thing to build (we cheat with dummy files).
                // However, we can't write dummy ico for windows because Setup would fail (needs to be valid icon)
                var icons = new List<string>();
                icons.Add(Path.Combine(workDir, "Icon.32.png"));
                File.WriteAllText(icons[^1], "Dummy file");

                icons.Add(Path.Combine(workDir, "Icon.64.png"));
                File.WriteAllText(icons[^1], "Dummy file");

                icons.Add(Path.Combine(workDir, "Icon.svg"));
                File.WriteAllText(icons[^1], "Dummy file");

                lines.Add($"{nameof(IconFiles)} = {string.Join(';', icons)}");
            }

            return lines.ToArray();
        }
    }
}