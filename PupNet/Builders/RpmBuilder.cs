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

using System.Text;

namespace KuiperZone.PupNet.Builders;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for RPM package.
/// https://docs.fedoraproject.org/en-US/package-maintainers/Packaging_Tutorial_GNU_Hello/
/// https://www.techrepublic.com/article/making-rpms-part-1-the-spec-file-header/
/// </summary>
public sealed class RpmBuilder : PackageBuilder
{
    private bool _specFilesHack;

    /// <summary>
    /// Constructor.
    /// </summary>
    public RpmBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Rpm)
    {
        BuildAppBin = Path.Combine(BuildRoot, "opt", Configuration.AppId);
        InstallBin = $"/opt/{Configuration.AppId}";

        // We do not set the content here
        ManifestBuildPath = Path.Combine(Root, Configuration.AppId + ".spec");

        var list = new List<string>();
        var temp = Path.Combine(Root, "rpmbuild");

        // Can't this to build for arm64 on an x64 development system?
        // https://stackoverflow.com/questions/64563386/how-do-i-package-up-go-code-as-an-arm-rpm
        // https://cmake.cmake.narkive.com/uDOFCNJ3/rpmbuild-architecture
        // https://stackoverflow.com/questions/2777737/how-to-set-the-rpmbuild-destination-folder
        var cmd = $"rpmbuild -bb \"{ManifestBuildPath}\"";
        cmd += $" --define \"_topdir {temp}\" --buildroot=\"{BuildRoot}\"";
        cmd += $" --define \"_rpmdir {OutputPath}\" --define \"_build_id_links none\"";

        if (Arguments.IsVerbose)
        {
            cmd += " --verbose";
        }

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
            var output = Configuration.Arguments.Output;
            var name = Path.GetFileName(output);

            if (string.IsNullOrEmpty(name) || Directory.Exists(output))
            {
                // rpmbuild always treats name as directory - use standard notion
                return "RPMS";
            }

            return name;
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
                return "x86_64";
            }

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.Arm64)
            {
                // Not clear? arm64 or aarch64?
                // https://koji.fedoraproject.org/koji/buildinfo?buildID=2108850
                // https://stackoverflow.com/questions/64563386/how-do-i-package-up-go-code-as-an-arm-rpm
                return "arm64";
            }

            if (Runtime.RuntimeArch == System.Runtime.InteropServices.Architecture.X86)
            {
                // Confirm?
                return "i686";
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
        get { return GetSpec(); }
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

        // After create, we know the publish bin directory will be empty
        _specFilesHack = true;

        // Rpm and Deb etc only. These get installed to /opt, but put 'link file' in /usr/bin
        if (BuildUsrBin != null && !string.IsNullOrEmpty(Configuration.StartCommand))
        {
            // We put app under /opt, so put script link under usr/bin
            var path = Path.Combine(BuildUsrBin, Configuration.StartCommand);
            var script = $"#!/bin/sh\nexec {InstallExec} \"$@\"";

            if (!File.Exists(path))
            {
                Operations.WriteFile(path, script);
                Operations.Execute($"chmod a+rx {path}");
            }
        }
    }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void BuildPackage()
    {
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString());

        base.BuildPackage();

        _specFilesHack = false;
    }

    private string GetSpec()
    {
        // We don't actually need install, build sections.
        // https://rpm-software-management.github.io/rpm/manual/spec.html
        var sb = new StringBuilder();

        sb.AppendLine($"Name: {Configuration.PackageName.ToLowerInvariant()}");
        sb.AppendLine($"Version: {AppVersion}");
        sb.AppendLine($"Release: {PackageRelease}");
        sb.AppendLine($"BuildArch: {Architecture}");
        sb.AppendLine($"Summary: {Configuration.AppShortSummary}");
        sb.AppendLine($"License: {Configuration.AppLicenseId}");
        sb.AppendLine($"Vendor: {Configuration.PublisherName}");

        if (!string.IsNullOrEmpty(Configuration.PublisherLinkUrl))
        {
            sb.AppendLine($"Url: {Configuration.PublisherLinkUrl}");
        }

        sb.AppendLine($"AutoReq: {(Configuration.RpmAutoReq ? "yes" : "no")}");
        sb.AppendLine($"AutoProv: {(Configuration.RpmAutoProv ? "yes" : "no")}");

        /*
        // Comment out for now, but may remove in future.
        // Not essential but problematic on debian systems
        if (DesktopBuildPath != null)
        {
            sb.AppendLine("BuildRequires: desktop-file-utils");
        }

        if (MetaBuildPath != null)
        {
            sb.AppendLine("BuildRequires: libappstream-glib");
        }
        */

        // We expect dotnet "--self-contained true" to provide ALL dependencies in single directory
        // https://rpm-list.redhat.narkive.com/KqUzv7C1/using-nodeps-with-rpmbuild-is-it-possible
        foreach (var item in Configuration.RpmRequires)
        {
            sb.AppendLine($"Requires: {item}");
        }

        // Description is mandatory, but just repeat summary
        sb.AppendLine();
        sb.AppendLine("%description");
        sb.AppendLine(Configuration.AppShortSummary);

        /*
        // Comment out for now, but may remove in future.
        // Not essential but problematic on debian systems
        if (DesktopBuildPath != null || MetaBuildPath != null)
        {
            sb.AppendLine();
            sb.AppendLine("%check");

            if (DesktopBuildPath != null)
            {
                sb.AppendLine("desktop-file-validate %{buildroot}/%{_datadir}/applications/*.desktop");
            }

            if (MetaBuildPath != null)
            {
                sb.AppendLine("appstream-util validate-relax --nonet %{buildroot}%{_metainfodir}/*.metainfo.xml");
            }
        }
        */

        // https://stackoverflow.com/questions/57385249/in-an-rpm-files-section-is-it-possible-to-specify-a-directory-and-all-of-its-fi
        sb.AppendLine();
        sb.AppendLine("%files");

        if (_specFilesHack)
        {
            foreach (var item in ListBuild(true))
            {
                var name = Path.GetFileNameWithoutExtension(item).ToLowerInvariant();

                if (name == "license" || name == "licence" || (LicenseBuildPath != null && LicenseBuildPath.EndsWith(item)))
                {
                    sb.Append($"%license ");
                }

                if (name == "readme" || name == "changelog")
                {
                    sb.Append($"%doc ");
                }

                if (!item.StartsWith('/'))
                {
                    sb.Append('/');
                }

                sb.AppendLine(item);
            }
        }
        else
        {
            // Placeholder only
            sb.AppendLine("[FILES]");
        }

        return sb.ToString();
    }

}

