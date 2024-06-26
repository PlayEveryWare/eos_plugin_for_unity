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

#if UNITY_EDITOR

// Uncomment the following line to enable the log filter defined below
//#define ENABLE_LOG_FILTER

#if ENABLE_LOG_FILTER

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;


    public class LogUtility
    {
        private class FilterableLogHandler : ILogHandler
        {
            private readonly ILogHandler defaultLogHandler = Debug.unityLogger.logHandler;

            private static readonly string[] FilterTerms = new string[]
            {
                "<Redacted>" // This is added to filter out log messages from EOS SDK that contain redacted information and clutter log windows.
            };

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                string message = string.Format(format, args);

                foreach (string term in FilterTerms)
                {
                    if (message.Contains(term))
                        return;
                }

                defaultLogHandler.LogFormat(logType, context, format, args);
            }

            public void LogException(Exception exception, Object context)
            {
                defaultLogHandler.LogException(exception, context);
            }
        }

        private class LogHandlerHookInitializer
        {
            static LogHandlerHookInitializer()
            {
                Debug.unityLogger.logHandler = new FilterableLogHandler();
            }

            public static void InitializeHook()
            {
                // Warn the user that the log filter is active
                Debug.LogWarning("CURRENT LOG IS BEING FILTERED");
            }
        }

        // This guarantees that the log filter class is set as the default log
        // handler as early in the application lifetime as possible.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void HookLogHandler()
        {
            LogHandlerHookInitializer.InitializeHook();
        }
    }
}
#endif
#endif