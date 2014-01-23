
#include "stdafx.h"


UINT __stdcall CheckForChocolatey(MSIHANDLE hInstall)
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	hr = WcaInitialize(hInstall, "CheckForChocolatey");
	if(FAILED(hr))
	{
		ExitTrace(hr, "Failed to initialize");
		return WcaFinalize(ERROR_INSTALL_FAILURE);
	}

	WcaLog(LOGMSG_STANDARD, "Initialized.");

	auto var = getenv("ChocolateyInstall");
	if(var != NULL)
		MsiSetProperty(hInstall, L"CHOCOLATEYINSTALLED", L"Yes");

	return WcaFinalize(er);
}


// DllMain - Initialize and cleanup WiX custom action utils.
extern "C" BOOL WINAPI DllMain(
	__in HINSTANCE hInst,
	__in ULONG ulReason,
	__in LPVOID
	)
{
	switch(ulReason)
	{
	case DLL_PROCESS_ATTACH:
		WcaGlobalInitialize(hInst);
		break;

	case DLL_PROCESS_DETACH:
		WcaGlobalFinalize();
		break;
	}

	return TRUE;
}
