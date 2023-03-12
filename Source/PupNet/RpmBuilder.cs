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
/// Extends <see cref="PackageBuilder"/> for RPM package.
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
        DesktopExec = $"/opt/{Configuration.AppId}/{AppExecName}";

        // We do not set the content here
        ManifestPath = Path.Combine(Root, Configuration.AppId + ".spec");

        var list = new List<string>();
        var temp = Path.Combine(Root, "rpmbuild");

        // https://stackoverflow.com/questions/2777737/how-to-set-the-rpmbuild-destination-folder
        var cmd = $"rpmbuild -bb \"{ManifestPath}\"";
        cmd += $" --define \"_topdir {temp}\" --buildroot=\"{BuildRoot}\"";
        cmd += $" --define \"_rpmdir {OutputPath}\" --define \"_build_id_links none\"";

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
        get { return GetSpec(); }
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

    private string GetSpec()
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

        var files = ListBuild();

        if (files.Count == 0)
        {
            // Placeholder only
            sb.Append("[FILES]");
        }

        foreach (var item in files)
        {
            if (!item.StartsWith('/'))
            {
                sb.Append('/');
            }

            sb.AppendLine(item);
        }

        return sb.ToString().TrimEnd();
    }

}

