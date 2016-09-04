CustomActions.dll
=================

CustomActions.dll is a collections of C functions to be used as Custom Action Type 1.
For the moment, it includes just one function ShellChangeNotify to call SHChangeNotify API.

To produce a small .dll output, some of the compiler/linker options are unusual.
Most significantly, it doesn't link any C Runtime Library.
Be careful if you are adding more codes.
