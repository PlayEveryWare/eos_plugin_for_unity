using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using System.Runtime.InteropServices;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

namespace PlayEveryWare.EpicOnlineServices
{
    public partial class EOSManager
    {
        //-------------------------------------------------------------------------
        //TODO: Hook up correctly for each platform?
        static string GetTempDir()
        {
            return Application.temporaryCachePath;
        }


        static private Int32 IsReadyForNetworkActivity()
        {
            return 1;
        }

        //-------------------------------------------------------------------------
        // Nothing to be done on windows for the moment
        static private void ConfigureSystemInitOptions(ref InitializeOptions initializeOptions)
        {
        }
    }
}
#endif

