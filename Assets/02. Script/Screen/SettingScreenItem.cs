using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingScreenItem : MonoBehaviour
{
    public Monitor Monitor;

    [Header("사용안함이 0번")]
    public Toggle[] DisplayToggleList;

    private void Start()
    {
        int displayCount = 2;

#if !UNITY_EDITOR
        displayCount = Display.displays.Length;
#endif

        for (int i = 0; i < DisplayToggleList.Length; i++)
        {
            DisplayToggleList[i].gameObject.SetActive(i <= displayCount);
        }
    }

    public void SetUi()
    {
        foreach (var toggle in DisplayToggleList)
        {
            toggle.isOn = false;
        }
        DisplayToggleList[Monitor.Data.Display + 1].isOn = true;
    }
}
