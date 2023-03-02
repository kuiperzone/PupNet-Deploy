<p style="text-align:left;margin-bottom:4em;">
    <img src="Images/PUBPAK.png" style="width:50%;max-width:650px;"/>
</p>

# Publish AppImage & Flatpak for .NET #

## Introduction ##

**PUBPAK for .NET** is a bash script deployment utility which calls `dotnet publish` and packages the output as an
[AppImage](https://appimage.org/) file, a [Flatpak](https://docs.flatpak.org/en/latest/introduction.html), or simple zip file.

To use it, fill out the option fields in a simple configuration file called `pubpak.conf` and, from within your
project, run:

    ./pubpak

This calls `dotnet publish` on your .NET project and generates, by default, a distributable AppImage output file for
use on Linux systems.

To generate a Flatpak [single file bundle](https://docs.flatpak.org/en/latest/single-file-bundles.html), you would type:

    ./pubpak -k flatpak

In both cases, a Linux "Desktop Entry" file is generated automagically from the values provided by you in a configuration
file. Additionally, Freedesktop.org AppStream metadata (`appdata.xml`) should be provided, but is not mandatory.

A demo "Hello World" application and boiler-plate configurations are contained within this project source, and are
described below. It will be instructive to build and deploy this application.

PUBPAK for .NET is licensed under the GNU General Public License v3.0 and is for use on Linux with the
[Microsoft .NET SDK](https://dotnet.microsoft.com/download).


======================================================

DISCUSSION
See the following before use:

[Is Flatpak Sandboxing Flawed? Should PUBPAK Support it?](https://github.com/kuiperzone/Pubpak/discussions/1)

======================================================

## Pre-requisits ##

The PUBPAK utility is a bash script which simplifies the package creation process by assembling numerous commands and
build files. However, it calls on other utilities to generate the final packages.

Namely, it requires:

* Bash shell
* .NET SDK 5.0 or later
* The [appimagetool](https://github.com/AppImage/AppImageKit) utility to build the AppImage file (1)
* The [flatpak-builder](https://docs.flatpak.org/en/latest/building-introduction.html#flatpak-builder) utility (2)
* The Linux "zip" utility is optional and typically may already be installed on your system

For AppImage output, you must first download [appimagetool](https://github.com/AppImage/AppImageKit). This itself
ships as an AppImage file. Save this to a directory on your system. You may optionally add this directory
to your `$PATH` (more below).

For Flatpak output, install the flatpak-builder tool:

    sudo dnf install flatpak flatpak-builder

or:

    sudo apt-get update && sudo apt-get install flatpak flatpak-builder

Additionally, you should install the [org.freedesktop.Platform and Sdk](https://docs.flatpak.org/en/latest/available-runtimes.html).
At the time of writing, the latest version was 22.08:

    flatpak install flathub org.freedesktop.Platform//22.08 org.freedesktop.Sdk//22.08

Notes:
1. Tested against appimagetool r13 (31 Dec 2020).
2. Tested against flatpak-builder 1.2.3


## Build HelloWorld Demo ##

A simple "HelloWorld" terminal application is provided along with a boiler-plate configuration (.conf),
`appdata.xml`, and dummy icon file. These files may be used as templates in your own projects.

Now, clone or download the PUBPAK for .Net project. Ensure that you have downloaded and installed one or both
of the `appimagetool` and `flatpak-builder` utilities described above. The `HelloWorld.csproj` will build
for .NET5 or above. If you need to change the `TargetFramework`, go ahead and change it.

The `pubpak` file itself is just a bash script so there is no need to "build" it, but do ensure that it has the
executable flag set. Note the `pubpak.conf` file which contains the deployment configuration.


### Build an AppImage ###

If you have NOT put the location of `appimagetool` in your `$PATH`, it will be necessary to edit `pubpak.conf` in order
to specify its location explicitly.

In `pubpak.conf`, change this line:

    APPIMAGETOOL_COMMAND="appimagetool"

to this as appropriate:

    APPIMAGETOOL_COMMAND="/home/user/Apps/appimagetool-x86_64.AppImage"

Now, from the top-level project (the same directory as the .conf file), simply type:

    ./pubpak -k appimage

By default, the `pubpak` will always look for a configuration file of name `pubpak.conf` in the current working directory.
However, it is possible to supply the .conf filename explicitly as:

    ./pubpak -f other.conf -k appimage

As shown in the screenshot below, a number of "build items" are assembled from the configuration values in the
`pubpak.conf` file, including a `dotnet publish` command to build the project source code, and a Desktop Entry file.

<p style="text-align:left;">
    <img title="Terminal Screenshot" alt="Terminal Screenshot" src="Images/Terminal.png" style="width:50%;max-width:500px;"/>
</p>

Note that the application has a version, which has actually been defined in the `pubpak.conf` file using
the `APP_VERSION` option. You may note that this version is being supplied to the `dotnet publish` command that will
build the source (more on this below).

Additionally, an AppStream metadata file has been provided in this example. This metadata is used by software centers
and, while not mandatory, should be provided (it is more important for Flatpak than AppImages). Open the
`Assets/appdata.xml` file and you will see that variables may be used for certain key application fields,
including `${APP_ID}` and `${APP_VERSION}`. These will be substituted during the build by values from `pubpak.conf`.

Finally, PUBPAK will ask for confirmation before calling on `appimagetool` to build the final output as:

    Deploy/HelloWorld-x86_64.AppImage

This file will, in theory, run on almost any Linux box any where. If the architecture is not specified, the output
will target the host machine (we have assumed x86_64 here).

Run `Deploy/HelloWorld-x86_64.AppImage` (or an aarch64 variant) from a terminal. The demo HelloWorld program will
merely output its version, command arguments and some path information (that's all it does).

Now, if you specify the .NET runtime identifier (architecture) explictly as follows:

    ./pubpak -k appimage -r linux-arm64

The output package will be built for ARM64 as: `Deploy/HelloWorld-aarch64.AppImage`

Note that it is possible to override the output filename and provide an explicit value.

### Build a Flatpak ###

To build a Flatpak "single file bundle", use:

    ./pubpak -k flatpak

The defalt output will be: `Deploy/HelloWorld-x86_64.flatpak`

In this case, the build items are assembled as before, but they now include a
[Flatpak manifest](https://docs.flatpak.org/en/latest/manifests.html). Again, this is assembled automagically
from options provided in `pubpak.conf`.  The generation of a Flatpak deployment file is a little more involved
than AppImage, and you may also note that a temporary "Flatpak repo" is created under the `Deploy/temp` directory.

Critically, you should note that the Flatpack manifest declares the
[sandbox permissions](https://docs.flatpak.org/en/latest/sandbox-permissions-reference.html) required by the application.
You may note that these are specified in `pubpak.conf` using the `FLATPAK_FINISH_ARGS` option, where the default value
is relatively permissive.

The Flatpak output generated by PUBPAK is what is called a
"[single file bundle](https://docs.flatpak.org/en/latest/single-file-bundles.html)", which you may install to your
system using:

    sudo flatpak install Deploy/HelloWorld-x86_64.flatpak

Shown below is the "HelloWorld" application installed into the Gnome Software Center.

<p style="text-align:left;">
    <img title="HelloWorld in Software Center" alt="HelloWorld in Software Center" src="Images/SoftwareCenter.png" style="width:40%;max-width:500px;"/>
</p>

For more information about Flatpaks,
see [https://docs.flatpak.org/en/latest/first-build.html](https://docs.flatpak.org/en/latest/first-build.html).

### Build a Zip File ###

Amazingly, it is possible with .NET to build and publish a Windows application from a Linux development machine.
In this case, a zip file may be built as a simple but convenient package.

Type:

    /pubpak -k zip -r win-x64

Here, the ouput will be: `Deploy/HelloWorld-win-x64.zip`

Note that the Desktop Entry, appdata.xml and manifest files are redundant for zip outputs, which merely package up the output
of `dotnet publish` in a trivial manner.

## Use PUBPAK in Your Project ##

As a minimum, there are only two files you really need. Drop the files, below, into your application source preferably
at the same level as your solution (.sln) or project (.csproj) file.

* `pubpak` - the utility (bash script)
* `pubpak.conf` - the project config

Note that if you do not wish to put `pubpak.conf` in the same directory as your .sln or .csproj file, you may specify
the location of your project with the `DOTNET_PROJECT_PATH` parameter from within the .conf file.

You may also, if you wish, put a single copy of the `pubpak` script in any directory on your system and add its
directory to your `$PATH`. This way, only the `pubpak.conf` file need go into your project.

Now edit the configuration file for your own application, providing an application name etc. This should be a
relatively trivial matter as **all parameters are fully documented** with comments. You can specify application
"Desktop Entry" fields here, as well as publish/build arguments, along with project and output locations.

<p style="text-align:left;">
    <img title="Example Configuration" alt="Example Configuration" src="Images/ExampleConf.png" style="width:50%;max-width:500px;"/>
</p>

Note that all project related paths in the .conf file are relative to the location of the .conf file, and not from
where command was called.

Invariably, you will wish to use an AppStream metadata file also. If you do not use a metadata file, your application
will not appear in the Gnome Software Center and cannot be included in catelogues. Copy, therefore, the `Assets/appdata.xml`
file into a suitable location. You can use this file as a boiler-plate, or generate a new file yourself. Refer to:
[https://docs.appimage.org/packaging-guide/optional/appstream.html](https://docs.appimage.org/packaging-guide/optional/appstream.html).

Ensure that your `pubpak.conf` file correctly references the AppStream metadata location using `APP_METADATA`. If you
do not wish to include AppStream metadata, ensure that `APP_METADATA` is empty.

Ensure also that your `pubpak.conf` references an icon file location using `APP_ICON_SRC`, as this is a mandatory
requirement.

**IMPORTNT**: By default, `pubpak` will always look for a file called `pubpak.conf` in the current working directly.
However, it is entirely possible to have multiple .conf files of different names in the same
directory. You are not restricted to a single conf file in your project. However, you will need to specify the
.conf filename when calling `pubpak` using the "-f" command option.


## App Versioning ##

There are 3 approaches to specifying the version of your application:

1. You may define the version in your .csproj file, as normal. In this case, the `APP_VERSION` in the `pubpak.conf`
should be left blank and will not be used to form the `dotnet publish` command when building your application. However,
if you are using AppStream metadata, it may still be necessary to declare (repeat) the version string here.

2. Use `APP_VERSION` field in `pubpak.conf`, and this will be supplied to the `dotnet publish` command when building
your application. Additionally, the `${APP_VERSION}` variable may used in the AppStream metadata file so that it
is propagated here as well.

2. Supply the version at the command line using the `-x, --app-version` option. This will override any
`APP_VERSION` field in `pubpak.conf`.

## AppStream Metadata Variables ##

Note that the `appdata.xml` supplied makes use of variables for certain fields.

I.e:

    <id>${APP_ID}</id>
    ...
    <release version="${APP_VERSION}" date="${ISO_DATE}">

All parameters in `pubpak.conf` may be used as variables in `appdata.xml`, and will be substituted during the
build. Additionally, `$ISO_DATE`, i.e. date of build ("2021-10-29"), may also be used.

For AppStream metadata reference, see:
[https://www.freedesktop.org/software/appstream/docs/chap-Quickstart.html#sect-Quickstart-DesktopApps](https://www.freedesktop.org/software/appstream/docs/chap-Quickstart.html#sect-Quickstart-DesktopApps).


## Post Publish Command ##

The configuration file contains an option called `POST_PUBLISH`. This may contain one or more commands, or point to a
script file. It is called after `dotnet publish`, but before the final AppImage or Flatpak output. You
can use this to create any required additional directory structures under `AppDir` or copy additional files there.

In principle, you may also use `POST_PUBLISH` to construct *non-.NET projects* deployments, but see below for this.

All variables in `pubpak.conf` will be exported prior to the `POST_PUBLISH` operation may be used in
shell commands. Additionally, the following variables are exported also:

* $ISO_DATE : date of build, i.e. "2021-10-29",
* $ARCH : architecture name (i.e. "aarch64"), if provided at command line
* $DOTNET_RID : dotnet runtime identifier string (i.e. "linux-x64), if provided at command line
* $PKG_KIND : package kind (i.e. "appimage", "flatpak", "zip")
* $APPDIR_ROOT : Package build directory root (i.e. "Deploy/temp/AppDir")
* $APPDIR_USR : Package user directory under root (i.e. "Deploy/temp/AppDir/usr")
* $APPDIR_BIN : Package bin directory under root (i.e. "Deploy/temp/AppDir/usr/bin")
* $APPRUN_TARGET : The expected target executable file (i.e. "Deploy/temp/AppDir/usr/bin/app-name")


## Signing Packages ##

Output packages may be signed using GPG with the options: `APPIMAGETOOL_ARGS` and `FLATPAK_BUILDER_ARGS`,
which allow signing flags to be supplied to the respective builder tools. Refer to their documentation.


## Command Line Usage ##

### Target Platform ###
By default, `pubpak` will build for the development machine. However, you can specify the dotnet "runtime identifier" as:

    ./pubpak -r linux-arm64

For information and a full list of valid identifiers, see:
[https://docs.microsoft.com/en-us/dotnet/core/rid-catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

**IMPORTANT:** Both the `appimagetool` and `flatpak-builder` applications document that they automatically detect the
architecture of the supplied binaries. In the event of an issue with this, you may specify the architecture explicitly
at the command line:

    ./pubpak -r linux-arm64 --arch aarch64

In this case, the value provided is supplied directly to `appimagetool` and `flatpak-builder` unchanged. Refer to the
respective documentation for permitted values.

### All Options ###
    Usage:
        pubpak [-flags] [-option-n value-n]

    Help Options:
        -h, --help
        Show help information flag.

        -v, --version
        Show version and about information flag.

    Build Options:
        -f, --conf value
        Specifies the conf file. Defaults to pubpak.conf.

        -k, --kind value
        Package output kind. Value must be one of: 'appimage', 'flatpak' or 'zip'.
        Default is 'appimage' if unspecified.

        -r, --runtime value
        Dotnet publish runtime identifier. Valid examples include: 'linux-x64' and 'linux-arm64'
        Default is empty and runtime is detected automatically.
        See also: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog

        -x, --app-version value
        Specifies application version. Overrides APP_VERSION in conf file.

        -p, --property value
        Specifies a property to be supplied to dotnet publish command. This option may appear
        multiple times in the command. Do not use for application version. Do not include ';' character.
        Example: pubpak -p DefineConstants=TRACE -p Configuration=Mac

        -a, --arch value
        Target architecture, i.e. as 'x86_64' or 'aarch64'. Note this is not normally necessary
        as, in most cases, both AppImage and Flatpak builders can detect the arch automatically.
        However, in the event of a problem with this, the value may be specified explicitly.

        -o, --output value
        Package output filename (excluding directory part). By default, the output name is derived
        from the application name and architecture. However, it may be overridden with this option.

        -b, --verbose
        Flag which indicates verbose output.

        -u, --run
        Flag which performs a test run of the application after successful build.

        -y, --skip-yes
        Flag which skips confirmation prompts (assumes yes).


## Additional Information ##

### Using PUBPAK for Non-.NET Projects ###

In principle, it is possible to use PUBPAK for .NET to build *non-.NET projects* (i.e. C++), although it was not
evisaged that this will be a primary use case.

To do this:

1. You must use a suitable build script and specify the file location using the `POST_PUBLISH` config parameter.

2. Your build script must populate the directory given by the `${APPDIR_BIN}` variable, and must contain a runnable
binary matching the name given by `${APP_MAIN}`.

3. You should also set `DOTNET_PROJECT_PATH="null"` in the configuration in order to disable
the `dotnet publish` operation.

### Gotchas ###

#### Flatpak Sandboxing with .NET Apps ####
Without explicit declaration of required permissions, Flatpaks applications have extremely limited access to the
host system. Moreover, it seems that .NET calls may effectively fail silently, with files written to alternate
locations rather than destinations specified.

Ensure, therefore, that the `FLATPAK_FINISH_ARGS` is correctly populated.

#### Symlinks in VirtualBox ####
If you are using VirtualBox with your project within a shared folder, note that symbolic links are disabled within
shared folders by VirtualBox, and this will prevent `appimagetool` from working. To overcome this, copy
your entire project to your home directory in the virtual machine. Alternatively, it is possible to enable shared-folder
symlinks in VirtualBox.

#### Metadata Validation ####
The `appimagetool` validates the application metadata (`appdata.xml`) prior to generating the AppImage output.
However, validation appears to be somewhat pedantic and may cause problems.

If you encouter these, see the following for clues and leads in problem solving:
https://github.com/AppImage/AppImageKit/issues/603

For AppImages, you may also leave the `APP_METADATA` configuration option empty to omit AppStream metadata.
This is not recommended for Flatpaks, however, as the application may not appear in the Gnome Software Center.


### Legacy Publish-AppImage ###

PUBPAK began life as "*Publish-AppImage for .NET*". It was renamed when support for Flatpaks was added.

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
