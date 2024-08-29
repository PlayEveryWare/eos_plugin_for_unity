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

#if !EOS_DISABLE
using System;
using System.Collections.Generic;
using UnityEngine;
using Epic.OnlineServices.Logging;

namespace PlayEveryWare.EpicOnlineServices
{
    public static class LogLevelUtility
    {
        public static string[] LogCategoryStringArray
        {
            get { return Enum.GetNames(typeof(LogCategory)); }
        }
        public static string[] LogLevelStringArray
        {
            get { return Enum.GetNames(typeof(LogLevel)); }
        }

        public static List<LogLevel> LogLevelList
        {
            get
            {
                LogLevelConfig logLevelConfig = null;
                try
                {
                    logLevelConfig = Config.Get<LogLevelConfig>();
                }
                catch (System.IO.FileNotFoundException)
                {
                    Debug.Log($"Log level config does not exist, using default");
                    return null; 
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    Debug.Log($"Exception when reading log level config, using default");
                    return null;
                }

                List<LogLevel> logLevels = new List<LogLevel>();
                for (int i = 0; i < LogCategoryStringArray.Length - 1; i++)
                {
                    if (Enum.TryParse(logLevelConfig.LogCategoryLevelPairs[i].Level, out LogLevel parsedLogLevel))
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
#endif