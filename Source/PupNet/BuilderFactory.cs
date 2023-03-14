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
/// Creates a concrete instance of <see cref="PackageBuilder"/>.
/// </summary>
public class BuilderFactory
{
    /// <summary>
    /// Creates.
    /// </summary>
    public PackageBuilder Create(ConfigurationReader conf)
    {
        switch (conf.Arguments.Kind)
        {
            case PackKind.AppImage: return new AppImageBuilder(conf);
            case PackKind.Flatpak: return new FlatpakBuilder(conf);
            case PackKind.Rpm: return new RpmBuilder(conf);
            case PackKind.Deb: return new DebBuilder(conf);
            case PackKind.WinSetup: return new WinSetupBuilder(conf);
            case PackKind.Zip: return new ZipBuilder(conf);
            default: throw new ArgumentException($"Invalid or unsupported {nameof(PackKind)} {conf.Arguments.Kind}");
        }
    }

    /// <summary>
    /// Creates with dummy configuration.
    /// </summary>
    public PackageBuilder Create(PackKind kind)
    {
        return Create(new ConfigurationReader(kind));
    }
}