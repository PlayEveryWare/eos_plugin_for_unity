using UnityEditor;
using UnityEngine;

public class PlaceholderTextEditorWindow : EditorWindow
{
    private string textFieldValue = "";
    private const string placeholderText = "Enter your text here...";

    [MenuItem("Window/Placeholder Text Example")]
    public static void ShowWindow()
    {
        GetWindow<PlaceholderTextEditorWindow>("Placeholder Text Example");
    }

    private void OnGUI()
    {
        // Set a unique name for the text field control
        GUI.SetNextControlName("MyTextField");

        // Draw the text field
        textFieldValue = EditorGUILayout.TextField(textFieldValue);

        // Check if the text field is empty and not focused
        if (string.IsNullOrEmpty(textFieldValue) && GUI.GetNameOfFocusedControl() != "MyTextField")
        {
            // Get the rect of the last control (the text field)
            Rect textFieldRect = GUILayoutUtility.GetLastRect();

            // Set up the style for the placeholder text
            GUIStyle placeholderStyle = new GUIStyle(GUI.skin.label);
            placeholderStyle.normal.textColor = Color.gray;
            placeholderStyle.fontStyle = FontStyle.Italic;

            // Adjust the position to overlay the placeholder text correctly
            textFieldRect.x += 2;
            textFieldRect.y += 1;

            // Draw the placeholder text over the text field
            EditorGUI.LabelField(textFieldRect, placeholderText, placeholderStyle);
        }
    }
}