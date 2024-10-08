// Driver.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "../../third_party/eos_sdk/include/eos_sdk.h"
#include <map>
#include "json.h"
#include <vector>
#include <algorithm>
#include <string>
#include <sstream>
#include <filesystem>

#include <stdio.h>
#include <stdlib.h>

#include <string>
#include <sstream>
#include <functional>
#include <utility>
#include <windows.h>
#include <optional>
#include <codecvt>
#include <vector>
#include <iostream>
#include <iterator>

struct SandboxDeploymentOverride
{
    std::string sandboxID;
    std::string deploymentID;
};

struct EOSConfig
{
    std::string productName;
    std::string productVersion;

    std::string productID;
    std::string sandboxID;
    std::string deploymentID;
    std::vector<SandboxDeploymentOverride> sandboxDeploymentOverrides;

    std::string clientSecret;
    std::string clientID;
    std::string encryptionKey;

    std::string overrideCountryCode;
    std::string overrideLocaleCode;

    // this is called platformOptionsFlags in C#
    uint64_t flags = 0;

    uint32_t tickBudgetInMilliseconds = 0;
    double taskNetworkTimeoutSeconds = 0.0;

    uint64_t ThreadAffinity_networkWork = 0;
    uint64_t ThreadAffinity_storageIO = 0;
    uint64_t ThreadAffinity_webSocketIO = 0;
    uint64_t ThreadAffinity_P2PIO = 0;
    uint64_t ThreadAffinity_HTTPRequestIO = 0;
    uint64_t ThreadAffinity_RTCIO = 0;

    bool isServer = false;

};

/**
 * \brief Trims the whitespace from the beginning and end of a string.
 *
 * \param str The string to trim.
 *
 * \return A string with no whitespace at the beginning or end.
 */
static std::string trim(const std::string& str);

/**
 * \brief
 * Takes a string, splits it by the indicated delimiter, trims the results of
 * the split, and returns a collection of any non-empty values.
 *
 * \param input The string to split and trim.
 *
 * \param delimiter The character at which to split the string.
 *
 * \return A list of string values.
 */
static std::vector<std::string> split_and_trim(const std::string& input, char delimiter = ',');

/**
 * \brief
 * Collects flag values from either a JSON array of strings, or a
 * comma-delimited list of values (like how Newtonsoft outputs things).
 *
 * \tparam T The type parameter (the enum type).
 *
 * \param
 * strings_to_enum_values A collection that maps string values to enum values.
 *
 * \param
 * default_value The default value to assign the flags if no matching string is
 * found.
 *
 * \param
 * iter The iterator into the json object element.
 *
 * \return A single flag value.
 */
template<typename T>
static T collect_flags(const std::map<const char*, T>* strings_to_enum_values, T default_value, json_object_element_s* iter);

static std::string trim(const std::string& str)
{
    const auto start = std::find_if_not(str.begin(), str.end(), ::isspace);
    const auto end = std::find_if_not(str.rbegin(), str.rend(), ::isspace).base();

    if (start < end)
    {
        return std::basic_string<char>(start, end);
    }
    else
    {
        return "";
    }
}

static std::vector<std::string> split_and_trim(const std::string& input, char delimiter)
{
    std::vector<std::string> result;
    std::stringstream ss(input);
    std::string item;

    while (std::getline(ss, item, delimiter))
    {
        std::string trimmedItem = trim(item);
        if (!trimmedItem.empty())
        {
            result.push_back(trimmedItem);
        }
    }

    return result;
}


/**
 * \brief
 * Maps string values to values defined by the EOS SDK regarding platform
 * creation.
 */
