Installer
=========

This folder contains files that is used to create an installer (disfr.msi).

(This file "Readme.md" is a part of the development files.
End-user readme, which is packaged into the .msi file, is "Readme.htm".)

## Background

The installer is written using [the WiX Toolset](http://wixtoolset.org).

I can't use WiX toolset in Visual Studio environment,
because I'm using free-of-charge Visual Studio Express 2015 for C# development,
and Microsoft doesn't allow its users to use any third party add-in with the Express edition.

That's why this folder and MS-DOS batch files are.

## CustomAction.dll

The installer requires a DLL custom action to do some small tasks.
I wrote CustomAction.dll for the purpose.
It is written in C++.

## How to use it

You need to run build.bat from a command prompt to produce the msi file for disfr.

Because there is no dependency management,
you need to ensure by yourself to build all Visual Studio projects under Release configuration before running the build.bat.
When you have built disfr.exe and CustomAction.dll, run build.bat.
You will get the disfr.msi.
Also be sure to rename the file into a name something like disfr-X.X.X-setup.msi,
before testing it.





