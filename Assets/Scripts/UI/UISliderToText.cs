using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISliderToText : MonoBehaviour
{
    public Slider sliderObject;
    public TextMeshProUGUI SliderText;

    public void updateText()
    {
        SliderText.SetText(sliderObject.value.ToString());
    }
}
