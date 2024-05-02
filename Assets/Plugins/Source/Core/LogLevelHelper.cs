/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Epic.OnlineServices.Logging;


namespace PlayEveryWare.EpicOnlineServices
{
    public static class LogLevelHelper
    {
        public static string[] LogCategoryStringArray
        {
            get { return Enum.GetNames(typeof(LogCategory)); }
        }
        public static string[] LogLevelStringArray
        {
            get { return Enum.GetNames(typeof(LogLevel)); }
        }

        public static LogLevelConfig LoadLogLevelConfigFileFromPath(string logLevelConfigPath)
        {
            string configDataAsString = "";
#if UNITY_ANDROID && !UNITY_EDITOR
            configDataAsString = AndroidFileIOHelper.ReadAllText(logLevelConfigPath);
#else

            if (!File.Exists(logLevelConfigPath))
            {
                throw new Exception("Couldn't find Log Level Config file: Please ensure log_level_config.json exists and is a valid config");
            }

            configDataAsString = File.ReadAllText(logLevelConfigPath);
#endif
            var configData = JsonUtility.FromJson<LogLevelConfig>(configDataAsString);

            Debug.Log("Loaded log level config file: " + configDataAsString);
            return configData;
        }

        public static List<LogLevel> LogLevelList
        {
            get
            {
                LogLevelConfig logLevelConfig = LoadLogLevelConfigFileFromPath(Path.Combine(Application.streamingAssetsPath, "EOS", "log_level_config.json"));
                
                var logLevels = new List<LogLevel>();
                for (int i = 0; i < LogCategoryStringArray.Length - 1; i++)
                {
                    if (Enum.TryParse(logLevelConfig.logCategoryLevelPairs[i].level, out LogLevel parsedLogLevel))
                    {
                        logLevels.Add(parsedLogLevel);
                    }
                    else
                    {
                        logLevels.Add(LogLevel.Info);
                        Debug.Log("Failed to Parse Log Level");
                    }
                }
                return logLevels;
            }
        }
    }
}