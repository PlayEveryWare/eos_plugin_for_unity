// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H

#if _WIN32
#define WIN32_LEAN_AND_MEAN
#pragma pack(push, 8)
#include <windows.h>
#pragma pack(pop)
#endif

// Configure platform defines
#if _WIN32 || _WIN64
#define PLATFORM_WINDOWS 1
#endif

// add headers that you want to pre-compile here
#include "framework.h"

// This needs to be define differently on different platforms ?
#define STATIC_EXPORT(return_type) extern "C" return_type

#endif //PCH_H
