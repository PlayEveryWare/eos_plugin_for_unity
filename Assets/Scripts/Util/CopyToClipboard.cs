using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CopyToClipboard : MonoBehaviour
{
    public Text Source;

    public void CopyOnClick()
    {
        Debug.LogFormat("Copy to clipboard: {0}", Source.text);
        GUIUtility.systemCopyBuffer = Source.text;
    }
}
