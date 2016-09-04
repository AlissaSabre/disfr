#include <ShlObj.h>
#include <Msi.h>
#include <MsiQuery.h>

UINT __stdcall ShellChangeNotify(MSIHANDLE hInstall)
{
	SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, NULL, NULL);
	return ERROR_SUCCESS;
}
