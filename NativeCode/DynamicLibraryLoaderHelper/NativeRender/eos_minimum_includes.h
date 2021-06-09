#pragma once
// This file has the minimum required EOS Header information, copied out from the various EOS SDK header files 

#include <inttypes.h>

//-------------------------------------------------------------------------
//*** BEGIN COPY code from EOS HEADERS
#define EOS_INITIALIZE_API_LATEST 3 // should be 4???
#define EOS_PLATFORM_OPTIONS_API_LATEST 8 // should be 9?
#ifdef __cplusplus
#define EXTERN_C extern "C"
#else
#define EXTERN_C
#endif

#if defined(EOS_BUILDING_SDK) && EOS_BUILDING_SDK > 0
#if EOS_USE_DLLEXPORT
#ifdef __GNUC__
#define EOS_API __attribute__ ((dllexport))
#else
#define EOS_API __declspec(dllexport)
#endif
#else
#if __GNUC__ >= 4
#define EOS_API __attribute__ ((visibility ("default")))
#else
#define EOS_API
#endif
#endif

#else

#if EOS_USE_DLLEXPORT
#if defined(EOS_MONOLITHIC) && EOS_MONOLITHIC > 0
#define EOS_API
#elif defined(EOS_BUILD_DLL) && EOS_BUILD_DLL > 0
#ifdef __GNUC__
#define EOS_API __attribute__ ((dllexport))
#else
#define EOS_API __declspec(dllexport)
#endif
#else
#ifdef __GNUC__
#define EOS_API __attribute__ ((dllimport))
#else
#define EOS_API __declspec(dllimport)
#endif
#endif
#else
#if __GNUC__ >= 4
#define EOS_API __attribute__ ((visibility ("default")))
#else
#define EOS_API
#endif
#endif
#endif

#ifdef EOS_HAS_ENUM_CLASS
#define EOS_ENUM_START(name) enum class name : int32_t {
#define EOS_ENUM_END(name) }
#else
#define EOS_ENUM_START(name) typedef enum name {
#define EOS_ENUM_END(name) , __##name##_PAD_INT32__ = 0x7FFFFFFF } name
#endif
#define EOS_ENUM(name, ...) EOS_ENUM_START(name) __VA_ARGS__ EOS_ENUM_END(name)


#if defined(_WIN32) && (defined(__i386) || defined(_M_IX86))
#define EOS_CALL __stdcall
#define EOS_MEMORY_CALL __stdcall
#else
#define EOS_CALL
#define EOS_MEMORY_CALL
#endif

#define EOS_PASTE(...) __VA_ARGS__
#define EOS_STRUCT(struct_name, struct_def)           \
	EXTERN_C typedef struct _tag ## struct_name {     \
		EOS_PASTE struct_def                          \
	} struct_name

#define EOS_RESULT_VALUE(Name, Value) Name = Value,
#define EOS_RESULT_VALUE_LAST(Name, Value) Name = Value



EOS_ENUM_START(EOS_EResult)
EOS_RESULT_VALUE(EOS_Success, 0)
EOS_RESULT_VALUE_LAST(EOS_UnexpectedError, 0x7FFFFFFF)
EOS_ENUM_END(EOS_EResult);
#define EOS_DECLARE_FUNC(return_type) EXTERN_C EOS_API return_type EOS_CALL


typedef int32_t EOS_Bool;
#define EOS_TRUE 1
#define EOS_FALSE 0

/** Client credentials. */
EOS_STRUCT(EOS_Platform_ClientCredentials, (
	const char* ClientId;
	const char* ClientSecret;
));

/**
 * Options for initializing the Epic Online Services SDK.
 */
EOS_STRUCT(EOS_InitializeOptions, (
	int32_t ApiVersion;
	EOS_AllocateMemoryFunc AllocateMemoryFunction;
	EOS_ReallocateMemoryFunc ReallocateMemoryFunction;
	EOS_ReleaseMemoryFunc ReleaseMemoryFunction;

	const char* ProductName;
	const char* ProductVersion;

	void* Reserved;
	void* SystemInitializeOptions;
));


EOS_STRUCT(EOS_Platform_Options, (

	int32_t ApiVersion;

void* Reserved;

const char* ProductId;

const char* SandboxId;

EOS_Platform_ClientCredentials ClientCredentials;

EOS_Bool bIsServer;

const char* EncryptionKey;

const char* OverrideCountryCode;

const char* OverrideLocaleCode;

const char* DeploymentId;

uint64_t Flags;

const char* CacheDirectory;
uint32_t TickBudgetInMilliseconds;
));

EXTERN_C typedef struct EOS_PlatformHandle* EOS_HPlatform;




/// ***** END 
//-------------------------------------------------------------------------
