using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DepthSensorBackView : MonoBehaviour
{
    public DepthSensor Sensor;
    public ScreenType Screen;

    public Monitor CurrentMonitor
    {
        get
        {
            return ScreenManager.Instance.ScreenList[(int)Screen];
        }
    }

    [Header("Data")]
    public DepthSensorBackViewData Data;
    public int StartX = 100;
    public int EndX = 400;
    public int StartY = 50;
    public int EndY = 400;

    public int ViewPortDepthWidth;
    public int ViewPortDepthHeight;


    private MatOfPoint2f ViewPortPointSrc;
    private MatOfPoint2f ViewPortPointDes;
    public Mat ViewPortPointTransformMat = null;

    public ScreenCheckData[] ScreenCheckList;

    public List<MatOfPoint> srcContours = new List<MatOfPoint>();
    public Point[] ContoursPointList;

    private Texture2D TouchTexture;
    private Color32[] TouchColors;
    private ushort[] DepthRawList;
    //private RenderTexture DepthRenderTexture;

    private Mat DepthRawMat;


    //터치 좌표를 만들기 위한 스크린 관련 데이터
    public float TouchScreenStartX;
    public float TouchScreenStartY;
    public float TouchScreenWidth;
    public float TouchScreenHeight;

    public RawImage TestRawImage;

    public void SetData()
    {
        StartX = (int)Mathf.Min(Data.TouchDepthPoints[0].x, Data.TouchDepthPoints[1].x, Data.TouchDepthPoints[2].x, Data.TouchDepthPoints[3].x);
        StartY = (int)Mathf.Min(Data.TouchDepthPoints[0].y, Data.TouchDepthPoints[1].y, Data.TouchDepthPoints[2].y, Data.TouchDepthPoints[3].y);
        EndX = (int)Mathf.Max(Data.TouchDepthPoints[0].x, Data.TouchDepthPoints[1].x, Data.TouchDepthPoints[2].x, Data.TouchDepthPoints[3].x);
        EndY = (int)Mathf.Max(Data.TouchDepthPoints[0].y, Data.TouchDepthPoints[1].y, Data.TouchDepthPoints[2].y, Data.TouchDepthPoints[3].y);

        ViewPortDepthWidth = EndX - StartX + 1;
        ViewPortDepthHeight = EndY - StartY + 1;


        TouchTexture = new Texture2D(ViewPortDepthWidth, ViewPortDepthHeight, TextureFormat.RGB24, false);
        TouchColors = new Color32[ViewPortDepthWidth * ViewPortDepthHeight];
        //        Debug.Log("TouchTexture : " + TouchTexture.width + " / " + TouchTexture.height);

        DepthRawMat = new Mat(ViewPortDepthHeight, ViewPortDepthWidth, CvType.CV_8UC1);

        if(TestRawImage != null)
        {
            TestRawImage.texture = TouchTexture;
        }

        ViewPortPointSrc = new MatOfPoint2f(new Point[] {
                    new Point(Data.TouchDepthPoints[0].x - StartX, EndY - Data.TouchDepthPoints[0].y),
                    new Point(Data.TouchDepthPoints[1].x - StartX, EndY - Data.TouchDepthPoints[1].y),
                    new Point(Data.TouchDepthPoints[2].x - StartX, EndY - Data.TouchDepthPoints[2].y),
                    new Point(Data.TouchDepthPoints[3].x - StartX, EndY - Data.TouchDepthPoints[3].y)});

        ViewPortPointDes = new MatOfPoint2f(new Point[] {
                    new Point(0, 0),
                    new Point(ViewPortDepthWidth, 0),
                    new Point(ViewPortDepthWidth, ViewPortDepthHeight),
                    new Point(0, ViewPortDepthHeight)
        });

        ViewPortPointTransformMat = Imgproc.getPerspectiveTransform(ViewPortPointSrc, ViewPortPointDes);

        //DepthRenderTexture = new RenderTexture(ViewPortDepthWidth, ViewPortDepthHeight, 24);
        //DepthRenderTexture.enableRandomWrite = true;
        //DepthRenderTexture.wrapMode = TextureWrapMode.Repeat;
        //DepthRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        //DepthRenderTexture.Create();

        //Debug.Log("OnDepthSettingChange End");
    }


    public void ChangeScreenData()
    {
        TouchScreenWidth = (CurrentMonitor.ScreenWidth * (1f - Data.ScreenRight - Data.ScreenLeft));
        TouchScreenHeight = (CurrentMonitor.ScreenHeight * (1f - Data.ScreenTop - Data.ScreenBottom));
        TouchScreenStartX = (CurrentMonitor.ScreenWidth * Data.ScreenLeft);
        TouchScreenStartY = (CurrentMonitor.ScreenHeight * Data.ScreenBottom);
    }

    public void DepthChange(ushort[] depthList)
    {
        DepthRawList = depthList;

        CurrentMonitor.OnSensorInputStart?.Invoke();

        for (int i = 0; i < ScreenCheckList.Length; i++)
        {
            ScreenCheckList[i].depth = DepthRawList[i];
        }

        for (int x = StartX; x <= EndX; x++)
        {
            for (int y = StartY; y <= EndY; y++)
            {
                int index = y * Sensor.DepthWidth + x;
                bool isOut = x < 0 || x >= Sensor.DepthWidth || y < 0 || y >= Sensor.DepthHeight;

                //깊이 기본값 체크
                if (isOut == false && DepthRawList[index] > 0 && Data.ScreenCheckRate > 0)
                {
                    if (ScreenCheckList[index].count >= Data.ScreenCheckRate)
                    {
                        ScreenCheckList[index].count = 0;

                        float avg = ScreenCheckList[index].sum / Data.ScreenCheckRate;
                        ScreenCheckList[index].sum = 0;
                        //평균값과 현재 값을 비교하여 minDist 넘게 차이가 난다면 디폴트값을 평균값으로 수정
                        float avgDiff = Mathf.Abs(avg - Sensor.SaveFloorDefault[index]);
                        float currentDiff = Mathf.Abs(DepthRawList[index] - Sensor.SaveFloorDefault[index]);
                        if (avgDiff < Data.ScreenCheckMaxDist && currentDiff < Data.ScreenCheckMaxDist)//
                        {
                            //    Debug.Log("기본값 수정 : " + x + ", " + y + " - " + CheckFloorDefault[index] + " -> " + avg + " / " + SaveFloorDefault[index]);
                            ScreenCheckList[index].checkScreen = DepthRawList[index];// (ushort)avg;
                        }
                    }
                    else
                    {
                        ScreenCheckList[index].sum += DepthRawList[index];
                        ScreenCheckList[index].count++;
                    }
                }


                float depth = 0;
                if (isOut == false)
                    depth = ScreenCheckList[index].checkScreen - DepthRawList[index];
                //계산 후 depth가 크면 가까운거, 작으면 먼거 (값이 작으면 스크린과 가까움)

                int touchIndex = (x - StartX) + (y - StartY) * ViewPortDepthWidth;

                ////벽이랑 가까울수록 흰색으로 (값이 클수록 흰색)
                //if(DepthRawList[index] > 0)
                //{
                //    byte color = (byte)((1 - (float)(DepthRawList[index] - MinDist) / MaxDist) * 255f);
                //    TouchColors[touchIndex] = new Color32(color, color, color, 255);
                //}
                //else
                //{
                //    TouchColors[touchIndex] = new Color32(255, 255, 255, 255);
                //}

                if (isOut)
                {
                    TouchColors[touchIndex] = DepthSensor.ColorBlack;
                    continue;
                }

                if ((depth >= Data.MinDist && (Data.IsMaxDistUse == false || depth <= Data.MaxDist) && DepthRawList[index] > 0))
                {
                    if(Data.IsTouchCenter == false && Data.IsOutlineAllTouch == false)
                    {
                        byte color = (byte)(10 + 245 * (1 - depth / (Data.MaxDist - Data.MinDist)));
                        TouchColors[touchIndex] = new Color32(color, color, color, 255);
                    }
                    else
                    {
                        TouchColors[touchIndex] = DepthSensor.ColorWhite;
                    }
                }
                else
                {
                    TouchColors[touchIndex] = DepthSensor.ColorBlack;
                }
            }
        }

        TouchTexture.SetPixels32(TouchColors);
        TouchTexture.Apply();

        //KinectSensorManager.Instance.Setting.TestRawImage.texture = TouchTexture;

        //Utils.textureToTexture2D(DepthRenderTexture, TouchTexture);
        Utils.texture2DToMat(TouchTexture, DepthRawMat);


        //if(KinectSensorManager.Instance.Setting.Test1 > 0)
        //    Imgproc.adaptiveThreshold(DepthRawMat, DepthRawMat, 255, Imgproc.ADAPTIVE_THRESH_GAUSSIAN_C, Imgproc.THRESH_BINARY , 
        //        KinectSensorManager.Instance.Setting.Test1, KinectSensorManager.Instance.Setting.Test2);

        Mat srcHierarchy = new Mat();
        MatOfInt4 mConvexityDefectsMatOfInt4 = new MatOfInt4();
        MatOfInt convexHullMatOfInt = new MatOfInt();
        List<Point> convexHullPointArrayList = new List<Point>();
        MatOfPoint convexHullMatOfPoint = new MatOfPoint();
        List<MatOfPoint> convexHullMatOfPointArrayList = new List<MatOfPoint>();
        if (Data.IsErosionNextExpansion)
        {
            if (Data.ErosionVal > 0)
            {
                Imgproc.erode(DepthRawMat, DepthRawMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(Data.ErosionVal, Data.ErosionVal)));
            }
            if (Data.ExpansionVal > 0)
            {
                Imgproc.dilate(DepthRawMat, DepthRawMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(Data.ExpansionVal, Data.ExpansionVal)));
            }
        }
        else
        {
            if (Data.ExpansionVal > 0)
            {
                Imgproc.dilate(DepthRawMat, DepthRawMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(Data.ExpansionVal, Data.ExpansionVal)));
            }
            if (Data.ErosionVal > 0)
            {
                Imgproc.erode(DepthRawMat, DepthRawMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(Data.ErosionVal, Data.ErosionVal)));
            }
        }

        if (ViewPortPointTransformMat != null)
        {
            Imgproc.warpPerspective(DepthRawMat, DepthRawMat, ViewPortPointTransformMat,
                new Size(ViewPortDepthWidth, ViewPortDepthHeight));
        }

        //KinectSensorManager.Instance.Setting.TestRawImage2.texture = TouchTexture2;
        srcContours.Clear();
        Imgproc.findContours(DepthRawMat, srcContours, srcHierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE); //Imgproc.RETR_CCOMP
        /*
        검색 방법 (ContourRetrieval.*)
ContourRetrieval.CComp : 모든 컨투어을 검색하여 두 레벨의 계층 구조로 구성합니다. 최상위 레벨은 구성 요소의 외곽(외부) 경계이고, 두 번째 레벨은 내곽(홀)의 경계입니다.
ContourRetrieval.External : 외곽 컨투어만 검출합니다.
ContourRetrieval.List : 모든 컨투어를 검출하여 list에 넣습니다.
ContourRetrieval.Tree : 모든 컨투어를 검출하여 Tree계층 구조로 만듭니다.

근사화 방법(ContourChain.*)
ContourChain.ApproxNone : 코너점들의 모든점을 반환합니다.
ContourChain.ApproxSimple : 코너점들 단순화 수평, 수직 및 대각선 요소를 끝점만 남겨 둡니다.
ContourChain.Code : 프리먼 체인 코드에서의 컨투어로 적용합니다.
ContourChain.ApproxTC89KCOS, ContourChain.ApproxTC89L1 : Teh - chin 알고리즘 적용합니다.
ContourChain.LinkRuns : 1의 수평 세그먼트를 연결하여 완전히 다른 등고선 검색 알고리즘을 사용합니다.
*/
        int count = 0;
        ContoursPointList = new Point[srcContours.Count];
        for (int i = 0; i < srcContours.Count; i++)
        {
            ContoursPointList[i] = new Point();
            //Debug.Log("" + i + " - " + Imgproc.contourArea(srcContours[i]) + " / " + srcContours.Count);
            var area = Imgproc.contourArea(srcContours[i]);
            if (area < Data.MinArea || (Data.IsMaxAreaUse && area > Data.MaxArea))
            {
                continue;
            }
            count++;
            //
            //


            var list = srcContours[i].toArray();

            if (Data.IsTouchCenter)
            {
                float[] radius = new float[1];
                Imgproc.minEnclosingCircle(new MatOfPoint2f(srcContours[i].toArray()), ContoursPointList[i], radius);

                var po = ViewPortToTouchPoint((int)ContoursPointList[i].x, (int)ContoursPointList[i].y);
                CurrentMonitor.SensorInputColor(po.x, po.y, Sensor.Data.ViewColor);
            }
            else if (Data.IsOutlineAllTouch)
            {
                for (int j = 0; j < list.Length; j += Data.OutlineAllTouchInterval)
                {
                    double[] buff = DepthRawMat.get((int)list[j].y, (int)list[j].x);

                    var po = ViewPortToTouchPoint((int)list[j].x, (int)list[j].y);
                    CurrentMonitor.SensorInputColor(po.x, po.y, Sensor.Data.ViewColor);
                }
            }
            else 
            {
                double maxDist = 0;
                int maxX = 0, maxY = 0;
                for (int j = 0; j < list.Length; j++)
                {
                    double[] buff = DepthRawMat.get((int)list[j].y, (int)list[j].x);

                    if (buff[0] > maxDist)
                    {
                        maxDist = buff[0];
                        maxX = (int)list[j].x;
                        maxY = (int)list[j].y;
                    }
                }

                var po = ViewPortToTouchPoint(maxX, maxY);
                CurrentMonitor.SensorInputColor(po.x, po.y, Sensor.Data.ViewColor);
            }

        }
        CurrentMonitor.OnSensorInputEnd?.Invoke();
    }

    /// <summary>
    /// 뷰포트 내의 좌표를 화면상의 좌표로 변환
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector2 ViewPortToTouchPoint(int x, int y)
    {
        float weightX = Data.XCurve.Evaluate((float)x / ViewPortDepthWidth) * ViewPortDepthWidth;
        float weightY = Data.YCurve.Evaluate((float)y / ViewPortDepthHeight) * ViewPortDepthHeight;
        //return new Vector2(
        //    (IsXFlip ? ScreenWidth - (x) * XRate + ScreenTouchLeft : (x) * XRate + ScreenTouchLeft) * Screen.width / (800 * Screen.width / Screen.height),
        //    (IsYFlip ? (y) * YRate + ScreenTouchBottom : ScreenHeight - (y) * YRate + ScreenTouchBottom) * Screen.height / 800
        //    );

        return new Vector2(
                TouchScreenStartX + (Data.IsXFlip ? (1 - (weightX / (float)ViewPortDepthWidth)) * TouchScreenWidth : weightX / (float)ViewPortDepthWidth * TouchScreenWidth),
                TouchScreenStartY + (!Data.IsYFlip ? (1 - (weightY / (float)ViewPortDepthHeight)) * TouchScreenHeight : weightY / (float)ViewPortDepthHeight * TouchScreenHeight)
                );
    }


    internal void SaveToCheckScreen()
    {
        if (ScreenCheckList == null || ScreenCheckList.Length != Sensor.SaveFloorDefault.Length)
        {
            ScreenCheckList = new ScreenCheckData[Sensor.SaveFloorDefault.Length];
        }


        for (int i = 0; i < Sensor.SaveFloorDefault.Length; i++)
        {
            ScreenCheckList[i].saveScreen = Sensor.SaveFloorDefault[i];
            ScreenCheckList[i].checkScreen = Sensor.SaveFloorDefault[i];
            ScreenCheckList[i].sum = 0;
            ScreenCheckList[i].count = 0;
        }
    }



    public void Load()
    {
        SetData();
        ChangeScreenData();

        SaveToCheckScreen();
    }
}


public struct ScreenCheckData
{
    public float depth;
    public float saveScreen;
    public float checkScreen;
    public float sum;
    public int count;
}
