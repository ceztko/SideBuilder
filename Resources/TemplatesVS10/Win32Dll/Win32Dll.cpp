// Win32Dll.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "$safeprojectname$.h"


// This is an example of an exported variable
$safeprjnameupcase$_API int n$safeprojectname$=0;

// This is an example of an exported function.
$safeprjnameupcase$ int fn$safeprojectname$(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see Win32Dll.h for the class definition
C$safeprojectname$::C$safeprojectname$()
{
	return;
}
