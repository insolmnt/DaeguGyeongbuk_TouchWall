using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthSensorTopView : MonoBehaviour
{
    public DepthSensor Sensor;
    public ScreenType Screen;

    public Camera CoordCamera;

    [Header("Data")]
    public DepthSensorTopViewData Data;
    public Monitor CurrentMonitor
    {
        get
        {
            return ScreenManager.Instance.ScreenList[(int)Screen];
        }
    }


    private float TouchWidth;
    private float TouchHeight;

    private float TouchScreenStartX;
    private float TouchScreenStartY;
    private float TouchScreenWidth;
    private float TouchScreenHeight;

    private ushort[] DepthList;
    public void ChangeScreenData()
    {
        TouchScreenWidth = (CurrentMonitor.ScreenWidth * (1f - Data.ScreenRight - Data.ScreenLeft));
        TouchScreenHeight = (CurrentMonitor.ScreenHeight * (1f - Data.ScreenTop - Data.ScreenBottom));
        TouchScreenStartX = (CurrentMonitor.ScreenWidth * Data.ScreenLeft);
        TouchScreenStartY = (CurrentMonitor.ScreenHeight * Data.ScreenBottom);
    }
    public void SetData()
    {
        Data.ScreenLeft = Mathf.Clamp(Data.ScreenLeft, 0, 1f);
        Data.ScreenRight = Mathf.Clamp(Data.ScreenRight, 0, 1f);
        Data.ScreenBottom = Mathf.Clamp(Data.ScreenBottom, 0, 1f);
        Data.ScreenTop = Mathf.Clamp(Data.ScreenTop, 0, 1f);

        TouchWidth = Data.TouchRight - Data.TouchLeft;
        TouchHeight = Data.TouchTop - Data.TouchBottom;

        ChangeScreenData();
        SetDepthCamera();
        SetYData();


    }
    //직선 기울기
    public float Slope = 1;
    public void SetYData()
    {
        Slope = (float)(Data.YRight - Data.YLeft) / (Sensor.DepthWidth - 1);
    }
    public int GetY(int x)
    {
        return (int)(x * Slope + Data.YLeft);
    }
    public void SetDepthCamera()
    {
        CoordCamera.transform.localEulerAngles = new Vector3(0, Data.Angle, 0);
        CoordCamera.transform.localPosition = new Vector3(Data.CameraPositionX, 0, Data.CameraPositionY);
    }

    public Vector2 GetPositon(int x, int y, int index)
    {
        try
        {
            var v3 = CoordCamera.ViewportToWorldPoint(new Vector3(x / (float)Sensor.DepthWidth, y / (float)Sensor.DepthHeight, Sensor.DepthData[index])) * Data.Scale * 0.05f;
            return new Vector2(Data.IsXFlip ? -v3.x : v3.x, Data.IsYFlip ? -v3.z : v3.z);
        }
        catch (System.Exception e)
        {
            Debug.LogError("" + x + ", " + y + " -> " + index + " / "+ Sensor.DepthData.Length);
            return Vector2.zero;
        }

        //var v3 = DepthCamera.ViewportToWorldPoint(new Vector3(x / (float)Sensor.DepthWidth, 0.5f, Sensor.DepthRawList[index])) * Data.Scale * 0.05f;
        //return new Vector2(Data.IsXFlip ? -v3.x : v3.x, Data.IsYFlip ? -v3.z : v3.z);
    }



    public int TouchCount = 0;
    private TouchData[] InputDataList = new TouchData[640];
    public int InputIndex = 0;
    private int TouchIndex = 0;
    private TouchData[] TouchDataList = new TouchData[640];
    public void DepthChange(ushort[] depthList)
    {
        DepthList = depthList;
        CurrentMonitor.OnSensorInputStart?.Invoke();


        InputIndex = 0;
        TouchIndex = 0;


        for (int i = 0; i < Sensor.DepthWidth; i++)
        {
            int y_ = GetY(i);
            if (y_ < 0 || y_ >= Sensor.DepthHeight)
            {
                continue;
            }

            int index = y_ * Sensor.DepthWidth + i;
            if (depthList[index] == 0)
            {
                continue;
            }

            var v2 = GetPositon(i, y_, index);
            float x = v2.x;
            float y = v2.y;

            if (x < Data.TouchLeft ||
                x > Data.TouchRight ||
                y < Data.TouchBottom ||
                y > Data.TouchTop)
                continue;


            x = TouchScreenStartX + TouchScreenWidth * (x - Data.TouchLeft) / TouchWidth;
            y = TouchScreenStartY + TouchScreenHeight * (y - Data.TouchBottom) / TouchHeight;

            InputDataList[InputIndex++] = new TouchData() { vector = new Vector2(x, y), index = i, dist = DepthList[index] };
        }



        for (int i = 0; i < InputIndex; i++)
        {
            var data = InputDataList[i];
            bool isIn = false;
            for (int j = 0; j < TouchIndex; j++)
            {
                var touch = TouchDataList[j];
                //연속되는 데이터인 경우(각도)
                if ((Data.GroupMaxCount <= 0 || (Data.GroupMaxCount > touch.count)) && Mathf.Abs(touch.index - data.index) <= Data.GroupAngle)
                {
                    if (data.dist < touch.dist)
                    {
                        touch.vector = data.vector;
                        touch.dist = data.dist;
                    }

                    touch.index = data.index;
                    touch.count++;
                    isIn = true;

                    TouchDataList[j] = touch;
                    break;
                }
            }
            if (isIn == false)
            {
                TouchDataList[TouchIndex++] = new TouchData { vector = data.vector, count = 1, dist = data.dist, index = data.index };
            }
        }


        TouchCount = TouchIndex;

        for (int i = 0; i < TouchIndex; i++)
        {
            if (TouchDataList[i].count > 0)
            {
                CurrentMonitor.SensorInputColor(TouchDataList[i].vector.x, TouchDataList[i].vector.y, Sensor.Data.ViewColor);
            }
        }

        CurrentMonitor.OnSensorInputEnd?.Invoke();
    }


    public void Load()
    {
        SetData();
    }

}
