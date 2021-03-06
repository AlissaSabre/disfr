disfr
=====
A Windows program to view/examine XLIFF and other bilingual contents.

## Summary

[XLIFF](http://docs.oasis-open.org/xliff/xliff-core/xliff-core.html) is an OASIS standard XML file format for bilingual documents.
It is used in most (if not all) of modern [CAT](https://en.wikipedia.org/wiki/Computer-assisted_translation) environments.

disfr is a handy tool to show the contents of XLIFF and similar/related bilingual files.
It has some features to further examine the files,
e.g., filtering and sorting segments, inspecting tags, 
or showing non-translatable portion of the source document.
Moreover, it may allow you comparing differences between versions in the future.
It has no way to edit/change the contents, however.

![screenshot](Documents/screenshot.jpg)

disfr runs only on Windows OS.  (It is a .NET Framework program written in C# using WPF, 
and, no, it does not strictly adhere the MVVM doctrine...  In fact, it heavily uses _code behinds_.  This note is for programmers.) 

For the moment, disfr is under development.
You can consider it is in a sort of an alpha stage.
Ready-to-run Windows Installer (*.msi) files are available on the [Releases](https://github.com/AlissaSabre/disfr/releases) page.
If you are a programmer, you should also be able to build the source files on HEAD of master branch always.

## File formats

It supports reading of the following file formats, which are more or less based on XLIFF standard:
* Standard XLIFF 1.2 files (*.xlf, *.xliff.)
* Zipped XLIFF 1.1 and 1.2 files.
* SDL Idiom WorldServer translation kit and return packages (*.xlz, *.wsxz.)
* SDL Trados Studio files (*.sdlxliff.)
* memoQ bilingual files (*.mqxlz, *.mqxliff.)
* Wordfast (WFA/WFP5) files (*.txlf.)
* Any other XLIFF 1.2 based bilingual files (by ignoring some unknown portions of contents.)

It can also read the following related file formats, which are not based on XLIFF:
* TMX translation memory exchange files (*.tmx.)
* SDL Trados Studio Translation Memories (*.sdltm.)
* Common bilingual glossaries on Microsoft Excel files (*.xls, *.xlsx)
* gettext po files (*.po) **NEW**

It may be capable of reading the following file formats in the future:
* Standard XLIFF 2.0 and packages containing it.
* .string files for iOS Apps.
* Common glossaries on so-called CSV files.

It can also _write_ portions of contents out to the following file formats (without editing):
* TMX files.
* Microsoft Excel (.xlsx) files.
* A simple bilingual table in HTML5 (similar to _external review_ files produced by CAT tools).

It may be capable of _writing_ the contents out to the following file formats in the future:
* So-called CSV.
* Office OpenXML WordProcessing document (aka .docx) as a so-called bilingual table.

## Other features

Major features of disfr (other than supported file formats) are:
* **Lightweight**: disfr usually starts up and reads a file faster than ordinary CAT tools.
* **Simple table view**: disfr shows everything in a simple two-dimensional table format with no frills.
* **Detailed metadata**: XLIFF or other bilingual files contain rich segment metadata, though large parts of them are consumed by CAT tools themselves and not exposed to users.  disfr presents a lot more of those metadata to users for handy inspection.
* **Filter as you type**: disfr supports filtering of segments by its unique _filter as you type_ UI.
* **Extensibility**: disfr has a simple _plugin_ mechanism so that you can add support for more file formats easily.  In particular, supports for sdltm and Excel files are implemented as respective plugins (though we have no API/SPI document for plugin authors yet).

## Legalese

### Credits and licenses

disfr is written by Alissa Sabre.  It is distributed under a _BSD License_.

disfr uses and its binary distribution includes [Dragablz – Dragable and Tearable TabControl for WPF](https://dragablz.net/) by ButchersBoy (James Willock) distributed under the MIT License.

disfr uses and its binary distribution includes [WpfColorFontDialog](https://www.nuget.org/packages/WpfColorFontDialog/) by Sverre Kristoffer Skodje distributed under the MIT License, based on an article [A WPF Font Picker (with Color)](https://www.codeproject.com/Articles/368070/A-WPF-Font-Picker-with-Color) by Alessio Saltarin posted on CodeProject and published under COPL.

disfr uses and its binary distribution includes Windows API Code Pack for Microsoft .NET Framework by Microsoft Corporation distributed under [its own license](https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE) packaged by aybe.

disfr uses and its binary distribution includes portions of [NetOffice](https://osdn.net/projects/netoffice/) by Sebastian Lange distributed under the MIT license.

disfr uses and its binary distribution includes [SQLite](https://www.sqlite.org) and [System.Data.SQLite](https://system.data.sqlite.org) by SQLite Development team.

disfr uses and its binary distribution includes portions of [YaccLexTools](https://github.com/ernstc/YaccLexTools) by Ernesto Cianciotta distributed under the MIT License, that includes Gardens Point LEX and Gardens Point Parser Generator tools by Queensland University of Technology distributed under a version of 2-clause BSD license ([this](https://raw.githubusercontent.com/ernstc/YaccLexTools/master/src/NuGet/YaccLexTools/tools/GPLEXcopyright.rtf) and [this](https://raw.githubusercontent.com/ernstc/YaccLexTools/master/src/NuGet/YaccLexTools/tools/GPPGcopyright.rtf)).

### Trademarks

Each of names and marks may be a trademark, a service mark, or a similar intellectual property of its owner under certain jurisdictions.