static const std::map<const char*, int> PLATFORM_CREATION_FLAGS_STRINGS_TO_ENUM = {
    
    {"EOS_PF_LOADING_IN_EDITOR",                          EOS_PF_LOADING_IN_EDITOR},
    {"LoadingInEditor",                                   EOS_PF_LOADING_IN_EDITOR},

    {"EOS_PF_DISABLE_OVERLAY",                            EOS_PF_DISABLE_OVERLAY},
    {"DisableOverlay",                                    EOS_PF_DISABLE_OVERLAY},

    {"EOS_PF_DISABLE_SOCIAL_OVERLAY",                     EOS_PF_DISABLE_SOCIAL_OVERLAY},
    {"DisableSocialOverlay",                              EOS_PF_DISABLE_SOCIAL_OVERLAY},

    {"EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9",                EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9},
    {"WindowsEnableOverlayD3D9",                          EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9},

    {"EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10",               EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10},
    {"WindowsEnableOverlayD3D10",                         EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10},

    {"EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL",              EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL},
    {"WindowsEnableOverlayOpengl",                        EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL},

    {"EOS_PF_CONSOLE_ENABLE_OVERLAY_AUTOMATIC_UNLOADING", EOS_PF_CONSOLE_ENABLE_OVERLAY_AUTOMATIC_UNLOADING},
    {"ConsoleEnableOverlayAutomaticUnloading",            EOS_PF_CONSOLE_ENABLE_OVERLAY_AUTOMATIC_UNLOADING},

    {"EOS_PF_RESERVED1",                                  EOS_PF_RESERVED1},
    {"Reserved1",                                         EOS_PF_RESERVED1}
};

/**
 * \brief Maps string values to values within the
 * EOS_EIntegratedPlatformManagementFlags enum.
 */
static const std::map<const char*, EOS_EIntegratedPlatformManagementFlags> INTEGRATED_PLATFORM_MANAGEMENT_FLAGS_STRINGS_TO_ENUM = {
    {"EOS_IPMF_Disabled",                        EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_Disabled },
    {"Disabled",                                 EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_Disabled },

    {"EOS_IPMF_LibraryManagedByApplication",     EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedByApplication },
    {"EOS_IPMF_ManagedByApplication",            EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedByApplication},
    {"ManagedByApplication",                     EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedByApplication},
    {"LibraryManagedByApplication",              EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedByApplication},

    {"ManagedBySDK",                             EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedBySDK },
    {"EOS_IPMF_ManagedBySDK",                    EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedBySDK },
    {"EOS_IPMF_LibraryManagedBySDK",             EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedBySDK },
    {"LibraryManagedBySDK",                      EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedBySDK },

    {"DisableSharedPresence",                    EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisablePresenceMirroring },
    {"EOS_IPMF_DisableSharedPresence",           EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisablePresenceMirroring },
    {"EOS_IPMF_DisablePresenceMirroring",        EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisablePresenceMirroring },
    {"DisablePresenceMirroring",                 EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisablePresenceMirroring},

    {"DisableSessions",                          EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisableSDKManagedSessions },
    {"EOS_IPMF_DisableSessions",                 EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisableSDKManagedSessions },
    {"EOS_IPMF_DisableSDKManagedSessions",       EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisableSDKManagedSessions },
    {"DisableSDKManagedSessions",                EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisableSDKManagedSessions },

    {"PreferEOS",                                EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferEOSIdentity },
    {"EOS_IPMF_PreferEOS",                       EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferEOSIdentity },
    {"EOS_IPMF_PreferEOSIdentity",               EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferEOSIdentity },
    {"PreferEOSIdentity",                        EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferEOSIdentity},

    {"PreferIntegrated",                         EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferIntegratedIdentity },
    {"EOS_IPMF_PreferIntegrated",                EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferIntegratedIdentity },
    {"EOS_IPMF_PreferIntegratedIdentity",        EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferIntegratedIdentity },
    {"PreferIntegratedIdentity",                 EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferIntegratedIdentity},

    {"EOS_IPMF_ApplicationManagedIdentityLogin", EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_ApplicationManagedIdentityLogin },
    {"ApplicationManagedIdentityLogin",          EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_ApplicationManagedIdentityLogin}
};

