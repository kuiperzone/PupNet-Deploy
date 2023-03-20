<p style="text-align:center;margin-bottom:4em;">
    <img src="Media/PupNet.1280x388.png" style="width:100%;"/>
</p>

# Publish and Package for .NET #

## Introduction ##

**PupNet Deploy** is cross-platform deployment utility that will publish your .NET project and package the output as a
ready-to-ship installation file in a single step.

**[DOWNLOAD](https://github.com/kuiperzone/PupNet/releases/latest)**

It has been possible to cross-compile console C# applications for sometime now. More recently, the
cross-platform [Avalonia](https://github.com/AvaloniaUI/Avalonia) replacement for WPF allows fully-featured
GUI applications to target a range of platforms, including: Linux, Windows, MacOS and Android.

Now, **PupNet Deploy** allows you to ship your dotnet application as:

* AppImage for Linux
* Setup File for Windows
* Flatpak
* Deb Package (binary)
* RPM Package (binary)
* Plain old Zip

To create an installation file, you fill out the parameters in a simple `pupnet.conf` file and, from within your project,
run a command like so:

    pupnet --runtime linux-x64 --kind appimage

In this case, PupNet calls `dotnet publish` on your project and generates a distributable [AppImage](https://github.com/AppImage/AppImageKit)
file fo use on Linux systems. Additionally, you may optionally provide `.desktop` and AppStream metadata files.
There is no need to write complex build-specific manifests, RPM spec or Debian control files. You need only supply your
deployment configuration once as a single `pupnet.conf`, and PupNet takes care of the underlying build-specific tasks.

Likewise, to package the same project for Windows:

    pupnet -r win-x64 -k setup

PupNet has good support for internationalization, desktop icons, publisher metadata and custom build operations.
Although developed for .NET, it is also possible to use it to deploy C++ and other kinds of applications.

The output of PupNet is a distributable file, such as an AppImage, flatpak or Setup.exe file -- it does
not auto-submit your project to a repository. Note also that, in order to build a Linux deployment, the build system
must be a Linux box and, likewise to build a Windows Setup file, a Windows system (hint: virtual machines are handy).

However, it is possible to build a Debian package on an RPM machine, and viceversa.


## Prerequisites ##

Install **PupNet** from the Download Page. If you use the AppImage deployment of PupNet, add it to your path and
consider renaming the AppImage file to `pupnet` so that the instructions below will match your system.

Out of the box, PupNet can create AppImages on Linux and Zip files on all platforms. In order to build other deployments,
however, you must first install the appropriate third-party builder tool against which PupNet will call.

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

PupNet requires `dpkg-deb`. So, as appropriate:

    sudo apt install dpkg

or:

    sudo dnf install dpkg

It is possible to build Debian packages using `dpkg-deb` on an RPM based system, although you should
use a virtual machines to test it.

### RPM Packages on Linux ###

PupNet requires `rpmbuild`. As appropriate:

    sudo dnf install rpmdevtools

or:

    sudo apt install rpm

It is possible to build RPM packages using `rpmbuild` on a Debian system although, again, you should
use a virtual machines to test it.


### Setup Files on Windows ###

PupNet leverages [InnoSetup](https://jrsoftware.org/isinfo.php) on Windows. Download and install it.

It will also be necessary to manually add the InnoSetup location to the user's PATH variable
so that PupNet can call the `iscc` compiler. See below:

<p style="text-align:left;margin-top:2em;margin-bottom:2em;">
    <img src="Media/Screenie-InnoPath.png" style="width:40%;max-width:400px;"/>
</p>


## PupNet Configuration ##

**Hello World for PupNet** is a demonstration project for PupNet Deploy. It will be instructive to discuss
the major configuration elements with reference to this simple project, as it demonstrates all the major
features of building distributable packages.

The Hello World project lives in [a separate git repo](https://github.com/kuiperzone/PupNet-HelloWorld) of its own:

<p style="text-align:left;margin-bottom:4em;">
    <a href="https://github.com/kuiperzone/PupNet-HelloWorld" style="outline-style:none;">
        <img src="Media/HelloWorld.1280x388.png" style="width:40%;max-width:400px;"/>
    </a>
</p>

Head over to this project in a different browser tab, or download it to your local drive.

Now, take a look at the `HelloWorld.pupnet.conf` in the root of the project.

<p style="text-align:left;margin-top:2em;margin-bottom:2em;">
    <img src="Media/Screenie-Configuration.png" style="width:60%;max-width:400px;"/>
</p>

This is a simple ini file in which the deployment project is configured. You will see that each parameter is documented.
Where local paths are given, they are local to the `.conf` file, rather than your current working directory.
Always use the forward-slash '/' as a path separator for cross-platform compatibility.

### Desktop File ###

Note the `DesktopFile` parameter which provides a path to a Linux `.desktop` file. If you leave it blank, one will
be generated automatically. In the Hello World demo, we have specified a file in order to show the contents...

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
here as different packages will install to different paths. It is important also that you use macros as shown with
their braces, i.e. `${INSTALL_EXEC}` and not `$INSTALL_EXEC`, as a simple search-and-replace operation is
used to populate them.

In the event that you explicitly require no desktop file, you may declare: `DesktopFile = NONE`. The `DesktopFile`
parameter is mostly ignored for Windows `Setup`, except that setting it to `NONE` is used indicate that you wish
no entry for your main application executable to be written under the Windows Start Menu (i.e. synonymous
behavior with that on Linux).

### AppStream Metadata ###

The `MetaFile` parameter specifies the path to a local AppStream metadata XML file. It is optional and, if
omitted, no `.metainfo.xml` file is shipped with your application.

Take a look, for example, at the file provided under: `Deploy/app.metainfo.xml`.

Again, you will notice that macro variables are used. These are entirely optional here and simply mean, that as far
as possible, deployment configuration data is specified in one place only (i.e. your `pupnet.conf` file).

Hint: You can use PupNet itself to generate a metadata template file:

    pupnet name --new meta

This will create a new file for you under: `name.metainfo.xml`


### Icons ###

Multiple desktop icons are provided with the `IconPaths` parameter. You can use semi-colon as a separator, such as:
`IconPaths = icon1.ico;icon2.64x64.png;icon3.svg`, or present them in multi-line form as shown in the Hello World demo:

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
empty. Otherwise use this field to specify the path to your solution or project file or directory relative to the location
of the configuration file.

### Dotnet Publish Arguments ###

The `DotnetPublishArgs` parameter is used to specify values to supply to the `dotnet publish` call:

    DotnetPublishArgs = -p:Version=${APP_VERSION} --self-contained true -p:DebugType=None -p:DebugSymbols=false

As an absolute minimum, we always want to include `--self-contained true`, as this will cause dotnet not only to
compile the application, but to publish all dependencies into a single directory.

### Application Versioning ###

The application version (plus release number) is specified in the `pupnet.conf` file as:

    AppVersionRelease = 3.2.1[5]

Here we are using the semantic version `3.2.1` plus a "package release number" in square brackets. The release number
applies specifically to the deployment package itself and is used by `rpm` and `deb` package kinds (it is stripped off
for the application's use). If you omit the release number, and just give `3.2.1` for example, it defaults to 1.

If you prefer, you can override the version in the configuration file from the command line using:

    pupnet --runtime linux-arm64 --kind deb --app-version 4.0.0[1]

Crucially, if we look again at the `DotnetPublishArgs` value:

    DotnetPublishArgs = -p:Version=${APP_VERSION} --self-contained true -p:DebugType=None -p:DebugSymbols=false

Here, we can see that the version from our configuration is supplied to the build process as a variable (only supplies the
application part is supplied, i.e. `3.2.1`). This is optional and you may remove it if you wish, although you will need to
specify the version both in application code and the `pupnet.conf` file in this case.

### Command-Line Applications ###

With the advent of the Avalonia cross-platform GUI framework, it was envisaged that a typical use case for PupNet
would be to deploy GUI applications that are integrated with the desktop. However, it is possible to support command-line centric
applications (except for flatpak), but note that by default, your application will not be in the user's path.

In `deb` and `rpm` deployments, the application is installed to the `/opt` directory, rather than `/usr/bin`.
If you wish that application be accessible from the command line, assign a suitable command name to the following:

    StartCommand = helloworld

This will cause a small bash script to be written under `/usr/bin` which will launch your application.

Under Windows, PupNet offers the option to include a "Command Prompt" entry under the Programs Menu in order to launch
a dedicated Console Window inside of which your application is accessible. See the `SetupCommandPrompt` parameter,
and the Windows screenshots below.

### Custom Post-Publish Operations ###

In `HelloWorld.pupnet.conf`, we see the lines:

    DotnetPostPublish = Deploy/PostPublish.sh
    DotnetPostPublishOnWindows = Deploy/PostPublish.bat

This is an advanced feature which allows you to call a command or script in order to perform additional operations after
`dotnet publish` has been called, but prior to building the package output. The above commands are called only on their
respective build systems, and should perform equivalent operation.

Such scripts may employ macro variables which are exported to the environment. The Hello World bash and batch scripts above,
for example, create a subdirectory and dummy file within the application build directory, the path of which is supplied
as the `${BUILD_APP_BIN}` environment variable (see below for a complete reference).

Here are the contents of our example bash script:

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
and populate the `${BUILD_APP_BIN}` directory with the output your build process -- whatever that may be. In principle,
you could use this to package the a C++ or Python application, provided that it is satisfactory that the application and
all its associated libraries are contained in a single directory.

If you do this, you will wish to disable PupNet from calling `dotnet publish`, which can be done by setting
`DotnetProjectPath = NONE`.


## Building Hello World ##

**Hello World for PupNet** can be built for all package kinds, including `appimage`, `flatpak`, `deb`, `rpm`, `zip`
and `setup` for Windows.

If you wish to build and try the demo, clone or download the [PupNet Hello World Project](https://github.com/kuiperzone/PupNet-HelloWorld)
to your local drive. Ensure that you have installed the prerequisites above, or at least those you wish to use.

In the terminal, CD into the root of the project directory.

### On Linux ###

Assuming you're on Linux, type:

    pupnet --kind appimage

This will show the following information and ask for confirmation before building the deployment file.

    PupNet 0.0.1
    Configuration: HelloWorld.pupnet.conf

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
that we may ensure that everything looks correct before hitting "y".

We can view more information, including the AppStream metadata contents, by using:

    pupnet --kind appimage --verbose

The `--verbose` option is useful in other areas too, as we will see below.

### On Windows ###

On a Windows machine with InnoSetup installed, type:

    pupnet --kind setup

This will generate the file: `HelloWorld.x64.exe`, which may be launched to install the program.

<p style="text-align:left;margin-top:2em;margin-bottom:2em;">
    <img src="Media/Screenie-Setup.png" style="width:50%;max-width:600px;"/>
</p>

On installation, we find we have new Program Menu entries, shown below. We can see that it has also
included a link to our home page, which is optional.

<p style="text-align:left;margin-top:2em;margin-bottom:2em;">
    <img src="Media/Screenie-StartMenu.png" style="width:40%;max-width:800px;"/>
</p>

Moreover, there is a "Command Prompt" option to launch a dedicated command window, as described above.


## Creating New PupNet Project Files ##

If you're starting a new project, you will wish to generate a new `pupnet.conf` file and possibly the associated
desktop and AppStream metadata files as well.

To generate a new `pupnet.conf` file in the current working directory:

    pupnet --new conf

This will create a new file, `app.pupnet.conf`, which will be a "light weight" configuration file lacking the
documentation comments.

To generate a "verbose" file with documentation under a custom name:

    pupnet ProjectName --new conf --verbose

In fact, we can generate a complete set of files, as follows:

    pupnet ProjectName --new all --verbose

This creates not only the `pupnet.conf` file, but the `.desktop` and a `.metainfo.xml` template as well.

## Help System Reference ##

### Command Arguments ###

Type `pupnet --help` to display command arguments as expected.

    USAGE:
        pupnet [file.conf] [--option-n value-n]

    Example:
        pupnet app.pupnet.conf -y -r linux-arm64

    If conf file is omitted, one in the working directory will be selected.

    Build Options:
        -k, --kind Zip|AppImage|Deb|Rpm|Flatpak|Setup
        Package output kind. If omitted, one is chosen according to the runtime.
        Example: pupnet HelloWorld -k Flatpak

        -r, --runtime value
        Dotnet publish runtime identifier. Default: linux-x64.
        Valid examples include: 'linux-x64', 'linux-arm64' and 'win-x64' etc.
        See: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog

        -c, --build value
        Optional build target (or 'Configuration' is dotnet terminology).
        Value should be 'Release' or 'Debug'. Default: Release.

        -e, --clean [flag only]
        Specifies whether to call 'dotnet clean' prior to 'dotnet publish'. Default: false.

        -v, --app-version value
        Specifies application version-release in form 'VERSION[RELEASE]', where value in square
        brackets is package release. Overrides AppVersionRelease in conf file.
        Example: 1.2.3[1].

        -p, --property value
        Specifies a property to be supplied to dotnet publish command. Do not use for
        app versioning. Example: -p DefineConstants=TRACE;DEBUG

        --arch value
        Force target architecture, i.e. as 'x86_64', 'amd64' or 'aarch64' etc. Note that this is
        not normally necessary as, in most cases, the architecture is defined by the dotnet runtime-id
        and will be successfully detected automatically. However, in the event of a problem, the value
        explicitly supplied here will be used to override. It should be provided in the form
        expected by the underlying package builder (i.e. rpmbuild, appimagetool or InnoSetup etc.).

        -o, --output value
        Force package output filename. Normally this is derived from parameters in the configuration.
        This value will be used to override. Example: -o AppName.AppImage

        --verbose [flag only]
        Indicates verbose output when building. It can also used with --new.

        -u, --run [flag only]
        Performs a test run of the application after successful build (where supported).

        -y, --skip-yes [flag only]
        Skips confirmation prompts (assumes yes).

    Other Options:

        -n, --new None|Conf|Desktop|Meta|All
        Creates a new empty conf file or associated file (i.e. desktop of metadata) for a new project.
        A base file name may optionally be given. If --verbose is used, a configuration file
        with full documentation comments is generated. Use All to generate a full set of
        configuration assets. Example: pupnet HelloWorld -n All --verbose

        -h, --help args|macros|conf
        Show help information. Optional value specifies what kind of information to display.
        Default is 'args'. Example: pupnet -h macros

        --version [flag only]
        Show version and associated information.


### Macro Reference ###

Type `pupnet --help macros` to see supported macro reference information:

    MACROS:
    Macro variables may be used with the following configuration items:
    DesktopFile, MetaFile, DotnetPublishArgs, DotnetPostPublish and DotnetPostPublishOnWindows.

    IMPORTANT: Always use the ${MACRO_NAME} form, and not $MACRO_NAME.

    APP_BASE_NAME
    AppBaseName value from conf file
    Example: ${APP_BASE_NAME} = HelloWorld

    APP_FRIENDLY_NAME
    AppFriendlyName value from conf file
    Example: ${APP_FRIENDLY_NAME} = Hello World

    APP_ID
    AppId value from conf file
    Example: ${APP_ID} = net.example.helloworld

    APP_LICENSE_ID
    AppLicenseId value from conf file
    Example: ${APP_LICENSE_ID} = LicenseRef-Proprietary

    APP_SHORT_SUMMARY
    AppShortSummary value from conf file
    Example: ${APP_SHORT_SUMMARY} = A HelloWorld application

    APP_VERSION
    Application version, exluding package-release extension
    Example: ${APP_VERSION} = 1.0.0

    BUILD_APP_BIN
    Application build directory (i.e. the output of dotnet publish or C++ make)
    Example: ${BUILD_APP_BIN} = /tmp/KuiperZone.PupNet/net.example.helloworld-linux-x64-Release-Rpm/AppDir/opt/net.example.helloworld

    BUILD_ARCH
    Build architecture: x64, arm64, arm or x86 (may differ from package output notation)
    Example: ${BUILD_ARCH} = x64

    BUILD_DATE
    Build date in 'yyyy-MM-dd' format
    Example: ${BUILD_DATE} = 2023-03-20

    BUILD_ROOT
    Root of the temporary application build directory
    Example: ${BUILD_ROOT} = /tmp/KuiperZone.PupNet/net.example.helloworld-linux-x64-Release-Rpm/AppDir

    BUILD_SHARE
    Linux 'usr/share' build directory under BuildRoot (empty for some deployments)
    Example: ${BUILD_SHARE} = /tmp/KuiperZone.PupNet/net.example.helloworld-linux-x64-Release-Rpm/AppDir/usr/share

    BUILD_TARGET
    Release or Debug (Release unless explicitly specified)
    Example: ${BUILD_TARGET} = Release

    BUILD_YEAR
    Build year as 'yyyy'
    Example: ${BUILD_YEAR} = 2023

    DEPLOY_KIND
    Deployment output kind: appimage, flatpak, rpm, deb, setup, zip
    Example: ${DEPLOY_KIND} = rpm

    DOTNET_RUNTIME
    Dotnet publish runtime identifier used (RID)
    Example: ${DOTNET_RUNTIME} = linux-x64

    INSTALL_BIN
    Path to application directory on target system (not the build system)
    Example: ${INSTALL_BIN} = /opt/net.example.helloworld

    INSTALL_EXEC
    Path to application executable on target system (not the build system)
    Example: ${INSTALL_EXEC} = /opt/net.example.helloworld/HelloWorld

    IS_TERMINAL_APP
    IsTerminalApp value from conf file
    Example: ${IS_TERMINAL_APP} = true

    PACKAGE_RELEASE
    Package release version
    Example: ${PACKAGE_RELEASE} = 1

    PRIME_CATEGORY
    PrimeCategory value from conf file
    Example: ${PRIME_CATEGORY} =

    PUBLISHER_COPYRIGHT
    PublisherCopyright value from conf file
    Example: ${PUBLISHER_COPYRIGHT} = Copyright (C) Your Name 1970

    PUBLISHER_EMAIL
    PublisherEmail value from conf file
    Example: ${PUBLISHER_EMAIL} = contact@example.net

    PUBLISHER_LINK_NAME
    PublisherLinkName value from conf file
    Example: ${PUBLISHER_LINK_NAME} = Home Page

    PUBLISHER_LINK_URL
    PublisherLinkUrl value from conf file
    Example: ${PUBLISHER_LINK_URL} = https://example.net

    PUBLISHER_NAME
    PublisherName value from conf file
    Example: ${PUBLISHER_NAME} = Your Name

### Configuration Reference ###

Type `pupnet --help conf` to see supported configuration reference information:

    ########################################
    # APP PREAMBLE
    ########################################

    # Mandatory application base name. This MUST BE the base name of the main executable
    # file. It should NOT include any directory part or extension, i.e. do not append
    # '.exe' or '.dll'. It should not contain spaces or invalid filename characters.
    # Example: HelloWorld
    AppBaseName = HelloWorld

    # Mandatory application friendly name. Example: Hello World
    AppFriendlyName = Hello World

    # Mandatory application ID in reverse DNS form. Example: net.example.helloworld
    AppId = net.example.helloworld

    # Mandatory application version and package release of form: 'VERSION[RELEASE]'. Use
    # optional square brackets to denote package release, i.e. '1.2.3[1]'. Release refers to
    # a change to the deployment package, rather the application. If release part is absent
    # (i.e. '1.2.3'), the release value defaults to '1'. Note that the version-release value
    # given here may be overridden from the command line.
    AppVersionRelease = 1.0.0[1]

    # Mandatory single line application description. Example: Yet another Hello World application.
    AppShortSummary = A HelloWorld application

    # Mandatory application license name. This should be one of the recognised SPDX license
    # identifiers, such as: 'MIT', 'GPL-3.0-or-later' or 'Apache-2.0'. For a proprietary or
    # custom license, use 'LicenseRef-Proprietary' or 'LicenseRef-LICENSE'.
    AppLicenseId = LicenseRef-Proprietary

    # Optional path to a copyright/license text file. If provided, it will be packaged with the
    # application and identified to package builder where supported. Example: Copyright.txt
    AppLicenseFile =

    ########################################
    # PUBLISHER
    ########################################

    # Mandatory publisher, group or creator. Example: Acme Ltd, or HelloWorld Team
    PublisherName = Your Name

    # Optional copyright statement. Example: Copyright (C) HelloWorld Team 1970
    PublisherCopyright = Copyright (C) Your Name 1970

    # Optional publisher or application web-link name. Note that Windows Setup packages
    # require both PublisherLinkName and PublisherLinkUrl in order to include the link as an item
    # in program menu entries. Default is: Home Page. Examples: Example.net, or: Hello World Online
    PublisherLinkName = Home Page

    # Optional publisher or application web-link URL. Example: https://example.net
    PublisherLinkUrl = https://example.net

    # publisher or maintainer email contact. Although optional, some packages (such as Debian)
    # require it and will fail unless provided. Example: <hello> helloworld@example.net
    PublisherEmail = contact@example.net

    ########################################
    # DESKTOP INTEGRATION
    ########################################

    # Optional command name to start the application from the terminal. If, for example,
    # AppBaseName is 'Zone.Kuiper.HelloWorld', the value here may be set to a simpler and/or
    # lower-case variant (i.e. 'helloworld'). It must not contain spaces or invalid filename characters.
    # Do not add any extension such as '.exe'. If empty, the application will not be in the path and cannot
    # be started from the command line. For Windows Setup packages, see also SetupCommandPrompt.
    # The StartCommand is not supported for all packages.
    # kinds. Default is empty (none).
    StartCommand =

    # Boolean (true or false) which indicates whether the application runs in the terminal, rather
    # than provides a GUI. It is used only to populate the 'Terminal' field of the .desktop file.
    IsTerminalApp = true

    # Optional path to a Linux desktop file. If empty (default), one will be generated automatically
    # from the information in this file. Supplying a custom file, however, allows for mime-types and
    # internationalisation. If supplied, the file MUST contain the line: 'Exec=${INSTALL_EXEC}'
    # in order to use the correct install location. Other macros may be used to help automate the content.
    # If no desktop entry is required, set the value to: 'NONE'. It has little effect for
    # Windows Setup, except that setting it to 'NONE' will cause the
    # application start menu entry to be omitted. Note. The contents of the files may use macro variables.
    # Use 'pupnet --help macro' for reference.
    # See: https://specifications.freedesktop.org/desktop-entry-spec/desktop-entry-spec-latest.html
    DesktopFile =

    # Optional category for the application. The value should be one of the recognised Freedesktop
    # top-level categories, such as: Audio, Development, Game, Office, Utility etc. Only a single value
    # should be provided here which will be used, where supported, to populate metadata. The default
    # is empty. See: https://specifications.freedesktop.org/menu-spec/latest/apa.html
    # Example: Utility
    PrimeCategory =

    # Optional icon file paths. The value may include multiple filenames separated with semicolon or
    # given in multi-line form. Valid types are SVG, PNG and ICO (ICO ignored on Linux). Note that the
    # inclusion of a scalable SVG is preferable on Linux, whereas PNGs must be one of the standard
    # sizes and MUST include the size in the filename in the form: name.32x32.png' or 'name.32.png'.
    # Example: Assets/app.svg;Assets/app.24x24.png;Assets/app.32x32.png;Assets/app.ico
    IconFiles =

    # Path to AppStream metadata file. It is optional, but recommended as it is used by software centers.
    # Note. The contents of the files may use macro variables. Use 'pupnet --help macro' for reference.
    # See: https://docs.appimage.org/packaging-guide/optional/appstream.html
    # Example: Deploy/app.metainfo.xml.
    MetaFile =

    ########################################
    # DOTNET PUBLISH
    ########################################

    # Optional path relative to this file in which to find the dotnet project (.csproj)
    # or solution (.sln) file, or the directory containing it. If empty (default), a single
    # project or solution file is expected under the same directory as this file.
    # IMPORTANT. If set to 'NONE', dotnet publish is disabled (not called).
    # Instead, only DotnetPostPublish is called. Example: Source/MyProject
    DotnetProjectPath =

    # Optional arguments supplied to 'dotnet publish'. Do NOT include '-r' (runtime), app version,
    # or '-c' (configuration) here as they will be added (i.e. via AppVersionRelease).
    # Typically you want as a minimum: '-p:Version=${APP_VERSION} --self-contained true'. Additional
    # useful arguments include: '-p:DebugType=None -p:DebugSymbols=false -p:PublishSingleFile=true
    # -p:PublishReadyToRun=true -p:PublishTrimmed=true -p:TrimMode=link'.
    # Note. This value may use macro variables. Use 'pupnet --help macro' for reference.
    # See: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
    DotnetPublishArgs = -p:Version=${APP_VERSION} --self-contained true -p:DebugType=None -p:DebugSymbols=false

    # Post-publish (or standalone build) commands on Linux (ignored on Windows). Multiple commands
    # may be specified, separated by semicolon or given in multi-line form. They are called after
    # dotnet publish, but before the final output is built. These could, for example, copy additional
    # files into the build directory given by ${BUILD_APP_BIN}. The working directory will be the
    # location of this file. This value is optional, but becomes mandatory if DotnetProjectPath equals 'NONE'.
    # Note. This value may use macro variables. Additionally, scripts may use these as environment variables.
    # Use 'pupnet --help macro' for reference.
    DotnetPostPublish =

    # Post-publish (or standalone build) commands on Windows (ignored on Linux). This should perform
    # the equivalent operations, as required, as DotnetPostPublish, but using DOS commands and batch scripts.
    # Multiple commands may be specified, separated by semicolon or given in multi-line form. Note. This value
    # may use macro variables. Additionally, scripts may use these as environment variables.
    # Use 'pupnet --help macro' for reference.
    DotnetPostPublishOnWindows =

    ########################################
    # PACKAGE OUTPUT
    ########################################

    # Optional package name (excludes version etc.). If empty, defaults to AppBaseName.
    # However, it is used not only to specify the base output filename, but to identify the application
    # in .deb and .rpm packages. You may wish, therefore, to ensure that the value represents a
    # unique name, such as the AppId. Naming requirements for this are strict and must
    # contain only alpha-numeric and '-', '+' and '.' characters. Example: HelloWorld
    PackageName = HelloWorld

    # Output directory, or subdirectory relative to this file. It will be created if it does not
    # exist and will contain the final deploy output files. If empty, it defaults to the location
    # of this file. Default: Deploy/bin
    OutputDirectory = Deploy/bin

    # Boolean (true or false) which sets whether to include the application version in the filename
    # of the package (i.e. 'HelloWorld-1.2.3-x86_64.AppImage'). It is ignored if the output
    # filename is specified at command line.
    OutputVersion = false

    ########################################
    # APPIMAGE OPTIONS
    ########################################

    # Additional arguments for use with appimagetool. Useful for signing. Default is empty.
    # See appimagetool --help. Example: --sign
    AppImageArgs =

    ########################################
    # FLATPAK OPTIONS
    ########################################

    # The runtime platform. Invariably for .NET (inc. Avalonia), this should be
    # 'org.freedesktop.Platform'.
    # Refer: https://docs.flatpak.org/en/latest/available-runtimes.html
    FlatpakPlatformRuntime = org.freedesktop.Platform

    # The platform SDK. Invariably for .NET (inc. Avalonia applications) this should
    # be 'org.freedesktop.Sdk'. The SDK must be installed on the build system.
    FlatpakPlatformSdk = org.freedesktop.Sdk

    # The platform runtime version. The latest available version may change periodically.
    # Refer to Flatpak documentation. Example: 22.08
    FlatpakPlatformVersion = 22.08

    # Flatpak manifest 'finish-args' sandbox permissions. Optional, but if empty, the
    # application will have extremely limited access to the host environment. This
    # option may be used to grant required application permissions. Values here should
    # be prefixed with '--' and separated by semicolon or given in multi-line form.
    # Example: --socket=wayland;--socket=x11;--filesystem=host;--share=network
    # Refer: https://docs.flatpak.org/en/latest/sandbox-permissions.html
    FlatpakFinishArgs = """
        --socket=wayland
        --socket=x11
        --filesystem=host
        --share=network
    """

    # Additional arguments for use with flatpak-builder. Useful for signing. Default is empty.
    # See flatpak-builder --help. Example: --gpg-keys=FILE
    FlatpakBuilderArgs =

    ########################################
    # WINDOWS SETUP OPTIONS
    ########################################

    # Optional command prompt title. The Windows installer will not add your application to the
    # path. However, if your package contains a command-line utility, setting this value will
    # ensure that a 'Command Prompt' menu entry is added which, when launched, will open a command
    # window with your application directory in its path. See also StartCommand.
    # Examples: Command Prompt, or: Command Prompt for Hello World
    SetupCommandPrompt =

    # Mandatory value which specifies minimum version of Windows that your software runs on.
    # Windows 8 = 6.2, Windows 10/11 = 10. Default: 10.
    # See 'MinVersion' parameter in: https://jrsoftware.org/ishelp/
    SetupMinWindowsVersion = 10

    # Optional name and parameters of the Sign Tool to be used to digitally sign: the installer,
    # uninstaller, and contained exe and dll files. If empty, files will not be signed.
    # See 'SignTool' parameter in: https://jrsoftware.org/ishelp/
    SetupSignTool =


## Gotchas ##

### Virtual Box and Symlinks ###
If you are using VirtualBox with your project, note that symbolic links are disabled within shared folders by VirtualBox
itself, and this may cause problems with generating AppImages. To overcome this, copy your entire project to your home
directory in the virtual machine. Alternatively, it is possible to enable shared-folder symlinks in VirtualBox.

### RPM and Debian Packages Cannot be Removed from Gnome Software Center GUI ###
If you install your RPM and DEB packages as a local file they will, courtesy of your AppStream metadata,
show up Gnome Software Center GUI, as would be expected. However, you may find that they cannot be launched
or removed using the GUI. Instead, they must be removed from the command line, like so:

    sudo dnf remove helloworld

This is not an issue with PupNet or AppStream metadata. Rather, having been installed from file, the Gnome Software
Center lacks certain other metadata it would get if the package had originated from a repository.
See [here](https://discourse.gnome.org/t/gnome-software-open-and-uninstall-button-not-working-for-app/14338/7).


## Additional Information ##

### A Brief Discussion on the Past and Future ###
PupNet Deploy began life as a bash script called "*Publish-AppImage for .NET*":

<p style="text-align:left;">
    <a href="https://github.com/kuiperzone/Publish-AppImage">
        <img title="Publish-AppImage" alt="Publish-AppImage" src="Media/Publish-AppImage.png" style="width:40%;max-width:250px;"/>
    </a>
</p>

I am a fan of [AppImage](https://github.com/AppImage/AppImageKit), but at the time and as a Fedora user, I was also
excited by Flatpak, and it was my original intention to add Flatpak as an output option to *Publish-AppImage*. However,
it wa difficult to handle the increased complexity in a bash script, so I re-wrote everything as a C# application and
*PupNet Deploy* is the result.

In the process, I had cause to reflect on certain things, including the notion that the
[sandbox model of Flatpak is arguably broken](https://ludocode.com/blog/flatpak-is-not-the-future).
Regardless, I still thought it useful for developers to be able ship software in formats convenient for users, so I
added RPM and Deb package output also, as well as traditional Windows Setup.

However, I would not be keen on adding more such formats in the Linux space. Rather, I would be interested to see
how things play out in the future. I note the trend toward centralised repository-only distribution. I support the
idea of freedom and that means that developers and users maintaining the freedom to create and share software
without being subject to centralisation.

### How to Extend PupNet ###
It may be advantageous at some point to have the ability to deploy to MacOS, Android and iOS. However, these are not
technologies with which I am familiar.

Anyone wishing to extend PuP, should study how the existing RPM, Flatpak and Windows `Builder` classes override and extend
the behaviour of the `PackageBuilder` base class. The base class is Linux centric, with irrelevant directories and
properties being ignored or set to null for the Windows Setup and Zip builders.

Moreover, a new enum value would need to be added to the `PackageKind` type. It would help to do a search on
existing `PackageKind` values, and add the new type behaviour to switch statements wherever encountered, including
the `BuilderFactory` class.

### Another Applications of Mine ###

[AvantGarde](https://github.com/kuiperzone/AvantGarde) is a cross-platform XAML previewer for the C# Avalonia Framework.
It was the first Avalonia preview solution for Linux.

<p style="text-align:left;">
    <a href="https://github.com/kuiperzone/AvantGarde">
        <img title="AvantGarde Screenshot" alt="AvantGarde Screenshot" src="Media/Screenie-AvantGarde.png" style="width:50%;max-width:500px;"/>
    </a>
</p>


### Copyright & License ###

Copyright (C) Andy Thomas, 2023. Website: https://kuiper.zone

PupNet is free software: you can redistribute it and/or modify it under
the terms of the GNU Affero General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later version.

PupNet is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along
with PupNet. If not, see <https://www.gnu.org/licenses/>.

#### Embedded Assets ####

PupNet embeds "appimagetool", from the "AppImageKit".
AppImageKit is Copyright (C) 2004-20 Simon Peter
https://github.com/AppImage/AppImageKit

### Non-code Assets ####
Images and non-code assets are not subject to AGPL.

Project Logo: Copyright (C) Andy Thomas, 2023.

All other copyright and trademarks are property of respective owners.

If you like this project, don't forget to like and share.
