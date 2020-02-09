disfr roadmap
=============
The source file/folder organization of disfr

## Solution and projects

disfr source files comprise a single VS solution.
The solution contains five product projects and several test projects.

Each project resides in its own folder.

## disfr project

This project produces disfr.exe, the main application.

The actual contents of disfr project consist mostly of UI.

## disfr-core project

This project produces disfr-core.dll,
a class library for read/write various bilingual files.
It also contains a primitive plug-in mechanic.

disfr-core.dll includes classes for several packages, divided into subfolders.

* Doc: Basic data types for disfr documents and readers of various file types.
* Writer: Writers, i.e., classes to write bilingual documents to external files.
* Plugin: A primitive plugin mechanic.
* Diff: A class library that provides basic functionarity of diff command.

## disfr.sdltm project

This project produces disfr.sdltm.dll,
a disfr plugin to read sdltm files.
It is a part of standard disfr install,
but it is isolated to a separte plugin so that it can be detached from disfr-core.dll easily.

## disfr.xlsx-writer project

This project produces disfr.xlsx-writer.dll,
a disfr plugin to write contents of a bilingual file into an xlsx file
(i.e., Microsoft Excel file).
The plugin requires Microsoft Excel (version Excel 2010 or later) to be installed on the PC to run.
It is a part of standard disfr install,
but it is isolated to a separate plugin so that it can be detached from disfr-core.dll easily.

## post-build copy

disfr-core and plagin projects copy their produced files to the binary directory of disfr.exe.
It is to facilitate debugging and to help wix finding those files.
The copying takes place as a post-build action.
