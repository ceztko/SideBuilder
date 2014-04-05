// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the WIN32DLL_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// WIN32DLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef $safeprjnameupcase$_EXPORTS
#define $safeprjnameupcase$_API __declspec(dllexport)
#else
#define $safeprjnameupcase$_API __declspec(dllimport)
#endif

// This class is exported from the Win32Dll.dll
class $safeprjnameupcase$_API C$safeprojectname$ {
public:
	C$safeprojectname$(void);
	// TODO: add your methods here.
};

extern $safeprjnameupcase$_API int n$safeprojectname$;

$safeprjnameupcase$_API int fn$safeprojectname$(void);
