// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H

// add headers that you want to pre-compile here
#include "framework.h"


#if _WIN32 || _WIN64
#define PLATFORM_WINDOWS 1
#endif

#if _WIN64
#define PLATFORM_64BITS 1
#define PLATFORM_BITS_DEBUG_STR "64-bits"
#else
#define PLATFORM_32BITS 1
#define PLATFORM_BITS_DEBUG_STR "32-bits"
#endif

#define STATIC_EXPORT(return_type) extern "C" return_type
#endif //PCH_H
