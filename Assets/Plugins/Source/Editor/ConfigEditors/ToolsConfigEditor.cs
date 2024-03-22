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

namespace PlayEveryWare.EpicOnlineServices.Editor
{
    using Config;
    using Utility;

    public class ToolsConfigEditor : ConfigEditor<ToolsConfig>
    {
        public ToolsConfigEditor() : base("Tools") { }

        public override void RenderContents()
        {
            string pathToIntegrityTool = (config.pathToEACIntegrityTool);
            string pathToIntegrityConfig = (config.pathToEACIntegrityConfig);
            string pathToEACCertificate = (config.pathToEACCertificate);
            string pathToEACPrivateKey = (config.pathToEACPrivateKey);
            string pathToEACSplashImage = (config.pathToEACSplashImage);
            string bootstrapOverideName = (config.bootstrapperNameOverride);
            bool useEAC = config.useEAC;

            GUIEditorUtility.AssigningPath("Path to EAC Integrity Tool", ref pathToIntegrityTool, "Select EAC Integrity Tool",
                tooltip: "EOS SDK tool used to generate EAC certificate from file hashes");
            GUIEditorUtility.AssigningPath("Path to EAC Integrity Tool Config", ref pathToIntegrityConfig, "Select EAC Integrity Tool Config",
                tooltip: "Config file used by integry tool. Defaults to anticheat_integritytool.cfg in same directory.", extension: "cfg", labelWidth: 200);
            GUIEditorUtility.AssigningPath("Path to EAC private key", ref pathToEACPrivateKey, "Select EAC private key", extension: "key",
                tooltip: "EAC private key used in integrity tool cert generation. Exposing this to the public will comprimise anti-cheat functionality.");
            GUIEditorUtility.AssigningPath("Path to EAC Certificate", ref pathToEACCertificate, "Select EAC public key", extension: "cer",
                tooltip: "EAC public key used in integrity tool cert generation");
            GUIEditorUtility.AssigningPath("Path to EAC splash image", ref pathToEACSplashImage, "Select 800x450 EAC splash image PNG", extension: "png",
                tooltip: "EAC splash screen used by launcher. Must be a PNG of size 800x450.");

            GUIEditorUtility.AssigningBoolField("Use EAC", ref useEAC, tooltip: "If set to true, uses the EAC");
            GUIEditorUtility.AssigningTextField("Bootstrapper Name Override", ref bootstrapOverideName, labelWidth: 180, tooltip: "Name to use instead of 'Bootstrapper.exe'");

            config.pathToEACIntegrityTool = pathToIntegrityTool;
            config.pathToEACIntegrityConfig = pathToIntegrityConfig;
            config.pathToEACPrivateKey = pathToEACPrivateKey;
            config.pathToEACCertificate = pathToEACCertificate;
            config.pathToEACSplashImage = pathToEACSplashImage;
            config.useEAC = useEAC;
            config.bootstrapperNameOverride = bootstrapOverideName;
        }
    }
}