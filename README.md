disfr
=====
A Windows program to view/examine XLIFF file contents.

## Summary

XLIFF is an OASIS standard XML file format for bilingual document used by professional translators.  It is used in most (if not all) of modern CAT environments.

disfr is a handy tool to show the contents of XLIFF and similar files.  It has some features to further examine the files, e.g., filtering and sorting segments, inspecting tags, or comparing differences between versions.  It has no way to edit/change the contents, however.

disfr is written in C# and uses WPF.  (And, no, it does not strictly adhere the MVVM doctrine...  In fact, it heavily uses _code behinds_.) 

For the moment, disfr is under development and is far before alpha stage.  You should however be able to build the sources on HEAD of master branch and to try running it.

## File formats

It reads the following file formats, which are more or less based on XLIFF standard:
* Standard XLIFF 1.1 and 1.2 files.
* Zipped XLIFF 1.1 and 1.2 files.
* SDL Idiom WorldServer translation kit and return packages.
* SDL Trados Studio files.  (in progress)
* Kilgray memoQ bilingual files.  (in progress)

It also reads the following file formats, which are not based on XLIFF:
* TMX files (raw/zipped, in progress)
* so-called CSV (tab-separated UTF-8 only; in progress)

It may be capable of reading the following file formats in the future:
* Standard XLIFF 2.0 and packages containing it.
* .po files for GNU gettext.
* .string files for iOS Apps.

It can also write contents in the following file formats:
* TMX file.

It may be capable of _writing_ the contents out into the following file formats in the future:
* so-called CSV.
* Office OpenXML WordProcessing document (aka .docx).
* Microsoft Excel book.
