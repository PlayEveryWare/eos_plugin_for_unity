using UnityEditor;
using PlayEveryWare.EpicOnlineServices;

// Ensure class initializer is called whenever scripts recompile
[InitializeOnLoad]
public static class PlayModeDetection
{
    // Register an event handler when the class is initialized
    static PlayModeDetection()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        // If play mode ended in the editor, shutdown the EOSManager as OnApplicationQuit isn't called in the editor.
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EOSManager.Instance.OnShutdown();
        }
    }
}
