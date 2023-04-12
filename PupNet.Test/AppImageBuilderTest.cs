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

using System.Runtime.InteropServices;
using KuiperZone.PupNet.Builders;

namespace KuiperZone.PupNet.Test;

public class AppImageBuilderTest
{
    [Fact]
    public void GetRuntimePath_RuntimeExistsForKnownArch()
    {
        // Ensure files are packaged (even on Windows)
        Assert.True(File.Exists(AppImageBuilder.GetRuntimePath(Architecture.X64)));
        Assert.True(File.Exists(AppImageBuilder.GetRuntimePath(Architecture.Arm64)));
        Assert.True(File.Exists(AppImageBuilder.GetRuntimePath(Architecture.Arm)));

        // There is no RID for linux-x86, so not possible to support for X86
        Assert.Throws<ArgumentException>(() => AppImageBuilder.GetRuntimePath(Architecture.X86));
    }

}
