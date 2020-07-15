Installer
=========

This folder contains files that is used to create an installer (disfr-X.X.X.X-setup.msi).

(This file "Readme.md" is a part of the development files.
End-user readme, which is packaged into the .msi file, is "Readme.htm".)

## Background

The installer is built using [the WiX Toolset](http://wixtoolset.org).

WiX toolset has its own Visual Studio Extension (VSIX) to integrate itself into Visual Studio environment.
However, I'm using Visual Studio Express edition, which doesn't allow use of VSIX.
So, I need to write necessary files by hands, deviating from common practices to use WiX.

In earlier versions of disfr, the installer building process was driven by an MS-DOS batch file.

In recent versions, the installer is a part of the visual studio solution as Insatller project.
It is expressed by `Installer.wixproj` project file, but it pretends a C# project but WiX project.
As a side-effect, when you opens the disfr solution with Visual Studio,
you will see a warning message "Load of property 'OutputType' failed."
It is normal (under the trick I employed.)
Please just ignroe it.

## CustomActions.dll

The installer requires a DLL custom action to do some small tasks upon installation.
I wrote `CustomActions.dll` for the purpose.
It is written in C++.

CustomActions is built as a part of the solution by its own project,
but only if you are making a Release build.

`CustomActions.dll` is a part of the installer,
and disfr doesn't use it in its runtime.

## How to build the installer

You can run WiX and build the installer by simply make the Release build of the solution
by Visual Studio or MSBuild command line tool.

Unlike older batch file based build, the dependency between other projects are managed by VS/MSB,
so if some of the binaries are out of date, they are rebuilt automatically.

The resulting msi file will be placed in the folder
`Installer\bin\Release` as usual under the name `disfr.msi` first
and renamed to `disfr-X.X.X-setup.msi` later.
