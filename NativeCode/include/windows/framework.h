#pragma once

// Configure platform defines
#define PLATFORM_WINDOWS 1
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#pragma pack(push, 8)
#include <windows.h>
#pragma pack(pop)

#define STATIC_EXPORT(return_type) extern "C" return_type
