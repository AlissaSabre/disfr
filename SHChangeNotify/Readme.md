SHChangeNotify
==============

SHChangeNotify is a very small Windows program to issue a call to a Windows API of same name like
```
SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, NULL, NULL);
```
The installer uses this program to tell Shell (Explorer) that some file association info are updated.
