using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.InventoryGame
{
    /// <summary>
    /// Handles initializing which canvas to show in the beginning and how to change between them.
    /// </summary>
    public class InventoryScreenManager : MonoBehaviour
    {
        [SerializeField] private GameObject loginScreen;
        [SerializeField] private GameObject inventoryGame;
        [SerializeField] private GameObject leaderboard;
        [Space]
        [SerializeField] private GameObject startingScreen;

        private void Start()
        {
            loginScreen.SetActive(startingScreen == loginScreen);
            inventoryGame.SetActive(startingScreen == inventoryGame);
            leaderboard.SetActive(startingScreen == leaderboard);
        }

        /// <summary>
        /// Called when logging out to hide the game and show the login screen.
        /// </summary>
        public void ShowLogin()
        {
            loginScreen.SetActive(true);
            inventoryGame.SetActive(false);
            leaderboard.SetActive(false);
        }

        /// <summary>
        /// Called when logging in to show the game.
        /// </summary>
        public void ShowGame()
        {
            loginScreen.SetActive(false);
            inventoryGame.SetActive(true);
            leaderboard.SetActive(false);
        }

        public void ShowLeaderboard()
        {
            loginScreen.SetActive(false);
            inventoryGame.SetActive(false);
            leaderboard.SetActive(true);
        }
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(InventoryScreenManager))]
    public class InventoryScreenManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var obj = (InventoryScreenManager)target;
            if (GUILayout.Button("Show Login"))
            {
                obj.ShowLogin();
            }
            if (GUILayout.Button("Show Inventory Game"))
            {
                obj.ShowGame();
            }
            if (GUILayout.Button("Show Leaderboard"))
            {
                obj.ShowLeaderboard();
            }

            DrawDefaultInspector();
        }
    }

#endif
}
