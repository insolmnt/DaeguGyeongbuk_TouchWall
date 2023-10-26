using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurveMaskImageSettingItem : MonoBehaviour
{
    public CurveMaskImage MaskImage;
    public CurveSetting CurveSetting;
    public SliderCtr ImageWidthSlider;
    public SliderCtr ImageHeightSlider;

    public Toggle ReverseToggle;
    public Toggle ShowLeftToggle;
    public Toggle ShowRIghtToggle;

    public SliderCtr TopCurveFadeSlider;
    public SliderCtr BottomCurveFadeSlider;

    [Header("Setting UI")]
    public Image ColorImage;
    private bool mIsLoad = false;
    private bool IsCurveSetting = false;
    private int CurrentKeyDotSize = 2;

    public void SetUI()
    {
        mIsLoad = false;
        ColorImage.color = MaskImage.Data.MaskColor;
        ImageWidthSlider.Val = MaskImage.Data.ImageWidth;
        ImageHeightSlider.Val = MaskImage.Data.ImageHeight;

        ReverseToggle.isOn = MaskImage.Data.IsReverse;

        ShowLeftToggle.isOn = MaskImage.Data.ShowLeft;
        ShowRIghtToggle.isOn = MaskImage.Data.ShowRight;

        TopCurveFadeSlider.Val = MaskImage.Data.TopCurveFade;
        BottomCurveFadeSlider.Val = MaskImage.Data.BottomCurveFade;
        mIsLoad = true;
    }

    public void OnDataChange()
    {
        if(mIsLoad == false)
        {
            return;
        }

        MaskImage.Data.IsReverse = ReverseToggle.isOn;

        MaskImage.Data.ImageWidth = (int)ImageWidthSlider.Val;
        MaskImage.Data.ImageHeight = (int)ImageHeightSlider.Val;


        MaskImage.Data.TopCurveFade = TopCurveFadeSlider.Val;
        MaskImage.Data.BottomCurveFade = BottomCurveFadeSlider.Val;

        MaskImage.SetData(MaskImage.Data);
    }

    public void OnShowToggleChange()
    {
        if(mIsLoad == false)
        {
            return;
        }

        MaskImage.Data.ShowLeft = ShowLeftToggle.isOn;
        MaskImage.Data.ShowRight = ShowRIghtToggle.isOn;

        MaskImage.SetImageUvRect();
    }

    public void OnTopCurveSettingButtonClick()
    {
        IsCurveSetting = true;
        SettingManager.Instance.SettingPanel.transform.localScale = Vector3.zero;
        MaskImage.SetMaskTexture(CurrentKeyDotSize);
        CurveSetting.ShowCurveSetting("상단 커브 설정", MaskImage.Data.TopCurve, (result) =>
        {
            MaskImage.Data.TopCurve = result;
            MaskImage.SetMaskTexture(CurrentKeyDotSize);
        }, true
        , () =>
        {
            IsCurveSetting = false;
            MaskImage.SetMaskTexture();
            SettingManager.Instance.SettingPanel.transform.localScale = Vector3.one;
        });
    }
    public void OnBottomCurveSettingButtonClick()
    {
        IsCurveSetting = true;
        SettingManager.Instance.SettingPanel.transform.localScale = Vector3.zero;
        MaskImage.SetMaskTexture(CurrentKeyDotSize);
        CurveSetting.ShowCurveSetting("하단 커브 설정", MaskImage.Data.BottomCurve, (result) =>
        {
            MaskImage.Data.BottomCurve = result;
            MaskImage.SetMaskTexture(CurrentKeyDotSize);
        }, true
        , () =>
        {
            IsCurveSetting = false;
            MaskImage.SetMaskTexture();
            SettingManager.Instance.SettingPanel.transform.localScale = Vector3.one;
        });
    }
    public void OnColorChangeButtonClick()
    {
        ColorSetting.Instance.ShowSetting("마스크 색상 설정", MaskImage.Data.MaskColor, true, (result) =>
        {
            MaskImage.Data.MaskColor = result;
            MaskImage.SetMaskTexture();
            SetUI();
        });
    }

    private void Update()
    {
        if (IsCurveSetting)
        {
            if(Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                CurrentKeyDotSize++;
                MaskImage.SetMaskTexture(CurrentKeyDotSize);
            }
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                CurrentKeyDotSize = Mathf.Max(CurrentKeyDotSize - 1, 0);
                MaskImage.SetMaskTexture(CurrentKeyDotSize);
            }
        }
    }
}
