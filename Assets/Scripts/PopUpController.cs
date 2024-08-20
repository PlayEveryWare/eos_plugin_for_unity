using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpController : MonoBehaviour
{
    public GameObject popupPanel;

    public GameObject blurPanel;


    // Start is called before the first frame update
    void Start()
    {
        Hide();
    }

    public void Show()
    {
        Debug.Log("Show");
        popupPanel.SetActive(true);
        blurPanel.SetActive(true);
    }

    private void Hide()
    {
        popupPanel.SetActive(false);
        blurPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
