#ifndef STEAM_CONFIG_HPP
#define STEAM_CONFIG_HPP
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

#pragma once

#include "config_legacy.h"
#include "include/Config/Config.hpp"

namespace pew::eos::config
{
    struct SteamConfig final : Config
    {
        /**
         * \brief Major version of the steam SDK used.
         */
        uint32_t steam_sdk_major_version;

        /**
         * \brief Minor version of the steam SDK used.
         */
        uint32_t steam_sdk_minor_version;

        /**
         * \brief Any platform management flags that need setting because of
         * Steam integration. Defaults to none.
         */
        EOS_EIntegratedPlatformManagementFlags integrated_platform_management_flags = EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_Disabled;

        bool try_get_library_path(std::filesystem::path& library_path) const
        {
            if (exists(_override_library_path))
            {
                library_path = absolute(_override_library_path);
                return true;
            }

            if (exists(_library_path))
            {
                library_path = absolute(_library_path);
                return true;
            }

            return false;
        }

        /**
         * \brief The path that try_get_library_path checks for the Steam
         * library: the override path if one was configured, otherwise the
         * default path next to this module.
         */
        const std::filesystem::path& get_library_path() const
        {
            return _library_path;
        }

        /**
         * \brief Whether the config shows an intent to use Steam (a Steam SDK
         * version, management flags, Steam API interface versions, or an
         * override library path was set).
         */
        bool is_steam_configured() const
        {
            return steam_sdk_major_version > 0 ||
                integrated_platform_management_flags != EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_Disabled ||
                !_steam_api_interface_versions_array.empty() ||
                _has_override_library_path;
        }

        std::string get_steam_api_interface_versions_array() const
        {
            // For each element in the array (each of which is a string of an api version information)
            // iterate across each character, and at the end of a string add a null terminator \0
            // then add one more null terminator at the end of the array
            std::vector<char> steamApiInterfaceVersionsAsCharArray;

            for (const auto& currentFullValue : _steam_api_interface_versions_array)
            {
                for (char currentCharacter : currentFullValue)
                {
                    steamApiInterfaceVersionsAsCharArray.push_back(currentCharacter);
                }

                steamApiInterfaceVersionsAsCharArray.push_back('\0');
            }
            steamApiInterfaceVersionsAsCharArray.push_back('\0');

            return steamApiInterfaceVersionsAsCharArray.data();
        }

        bool is_managed_by_application() const
        {
            return static_cast<std::underlying_type_t<EOS_EIntegratedPlatformManagementFlags>>(integrated_platform_management_flags &
                EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedByApplication);
        }

        bool is_managed_by_sdk() const
        {
            return static_cast<std::underlying_type_t<EOS_EIntegratedPlatformManagementFlags>>(integrated_platform_management_flags &
                EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedBySDK);
        }

    private:
        std::filesystem::path _library_path;
        std::filesystem::path _override_library_path;
        std::vector<std::string> _steam_api_interface_versions_array;
        bool _has_override_library_path = false;

        explicit SteamConfig() : Config("eos_steam_config.json"),
            steam_sdk_major_version(0),
            steam_sdk_minor_version(0)
        {
            _library_path = io_helpers::get_path_relative_to_current_module(STEAM_SDK_DLL_NAME);
        }

        void parse_json_element(const std::string& name, json_value_s& value) override
        {
            if (name == "overrideLibraryPath")
            {
                const auto override_library_path_element = json_value_as_string(&value);
                if (override_library_path_element == nullptr)
                {
                    return;
                }
                const char* path = override_library_path_element->string;
                if (!string_helpers::is_empty_or_whitespace(path))
                {
                    _library_path = path;
                    _has_override_library_path = true;
                }
            }
            else if (name == "steamSDKMajorVersion")
            {
                steam_sdk_major_version = parse_number<uint32_t>(value);
            }
            else if (name == "steamSDKMinorVersion")
            {
                steam_sdk_minor_version = parse_number<uint32_t>(value);
            }
            else if (name == "steamApiInterfaceVersionsArray")
            {
                _steam_api_interface_versions_array = parse_json_array<std::string>(value);
            }
            else if (name == "integratedPlatformManagementFlags")
            {
                integrated_platform_management_flags =
                    parse_flags<EOS_EIntegratedPlatformManagementFlags>(
                        &config_legacy::INTEGRATED_PLATFORM_MANAGEMENT_FLAGS_STRINGS_TO_ENUM,
                        EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_Disabled,
                        &value);
            }
        }

        friend struct Config;
    };
}

#endif