template<typename T>
static T collect_flags(const std::map<const char*, T>* strings_to_enum_values, T default_value, json_object_element_s* iter)
{
    T flags_to_return = static_cast<T>(0);
    bool flag_set = false;

    // Stores the string values that are within the JSON
    std::vector<std::string> string_values;

    // If the string values are stored as a JSON array of strings
    if (iter->value->type == json_type_array)
    {
        // Do things if the type is an array
        json_array_s* flags = json_value_as_array(iter->value);
        for (auto e = flags->start; e != nullptr; e = e->next)
        {
            string_values.emplace_back(json_value_as_string(e->value)->string);
        }
    }
    // If the string values are comma delimited
    else if (iter->value->type == json_type_string)
    {
        const std::string flags = json_value_as_string(iter->value)->string;
        string_values = split_and_trim(flags);
    }

    // Iterate through the string values
    for (const auto str : string_values)
    {
        // Skip if the string is not in the map
        if (strings_to_enum_values->find(str.c_str()) == strings_to_enum_values->end())
        {
            continue;
        }

        // Otherwise, append the enum value
        flags_to_return |= strings_to_enum_values->at(str.c_str());
        flag_set = true;
    }

    return flag_set ? flags_to_return : default_value;
}


//-------------------------------------------------------------------------
static uint64_t json_value_as_uint64(json_value_s* value, uint64_t default_value = 0)
{
    uint64_t val = 0;
    json_number_s* n = json_value_as_number(value);

    if (n != nullptr)
    {
        char* end = nullptr;
        val = strtoull(n->number, &end, 10);
    }
    else
    {
        // try to treat it as a string, then parse as long
        char* end = nullptr;
        json_string_s* val_as_str = json_value_as_string(value);
        if (val_as_str == nullptr || strlen(val_as_str->string) == 0)
        {
            val = default_value;
        }
        else
        {
            val = strtoull(val_as_str->string, &end, 10);
        }
    }

    return val;
}

//-------------------------------------------------------------------------
static uint32_t json_value_as_uint32(json_value_s* value, uint32_t default_value = 0)
{
    uint32_t val = 0;
    json_number_s* n = json_value_as_number(value);

    if (n != nullptr)
    {
        char* end = nullptr;
        val = strtoul(n->number, &end, 10);
    }
    else
    {
        // try to treat it as a string, then parse as long
        char* end = nullptr;
        json_string_s* val_as_str = json_value_as_string(value);

        if (val_as_str == nullptr || strlen(val_as_str->string) == 0)
        {
            val = default_value;
        }
        else
        {
            val = strtoul(val_as_str->string, &end, 10);
        }
    }

    return val;
}

//-------------------------------------------------------------------------
static double json_value_as_double(json_value_s* value, double default_value = 0.0)
{
    double val = 0.0;
    json_number_s* n = json_value_as_number(value);

    if (n != nullptr)
    {
        char* end = nullptr;
        val = strtod(n->number, &end);
    }
    else
    {
        // try to treat it as a string, then parse as long
        char* end = nullptr;
        json_string_s* val_as_str = json_value_as_string(value);

        if (val_as_str == nullptr || strlen(val_as_str->string) == 0)
        {
            val = default_value;
        }
        else
        {
            val = strtod(val_as_str->string, &end);
        }
    }

    return val;
}


