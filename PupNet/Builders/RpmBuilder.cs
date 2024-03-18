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
/// Extends <see cref="PackageBuilder"/> for RPM package.
/// https://docs.fedoraproject.org/en-US/package-maintainers/Packaging_Tutorial_GNU_Hello/
/// https://www.techrepublic.com/article/making-rpms-part-1-the-spec-file-header/
/// </summary>
public sealed class RpmBuilder : PackageBuilder
{
    private bool _specFilesHack;
    private readonly string _rpmPackageName;
    private readonly string _buildOutputDirectory;

    /// <summary>
    /// Constructor.
    /// </summary>
    public RpmBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Rpm)
    {
        _rpmPackageName = Configuration.PackageName.ToLowerInvariant();

        BuildAppBin = Path.Combine(BuildRoot, "opt", Configuration.AppId);
        InstallBin = $"/opt/{Configuration.AppId}";

        // We do not set the content here
        ManifestBuildPath = Path.Combine(Root, Configuration.AppId + ".spec");

        var list = new List<string>();
        var buildDir = Path.Combine(Root, "rpmbuild");

        // We are going to put the final rpm file in a temporary directory.
        // The rpmbuild directory always creates a subdirectory and filename of its own volition,
        // or form: "out/X86_64/appname-1.5.0-1.x86_64.rpm". We need to find it later and
        // and copy it to our final output location.
        _buildOutputDirectory = $"{buildDir}/out";

        // Can't build for arm64 on an x64 development system?
        // https://stackoverflow.com/questions/64563386/how-do-i-package-up-go-code-as-an-arm-rpm
        // https://cmake.cmake.narkive.com/uDOFCNJ3/rpmbuild-architecture
        // https://stackoverflow.com/questions/2777737/how-to-set-the-rpmbuild-destination-folder
        var cmd = $"rpmbuild -bb \"{ManifestBuildPath}\"";
        cmd += $" --define \"_topdir {buildDir}\" --buildroot=\"{BuildRoot}\"";
        cmd += $" --define \"_rpmdir {_buildOutputDirectory}\" --define \"_build_id_links none\"";

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
            var output = Path.GetFileName(Configuration.Arguments.Output);

            if (string.IsNullOrEmpty(output))
            {
                // name-version-release.architecture.rpm
                // https://docs.oracle.com/en/database/oracle/oracle-database/18/ladbi/rpm-packages-naming-convention.html
                return $"{_rpmPackageName}_{AppVersion}-{PackageRelease}.{Architecture}.rpm";
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
                Operations.Execute($"chmod a+rx \"{path}\"");
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

        // Now, we have built rpm to temporary directory (see PackageCommands in constructor).
        // We have a special method to locate the file, and we will copy to final output location.
        Operations.CopyFile(LocatePackageFilePath(), OutputPath);
    }

    private string LocatePackageFilePath(string? dir = null)
    {
        // The rpmbuild directory always creates a subdirectory and filename of its own volition,
        // or form: "out/X86_64/appname-1.5.0-1.x86_64.rpm". Our own determination of the path
        // should normally work, but we will look for the file if, for example, the architecture
        // does not match our own, or the naming convention is otherwise different. For this to
        // work, it is expected that the rpmbuild directory is re-created each time.
        dir ??= Path.Combine(_buildOutputDirectory, Architecture);

        if (Directory.Exists(dir))
        {
            var expect = $"{_rpmPackageName}_{AppVersion}-{PackageRelease}.{Architecture}.rpm";
            var path = Path.Combine(dir, expect);

            if (File.Exists(path))
            {
                // Found in expected location
                return path;
            }

            // In this case, we expect a single rpm file
            var files = Directory.GetFiles(dir, "*.rpm", System.IO.SearchOption.TopDirectoryOnly);

            if (files.Length == 1)
            {
                return files[0];
            }

            if (files.Length > 1)
            {
                // We don't which one to use
                throw new FileNotFoundException("Multiple *.rpm files found under " + dir);
            }

            throw new FileNotFoundException("Expected *.rpm file not found under " + dir);
        }

        // In this case, we expect a single directory under dir.
        var opts = new EnumerationOptions();
        opts.RecurseSubdirectories = false;
        opts.ReturnSpecialDirectories = false;
        opts.IgnoreInaccessible = true;

        var subs = Directory.GetDirectories(dir, "*", opts);

        if (subs.Length == 1)
        {
            // Recurse with expected directory
            return LocatePackageFilePath(subs[0]);
        }

        throw new DirectoryNotFoundException("Expected single rpm output subdirectory not found under " + dir);
    }

    private string GetSpec()
    {
        // We don't actually need install, build sections.
        // https://rpm-software-management.github.io/rpm/manual/spec.html
        var sb = new StringBuilder();

        sb.AppendLine($"Name: {_rpmPackageName}");
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

        if (Configuration.AppDescription.Count != 0)
        {
            foreach (var item in Configuration.AppDescription)
            {
                sb.AppendLine(item);
            }
        }
        else
        {
            // Fallback
            sb.AppendLine(Configuration.AppShortSummary);
        }

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

