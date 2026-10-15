/*
 * Copyright (c) 2026 Epic Games Inc
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#include <pch.h>
#include "eos_helpers.h"
#include <filesystem>
#include <sstream>
#include "config_legacy.h"
#include "eos_library_helpers.h"
#include "io_helpers.h"
#include "json_helpers.h"
#include "logging.h"
#include <codecvt>
#include <eos_types.h>

#include "../../../include/DLLHContext.h"
#include "Config/PlatformConfig.hpp"
#include "Config/ProductConfig.hpp"
#include "Config/SteamConfig.hpp"
#include "Config/WindowsConfig.hpp"

namespace pew::eos
{
    PEW_EOS_API_FUNC(void*) EOS_GetPlatformInterface()
    {
        return eos_library_helpers::eos_platform_handle;
    }

    typedef bool(__cdecl* SteamAPI_Init_t)();

    /**
     * @brief Initializes the steam api using the given function name.
     * @param steam_dll_handle Pointer to the steam dll 
     * @param function_name The name of the function to initialize with.
     * @return True if the steam api was initialized, false otherwise.
     */
    static bool initialize_steam_api(void* steam_dll_handle, const std::string& function_name);

    /**
     * @brief Loads and initializes the Steam API DLL using a string path.
     *
     * Attempts to load the Steam API DLL from the specified path. If the DLL is not already
     * loaded, this function tries to load it and then calls `SteamAPI_Init`.
     *
     * @param steam_dll_filename The string path to the Steam API DLL.
     */
    void eos_call_steam_init(const std::string& steam_dll_filename);

    void eos_set_loglevel_via_config()
    {
        if (eos_library_helpers::EOS_Logging_SetLogLevel_ptr == nullptr)
        {
            return;
        }

        auto path_to_log_config_json = config_legacy::get_path_for_eos_service_config(EOS_LOGLEVEL_CONFIG_FILENAME);

        if (!exists(path_to_log_config_json))
        {
            logging::log_inform("Log level config not found, using default log levels");
            return;
        }

        json_value_s* log_config_as_json = json_helpers::read_config_json_as_json_from_path(path_to_log_config_json);
        config_legacy::LogLevelConfig log_config = config_legacy::log_config_from_json_value(log_config_as_json);
        free(log_config_as_json);

        // Validation to prevent out of range exception
        if (log_config.category.size() != log_config.level.size())
        {
            logging::log_warn("Log level config entries out of range");
            return;
        }

        // Last in the vector is AllCategories, and will not be set
        size_t individual_category_size = log_config.category.size() > 0 ? log_config.category.size() - 1 : 0;
        if (individual_category_size == 0)
        {
            logging::log_warn("Log level config entries empty");
            return;
        }

        for (size_t i = 0; i < individual_category_size; i++)
        {
            eos_library_helpers::EOS_Logging_SetLogLevel_ptr((EOS_ELogCategory)i, logging::eos_loglevel_str_to_enum(log_config.level[i]));
        }

        logging::log_inform("Log levels set according to config");
    }

    void EOS_Platform_Options_debug_log(const EOS_Platform_Options& platform_options)
    {
        std::stringstream output;
        output << platform_options.ApiVersion << "\n";
        output << platform_options.bIsServer << "\n";
        output << platform_options.Flags << "\n";
        output << platform_options.CacheDirectory << "\n";

        output << platform_options.EncryptionKey << "\n";
        if (platform_options.OverrideCountryCode)
        {
            output << platform_options.OverrideCountryCode << "\n";
        }

        if (platform_options.OverrideLocaleCode)
        {
            output << platform_options.OverrideLocaleCode << "\n";
        }
        output << platform_options.ProductId << "\n";
        output << platform_options.SandboxId << "\n";
        output << platform_options.DeploymentId << "\n";
        output << platform_options.ClientCredentials.ClientId << "\n";
        output << platform_options.ClientCredentials.ClientSecret << "\n";

        auto* rtc_options = platform_options.RTCOptions;
        auto* windows_rtc_options = (EOS_Windows_RTCOptions*)rtc_options->PlatformSpecificOptions;

        output << windows_rtc_options->ApiVersion << "\n";
        output << windows_rtc_options->XAudio29DllPath << "\n";

        logging::log_inform(output.str().c_str());
    }

    EOS_InitializeOptions get_initialize_options(
        const PlatformConfig& platform_config, 
        const ProductConfig& product_config,
        int reserved_values[2],
        EOS_Initialize_ThreadAffinity& override_thread_affinity) 
    {
        // Populate the reserved array
        reserved_values[0] = 1;
        reserved_values[1] = 1;

        // Populate the thread affinity from the platform configuration
        override_thread_affinity = platform_config.thread_affinity;
        override_thread_affinity.ApiVersion = EOS_INITIALIZE_THREADAFFINITY_API_LATEST;

        // Construct and populate the initialize options structure
        EOS_InitializeOptions sdk_initialize_options = {};
        sdk_initialize_options.ApiVersion = EOS_INITIALIZE_API_LATEST;
        sdk_initialize_options.AllocateMemoryFunction = nullptr;
        sdk_initialize_options.ReallocateMemoryFunction = nullptr;
        sdk_initialize_options.ReleaseMemoryFunction = nullptr;
        sdk_initialize_options.ProductName = product_config.product_name.c_str();
        sdk_initialize_options.ProductVersion = product_config.product_version.c_str();
        sdk_initialize_options.Reserved = reserved_values;
        sdk_initialize_options.SystemInitializeOptions = nullptr;
        sdk_initialize_options.OverrideThreadAffinity = &override_thread_affinity;

        return sdk_initialize_options;
    }

    void eos_init(const PlatformConfig& platform_config, const ProductConfig& product_config)
    {
        // Allocate the required resources
        int reserved_values[2];
        EOS_Initialize_ThreadAffinity override_thread_affinity;
        
        auto sdk_initialization_options = get_initialize_options(platform_config, product_config, reserved_values, override_thread_affinity);

        logging::log_inform("call EOS_Initialize");
        EOS_EResult InitResult = eos_library_helpers::EOS_Initialize_ptr(&sdk_initialization_options);
        if (InitResult != EOS_EResult::EOS_Success)
        {
            logging::log_error("Unable to do eos init");
        }
        if (eos_library_helpers::EOS_Logging_SetLogLevel_ptr != nullptr)
        {
            eos_library_helpers::EOS_Logging_SetLogLevel_ptr(EOS_ELogCategory::EOS_LC_ALL_CATEGORIES, EOS_ELogLevel::EOS_LOG_VeryVerbose);
        }

        if (eos_library_helpers::EOS_Logging_SetCallback_ptr != nullptr)
        {
            eos_library_helpers::EOS_Logging_SetCallback_ptr(&logging::eos_log_callback);
        }
    }

    void eos_call_steam_init(const std::string& steam_dll_filename)
    {
        // Default (fallback) name of the steam dll to load.
        constexpr const char* DEFAULT_STEAM_DLL_NAME = "steam_api.dll";

        const auto steam_dll_path = std::filesystem::path(steam_dll_filename);

        // Check to see if the path exists, stop and log a warning if it does not.
        if (!exists(steam_dll_path))
        {
            logging::log_warn("Steam DLL provided (\""
                + steam_dll_path.string() + "\") does not exist; checking for other options.");
        }

        // Get a handle to the steam dll
        HANDLE steam_dll_handle = GetModuleHandleA(steam_dll_path.filename().string().c_str());

        // If getting a handle to the steam dll was not successful, log a
        // warning, and if the filename given as a parameter is something other
        // than "steam_api.dll", try using that to get a library handle.
        if (steam_dll_handle == nullptr)
        {
            logging::log_warn("Steam DLL exists, but a handle to the module was not able to be loaded.");
            if (steam_dll_filename != DEFAULT_STEAM_DLL_NAME)
            {
                logging::log_inform(
                    "Because Steam DLL could not be loaded, and a "
                    "different dll file that the default was indicated, trying "
                    "again with the default dll file name of "
                    "\"" + std::string(DEFAULT_STEAM_DLL_NAME) + "\".");

                // TODO: Isn't it supposed to be the basename of the dll, like
                // it is in the first call to GetModuleHandleA?
                steam_dll_handle = GetModuleHandleA(DEFAULT_STEAM_DLL_NAME);
            }

            // If the steam dll handle is *STILL* not loaded, then attempt to
            // use load_library_at_path to load the dll - because _it_ uses the
            // LoadLibrary method, different from the GetModuleHandleA functions
            // used above.
            if (steam_dll_handle == nullptr)
            {
                steam_dll_handle = eos_library_helpers::load_library_at_path(steam_dll_path);
            }

            // If a handle to the steam dll has still not been obtained, then
            // log an error and stop.
            if (steam_dll_handle == nullptr)
            {
                logging::log_error(
                    "Could not obtain a handle to the steam library. "
                    "Steam initialization has failed.");
                return;
            }
        }

        logging::log_inform(
            "Handle to the Steam library has been retrieved. Next "
            "attempting to load a pointer to the SteamAPI_Init function from "
            "within it.");

        // Usage
        if (!initialize_steam_api(steam_dll_handle, "SteamAPI_InitSafe"))
        {
            // Retry with "SteamAPI_Init" if "SteamAPI_InitSafe" fails.
            initialize_steam_api(steam_dll_handle, "SteamAPI_Init");
        }
    }

    static bool initialize_steam_api(void* steam_dll_handle, const std::string& function_name)
    {
        // Get a pointer to the specified SteamAPI_Init function within the steam library.
        const auto SteamAPI_Init_Fn = pew::eos::eos_library_helpers::load_function_with_name<SteamAPI_Init_t>(steam_dll_handle, function_name.c_str());

        // If the function pointer is null, log an error and return false.
        if (SteamAPI_Init_Fn == nullptr)
        {
            logging::log_error(
                "Could not load a pointer to the " + function_name +
                " function within the loaded steam library handle.");
            return false;
        }

        // Attempt to initialize Steam and log the result.
        if (SteamAPI_Init_Fn())
        {
            logging::log_inform(function_name + " returned true. Steam has been initialized.");
            return true;
        }
        else
        {
            logging::log_error(function_name + " returned false. Steam was not able to be initialized.");
            return false;
        }
    }
    

    void apply_steam_settings(EOS_Platform_Options& platform_options) 
    {
        // =================== START APPLY STEAM OPTIONS =======================

        // This struct is used to guarantee the lifetime of the string field 
        // members without introducing dangling pointers.
        struct SteamSettingsContext 
        {
            std::string override_library_path;
            std::string api_interface_versions;
        };

        EOS_IntegratedPlatform_Steam_Options steam_platform = { 0 };
        EOS_HIntegratedPlatformOptionsContainer integrated_platform_options_container = nullptr;

        // Context to manage string lifetimes
        SteamSettingsContext context;

        const auto eos_steam_config = Config::get<SteamConfig>();
        std::filesystem::path steam_library_path;

        // The Steam SDK version is only applied when the Steam library is
        // found, so adding the options without it makes the SDK reject them.
        // TODO: overrideLibraryPath is parsed into _library_path rather than
        //       _override_library_path, so only one path is checked here: the
        //       override if set, otherwise the default next to this module.
        if (!eos_steam_config->try_get_library_path(steam_library_path))
        {
            if (eos_steam_config->is_steam_configured())
            {
                logging::log_warn("Steam integration configured but library not found at: " +
                    eos_steam_config->get_library_path().string() + ". Skipping Steam integrated platform options.");
            }
            else
            {
                logging::log_inform("Steam integration not configured. Skipping Steam integrated platform options.");
            }
            return;
        }

        // Configure Steam platform options
        steam_platform.SteamMajorVersion = eos_steam_config->steam_sdk_major_version;
        steam_platform.SteamMinorVersion = eos_steam_config->steam_sdk_minor_version;

        if (eos_steam_config->is_managed_by_application()) {
            eos_call_steam_init(steam_library_path.string());
            context.override_library_path = steam_library_path.filename().string();
            steam_platform.OverrideLibraryPath = context.override_library_path.c_str();
        }

        context.api_interface_versions = eos_steam_config->get_steam_api_interface_versions_array();
        steam_platform.SteamApiInterfaceVersionsArray = context.api_interface_versions.c_str();

        const auto size = context.api_interface_versions.size();
        if (size > EOS_INTEGRATEDPLATFORM_STEAM_MAX_STEAMAPIINTERFACEVERSIONSARRAY_SIZE) 
        {
            logging::log_error("Steam API interface versions array exceeds maximum allowed size.");
        }
        else 
        {
            steam_platform.SteamApiInterfaceVersionsArrayBytes = static_cast<uint32_t>(size);
        }

        // Create and configure integrated platform options
        EOS_IntegratedPlatform_Options steam_integrated_platform_option = {};
        steam_integrated_platform_option.ApiVersion = EOS_INTEGRATEDPLATFORM_OPTIONS_API_LATEST;
        steam_integrated_platform_option.Type = EOS_IPT_Steam;
        steam_integrated_platform_option.Flags = eos_steam_config->integrated_platform_management_flags;
        steam_integrated_platform_option.InitOptions = &steam_platform;

        steam_platform.ApiVersion = EOS_INTEGRATEDPLATFORM_STEAM_OPTIONS_API_LATEST;

        // Create and add the options container
        EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainerOptions options = { EOS_INTEGRATEDPLATFORM_CREATEINTEGRATEDPLATFORMOPTIONSCONTAINER_API_LATEST };
        eos_library_helpers::EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer_ptr(&options, &integrated_platform_options_container);
        platform_options.IntegratedPlatformOptionsContainerHandle = integrated_platform_options_container;

        EOS_IntegratedPlatformOptionsContainer_AddOptions add_options = { EOS_INTEGRATEDPLATFORMOPTIONSCONTAINER_ADD_API_LATEST };
        add_options.Options = &steam_integrated_platform_option;
        eos_library_helpers::EOS_IntegratedPlatformOptionsContainer_Add_ptr(integrated_platform_options_container, &add_options);
        // =================== END APPLY STEAM OPTIONS =========================
    }

    EOS_Platform_Options get_create_options(const PlatformConfig& platform_config, const ProductConfig& product_config)
    {
        EOS_Platform_Options platform_options = { 0 };
        platform_options.ApiVersion = EOS_PLATFORM_OPTIONS_API_LATEST;
        platform_options.bIsServer = platform_config.is_server;
        platform_options.Flags = platform_config.platform_options_flags;

        // Get the cache directory
        platform_options.CacheDirectory = platform_config.get_cache_directory();

        platform_options.EncryptionKey = platform_config.client_credentials.encryption_key.c_str();

        platform_options.OverrideCountryCode = platform_config.overrideCountryCode.empty() ? nullptr : platform_config.overrideCountryCode.c_str();
        platform_options.OverrideLocaleCode = platform_config.overrideLocaleCode.empty() ? nullptr : platform_config.overrideLocaleCode.c_str();

        platform_options.ProductId = product_config.product_id.c_str();
        platform_options.SandboxId = platform_config.deployment.sandbox.id.c_str();
        platform_options.DeploymentId = platform_config.deployment.id.c_str();
        platform_options.ClientCredentials.ClientId = platform_config.client_credentials.client_id.c_str();
        platform_options.ClientCredentials.ClientSecret = platform_config.client_credentials.client_secret.c_str();

        platform_options.TickBudgetInMilliseconds = platform_config.tick_budget_in_milliseconds;

        if (platform_config.task_network_timeout_seconds > 0)
        {
            // Make sure this gets deleted
            platform_options.TaskNetworkTimeoutSeconds = new double(platform_config.task_network_timeout_seconds);
        }

        platform_options.RTCOptions = platform_config.get_platform_rtc_options().get();

        apply_steam_settings(platform_options);

        return platform_options;
    }

    // NOTE: This compile conditional is here because these functions are only 
    //       utilized to test the compatibility between native and managed 
    //       components of the plugin to guarantee their equivalency.