static EOSConfig eos_config_from_json_value(json_value_s* config_json)
{
    // Create platform instance
    struct json_object_s* config_json_object = json_value_as_object(config_json);
    struct json_object_element_s* iter = config_json_object->start;
    EOSConfig eos_config;

    while (iter != nullptr)
    {
        if (!strcmp("productName", iter->name->string))
        {
            eos_config.productName = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("productVersion", iter->name->string))
        {
            eos_config.productVersion = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("productID", iter->name->string))
        {
            eos_config.productID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("sandboxID", iter->name->string))
        {
            eos_config.sandboxID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("deploymentID", iter->name->string))
        {
            eos_config.deploymentID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("sandboxDeploymentOverrides", iter->name->string))
        {
            json_array_s* overrides = json_value_as_array(iter->value);
            eos_config.sandboxDeploymentOverrides = std::vector<SandboxDeploymentOverride>();
            for (auto e = overrides->start; e != nullptr; e = e->next)
            {
                struct json_object_s* override_json_object = json_value_as_object(e->value);
                struct json_object_element_s* ov_iter = override_json_object->start;
                struct SandboxDeploymentOverride override_item = SandboxDeploymentOverride();
                while (ov_iter != nullptr)
                {
                    if (!strcmp("sandboxID", ov_iter->name->string))
                    {
                        override_item.sandboxID = json_value_as_string(ov_iter->value)->string;
                    }
                    else if (!strcmp("deploymentID", ov_iter->name->string))
                    {
                        override_item.deploymentID = json_value_as_string(ov_iter->value)->string;
                    }
                    ov_iter = ov_iter->next;
                }
                eos_config.sandboxDeploymentOverrides.push_back(override_item);
            }
        }
        else if (!strcmp("clientID", iter->name->string))
        {
            eos_config.clientID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("clientSecret", iter->name->string))
        {
            eos_config.clientSecret = json_value_as_string(iter->value)->string;
        }
        if (!strcmp("encryptionKey", iter->name->string))
        {
            eos_config.encryptionKey = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("overrideCountryCode ", iter->name->string))
        {
            eos_config.overrideCountryCode = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("overrideLocaleCode", iter->name->string))
        {
            eos_config.overrideLocaleCode = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("platformOptionsFlags", iter->name->string))
        {
            eos_config.flags = static_cast<uint64_t>(collect_flags(&PLATFORM_CREATION_FLAGS_STRINGS_TO_ENUM, 0, iter));
        }
        else if (!strcmp("tickBudgetInMilliseconds", iter->name->string))
        {
            eos_config.tickBudgetInMilliseconds = json_value_as_uint32(iter->value);
        }
        else if (!strcmp("taskNetworkTimeoutSeconds", iter->name->string))
        {
            eos_config.taskNetworkTimeoutSeconds = json_value_as_double(iter->value);
        }
        else if (!strcmp("ThreadAffinity_networkWork", iter->name->string))
        {
            eos_config.ThreadAffinity_networkWork = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_storageIO", iter->name->string))
        {
            eos_config.ThreadAffinity_storageIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_webSocketIO", iter->name->string))
        {
            eos_config.ThreadAffinity_webSocketIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_P2PIO", iter->name->string))
        {
            eos_config.ThreadAffinity_P2PIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_HTTPRequestIO", iter->name->string))
        {
            eos_config.ThreadAffinity_HTTPRequestIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_RTCIO", iter->name->string))
        {
            eos_config.ThreadAffinity_RTCIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("isServer", iter->name->string))
        {
            // In this JSON library, true and false are _technically_ different types. 
            if (json_value_is_true(iter->value))
            {
                eos_config.isServer = true;
            }
            else if (json_value_is_false(iter->value))
            {
                eos_config.isServer = false;
            }
        }

        iter = iter->next;
    }

    return eos_config;
}

//-------------------------------------------------------------------------
static json_value_s* read_config_json_as_json_from_path(std::filesystem::path path_to_config_json)
{
    uintmax_t config_file_size = std::filesystem::file_size(path_to_config_json);
    FILE* file = nullptr;
    errno_t config_file_error = _wfopen_s(&file, path_to_config_json.wstring().c_str(), L"r");
    char* buffer = (char*)calloc(1, static_cast<size_t>(config_file_size));

    size_t bytes_read = fread(buffer, 1, static_cast<size_t>(config_file_size), file);
    fclose(file);
    struct json_value_s* config_json = json_parse(buffer, bytes_read);
    free(buffer);

    return config_json;
}

int main()
{
    auto eos_config_as_json = read_config_json_as_json_from_path("C:\\Users\\PaulPEW\\dev\\repos\\eos_plugin_for_unity\\Assets\\StreamingAssets\\EOS\\EpicOnlineServicesConfig.json");
    auto eos_config = eos_config_from_json_value(eos_config_as_json);
    std::cout << "Hello World!\n";
    
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
