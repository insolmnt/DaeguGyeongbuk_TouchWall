using Coffee.UIExtensions;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class AreaColor : MonoBehaviour
{
    public UIDissolve FadeImage;
    public Texture2D FirstFadeTexture;
    public Texture2D SecondFadeTexture;

    public Image TextImage;

    public CanvasGroup WaitImage;

    [Header("Data")]
    [Range(0, 1f)]
    public float TVal = 0;
    public float BeforeTVal = 0;
    public void Clear()
    {
        WaitImage.alpha = 0;
        TextImage.color = new Color(TextImage.color.r, TextImage.color.g, TextImage.color.b, 0);
    }

    public void SetColor(Color mainColor, Color fadeColor)
    {
        FadeImage.GetComponent<Image>().color = mainColor;
        FadeImage.color = fadeColor;
    }


    [ContextMenu("Show")]
    public void Show()
    {
        StartCoroutine(ShowEvent());
    }
    private IEnumerator ShowEvent()
    {
        BeforeTVal = 0;
        TVal = 0;

        WaitImage.alpha = 0;
        TextImage.color = new Color(TextImage.color.r, TextImage.color.g, TextImage.color.b, 0);

        FadeImage.effectFactor = 1;
        FadeImage.noiseTexture = FirstFadeTexture;


        DOTween.To(() => TVal, x => TVal = x, 1, 2f).SetEase(Ease.InSine);

        yield return new WaitForSeconds(2.5f);

        WaitImage.DOFade(1, 2f);
        TextImage.transform.localScale = Vector3.one * 1.5f;
        TextImage.transform.DOScale(Vector3.one, 0.5f);
        TextImage.color = new Color(TextImage.color.r, TextImage.color.g, TextImage.color.b, 0);
        TextImage.DOColor(new Color(TextImage.color.r, TextImage.color.g, TextImage.color.b, 1), 0.5f);
    }
    [ContextMenu("Hide")]
    public void Hide()
    {
        WaitImage.DOFade(0, 2f);
        TextImage.DOColor(new Color(1, 1, 1, 0), 2f);
    }

    private void Update()
    {
        if (TVal != BeforeTVal)
        {
            if(TVal <= 0.5f && BeforeTVal > 0.5f)
            {
                FadeImage.noiseTexture = FirstFadeTexture;
            }
            else if (TVal > 0.5f && BeforeTVal < 0.5f)
            {
                FadeImage.noiseTexture = SecondFadeTexture;
            }
            BeforeTVal = TVal;

            if(TVal <= 0.5f)
            {
                FadeImage.effectFactor = 1 - TVal * 2f;
            }
            else
            {
                FadeImage.effectFactor = (TVal - 0.5f) * 2f;
            }
        }
    }
}
