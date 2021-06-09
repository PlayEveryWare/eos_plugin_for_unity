#pragma once

#include <vector>

struct NULLStruct
{
};

typedef NULLStruct PlatformSpecificContext;

struct DLLHContext
{
	PlatformSpecificContext platform_specific_ctx;
};
