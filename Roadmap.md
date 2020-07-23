disfr roadmap
=============
The source file/folder organization of disfr repository

## Solution and projects

disfr source files comprise a single VS solution.
The solution contains five C# projects to produce product binary files, 
two projects to form the installer, 
several test projects,
and some _solution folders_ holding documentation files.

Each project resides in its own folder.

## Product projects

### disfr project

This project produces `disfr.exe`, the main application.

The actual contents of disfr project consist mostly of UI.

### disfr-core project

This project produces `disfr-core.dll`,
a class library for read/write various bilingual files.
It also contains a primitive plug-in mechanic.

`disfr-core.dll` includes classes in several namespaces,
with namespaces divided into subfolders.

* Doc: Basic data types for disfr documents and readers of various file types.
* Writer: Writers, i.e., classes to write bilingual documents to external files.
* Plugin: A primitive plugin mechanic.
* Diff: A class library that provides basic functionarity similar to `diff` command.

Reading of XLIFF and TMX files are implemented in this library and require no plugins to run.

### disfr.sdltm project

This project produces `disfr.sdltm.dll`,
a disfr plugin to read sdltm files.

It is a part of standard disfr install,
but it is isolated to a separte plugin so that it can be detached from disfr-core.dll easily.

### disfr.ExcelGlossary project

This project produces `disfr.ExcelGlossary.dll`,
a disfr plugin to read common glossary files on xls or xlsx (Microsoft Excel) files.
The plugin requires Microsoft Excel (version Excel 2010 or later) to be installed on the PC to run.

It is a part of standard disfr install,
but it is isolated to a separate plugin so that it can be detached from disfr-core.dll easily.

### disfr.xlsx-writer project

This project produces `disfr.xlsx-writer.dll`,
a disfr plugin to _write_ contents of a bilingual file into an xlsx file
(i.e., Microsoft Excel file).
The plugin requires Microsoft Excel (version Excel 2010 or later) to be installed on the PC to run.

It is a part of standard disfr install,
but it is isolated to a separate plugin so that it can be detached from disfr-core.dll easily.

### About post-build copy

`disfr-core` and plagin projects copy their product files
to the binary directory of `disfr` project (i.e., `disfr\bin\$(Configuration)`)  after build.
It is to facilitate debugging and to help wix finding those files.
The copying takes place as a post-build event using `xcopy` command.

## Installer projects

### Installer

This project pretends a C# project to Visual Studio solution,
but it is actually a WiX project.
Its project file (`Installer.wixproj`) has a file name extension `wixproj`.
I used the trick because I'm using Visual Studio Express editions to develop disfr,
and VS Express doesn't allow use of WiX Visual Studio Extension.

See [a separate file](Installer/Readme.md) for details.

You need [WiX Toolset](https://wixtoolset.org/) 3.11 or later to build the disfr installer (msi file).

### CustomActions

This is a C++ project to produce a small DLL, that defines a set of _custom actions_ for Windows Installer.
It is a part of the installer.
disfr itself doesn't use the `CustomActions.dll` to run.