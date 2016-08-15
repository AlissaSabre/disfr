disfr
=====
A Windows program to view/examine XLIFF file contents.

## Summary

[XLIFF](http://docs.oasis-open.org/xliff/xliff-core/xliff-core.html) is an OASIS standard XML file format for bilingual documents.
It is used in most (if not all) of modern [CAT](https://en.wikipedia.org/wiki/Computer-assisted_translation) environments.

disfr is a handy tool to show the contents of XLIFF and similar related files.
It has some features to further examine the files,
e.g., filtering and sorting segments, inspecting tags, or comparing differences between versions.
It has no way to edit/change the contents, however.

disfr is a Windows program written in C# and using WPF.
(And, no, it does not strictly adhere the MVVM doctrine...  In fact, it heavily uses _code behinds_.) 

For the moment, disfr is under development.
You can consider it is in a sort of an alpha stage.
Ready-to-run Windows Installer (*.msi) files are available on the [Releases](https://github.com/AlissaSabre/disfr/releases) page.
If you are a programmer, you should also be able to build the source files on HEAD of master branch always.

## File formats

It reads the following file formats, which are more or less based on XLIFF standard:
* Standard XLIFF 1.1 and 1.2 files (*.xlf, *.xliff.)
* Zipped XLIFF 1.1 and 1.2 files.
* SDL Idiom WorldServer translation kit and return packages (*.xlz, *.wsxz.)
* SDL Trados Studio files (*.sdlxliff.)
* Kilgray memoQ bilingual files (*.mqxlz, *.mqxliff.)
* Any other XLIFF based bilingual files (by ignoring some unknown portions of contents.)

It also reads the following file formats, which are not based on XLIFF:
* TMX translation memory exchange files (*.tmx; zipped TMX is in progress.)
* SDL Trados Studio Translation Memories (*.sdltm.)
* so-called CSV (tab-separated UTF-8 only; in progress.)

It may be capable of reading the following file formats in the future:
* Standard XLIFF 2.0 and packages containing it.
* .po files for GNU gettext.
* .string files for iOS Apps.

It can also _write_ the portions of contents out to the following file formats:
* TMX file.
* Microsoft Excel (.xlsx) file.

It may be capable of _writing_ the contents out to the following file formats in the future:
* so-called CSV.
* Office OpenXML WordProcessing document (aka .docx) as a so-called bilingual table.
* HTML bilingual table.
