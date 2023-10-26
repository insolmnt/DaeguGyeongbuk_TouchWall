using SoftMasking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitorMask : MonoBehaviour
{
    public ScreenManager Manager;
    public Monitor Monitor;


    [Header("카메라 rawTexture")]
    public RawImage CurrentRawImage;

    [Header("마스크 이미지")]
    public SoftMask MaskRawImage;


    static public int MaskWidth = 1280 * 2;
    static public int MaskHeight = 800 * 2;
    [Header("Data")]
    public Texture2D MaskTexture;
    public float BeforeFadeLeft = 0;
    public float BeforeFadeRight = 0;
    public float BeforeFadeUp = 0;
    public float BeforeFadeDown = 0;
    public float BeforeCutLeft = 0;
    public float BeforeCutRight = 0;
    public float BeforeCutTop = 0;
    public float BeforeCutBottom = 0;

    [ContextMenu("Test")]
    private void TestSetData()
    {
        SetData(false);
    }

    public void SetData(bool isCheck = true)
    {
        //Debug.Log(name + " 마스크 이미지 설정");

        var fadeUp = Monitor.Data.FadeTop;
        var fadeDown = Monitor.Data.FadeBottom;
        var fadeLeft = Monitor.Data.FadeLeft;
        var fadeRight = Monitor.Data.FadeRight;

        var cutTop = Monitor.Data.CutTop;
        var cutRight = Monitor.Data.CutRight;
        var cutBottom = Monitor.Data.CutBottom;
        var cutLeft = Monitor.Data.CutLeft;


        if (MaskRawImage.gameObject.activeSelf == false)
        {
            return;
        }
        Debug.Log(name + " 마스크 설정");


        if (isCheck &&
           BeforeFadeLeft == fadeLeft &&
           BeforeFadeRight == fadeRight &&
           BeforeFadeUp == fadeUp &&
           BeforeFadeDown == fadeDown &&
           BeforeCutLeft == cutLeft &&
           BeforeCutRight == cutRight &&
           BeforeCutTop == cutTop &&
           BeforeCutBottom == cutBottom)
        {
            return;
        }
        BeforeFadeLeft = fadeLeft;
        BeforeFadeRight = fadeRight;
        BeforeFadeUp = fadeUp;
        BeforeFadeDown = fadeDown;
        BeforeCutLeft = cutLeft;
        BeforeCutRight = cutRight;
        BeforeCutTop = cutTop;
        BeforeCutBottom = cutBottom;



        if (MaskTexture == null || MaskTexture.width != MaskWidth || MaskTexture.height != MaskHeight)
        {
            MaskTexture = new Texture2D(MaskWidth, MaskHeight, TextureFormat.ARGB32, false);
            MaskRawImage.texture = MaskTexture;
        }

        var colors = new Color[MaskWidth * MaskHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        float leftStart = 0;
        if (cutLeft > 0)
        {
            leftStart = cutLeft * MaskWidth;
            for (int x = 0; x < leftStart; x++)
            {
                for (int y = 0; y < MaskHeight; y++)
                {
                    int index = x + y * MaskWidth;
                    colors[index] = new Color(1, 1, 1, 0);
                }
            }
        }
        Debug.Log("leftStart : " + leftStart);
        if (fadeLeft > 0)
        {
            float len = fadeLeft * MaskWidth;
            Debug.Log("fadeLeft len : " + len);
            int start = (int)Mathf.Floor(leftStart);
            var curve = Monitor.Data.CurveLeft;


            for (int x = start; x < start + len; x++)
            {
                float alpha = curve.Evaluate(Mathf.Clamp(Mathf.Lerp(0, 1f, (x - start) / len), 0, 1));

                for (int y = 0; y < MaskHeight; y++)
                {
                    int index = x + y * MaskWidth;
                    colors[index] = new Color(1, 1, 1, alpha);
                }
            }
        }

        float rightEnd = MaskWidth;
        if (cutRight > 0)
        {
            rightEnd = MaskWidth - cutRight * MaskWidth;

            for (int x = (int)Mathf.Floor(rightEnd); x < MaskWidth; x++)
            {
                for (int y = 0; y < MaskHeight; y++)
                {
                    int index = x + y * MaskWidth;
                    colors[index] = new Color(1, 1, 1, 0);
                }
            }
        }
        Debug.Log("rightEnd : " + rightEnd);

        if (fadeRight > 0)
        {
            float len = fadeRight * MaskWidth;
            Debug.Log("fadeRight len : " + len);


            var curve = Monitor.Data.CurveRight;

            int start = (int)Mathf.Floor(rightEnd - len);
            for (int x = start; x < rightEnd; x++)
            {
                float alpha = curve.Evaluate(Mathf.Clamp(Mathf.Lerp(0, 1f, 1 - (x - start) / len), 0, 1));
                for (int y = 0; y < MaskHeight; y++)
                {
                    int index = x + y * MaskWidth;
                    colors[index] = new Color(1, 1, 1, alpha);
                }
            }
        }


        int topEnd = MaskHeight;
        if (cutTop > 0)
        {
            topEnd = (int)Mathf.Floor(MaskHeight - cutTop * MaskHeight);
            for (int x = 0; x < MaskWidth; x++)
            {
                for (int y = topEnd; y < MaskHeight; y++)
                {
                    int index = x + y * MaskWidth;
                    colors[index] = new Color(1, 1, 1, 0);
                }
            }
        }
        Debug.Log("topEnd : " + topEnd);
        if (fadeUp > 0)
        {
            float len = fadeUp * MaskHeight;
            int startY = (int)Mathf.Floor(topEnd - len);
            Debug.Log("fadeUp starY : " + startY); 
            var curve = Monitor.Data.CurveTop;

            for (int y = startY; y < topEnd; y++)
            {
                float alpha = curve.Evaluate(Mathf.Clamp(Mathf.Lerp(0, 1f, 1 - (y - startY) / len), 0, 1));
                for (int x = 0; x < MaskWidth; x++)
                {
                    int index = x + y * MaskWidth;
                    if (colors[index].a > alpha)
                        colors[index] = new Color(1, 1, 1, alpha);
                }
            }
        }

        float bottomStart = 0;
        if (cutBottom > 0)
        {
            bottomStart = cutBottom * MaskHeight;
            for (int x = 0; x < MaskWidth; x++)
            {
                for (int y = 0; y < bottomStart; y++)
                {
                    int index = x + y * MaskWidth;
                    colors[index] = new Color(1, 1, 1, 0);
                }
            }
        }
        Debug.Log("bottomStart : " + bottomStart);
        if (fadeDown > 0)
        {
            float len = fadeDown * MaskHeight;
            int start = (int)Mathf.Floor(bottomStart);
            var curve = Monitor.Data.CurveBottom;

            for (int y = start; y < start + len; y++)
            {
                float alpha = curve.Evaluate(Mathf.Clamp(Mathf.Lerp(0, 1f, (y - start) / len), 0, 1));

                for (int x = 0; x < MaskWidth; x++)
                {
                    int index = x + y * MaskWidth;
                    if (colors[index].a > alpha)
                        colors[index] = new Color(1, 1, 1, alpha);
                }
            }
        }
        MaskTexture.SetPixels(colors);
        MaskTexture.Apply();
    }
}
