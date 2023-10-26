using DG.Tweening;
using geniikw.DataRenderer2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ArrowLine : MonoBehaviour
{
    public UILine Line;

    public Image StartTriangle;
    public Image EndTriangle;

    public AnimationCurve TriangleScale;

    public float ShowTime = 3f;

    public bool IsCenter = false;

    [Header("Data")]
    [Range(0, 1f)]
    public float LineTVal = 1;
    public float BeforeLineTVal = 0;

    public void Show()
    {
        BeforeLineTVal = 0;
        LineTVal = 0;
        DOTween.To(() => LineTVal, x => LineTVal = x, 1, ShowTime).SetEase(Ease.InSine);
    }
    public void Hide()
    {

    }

    public Vector3 GetPosition(float t)
    {
        return Line.line.GetPosition(t);
    }

    public void SetColor(Color color)
    {
        Line.line.option.color.colorKeys = new GradientColorKey[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) };
        Line.GeometyUpdateFlagUp();
        if (StartTriangle != null)
        {
            StartTriangle.color = color;
        }
        if (EndTriangle != null)
        {
            EndTriangle.color = color;
        }
    }



    private void Update()
    {
        if (LineTVal != BeforeLineTVal)
        {
            if (LineTVal == 0)
            {
                if (StartTriangle != null)
                {
                    StartTriangle.gameObject.SetActive(false);
                }
                if (EndTriangle != null)
                {
                    EndTriangle.gameObject.SetActive(false);
                }
            }
            else if (BeforeLineTVal == 0)
            {
                if (StartTriangle != null)
                {
                    StartTriangle.gameObject.SetActive(true);
                }
                if (EndTriangle != null)
                {
                    EndTriangle.gameObject.SetActive(true);
                }
            }
            BeforeLineTVal = LineTVal;



            if(IsCenter) //양방향
            {
                Line.line.option.startRatio = 0.5f - LineTVal * 0.5f;
                Line.line.option.endRatio = 0.5f + LineTVal * 0.5f;
                Line.GeometyUpdateFlagUp();
            }
            else //단방향
            {
                Line.line.option.startRatio = 0;
                Line.line.option.endRatio = LineTVal;
                Line.GeometyUpdateFlagUp();
            }

            if (StartTriangle != null)
            {
                StartTriangle.transform.parent.localPosition = Line.line.GetPosition(0.001f);
                var next = Line.line.GetPosition(0f);
                var dir = StartTriangle.transform.parent.localPosition - next;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                StartTriangle.transform.parent.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                StartTriangle.transform.localScale = Vector3.one * TriangleScale.Evaluate(LineTVal);
            }


            if (EndTriangle != null)
            {
                EndTriangle.transform.parent.localPosition = Line.line.GetPosition(0.995f);
                var next = Line.line.GetPosition(1f);
                var dir = EndTriangle.transform.parent.localPosition - next;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                EndTriangle.transform.parent.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                EndTriangle.transform.localScale = Vector3.one * TriangleScale.Evaluate(LineTVal);
            }
        }
    }
}
