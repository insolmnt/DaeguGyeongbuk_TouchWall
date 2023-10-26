using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingScreen : Setting
{
    public ScreenManager Manager;
    public SettingScreenItem[] ItemList;

    public CurveSetting CurveSetting;

    [Header("세부설정")]
    public GameObject DetailSettingPanel;
    public SliderCtr CutTopSlider;
    public SliderCtr CutRightSlider;
    public SliderCtr CutBottomSlider;
    public SliderCtr CutLeftSlider;
    public SliderCtr FadeTopSlider;
    public SliderCtr FadeRightSlider;
    public SliderCtr FadeBottomSlider;
    public Image[] ColorImage;
    public SliderCtr FadeLeftSlider;


    [Header("키스콘 설정")]
    public GameObject KeystoneLinePanel;
    public VerticalLayoutGroup KeystoneLineVertical;
    public HorizontalLayoutGroup KeystoneLineHorizontal;
    public Image ScreenTopLine;
    public Image ScreenBottomLine;

    [Header("키스콘 설정")]
    public Canvas UICanvas;
    public Camera UICamera;

    [Header("Data")]
    public bool IsShowSetting;
    public float KeystoneLineSpacing = 50;
    public int CurrentSetting = -1;
    public override void Init()
    {
        Load();
    }

    public override void Load()
    {
        Manager.Load();

        foreach (var item in ItemList)
        {
            item.SetUi();
        }
    }

    public override void Save()
    {
        Manager.Save();
    }

    private void SetLineSpacting(float spacing)
    {
        KeystoneLineSpacing = Mathf.Max(spacing, 5);
        KeystoneLineVertical.spacing = KeystoneLineSpacing;
        KeystoneLineHorizontal.spacing = KeystoneLineSpacing;
    }

    private bool mIsShow = false;
    public override void Show(bool isShow)
    {
        mIsShow = false;
        IsShowSetting = isShow;
        CurveSetting.OnCloseButtonClick();
        CloseDetaileSetting();

        if (isShow)
        {
        }
        else
        {
            ShowKeystoneLine(false);
            if (CurrentSetting >= 0)
            {
                OnKeystoneSettingClick(CurrentSetting);
            }
        }

        mIsShow = true;
    }
    private void ShowKeystoneLine(bool isShow)
    {
        KeystoneLinePanel.SetActive(isShow);
        foreach (var monitor in Manager.ScreenList)
        {
            monitor.ShowKeystoneLine(isShow);
        }

        if (isShow)
        {
            float dist = ((float)Manager.Data.ScreenHeight / Manager.Data.ScreenWidth * 16) * 20;
            ScreenTopLine.transform.localPosition = new Vector3(0, dist, 0);
            ScreenBottomLine.transform.localPosition = new Vector3(0, -dist, 0);
        }
    }

    private bool mIsDetaileSettingShow = false;
    private Monitor DetaileSettingCurrentMonitor = null;
    public void ShowDetaileSetting(SettingScreenItem item)
    {
        DetailSettingPanel.SetActive(true);
        var monitor = item.Monitor;
        DetaileSettingCurrentMonitor = monitor;

        mIsDetaileSettingShow = false;
        CutTopSlider.Val = monitor.Data.CutTop;
        CutRightSlider.Val = monitor.Data.CutRight;
        CutBottomSlider.Val = monitor.Data.CutBottom;
        CutLeftSlider.Val = monitor.Data.CutLeft;
        FadeTopSlider.Val = monitor.Data.FadeTop;
        FadeRightSlider.Val = monitor.Data.FadeRight;
        FadeBottomSlider.Val = monitor.Data.FadeBottom;
        FadeLeftSlider.Val = monitor.Data.FadeLeft;
        for(int i=0; i<4; i++)
        {
            ColorImage[i].color = monitor.Data.ScreenColor[i];
        }

        mIsDetaileSettingShow = true;
    }
    public void OnDetaileSettingColorButtonClick(int index)
    {
        ColorSetting.Instance.ShowSetting("색상설정", DetaileSettingCurrentMonitor.Data.ScreenColor[index], false, (color) =>
        {
            DetaileSettingCurrentMonitor.Data.ScreenColor[index] = color;
            ColorImage[index].color = color;
            DetaileSettingCurrentMonitor.SetColorData();
        });
    }
    public void CloseDetaileSetting()
    {
        DetailSettingPanel.SetActive(false);
        DetaileSettingCurrentMonitor = null;
    }
    public void OnDetaileSettingDataChange()
    {
        if (mIsDetaileSettingShow == false)
        {
            return;
        }

        DetaileSettingCurrentMonitor.Data.CutTop = CutTopSlider.Val;
        DetaileSettingCurrentMonitor.Data.CutRight = CutRightSlider.Val;
        DetaileSettingCurrentMonitor.Data.CutBottom = CutBottomSlider.Val;
        DetaileSettingCurrentMonitor.Data.CutLeft = CutLeftSlider.Val;
        DetaileSettingCurrentMonitor.Data.FadeTop = FadeTopSlider.Val;
        DetaileSettingCurrentMonitor.Data.FadeRight = FadeRightSlider.Val;
        DetaileSettingCurrentMonitor.Data.FadeBottom = FadeBottomSlider.Val;
        DetaileSettingCurrentMonitor.Data.FadeLeft = FadeLeftSlider.Val;
        DetaileSettingCurrentMonitor.Mask.SetData();
    }
    public void DetaileSettingCurveChangeButtonClick(int index)
    {
        if(DetaileSettingCurrentMonitor == null)
        {
            return;
        }

        if (index == 0)
        {
            CurveSetting.ShowCurveSetting("위쪽 페이드 강도 설정", DetaileSettingCurrentMonitor.Data.CurveTop, (result) =>{
                DetaileSettingCurrentMonitor.Data.CurveTop = result;
                DetaileSettingCurrentMonitor.Mask.SetData(false);
            });
        }
        else if (index == 1)
        {
            CurveSetting.ShowCurveSetting("오른쪽 페이드 강도 설정", DetaileSettingCurrentMonitor.Data.CurveRight, (result) => {
                DetaileSettingCurrentMonitor.Data.CurveRight = result;
                DetaileSettingCurrentMonitor.Mask.SetData(false);
            });
        }
        else if(index == 2)
        {
            CurveSetting.ShowCurveSetting("아래쪽 페이드 강도 설정", DetaileSettingCurrentMonitor.Data.CurveBottom, (result) => {
                DetaileSettingCurrentMonitor.Data.CurveBottom = result;
                DetaileSettingCurrentMonitor.Mask.SetData(false);
            });
        }
        else if(index == 3)
        {
            CurveSetting.ShowCurveSetting("왼쪽 페이드 강도 설정", DetaileSettingCurrentMonitor.Data.CurveLeft, (result) => {
                DetaileSettingCurrentMonitor.Data.CurveLeft = result;
                DetaileSettingCurrentMonitor.Mask.SetData(false);
            });
        }
    }
    private bool mIsMaskNull = false;
    private void SetMaskNull(bool isNull)
    {
        mIsMaskNull = isNull;
        foreach (var item in ItemList)
        {
            item.Monitor.Mask.MaskRawImage.texture = isNull ? null : item.Monitor.Mask.MaskTexture;
        }
    }

    public void OnKeystoneSettingClick(int index)
    {
        bool isClose = CurrentSetting == index;

        SettingManager.Instance.SettingPanel.gameObject.SetActive(isClose);
        SetMaskNull(!isClose);

        foreach (var item in ItemList)
        {
            item.Monitor.ShowKeystoneSetting(false);
        }

        if (CurrentSetting >= 0)
        {
            ItemList[CurrentSetting].Monitor.Save();
        }

        foreach (var item in ItemList)
        {
            item.Monitor.MonitorCanvas.sortingOrder = 100;
        }
        if (isClose)
        {
            CurrentSetting = -1;
        }
        else
        {
            CurrentSetting = index;
            ItemList[index].Monitor.MonitorCanvas.sortingOrder = 200;
            ItemList[index].Monitor.ShowKeystoneSetting(true);

        }
    }


    public void OnSliderChange()
    {
        if(mIsShow == false)
        {
            return;
        }
    }
    public void OnCurveSettingButtonClick()
    {
        CurveSetting.ShowCurveSetting("좌측-우측 페이드 밝기 설정", Manager.ScreenList[0].Data.CurveRight, (result) =>
        {
            Manager.ScreenList[0].Data.CurveRight = result;
            Manager.ScreenList[1].Data.CurveLeft = result;
            Manager.ScreenList[0].Mask.SetData(false);
            Manager.ScreenList[1].Mask.SetData(false);

            Manager.ScreenList[0].Save();
            Manager.ScreenList[1].Save();
        });
    }

    public void OnDisplayToggleChange(SettingScreenItem item)
    {
        if (mIsShow == false)
        {
            return;
        }

        for (int i = 0; i < item.DisplayToggleList.Length; i++)
        {
            if (item.DisplayToggleList[i].isOn)
            {
                item.Monitor.Data.Display = i - 1;
                if (item.Monitor.Data.Display >= 0 && Display.displays.Length > item.Monitor.Data.Display)
                {
                    Display.displays[item.Monitor.Data.Display].Activate();
                }
                break;
            }
        }

        item.Monitor.SetData();
        item.Monitor.Save();

        Manager.DisplayChange();
    }



    private void Update()
    {
        if(IsShowSetting == false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowKeystoneLine(!KeystoneLinePanel.gameObject.activeSelf);
        }

        if (Input.GetKey(KeyCode.PageDown))
        {
            SetLineSpacting(KeystoneLineSpacing - Time.deltaTime * 30f);
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            SetLineSpacting(KeystoneLineSpacing + Time.deltaTime * 30f);
        }


        if (Input.GetKeyDown(KeyCode.F5))
        {
            OnKeystoneSettingClick(0);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            OnKeystoneSettingClick(1);
        }

        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            SetMaskNull(!mIsMaskNull);
        }
    }
}
