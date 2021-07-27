#pragma once

// Configure platform defines
#define PLATFORM_WINDOWS 1
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#pragma pack(push, 8)
#include <windows.h>
#pragma pack(pop)

#define STATIC_EXPORT(return_type) extern "C" return_type

#define DLL_EXPORT(return_value) extern "C" __declspec(dllexport) return_value  __stdcall

#define FUN_EXPORT(return_value) DLL_EXPORT(return_value)
