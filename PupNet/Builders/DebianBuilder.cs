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

using System.Text;

namespace KuiperZone.PupNet.Builders;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Debian package.
/// https://www.baeldung.com/linux/create-debian-package
/// </summary>
public sealed class DebianBuilder : PackageBuilder
{
    private readonly string _debianPackageName;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DebianBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Deb)
    {
        _debianPackageName = Configuration.PackageName.ToLowerInvariant();

        BuildAppBin = Path.Combine(BuildRoot, "opt", Configuration.AppId);
        InstallBin = $"/opt/{Configuration.AppId}";

        // We do not set the content here
        ManifestBuildPath = Path.Combine(BuildRoot, "DEBIAN/control");

        var list = new List<string>();
        var cmd = "dpkg-deb --root-owner-group ";

        if (Arguments.IsVerbose)
        {
            cmd += "--verbose ";
        }

        var archiveDirectory = Path.Combine(OutputDirectory, OutputName);
        cmd += $"--build \"{BuildRoot}\" \"{archiveDirectory}\"";
        list.Add(cmd);
        PackageCommands = list;
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string OutputName
    {
        get
        {
            var output = Path.GetFileName(Configuration.Arguments.Output);

            if (string.IsNullOrEmpty(output))
            {
                // packagename_version-release_architecture.deb
                // https://kerneltalks.com/tools/understanding-package-naming-convention-rpm-deb/
                return $"{_debianPackageName}_{AppVersion}-{PackageRelease}_{Architecture}.deb";
            }

            return output;
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string Architecture
    {
        get
        {
            if (Arguments.Arch != null)
            {
                return Arguments.Arch;
            }

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.X64)
            {
                return "amd64";
            }

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.Arm64)
            {
                return "arm64";
            }

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.X86)
            {
                // Not sure about this?
                // https://en.wikipedia.org/wiki/X32_ABI
                return "x32";
            }

            return Runtime.RuntimeArch.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string BuildAppBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string InstallBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestBuildPath { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestContent
    {
        get { return GetControlFile(); }
        set { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsStartCommand { get; } = true;

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsPostRun { get; }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void Create(string? desktop, string? metainfo)
    {
        base.Create(desktop, metainfo);

        // Rpm and Deb etc only. These get installed to /opt, but put 'link file' in /usr/bin
        if (BuildUsrBin != null && !string.IsNullOrEmpty(Configuration.StartCommand))
        {
            // We put app under /opt, so put script link under usr/bin
            var path = Path.Combine(BuildUsrBin, Configuration.StartCommand);
            var script = $"#!/bin/sh\nexec {InstallExec} \"$@\"";

            if (!File.Exists(path))
            {
                Operations.WriteFile(path, script);
                Operations.Execute($"chmod a+rx \"{path}\"");
            }
        }
    }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void BuildPackage()
    {
        if (Configuration.AppLicenseFile != null && BuildUsrShare != null)
        {
            var dest = Path.Combine(BuildUsrShare, "doc", _debianPackageName, "Copyright");
            Operations.CopyFile(Configuration.AppLicenseFile, dest, true);
        }

        base.BuildPackage();
    }

    private static string ToSection(string? category)
    {
        // https://www.debian.org/doc/debian-policy/ch-archive.html#s-subsections
        switch (category?.ToLowerInvariant())
        {
            case "audiovideo" : return "video";
            case "audio" : return "sound";
            case "video" : return "video";
            case "development" : return "development";
            case "education" : return "education";
            case "game" : return "games";
            case "graphics" : return "graphics";
            case "network" : return "net";
            case "office" : return "text";
            case "science" : return "science";
            case "settings" : return "utils";
            case "system" : return "utils";
            case "utility" : return "utils";
            default: return "misc";
        }
    }

    private string GetControlFile()
    {
        // https://www.debian.org/doc/debian-policy/ch-controlfields.html
        var sb = new StringBuilder();

        sb.AppendLine($"Package: {_debianPackageName}");
        sb.AppendLine($"Version: {AppVersion}-{PackageRelease}");

        // Section is recommended
        // https://askubuntu.com/questions/27513/what-is-the-difference-between-debian-contrib-non-free-and-how-do-they-corresp
        // https://www.debian.org/doc/debian-policy/ch-archive.html#s-subsections
        sb.AppendLine($"Section: multiverse/{ToSection(Configuration.PrimeCategory)}");

        sb.AppendLine($"Priority: optional");
        sb.AppendLine($"Architecture: {Architecture}");
        sb.AppendLine($"Description: {Configuration.AppShortSummary}");

        // https://www.debian.org/doc/debian-policy/ch-controlfields.html#s-f-description
        foreach (var item in Configuration.AppDescription)
        {
            if (!string.IsNullOrEmpty(item))
            {
                sb.Append("  ");
                sb.AppendLine(item);
            }
            else
            {
                sb.AppendLine(" .");
            }
        }


        if (!string.IsNullOrEmpty(Configuration.PublisherLinkUrl))
        {
            sb.AppendLine($"Homepage: {Configuration.PublisherLinkUrl}");
        }

        // https://www.debian.org/doc/debian-policy/ch-controlfields.html#s-f-maintainer
        sb.AppendLine($"Maintainer: {Configuration.PublisherEmail}");

        // Treated as comments
        sb.AppendLine($"License: {Configuration.AppLicenseId}");
        sb.AppendLine($"Vendor: {Configuration.PublisherName}");

        bool started = false;
        foreach (var item in Configuration.DebianRecommends)
        {
            sb.Append(started ? ", " : "Recommends: ");
            sb.Append(item);
            started = true;
        }

        // Required
        sb.AppendLine();

        return sb.ToString();
    }

}

