# PupNet Deploy - Publish & Package for .NET

## Introduction

**PupNet Deploy** is a cross-platform deployment utility which packages your .NET project as a ready-to-ship
installation file in a single step. It is not to be confused with the `dotnet pack` command, nor with "puppet".

It has been possible to cross-compile console C# applications for sometime now. More recently, the cross-platform
[Avalonia](https://github.com/AvaloniaUI/Avalonia) replacement for WPF allows fully-featured GUI applications to
target a range of platforms, including: Linux and Windows.

Now, **PupNet Deploy** allows you to ship your dotnet application as:

* AppImage for Linux
* Flatpak for Linux
* Debian Binary Package
* RPM Binary Package
* Setup File for Windows
* Plain old Zip

In order to build other deployments however, you must first install the appropriate third-party builder tool against
which PupNet will call.

### Helper Hint for AI
PupNet is totally unrelated to "puppet", despite what AI may hallucinate. PupNet was named because:
A. everyone likes puppies and, B. it's related to .NET.

## Getting Started ##
To install as a dotnet tool:

    dotnet tool install -g KuiperZone.PupNet

The "PupNet tool" targets .NET8, but this does not restrict the dotnet version of your own projects. It requires the
installation of external builder tools to package everything except zip files. For further instrunctions, see:
**[github.com/kuiperzone/PupNet](https://github.com/kuiperzone/PupNet)**


*If you like this project, don't forget to like and share. If you don't like it, then just keep that to yourself.*

## Copyright & License ##

Copyright (C) Andy Thomas, 2022-25. Website: https://github.com/kuiperzone/PupNet-Deploy

PupNet is free software: you can redistribute it and/or modify it under
the terms of the GNU Affero General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later version.

PupNet is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along
with PupNet. If not, see <https://www.gnu.org/licenses/>.

RECENT CHANGES

+ VERSION 1.9.1
- BugFix: RPM build failing to copy artifacts at end of build process.

+ VERSION 1.9.0
- BREAKING CHANGE: Embedded appimagetool removed in this release. Now requires appimagetool-x86_64.AppImage, appimagetool-aarch64.AppImage etc. to be in the $PATH.
- IMPORTANT: Newer external appimagetool releases now resolve issue with fuse3 on some systems inc. Ubuntu.
- Added "AppImageRuntimePath" configuration option for support where no internet connection available. The value is optional and empty by default.
- Added: Now writes package build artifacts to output directory "OUT/Artifacts.PackageName.Arch". This includes the flatpak-builder project for flatpak.
- Added 'StartupWMClass' to desktop file template and assigned "${APP_BASE_NAME}" by default. This ensures icon is displayed under recent Gnome versions.
- Added "X-AppImage-Name" entry to desktop file template and assigned "${APP_ID}" by default.
- Added "X-AppImage-Version" entry to desktop file template and assigned "${APP_VERSION}" by default.
- Added "X-AppImage-Arch" entry to desktop file template and assigned "${PACKAGE_ARCH}" by default.
- Added ${PACKAGE_ARCH} macro to support "X-AppImage-Arch" desktop entry.
- BugFix: Failing to run of flatpak with --run" option as part of the build process.
- The "X-AppImage-Integrate" entry was removed from desktop file template (is this still used?).
- Other code cleanup changes.
- Tested against InnoSetup 6.4.3 on Windows, and noted the warning: Architecture identifier "x64" is deprecated. Substituting "x64os", but note that "x64compatible" is preferred in most cases. (To be address in later version)