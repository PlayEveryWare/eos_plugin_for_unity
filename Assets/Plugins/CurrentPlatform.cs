using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Platform
{
#if EOS_PREVIEW_PLATFORM
    public const bool IS_EOS_PREVIEW_ENABLED = true;
#else
    public const bool IS_EOS_PREVIEW_ENABLED = false;
#endif
#if EOS_DISABLE
    public const bool IS_EOS_DISABLED = true;
#else
    public const bool IS_EOS_DISABLED = false;
#endif

#if UNITY_EDITOR
    public const bool IS_EDITOR = true;
#else
    public const bool IS_EDITOR = false;
#endif
#if UNITY_EDITOR_WIN
    public const bool IS_EDITOR_WIN = true;
#else
    public const bool IS_EDITOR_WIN = false;
#endif
#if UNITY_EDITOR_OSX
    public const bool IS_EDITOR_OSX = true;
#else
    public const bool IS_EDITOR_OSX = false;
#endif

#if UNITY_ANDROID
    public const bool IS_ANDROID = true;
#else
    public const bool IS_ANDROID = false;
#endif

#if UNITY_IOS
    public const bool IS_IOS = true;
#else
    public const bool IS_IOS = false;
#endif

#if UNITY_STANDALONE_OSX
    public const bool STANDALONE_OSX = true;
#else
    public const bool STANDALONE_OSX = false;
#endif

#if UNITY_STANDALONE_LINUX
    public const bool STANDALONE_LINUX = true;
#else
    public const bool STANDALONE_LINUX = false;
#endif

#if UNITY_STANDALONE_WIN
    public const bool STANDALONE_WIN = true;
#else
    public const bool STANDALONE_WIN = false;
#endif

}
