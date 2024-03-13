/*
* Copyright (c) 2023 PlayEveryWare
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

using UnityEditor;
using UnityEditor.Build.Reporting;

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    using System.Linq;

    public class ScriptingDefineUtility
    {
        private static bool DoesScriptingDefineExist(string scriptingDefineSymbols, string defineAsString)
        {
            return scriptingDefineSymbols.Split(';').Any(define => define == defineAsString);
        }

        //-------------------------------------------------------------------------
        private static bool DoesScriptingDefineExist(BuildReport report, string defineAsString)
        {
#if UNITY_2021_2_OR_NEWER
            var namedBuildTarget =
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(report.summary.platformGroup);
            var defineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            // Ensure that we don't do checks for config files if the current platform group has EOS disabled
            var defineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(report.summary.platformGroup);
#endif
            return DoesScriptingDefineExist(defineSymbolsForGroup, defineAsString);
        }

        //-------------------------------------------------------------------------
        private static bool DoesScriptingDefineExist(BuildTarget buildTarget, string defineAsString)
        {
#if UNITY_2021_2_OR_NEWER
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            UnityEditor.Build.NamedBuildTarget namedBuildTarget =
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            var defineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var defineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif

            return DoesScriptingDefineExist(defineSymbolsForGroup, defineAsString);
        }

        //-------------------------------------------------------------------------
        public static bool IsEOSDisabled(BuildReport report)
        {
            return ScriptingDefineUtility.DoesScriptingDefineExist(report, "EOS_DISABLE");
        }

        //-------------------------------------------------------------------------
        public static bool IsEOSDisabled(BuildTarget buildTarget)
        {
            return ScriptingDefineUtility.DoesScriptingDefineExist(buildTarget, "EOS_DISABLE");
        }
    }
}
