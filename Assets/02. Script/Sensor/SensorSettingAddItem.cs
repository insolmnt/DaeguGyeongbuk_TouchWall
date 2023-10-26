using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorSettingAddItem : MonoBehaviour
{
    public SensorModel Type;
    public Toggle SelectToggle;

    public GameObject SubPanel;
    public InputField SubInputField;
    public SliderCtr SubSlider;

    public bool IsOn
    {
        get
        {
            return SelectToggle.isOn;
        }
        set
        {
            SelectToggle.isOn = value;
            if (SubPanel != null)
            {
                SubPanel.gameObject.SetActive(value);
            }
        }
    }
    public void OnChange()
    {
        if (SubPanel != null)
        {
            SubPanel.gameObject.SetActive(IsOn);
        }
    }
}
