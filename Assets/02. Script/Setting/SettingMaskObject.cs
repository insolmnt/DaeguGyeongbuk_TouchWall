using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingMaskObject : Setting
{
    public MaskObjectManager Manager;
    public CurveMaskImageSettingItem MainMask;
    public CurveMaskImageSettingItem MiddleMask;

    [Header("Data")]
    private bool IsShowSetting = false;

    public override void Init()
    {
        Load();
    }

    public override void Show(bool isShow)
    {
        IsShowSetting = isShow;
        if (isShow)
        {
            MainMask.SetUI();
            MiddleMask.SetUI();
        }
        else
        {
        }
    }

    public override void Load()
    {
        Manager.Load();
    }

    public override void Save()
    {
        Manager.Save();
    }

    private void Update()
    {
        if (IsShowSetting == false)
        {
            return;
        }

        if(CurveSetting.Instance != null && CurveSetting.Instance.gameObject.activeSelf)
        {
            return;
        }
    }
}
