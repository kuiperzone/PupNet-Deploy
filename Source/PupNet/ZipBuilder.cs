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

namespace KuiperZone.PupNet;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Zip package.
/// </summary>
public class ZipBuilder : PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public ZipBuilder(ConfigurationReader conf)
        : base(conf, PackKind.Zip)
    {
        PublishBin = Path.Combine(AppRoot, "Publish");
        DesktopExec = AppExecName;

        // Not used
        ManifestPath = null;
        ManifestContent = null;
        PackageCommands = Array.Empty<string>();
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
    public override string? ManifestContent { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsRunOnBuild { get; } = true;

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool CheckInstalled()
    {
        // Always available
        return true;
    }

    /// <summary>
    /// Implements. Does nothing.
    /// </summary>
    public override void WriteVersion()
    {
    }

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void BuildPackage()
    {
        // Package commands empty - does nothing
        base.BuildPackage();

        Operations.Zip(PublishBin, OutputPath);

        if (Arguments.IsRun)
        {
            // Just run the build
            Directory.SetCurrentDirectory(PublishBin);
            Operations.Execute(AppExecName);
        }
    }

}

