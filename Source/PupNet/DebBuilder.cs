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

using System.Runtime.InteropServices;
using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Debian package.
/// https://www.baeldung.com/linux/create-debian-package
/// </summary>
public class DebBuilder : PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public DebBuilder(ConfigurationReader conf)
        : base(conf, PackKind.Deb)
    {
        PublishBin = Path.Combine(AppRoot, "opt", Configuration.AppId);
        DesktopExec = $"/opt/{Configuration.AppId}/{AppExecName}";

        // We do not set the content here
        ManifestPath = Path.Combine(AppRoot, "DEBIAN/control");

        var list = new List<string>();
        var cmd = "dpkg-deb --root-owner-group ";

        if (Arguments.IsVerbose)
        {
            cmd += "--verbose ";
        }

        var archiveDirectory = Path.Combine(OutputDirectory, OutputName);
        cmd += $"--build \"{AppRoot}\" \"{archiveDirectory}\"";
        list.Add(cmd);
        PackageCommands = list;
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string DesktopExec{ get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string PublishBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestPath { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestContent
    {
        get { return GetControl(); }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsRunOnBuild { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool CheckInstalled()
    {
        return WriteVersion("dpkg-deb", "--version", true);
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void WriteVersion()
    {
        WriteVersion("dpkg-deb", "--version");
    }

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
            var script = $"#!/bin/sh\nexec {DesktopExec} \"$@\"";

            if (!File.Exists(path))
            {
                Operations.WriteFile(path, script);
                Operations.Execute($"chmod a+x {path}");
            }
        }
    }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void BuildPackage()
    {
        Operations.CreateDirectory(Path.GetDirectoryName(ManifestPath));
        Operations.CreateDirectory(OutputPath);
        base.BuildPackage();
    }

    private string GetControl()
    {
        // https://www.debian.org/doc/debian-policy/ch-controlfields.html
        var sb = new StringBuilder();

        sb.AppendLine($"Package: {Configuration.AppBaseName.ToLowerInvariant()}");
        sb.AppendLine($"Version: {AppVersion}-{PackRelease}");
        sb.AppendLine($"Section: misc");
        sb.AppendLine($"Priority: optional");
        sb.AppendLine($"Architecture: {Architecture}");
        sb.AppendLine($"Description: {Configuration.AppSummary}");

        if (!string.IsNullOrEmpty(Configuration.AppUrl))
        {
            sb.AppendLine($"Homepage: {Configuration.AppUrl}");
        }

        // Annoying!
        // https://www.debian.org/doc/debian-policy/ch-controlfields.html#s-f-maintainer
        sb.AppendLine($"Maintainer: Not used <example-inc@example.com>");

        // Treated as comments
        sb.AppendLine($"License: {Configuration.AppLicense}");
        sb.AppendLine($"Vendor: {Configuration.AppVendor}");
        sb.AppendLine();


        return sb.ToString();
    }

}

