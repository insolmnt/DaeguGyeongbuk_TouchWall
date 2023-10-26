using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSetting : MonoBehaviour
{
    static public ColorSetting Instance
    {
        get
        {
            if (instance == null)
            {
                var prefab = Resources.Load<ColorSetting>("ColorSetting");
                if (prefab == null)
                {
                    Debug.LogError("ColorSetting NULL!!!!!");
                    return null;
                }
                instance = Instantiate(prefab);
            }
            return instance;
        }
    }
    static private ColorSetting instance;

    private Action<Color> OnColorChange;

    public GameObject SettingPanel;

    public Text TitleText;

    public Gradient ColorGradient;
    public RawImage ColorGradientImage;
    public Slider ColorGradientSlider;
    private Texture2D ColorGradientTexture;

    public SliderCtr ColorRSlider;
    public SliderCtr ColorGSlider;
    public SliderCtr ColorBSlider;
    public SliderCtr ColorASlider;

    public InputField ColorField;
    public Color CurrentColor;
    public Image CurrentColorImage;

    private bool mIsColorLoad = true;
    private bool mIsColorChange = false;
    private Color StartColor;
    public void SetColor(Color color)
    {
        mIsColorLoad = false;
        ColorRSlider.Val = color.r;
        ColorGSlider.Val = color.g;
        ColorBSlider.Val = color.b;
        ColorASlider.Val = color.a;

        if (ColorField != null)
        {
            ColorField.text = ColorUtility.ToHtmlStringRGBA(color);
        }

        CurrentColor = color;
        CurrentColorImage.color = new Color(CurrentColor.r, CurrentColor.g, CurrentColor.b, 1);
        mIsColorLoad = true;
    }

    private void OnEnable()
    {
        if (ColorGradientTexture == null)
        {
            ColorGradientTexture = new Texture2D(1024, 1, TextureFormat.ARGB32, false);

            for (int i = 0; i < ColorGradientTexture.width; i++)
            {
                ColorGradientTexture.SetPixel(i, 0, ColorGradient.Evaluate((float)i / (ColorGradientTexture.width - 1)));
            }
            ColorGradientTexture.Apply();
            if (ColorGradientImage != null)
            {
                ColorGradientImage.texture = ColorGradientTexture;
            }
        }
    }

    private bool mIsDefault = false;
    public void OnColorGradientSliderChange()
    {
        if (mIsDefault)
        {
            return;
        }

        var color = ColorGradient.Evaluate(ColorGradientSlider.value);
        SetColor(new Color(color.r, color.g, color.b, CurrentColor.a));
        if (OnColorChange != null)
        {
            OnColorChange(CurrentColor);
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        HideSetting();
    //    }
    //}

    public void OnColorInputFieldChange()
    {
        if (mIsColorLoad == false || mIsColorChange == true)
        {
            return;
        }

        var color = CurrentColor;
        if (ColorUtility.TryParseHtmlString(ColorField.text, out color))
        {
            SetColor(color);
            if (OnColorChange != null)
            {
                OnColorChange(CurrentColor);
            }
        }
        else
        {
            //ColorField.text = ColorUtility.ToHtmlStringRGBA(CurrentColor);
        }
    }

    static public bool IsShowSetting = false;
    public void ShowSetting(string title, Color startColor, bool useAlpha, Action<Color> onResult)
    {
        HideSetting();

        ColorASlider.gameObject.SetActive(useAlpha);

        mIsDefault = true;
        if (TitleText != null)
        {
            TitleText.text = title;
        }
        IsShowSetting = true;

        OnColorChange = onResult;
        SettingPanel.SetActive(true);

        ColorField.Select();
        if (ColorGradientSlider != null)
        {
            ColorGradientSlider.value = 0;
        }

        StartColor = startColor;
        SetColor(startColor);
        mIsDefault = false;
    }

    public void HideSetting()
    {
        IsShowSetting = false;
        SettingPanel.SetActive(false);
    }

    public void OnColorSliderChange()
    {
        if (mIsColorLoad == false)
        {
            return;
        }
        mIsColorChange = true;
        CurrentColor = new Color(ColorRSlider.Val, ColorGSlider.Val, ColorBSlider.Val, ColorASlider.Val);


        CurrentColorImage.color = new Color(CurrentColor.r, CurrentColor.g, CurrentColor.b, 1);
        if (OnColorChange != null)
        {
            OnColorChange(CurrentColor);
        }

        if (ColorField != null)
        {
            ColorField.text = ColorUtility.ToHtmlStringRGBA(CurrentColor);
        }

        mIsColorChange = false;
    }
}
