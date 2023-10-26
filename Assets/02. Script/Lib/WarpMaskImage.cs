using Fenderrio.ImageWarp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RawImageWarp))]
public class WarpMaskImage : MonoBehaviour
{
    public MaskObjectManager Manager;

    public RawImageWarp WarpImage;

    [Header("Data")]
    public WarpMaskImageData Data;
    public float PositionX
    {
        get
        {
            return Data.Keystone.PointList[2].x + Data.Display * 2000;
        }
    }


    public void SetDisplay(int display)
    {
        Data.Display = display;

        WarpImage.rectTransform.SetParent(Manager.WarpMaskImageParent[Data.Display]);
        WarpImage.transform.localScale = Vector3.one;
        WarpImage.transform.localPosition = Vector3.zero;
        WarpImage.transform.localEulerAngles = Vector3.zero;
    }
    public void SetKeystone()
    {
        if(WarpImage == null)
        {
            return;
        }
        var image = WarpImage;
        var data = Data.Keystone;

        image.cornerOffsetTR = data.PointList[0];
        image.cornerOffsetBR = data.PointList[1];
        image.cornerOffsetBL = data.PointList[2];
        image.cornerOffsetTL = data.PointList[3];

        var canvas = GetComponentInParent<Canvas>();
        Debug.Log("Canvs Size : " + canvas.renderingDisplaySize);
        float width = 800f / canvas.renderingDisplaySize.y * canvas.renderingDisplaySize.x;
        float height = 800f;

        //Top A왼 B오
        //Right A위 B아래
        //Bottom A오 B왼
        //Left A아래  B위

        var top = new Vector3(
                (width + image.cornerOffsetTR.x - image.cornerOffsetTL.x) / 3f,
                (image.cornerOffsetTR.y - image.cornerOffsetTL.y) / 3f,
                0);
        var left = new Vector3(
            (image.cornerOffsetTL.x - image.cornerOffsetBL.x) / 3f,
            (height + image.cornerOffsetTL.y - image.cornerOffsetBL.y) / 3f,
            0);
        var right = new Vector3(
            (image.cornerOffsetBR.x - image.cornerOffsetTR.x) / 3f,
            -(height + image.cornerOffsetTR.y - image.cornerOffsetBR.y) / 3f,
            0);
        var bottom = new Vector3(
            -(width + image.cornerOffsetBR.x - image.cornerOffsetBL.x) / 3f,
            -(image.cornerOffsetBR.y - image.cornerOffsetBL.y) / 3f,
            0);


        image.topBezierHandleA = (Vector2)top + data.BezierList[0];
        image.topBezierHandleB = -(Vector2)top + data.BezierList[1];

        image.leftBezierHandleA = (Vector2)left + data.BezierList[6];
        image.leftBezierHandleB = -(Vector2)left + data.BezierList[7];

        image.rightBezierHandleA = (Vector2)right + data.BezierList[2];
        image.rightBezierHandleB = -(Vector2)right + data.BezierList[3];

        image.bottomBezierHandleA = (Vector2)bottom + data.BezierList[4];
        image.bottomBezierHandleB = -(Vector2)bottom + data.BezierList[5];
    }

    public void SetData(WarpMaskImageData data) 
    {
        Data = data;

        WarpImage.color = Data.ImageColor;

        SetDisplay(Data.Display);
        SetKeystone();
    }

    [ContextMenu("Test")]
    private void Test()
    {
        SetData(Data);
    }

}

[System.Serializable]
public class WarpMaskImageData
{
    public int Display = 0;
    public Color ImageColor = Color.blue;

    public bool isOnltyRectangle = true;
    public MonitorKeystoneData Keystone = new MonitorKeystoneData()
    {
        PointList = new Vector2[]
        {
            new Vector2(-500, -250),
            new Vector2(-500, 250),
            new Vector2(500, 250),
            new Vector2(500, -250)
        }
    };
}