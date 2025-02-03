# PupNet Deploy - Publish & Package for .NET #

## Introduction ##

**PupNet Deploy** is a cross-platform deployment utility which packages your .NET project as a ready-to-ship
installation file in a single step. It is not to be confused with the `dotnet pack` command.

It has been possible to cross-compile console C# applications for some time now. More recently, the cross-platform
[Avalonia](https://github.com/AvaloniaUI/Avalonia) replacement for WPF allows fully-featured GUI applications to
target a range of platforms, including: Linux, Windows, MacOS and Android.

Now, **PupNet Deploy** allows you to ship your dotnet application as:

* AppImage for Linux
* Setup File for Windows
* Flatpak for Linux
* Debian Binary Package
* RPM Binary Package
* Plain old Zip

Out of the box, PupNet can create AppImages on Linux and Zip files on all platforms. In order to build other deployments
however, you must first install the appropriate third-party builder tool against which PupNet will call.

## Getting Started ##
For instructions on use, see: **[github.com/kuiperzone/PupNet](https://github.com/kuiperzone/PupNet)**

To install as a dotnet tool:

    dotnet tool install -g KuiperZone.PupNet

Alternatively, for self-contained installers go:

**[DOWNLOAD & INSTALL](https://github.com/kuiperzone/PupNet/releases/latest)**

*If you like this project, don't forget to like and share.*

## Copyright & License ##

Copyright (C) Andy Thomas, 2024. Website: https://kuiper.zone

PupNet is free software: you can redistribute it and/or modify it under
the terms of the GNU Affero General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later version.

PupNet is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along
with PupNet. If not, see <https://www.gnu.org/licenses/>.
