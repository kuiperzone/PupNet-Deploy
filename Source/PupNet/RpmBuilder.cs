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

using System.Text;

namespace KuiperZone.PupNet;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Flatpak package.
/// </summary>
public class RpmBuilder : PackageBuilder
{
    private const string RootName = "BuildRoot";

    /// <summary>
    /// Constructor.
    /// </summary>
    public RpmBuilder(ConfigurationReader conf)
        : base(conf, PackKind.Rpm, RootName)
    {
        PublishBin = Path.Combine(BuildRoot, "opt", Configuration.AppId);
        DesktopExec = AppExecName;

        ManifestPath = Path.Combine(PackRoot, Configuration.AppId + ".spec");


        var list = new List<string>();

        if (BuildUsrBin != null)
        {
            // We put app under /opt, so put script link under usr/bin
            var path = Path.Combine(BuildUsrBin, AppExecName);
            var script = $"#!/bin/sh\nexec {DesktopExec} \\\"$@\\\"";
            list.Add($"echo -e \"{script}\" > \"{path}\"");
            list.Add($"chmod a+x \"{path}\"");
        }

        var temp = Path.Combine(PackRoot, "rpmbuild");
        var output = Path.Combine(OutputDirectory, OutputName);

        // https://stackoverflow.com/questions/2777737/how-to-set-the-rpmbuild-destination-folder
        var cmd = $"rpmbuild -bb \"{ManifestPath}\"";
        cmd += $" --define \"_topdir {temp}\" --buildroot=\"{BuildRoot}\"";
        cmd += $" --define \"_rpmdir {output}\" --define \"_build_id_links none\"";

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
        get
        {
            if (Directory.Exists(BuildRoot))
            {
                return GetSpec(FileOps.ListFiles(BuildRoot));
            }

            return GetSpec();
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override IReadOnlyCollection<string> PackageCommands { get; }

    private string GetSpec(IReadOnlyCollection<string>? files = null)
    {
        // We don't actually need install, build sections.
        var sb = new StringBuilder();

        sb.AppendLine($"Name: {Configuration.AppBaseName}");
        sb.AppendLine($"Version: {AppVersion}");
        sb.AppendLine($"Release: {PackRelease}");
        sb.AppendLine($"BuildArch: {Configuration.GetBuildArch()}");
        sb.AppendLine($"Summary: {Configuration.AppSummary}");
        sb.AppendLine($"License: {Configuration.AppLicense}");
        sb.AppendLine($"Vendor: {Configuration.AppVendor}");

        if (!string.IsNullOrEmpty(Configuration.AppUrl))
        {
            sb.AppendLine($"Url: {Configuration.AppUrl}");
        }

        // We expect dotnet "--self-contained true" to provide ALL dependencies in single directory
        // https://rpm-list.redhat.narkive.com/KqUzv7C1/using-nodeps-with-rpmbuild-is-it-possible
        sb.AppendLine();
        sb.AppendLine("AutoReqProv: no");

        if (DesktopPath != null || MetaInfoPath != null)
        {
            sb.AppendLine("BuildRequires: libappstream-glib");
            sb.AppendLine();
            sb.AppendLine("%check");

            if (DesktopPath != null)
            {
                sb.AppendLine("desktop-file-validate %{buildroot}/%{_datadir}/applications/*.desktop");
            }

            if (MetaInfoPath != null)
            {
                sb.AppendLine("appstream-util validate-relax --nonet %{buildroot}%{_metainfodir}/*.metainfo.xml");
            }
        }

        // Description is mandatory, but just repeat summary
        sb.AppendLine();
        sb.AppendLine("%description");
        sb.AppendLine(Configuration.AppSummary);

        sb.AppendLine();
        sb.AppendLine("%files");

        if (files != null && files.Count != 0)
        {
            foreach (var item in files)
            {
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
            sb.Append("[FILES]");
        }

        return sb.ToString().TrimEnd();
    }

}

