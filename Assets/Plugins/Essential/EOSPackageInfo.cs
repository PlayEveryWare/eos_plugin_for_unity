/*
* Copyright (c) 2021 PlayEveryWare
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

public static class EOSPackageInfo
{
    /// <value>Hard-coded configuration file name ("EpicOnlineServicesConfig.json")</value>
    public static readonly string ConfigFileName = "EpicOnlineServicesConfig.json";
    public const string UnknownVersion = "?.?.?";

    //-------------------------------------------------------------------------

    public static string GetPackageName()
    {
        return "com.playeveryware.eos";
    }

    //-------------------------------------------------------------------------
    //VERSION START
    private struct VersionReader
    {
        public string version;
    }
    public static string GetPackageVersion()
    {
        try
        {
            var pathToManifest = System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "EOSUnityPlugin_package_template", "package.json");
            var contents = System.IO.File.ReadAllText(pathToManifest);
            var versionData = UnityEngine.JsonUtility.FromJson<VersionReader>(contents);
            return versionData.version;
        }
        catch
        {
            var versionAsset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("eosPluginVersion");
            if (versionAsset != null)
            {
                return versionAsset.text;
            }
            else
            {
                return UnknownVersion;
            }
        }
    }
    //VERSION END
}
