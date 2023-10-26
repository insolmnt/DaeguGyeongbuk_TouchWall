using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurveMaskImage : MonoBehaviour
{
    public RawImage[] OutRawImageList;

    [Header("Data")]
    public CurveMaskImageData Data;
    public Texture2D MaskTexture;

    private void CreateMaskTexture()
    {
        if(MaskTexture != null && MaskTexture.width == Data.ImageWidth && MaskTexture.height == Data.ImageHeight)
        {
            return;
        }

        if(MaskTexture != null)
        {
            DestroyImmediate(MaskTexture);
        }

        MaskTexture = new Texture2D(Data.ImageWidth, Data.ImageHeight, TextureFormat.ARGB32, false);

        foreach(var image in OutRawImageList)
        {
            image.texture = MaskTexture;
        }
    }

    public void SetImageUvRect()
    {
        if(OutRawImageList.Length != 2)
        {
            return;
        }

        var left = OutRawImageList[0];
        var right = OutRawImageList[1];

        left.gameObject.SetActive(Data.ShowLeft);
        right.gameObject.SetActive(Data.ShowRight);

        if(Data.ShowLeft && Data.ShowRight)
        {
            left.uvRect = new Rect(0, 0, 0.5f, 1f);
            right.uvRect = new Rect(0.5f, 0, 0.5f, 1f);
        }
        else
        {
            left.uvRect = new Rect(0, 0, 1f, 1f);
            right.uvRect = new Rect(0, 0, 1f, 1f);
        }
    }


    public void SetMaskTexture(int keyDotSize = 0)
    {
        Color[] colors = new Color[Data.ImageWidth * Data.ImageHeight];

        Color maskColor = Data.MaskColor;
        Color clearColor = new Color(Data.MaskColor.r, Data.MaskColor.g, Data.MaskColor.b, 0);
        if (Data.IsReverse)
        {
            maskColor = new Color(Data.MaskColor.r, Data.MaskColor.g, Data.MaskColor.b, 0);
            clearColor = Data.MaskColor;
        }
        for (int x = 0; x< Data.ImageWidth; x++)
        {
            float top = Data.TopCurve.Evaluate((float)x / Data.ImageWidth);
            float bottom = Data.BottomCurve.Evaluate((float)x / Data.ImageWidth);

            for(int y=0; y< Data.ImageHeight; y++)
            {
                float yVal = (float)y / Data.ImageHeight;
               
                if(yVal < top && yVal > bottom)
                {
                    float tVal = Data.ImageHeight * Data.TopCurveFade * 0.0001f;
                    float bVal = Data.ImageHeight * Data.BottomCurveFade * 0.0001f;
                    float tStart = top - tVal;
                    float bEnd = bottom + bVal;
                    if (yVal > tStart)
                    {
                        float alpha = 1 - (yVal - tStart) / tVal;
                        if (Data.IsReverse)
                        {
                            alpha = 1 - alpha;
                        }
                        colors[x + y * Data.ImageWidth] = new Color(Data.MaskColor.r, Data.MaskColor.g, Data.MaskColor.b, alpha);
                    }
                    else if (yVal < bEnd)
                    {
                        float alpha = 1 - (bEnd - yVal) / tVal;
                        if (Data.IsReverse)
                        {
                            alpha = 1 - alpha;
                        }
                        colors[x + y * Data.ImageWidth] = new Color(Data.MaskColor.r, Data.MaskColor.g, Data.MaskColor.b, alpha);
                    }
                    else
                    {
                        colors[x + y * Data.ImageWidth] = maskColor;
                    }
                }
                else
                {
                    colors[x + y * Data.ImageWidth] = clearColor;
                }
            }
        }


        //커브 키값마다 점 찍기
        if(keyDotSize > 0)
        {
            foreach (var key in Data.TopCurve.keys)
            {
                ShowKeyDotColor(colors, (int)(Data.ImageWidth * key.time), (int)(Data.ImageHeight * key.value), keyDotSize);
            }
            foreach (var key in Data.BottomCurve.keys)
            {
                ShowKeyDotColor(colors, (int)(Data.ImageWidth * key.time), (int)(Data.ImageHeight * key.value), keyDotSize);
            }
        }


        MaskTexture.SetPixels(colors);
        MaskTexture.Apply();
    }
    private void ShowKeyDotColor(Color[] colors, int x, int y, int size)
    {
        for(int i = -size; i<=size; i++)
        {
            for(int j = -size; j<=size; j++)
            {
                if(x + i >= 0 && x + i < Data.ImageWidth && y + j >= 0 && y + j < Data.ImageHeight)
                {
                    colors[x + i + (y + j) * Data.ImageWidth] = Color.red;
                }
            }
        }
    }



    public void SetData(CurveMaskImageData data)
    {
        Data = data;

        CreateMaskTexture();
        SetImageUvRect();
        SetMaskTexture();
    }

    [ContextMenu("SetData")]
    private void Test()
    {
        SetData(Data);
    }
}


[System.Serializable]
public class CurveMaskImageData
{
    public Color MaskColor = Color.black;

    public int ImageWidth = 1920;
    public int ImageHeight = 1080;

    public bool ShowLeft = true;
    public bool ShowRight = true;

    public float TopCurveFade = 0;
    public float BottomCurveFade = 0;

    public AnimationCurve TopCurve = new AnimationCurve { keys = new Keyframe[] { 
        new Keyframe() { time = 0, value = 1, inTangent = 0, outTangent = 0 }, 
        new Keyframe() { time = 1, value = 1, inTangent = 0, outTangent = 0 } } };
    public AnimationCurve BottomCurve = new AnimationCurve { keys = new Keyframe[] { 
        new Keyframe() { time = 0, value = 0, inTangent = 0, outTangent = 0 }, 
        new Keyframe() { time = 1, value = 0, inTangent = 0, outTangent = 0 } } };

    public bool IsReverse = true;
}