#if _DEBUG
    PEW_EOS_API_FUNC(EOS_Platform_Options) PEW_EOS_Get_CreateOptions()
    {
        static const auto platform_config = Config::get<WindowsConfig>();
        static const auto product_config = Config::get<ProductConfig>();

        static const auto create_options = get_create_options(*platform_config, *product_config);

        // delete the one allocation
        delete create_options.TaskNetworkTimeoutSeconds;

        return create_options;
    }

    PEW_EOS_API_FUNC(EOS_InitializeOptions) PEW_EOS_Get_InitializeOptions()
    {
        static const auto platform_config = Config::get<WindowsConfig>();
        static const auto product_config = Config::get<ProductConfig>();

        // Allocate the required resources
        int reserved_values[2];
        EOS_Initialize_ThreadAffinity override_thread_affinity;

        static const auto initialize_options = get_initialize_options(*platform_config, *product_config, reserved_values, override_thread_affinity);

        return initialize_options;
    }
#endif

    void eos_create(const PlatformConfig& platform_config, const ProductConfig& product_config)
    {
        auto platform_options = get_create_options(platform_config, product_config);

        logging::log_inform("Calling EOS_Platform_Create");
        eos_library_helpers::eos_platform_handle = eos_library_helpers::EOS_Platform_Create_ptr(&platform_options);

        // If there is an integrated platform options container, make sure that it is freed.
        if (platform_options.IntegratedPlatformOptionsContainerHandle)
        {
            eos_library_helpers::EOS_IntegratedPlatformOptionsContainer_Release_ptr(platform_options.IntegratedPlatformOptionsContainerHandle);
        }

        if (!eos_library_helpers::eos_platform_handle)
        {
            logging::log_error("Failed to create the EOS SDK Platform.");
        }
        else
        {
            logging::log_inform("Successfully created the EOS SDK Platform.");
        }

        // Delete the one allocation
        delete platform_options.TaskNetworkTimeoutSeconds;
    }
}
