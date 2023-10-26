using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSetting : MonoBehaviour
{
    static public TextSetting Instance
    {
        get
        {
            if (instance == null)
            {
                var prefab = Resources.Load<TextSetting>("TextSetting");
                if (prefab == null)
                {
                    Debug.Log("NULL!!!!!");
                }
                instance = Instantiate(prefab);
            }
            return instance;
        }
    }
    static private TextSetting instance;

    private Action<string> OnTextChange;

    public GameObject SettingPanel;

    public Text TitleText;
    public InputField TextInput;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideSetting();
        }
    }

    public bool IsShowSetting = false;
    public void ShowSetting(string title, string startText, Action<string> onResult)
    {
        HideSetting();
        SettingPanel.gameObject.SetActive(true);

        if (TitleText != null)
        {
            TitleText.text = title;
        }
        IsShowSetting = true;

        OnTextChange = onResult;

        TextInput.text = startText;
    }

    public void HideSetting()
    {
        IsShowSetting = false;
        SettingPanel.SetActive(false);
    }

    public void OnOkButtonClick()
    {
        if(OnTextChange != null)
        {
            OnTextChange(TextInput.text);
        }

        HideSetting();
    }
}
