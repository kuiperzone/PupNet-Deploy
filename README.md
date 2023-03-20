<p style="text-align:center;margin-bottom:4em;">
    <img src="Media/PupNet.1280x388.png" style="width:100%;"/>
</p>

# Publish and Package for .NET #

## Introduction ##

**PupNet** is cross-platform deployment utility that will publish your .NET project and package the output as a
ready-to-ship installation file in a single step.

**[DOWNLOAD](https://github.com/kuiperzone/PupNet/releases/latest)**

PupNet is licensed under GNU Affero General Public License (AGPL-3.0-or-later), although this does not prevent
its use in commercial projects provided the terms are respected.

It has been possible to cross-compile command-line C# applications for sometime now. More recently, the
cross-platform [Avalonia](https://github.com/AvaloniaUI/Avalonia) replacement for WPF allows fully-featured
GUI applications to be built for a range of platforms, including: Linux, Windows, MacOS and Android.

**PupNet** is a command-line utility which allows you to ship your application as:

* AppImage for Linux
* Setup File for Windows
* Flatpak
* Deb Package (binary)
* RPM Package (binary)
* Plain old Zip

To use it, you fill out the parameters in a simple `pupnet.conf` file and, from within your project, run a command like so:

    pupnet --runtime linux-x64 --kind appimage

In this case, it will call `dotnet publish` on your project and generate a distributable AppImage output file for
use on Linux systems. Additionally, you may optionally provide `.desktop` and AppStream metadata files. There is no need
to provide a complex build-specific manifest, RPM spec or Debian control file. You provide your deployment
configuration once in a single `pupnet.conf`, and PupNet takes care of the underlying build-specific tasks.

Likewise, to package the same project for Windows:

    pupnet -r win-x64 -k setup

PupNet has good support for internationalization, desktop icons, publisher metadata and custom build operations.
Although developed for .NET, it is also possible to use it to deploy C++ and other applications.

The output of PupNet is a distributable local file, such as an AppImage, flatpak or Setup.exe file -- it does
not auto-submit your project to a repository. Note also that, in order to build a Linux deployment, the build system
must be a Linux box and, likewise to build a Windows Setup file, a Windows system (hint: virtual machines are handy).


## Prerequisites ##

Install **PupNet** from the Download Page. If you use the AppImage deployment of PupNet, add it to your path and
consider renaming the AppImage file to `pupnet` so that the instructions below will match your system.

Out of the box, PupNet can create AppImages on Linux and Zip files on all platforms. In order to build other deployments,
you must first install the appropriate third-party builder tool against which PupNet will call.

### Flatpaks on Linux ###
PupNet requires `flatpak` and `flatpak-builder`. It will also be necessary to install a flatpak platform SDK
and runtime. As appropriate:

    sudo dnf install flatpak flatpak-builder

or:

    sudo apt install flatpak flatpak-builder

And then use flatpak to install:

    sudo flatpak install flathub org.freedesktop.Platform//22.08 org.freedesktop.Sdk//22.08

Here, the version number (22.08) was the latest at the time of writing, but will be subject to update.
See [Flatpak Available Runtimes](https://docs.flatpak.org/en/latest/available-runtimes.html).

### Deb Packages on Linux ###

PupNet requires `dpkg-deb`. As appropriate:

    sudo apt install dpkg

or:

    sudo dnf install dpkg

It is possible to install `dpkg-deb` on an RPM based system in order to build Debian packages, although we
should not attempt to install such a package on the system itself (use a virtual machines to test).

### RPM Packages on Linux ###

PupNet requires `rpmbuild`. As appropriate:

    sudo dnf install rpmdevtools

or:

    sudo apt install rpm

It is possible to install `rpmbuild` on a Debian based system in order to build RPM packages, although we
should not attempt to install such a package on the system itself (use a virtual machines to test).

### Setup Files on Windows ###

PupNet leverages [InnoSetup](https://jrsoftware.org/isinfo.php) on Windows. Download and install it.

It will also be necessary to manually add the InnoSetup location to the PATH variable so that PupNet can call the
`iscc` compiler. See below:

<p style="text-align:left;margin-top:2em;margin-bottom:2em;">
    <img src="Media/Screenie-InnoPath.png" style="width:40%;max-width:400px;"/>
</p>


## PupNet Configuration ##

**Hello World for PupNet** is a demonstration project for PupNet Deploy. It lives in [a separate
git repo](https://github.com/kuiperzone/PupNet-HelloWorld) of its own:

<p style="text-align:left;margin-bottom:4em;">
    <a href="https://github.com/kuiperzone/PupNet-HelloWorld" style="outline-style:none;">
        <img src="Media/HelloWorld.1280x388.png" style="width:40%;max-width:400px;"/>
    </a>
</p>

It will be instructive to discuss the major configuration elements with reference to this simple project,
as it demonstrates all the major features of building distributable packages with PupNet.
It can be built for all package kinds, including AppImage, Flatpak, DEB and RPM formats on Linux,
and as a Setup file on Windows. It provides an example of using desktop and AppStream metadata files,
as well as icons and a post-publish script.

Take a look at the `HelloWorld.pupnet.conf` in the root of the project.

<p style="text-align:left;margin-top:2em;margin-bottom:2em;">
    <img src="Media/Screenie-Configuration.png" style="width:60%;max-width:400px;"/>
</p>

This is a simple ini file in which the deployment project is configured. You will see that each parameter
is documented. Where local paths are given, they are local to the `.conf` file, and not your current working directory.
Always use the forward-slash '/' as a path separator.

### Desktop File ###

Note the `DesktopFile` parameter which specifies the path (local to the conf file) to a Linux `.desktop` file.
If you leave it blank, one will be generated automatically. In the Hello World demo, we have specified a file path
in order to show the contents...

Open the file `Deploy/app.desktop`, and you will see:

    [Desktop Entry]
    Type=Application
    Name=${APP_FRIENDLY_NAME}
    Icon=${APP_ID}
    Comment=${APP_SHORT_SUMMARY}
    Exec=${INSTALL_EXEC}
    TryExec=${INSTALL_EXEC}
    Terminal=${IS_TERMINAL_APP}
    Categories=${PRIME_CATEGORY}
    MimeType=
    Keywords=

Note that the contents above are entirely populated using macro variables (more below on these). However, you can
easily extend this file to specify a `MimeType` and `Keywords`, as well as adding international translations and
additional entries.

IMPORTANT: While you can edit this file as you wish, the file must contain the line below with its macro:

    Exec=${INSTALL_EXEC}

Or, if you prefer:

    Exec=${INSTALL_BIN}/app-name

The reason for this is that the actual path on installation will depend on the package type. You cannot hard code this
here as Flatpak and Deb packages, for example, will install to different paths. It is important also that you use macros
as shown with their braces, i.e. `${INSTALL_EXEC}` and not `$INSTALL_EXEC`, as a simple search-and-replace operation is
used to populate them.

In the event that you explicitly require no desktop file, you may declare: `DesktopFile = NONE`. The `DesktopFile`
parameter is mostly ignored for Windows `Setup`, except that setting it to `NONE` is used indicate that you wish
no entry for your main application executable to be written under the Windows Start Menu (i.e. almost synonymous
behavior with that on Linux).

### AppStream Metadata ###

The `MetaFile` parameter specifies the path to a local AppStream metadata XML file. It is optional and, if
omitted, no `.metainfo.xml` file is shipped with your application.

Take a look, for example, at the file provided under: `Deploy/app.metainfo.xml`.

Again, you will notice that macro variables are used. These are entirely optional here and simply mean, that as far
as possible, deployment configuration data is specified in one place only (i.e. your `pupnet.conf` file).

Hint: You can use PupNet to generate a metadata template file:

    pupnet name --new meta

This will create a new file for you under: `name.metainfo.xml`


### Icons ###

Multiple desktop icons may be specified with the `IconPaths` parameter. You can use semi-colon as a separator, such
as: `IconPaths = icon1.ico;icon2.64x64.png;icon3.svg`, or in multi-line form as shown in the Hello World demo:

    IconFiles = """
        Deploy/HelloWorld.16x16.png
        Deploy/HelloWorld.24x24.png
        Deploy/HelloWorld.32x32.png
        Deploy/HelloWorld.48x48.png
        Deploy/HelloWorld.64x64.png
        Deploy/HelloWorld.svg
        Deploy/HelloWorld.ico
    """

Note the use of *triple quotes* for multi-line values. Use only the `svg`, `png` and `ico` file types.

On Linux, `svg` and `png` files will be installed appropriately and the Windows `ico` file ignored. It is
important to specify the size of PNG files by embedding their size in the filename, as shown, or simply as: `name.64.png`.

For Windows `Setup` packages, only the `ico` file is used.

### Dotnet Project Path ###

When building the deployment, PupNet first calls `dotnet publish` on your project. To do this, it needs to know where
your project or solution lives. In `HelloWorld.pupnet.conf`, you may note that the `DotnetProjectPath` value is empty,
however. This is because the `pupnet.conf` sits in the same directory as the `HelloWorld.sln` file.

If your `pupnet.conf` files shares the same directory as your `.sln` or `.csproj` file, you may leave `DotnetProjectPath`
empty. Otherwise use this field specify the path to your solution or project file or directory.

### Application Versioning ###

The application version (plus release number) is specified in the `pupnet.conf` file as:

    AppVersionRelease = 3.2.1[5]

Here we are using the semantic version `3.2.1` plus a "package release number" in square brackets. The release number
applies specifically to the deployment package itself and is used by `rpm` and `deb` package kinds (it is stripped off
for the application's use). If you omit the release number, and just give `3.2.1` for example, it defaults to 1.

You can override the version given in the configuration file at the command line using:

    pupnet --runtime linux-arm64 --kind deb --app-version 4.0.0[1]

Crucially, take a look at the following parameter:

    DotnetPublishArgs = -p:Version=${APP_VERSION} --self-contained true -p:DebugType=None -p:DebugSymbols=false

Here we are specifying parameters to supply to the `dotnet publish` call, and you can see that we also pass the
version in our configuration to the build process (the variable only supplies the application part, i.e. `3.2.1`).
This is optional and you may remove this is you wish, although you will need to specify the version both in application
code and the `pupnet.conf` file.

### Custom Post-Publish Operation ###

In `HelloWorld.pupnet.conf`, we see the lines:

    DotnetPostPublish = Deploy/PostPublish.sh
    DotnetPostPublishOnWindows = Deploy/PostPublish.bat

This is an advanced feature which allows you to call a command or script to perform additional operations after
`dotnet publish` has been called, but prior to building the package. The above commands are called only on their
respective build systems, and should perform equivalent operation.

Post-publish scripts may employ supported macro variables which are exported to the environment. The Hello World
scripts above actually create a subdirectory and dummy file within the application build directory, supplied as
`${BUILD_APP_BIN}`.

Here is the contents of the example bash script:

    #!/bin/bash
    # This is a dummy bash script used for demonstration and test. It outputs a few variables
    # and creates a dummy file in the application directory which will be detected by the program.

    echo
    echo "==========================="
    echo "POST_PUBLISH BASH SCRIPT"
    echo "==========================="
    echo

    # Some useful macros / environment variables
    echo "BUILD_ARCH: ${BUILD_ARCH}"
    echo "BUILD_TARGET: ${BUILD_TARGET}"
    echo "BUILD_SHARE: ${BUILD_SHARE}"
    echo "BUILD_APP_BIN: ${BUILD_APP_BIN}"
    echo

    # Directory and file will be detected by HelloWorld Program
    echo "Do work..."
    set -x #echo on
    mkdir -p "${BUILD_APP_BIN}/subdir"
    touch "${BUILD_APP_BIN}/subdir/file.test"
    set +x #echo off

    echo
    echo "==========================="
    echo "POST_PUBLISH END"
    echo "==========================="
    echo

Additionally, you may leverage these so called post-publish operations to perform the actual build operation itself
and to populate the `${BUILD_APP_BIN}` directory. In principle, you could use this to package the a C++ or Python
application, provided that it is satisfactory to package the application and all its associated libraries in a
single directory.

If you do this, you will wish to disable PupNet from calling `dotnet publish`, which can be done by setting
`DotnetProjectPath = NONE`.


## Building Hello World ##

If you wish to build and try the demo, clone or download the [PupNet Hello World Project](https://github.com/kuiperzone/PupNet-HelloWorld)
to your local drive. Ensure that you have installed the prerequisites above, or at least those you wish to use.

In the terminal, CD into the root of the project directory.

Assuming you're on Linux, type:

    pupnet --kind appimage

This will show the following information and ask for confirmation before building the deployment file:

    PupNet 0.0.1
    Configuration: ./HelloWorld.pupnet.conf

    ============================================================
    APPLICATION: HelloWorld 3.2.1 [5]
    ============================================================

    AppBaseName: HelloWorld
    AppId: zone.kuiper.helloworld
    AppVersion: 3.2.1
    PackageRelease: 5
    StartCommand: helloworld [Not Supported]

    ============================================================
    OUTPUT: APPIMAGE
    ============================================================

    PackageKind: appimage
    Runtime: linux-x64
    Arch: Auto (x86_64)
    Build: Release
    OutputName: HelloWorld.x86_64.AppImage
    OutputDirectory: /mnt/DEVEL-1T/DOTNET/GITHUB/PupNet-HelloWorld/Deploy/bin

    ============================================================
    DESKTOP: app.desktop
    ============================================================

    [Desktop Entry]
    Type=Application
    Name=Hello World
    Icon=zone.kuiper.helloworld
    Comment=A Hello World application
    Exec=usr/bin/HelloWorld
    TryExec=usr/bin/HelloWorld
    Terminal=true
    Categories=Utility
    MimeType=
    Keywords=

    ============================================================
    BUILD PROJECT
    ============================================================

    dotnet publish -r linux-x64 -c Release -p:Version=3.2.1 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o "/tmp/KuiperZone.PupNet/zone.kuiper.helloworld-linux-x64-Release-AppImage/AppDir/usr/bin"

    /mnt/DEVEL-1T/DOTNET/GITHUB/PupNet-HelloWorld/Deploy/PostPublish.sh

    ============================================================
    BUILD PACKAGE: HelloWorld.x86_64.AppImage
    ============================================================

    /tmp/.mount_PupNetK4O50z/usr/bin/appimagetool-x86_64.AppImage  "/tmp/KuiperZone.PupNet/zone.kuiper.helloworld-linux-x64-Release-AppImage/AppDir" "/mnt/DEVEL-1T/DOTNET/GITHUB/PupNet-HelloWorld/Deploy/bin/HelloWorld.x86_64.AppImage"

    ============================================================
    ISSUES
    ============================================================

    [None Detected]

    Continue? [N/y] or ESC aborts?

This tells us that it will create a file called `HelloWorld.x86_64.AppImage`, under the `Deploy/bin` directory.
Moreover, it shows the expanded contents of the desktop file, and gives the `dotnet publish` call it will make so
that we may ensure that everything looks correct by hitting "y".

We can see more information, including the AppStream metadata contents by using:

    pupnet --kind appimage --verbose

The `--verbose` option is useful in other areas too, as we will see below.

Other valid package "kinds" include: `flatpak`, `deb`, `rpm`, `zip` and `setup`.

Let's switch to a Windows machine with InnoSetup installed, and type:

    pupnet --kind setup

In our case, this will gives the file: `HelloWorld.x64.exe`

<p style="text-align:left;margin-top:2em;margin-bottom:2em;">
    <img src="Media/Screenie-Setup.png" style="width:50%;max-width:600px;"/>
</p>









### Legacy Publish-AppImage ###

Pup.NET began life as "*Publish-AppImage for .NET*". It was renamed when support for Flatpaks was added.

<p style="text-align:left;">
    <a href="https://github.com/kuiperzone/Publish-AppImage">
        <img title="Publish-AppImage" alt="Publish-AppImage" src="Images/Publish-AppImage.png" style="width:40%;max-width:250px;"/>
    </a>
</p>

See: [https://github.com/kuiperzone/Publish-AppImage](https://github.com/kuiperzone/Publish-AppImage)

### Cross-Platform GUI Development ###

You may also be interested in the [Avalonia](https://github.com/AvaloniaUI/Avalonia) framework -- the replacement for WPF.

You can now create cross-platform GUI apps using C# which run virtually any where, including
Windows, Linux, MacOS and Android. Using **PUBPAK for .NET**, you can easily deploy them on Linux
without runtime concerns.

Shown below is another cross-platform project of mine -- a cross-platform Avalonia XAML Previewer
called [AvantGarde](https://github.com/kuiperzone/AvantGarde).

<p style="text-align:left;">
    <a href="https://github.com/kuiperzone/AvantGarde">
        <img title="AvantGarde Screenshot" alt="AvantGarde Screenshot" src="Images/AvantGarde.png" style="width:50%;max-width:500px;"/>
    </a>
</p>

**PUBPAK for .NET** was created by Andy Thomas at [https://kuiper.zone](https://kuiper.zone).

If you like this project, don't forget to like and share.
