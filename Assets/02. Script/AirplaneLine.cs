using DG.Tweening;
using geniikw.DataRenderer2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[ExecuteInEditMode]
public class AirplaneLine : MonoBehaviour
{
    public ArrowLine Line;

    public Image AirplaneImage;
    public Image AirportIconImage;
    public Image AirportTextImage;

    public AnimationCurve AirplaneScale;
    public AnimationCurve AirplaneFade;

    public float ArrowShowTime = 5f;
    public float ShowTime = 5f;

    public CanvasGroup Group;

    [Header("Data")]
    public bool IsShow = false;
    [Range(0, 1f)]
    public float AirplaneTVal = 0;
    public float BeforeAirplaneTVal = 0;

    [Range(0, 1f)]
    public float LineTVal = 1;
    public float BeforeLineTVal = 0;

    private void Start()
    {
        Clear();
    }

    public void Clear()
    {
        BeforeAirplaneTVal = -1;
        AirplaneTVal = 0;

        BeforeLineTVal = -1;
        LineTVal = 0;

        if(AirportIconImage != null)
        {
            AirportIconImage.color = new Color(AirportIconImage.color.r, AirportIconImage.color.g, AirportIconImage.color.b, 0);
        }
        if(AirportTextImage != null)
        {
            AirportTextImage.color = new Color(AirportTextImage.color.r, AirportTextImage.color.g, AirportTextImage.color.b, 0);
        }
    }
    public void SetColor(Color lineColor, Color airplaneColor)
    {
        AirplaneImage.color = airplaneColor;
        Line.SetColor(lineColor);
    }

    [ContextMenu("Show")]
    public void Show()
    {
        StartCoroutine(ShowEvent());
    }
    private IEnumerator ShowEvent()
    {
        IsShow = true;
        Group.alpha = 1;
        Clear();

        DOTween.To(() => Line.LineTVal, x => Line.LineTVal = x, 1, ArrowShowTime).SetEase(Ease.InSine);
        yield return new WaitForSeconds(ArrowShowTime - 0.1f);
        if (AirportIconImage != null)
        {
            ShowImage(AirportIconImage);
        }

        if (AirportTextImage != null)
        {
            ShowImage(AirportTextImage);
        }

        //yield return new WaitForSeconds(1f);

        DOTween.To(() => AirplaneTVal, x => AirplaneTVal = x, 1, ShowTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(ShowTime);

        IsShow = false;
    }
    private void ShowImage(Image image)
    {
        float fadeTime = 1f;

        image.transform.localScale = Vector3.one * 1.5f;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

        image.transform.DOScale(Vector3.one, fadeTime);
        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 1), fadeTime);
    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        Group.DOFade(0, 2f).onComplete = () =>
        {
            Clear();
        };
    }

    private void Update()
    {
        if(AirplaneTVal != BeforeAirplaneTVal)
        {
            BeforeAirplaneTVal = AirplaneTVal;
            AirplaneImage.transform.parent.localPosition = Line.GetPosition(AirplaneTVal);
            var next = Line.GetPosition(AirplaneTVal + 0.01f);

            var dir = AirplaneImage.transform.parent.localPosition - next;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            AirplaneImage.transform.parent.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            AirplaneImage.transform.localScale = Vector3.one * AirplaneScale.Evaluate(AirplaneTVal);
            AirplaneImage.color = new Color(AirplaneImage.color.r, AirplaneImage.color.g, AirplaneImage.color.b, AirplaneFade.Evaluate(AirplaneTVal));
        }


        if (LineTVal != BeforeLineTVal)
        {
            BeforeLineTVal = LineTVal;

            Line.LineTVal = LineTVal;
        }
    }
}
