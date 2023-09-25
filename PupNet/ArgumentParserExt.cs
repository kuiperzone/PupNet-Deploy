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

using System.Diagnostics.CodeAnalysis;
using KuiperZone.Utility.Yaap;

namespace KuiperZone.PupNet;

/// <summary>
/// Extensions for Yaap ArgumentParser 1.0.2.
/// </summary>
public static class ArgumentParserExt
{
    [return: NotNullIfNotNull("def")]
    public static string? GetOrDefault(this ArgumentParser src, string? key, string? def)
    {
        return src[key] ?? def;
    }

    [return: NotNullIfNotNull("def")]
    public static string? GetOrDefault(this ArgumentParser src, string key1, string key2, string? def)
    {
        return GetOrDefault(src, key1 ?? throw new ArgumentNullException(nameof(key1)),
            GetOrDefault(src, key2 ?? throw new ArgumentNullException(nameof(key2)), def));
    }

    public static T GetOrDefault<T>(this ArgumentParser src, string key1, string key2, T def)
        where T : IConvertible
    {
        return src.GetOrDefault<T>(key1 ?? throw new ArgumentNullException(nameof(key1)),
            src.GetOrDefault<T>(key2 ?? throw new ArgumentNullException(nameof(key1)), def));
    }

}