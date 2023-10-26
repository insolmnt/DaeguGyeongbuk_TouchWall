using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidarView : MonoBehaviour
{
    public LidarSensor Sensor;
    public ScreenType Screen;

    [Header("Data")]
    public LidarViewData Data;
    public SensorScreenData ScreenData;

    public Monitor CurrentMonitor
    {
        get
        {
            return ScreenManager.Instance.ScreenList[(int)Screen];
        }
    }


    static public float[] CosData = null;
    static public float[] SinData = null;

    public float TouchWidth;
    public float TouchHeight;
    public void SetData()
    {
        TouchWidth = Data.TouchRight - Data.TouchLeft;
        TouchHeight = Data.TouchTop - Data.TouchBottom;
    }


    private void Start()
    {
        if(SinData == null)
        {
            SinData = new float[360 * 10 + 1];
            CosData = new float[360 * 10 + 1];
            for (int i = 0; i < CosData.Length; i++)
            {
                CosData[i] = Mathf.Cos((i * 0.1f) * Mathf.Deg2Rad);
            }
            for (int i = 0; i < SinData.Length; i++)
            {
                SinData[i] = Mathf.Sin((i * 0.1f) * Mathf.Deg2Rad);
            }
        }
    }


    public void SetScreenData()
    {
        
        ScreenData.TouchScreenWidth = (CurrentMonitor.ScreenWidth * (1f - Data.ScreenRight - Data.ScreenLeft));
        ScreenData.TouchScreenHeight = (CurrentMonitor.ScreenHeight * (1f - Data.ScreenTop - Data.ScreenBottom));
        ScreenData.TouchScreenStartX = (CurrentMonitor.ScreenWidth * Data.ScreenLeft);
        ScreenData.TouchScreenStartY = (CurrentMonitor.ScreenHeight * Data.ScreenBottom);
    }





    private List<TouchData> InputDataList = new List<TouchData>();
    private List<TouchData> TouchDataList = new List<TouchData>();
    internal int TouchCount = 0;
    internal int PointCount = 0;
    public void ReadSensorData(List<DistanceInfo> list)
    {
        CurrentMonitor.OnSensorInputStart?.Invoke();
        InputDataList.Clear();
        TouchDataList.Clear();
        try
        {
            int cnt = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var Info = list[i];
                if (Info.Distance < Data.MinDist)
                    continue;

                int angle = (int)((Info.Angle + Data.Angle + 720) % 360) * 10;
                float x = SinData[angle] * Info.Distance * Data.Scale * (Data.IsXFlip ? -1 : 1);
                float y = CosData[angle] * Info.Distance * Data.Scale * (Data.IsYFlip ? -1 : 1);
                //
                //float x = LidarSensorManager.SinData[(int)((Info.Angle + 180 + Data.Angle) * 100)] * Info.Distance * Data.Scale * (Data.IsXFlip ? -1 : 1);
                //float y = LidarSensorManager.CosData[(int)((Info.Angle + 180 + Data.Angle) * 100)] * Info.Distance * Data.Scale * (Data.IsYFlip ? -1 : 1);

                if (x < Data.TouchLeft ||
                    x > Data.TouchRight ||
                    y < Data.TouchBottom ||
                    y > Data.TouchTop)
                    continue;


                Vector2 v2 = new Vector2(x, y);
                bool check = false;
                foreach (var ignoe in Data.IgnoreList)
                {
                    if (ignoe.Contains(v2))
                    {
                        check = true;
                        break;
                    }
                }
                if (check)
                {
                    continue;
                }



                x = ScreenData.TouchScreenStartX + ScreenData.TouchScreenWidth * (x - Data.TouchLeft) / TouchWidth;
                y = ScreenData.TouchScreenStartY + ScreenData.TouchScreenHeight * (y - Data.TouchBottom) / TouchHeight;

                //if (InputIndex >= InputDataList.Length - 1)
                //{
                //    continue;
                //}


                InputDataList.Add(new TouchData() { vector = new Vector2(x, y), index = i, dist = Info.Distance });
                cnt++;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
        PointCount = InputDataList.Count;
        for (int i = 0; i < InputDataList.Count; i++)
        {
            var data = InputDataList[i];

            bool isIn = false;
            for (int j = 0; j < TouchDataList.Count; j++)
            {
                var touch = TouchDataList[j];

                if(Data.GroupMaxCount > 0 && touch.count >= Data.GroupMaxCount)
                {
                    continue;
                }

                //연속되는 데이터인 경우(각도)
                if (Mathf.Abs(touch.index - data.index) <= Data.GroupAngle)
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
                TouchDataList.Add(new TouchData { vector = data.vector, count = 1, dist = data.dist, index = data.index });
            }
        }


        TouchCount = TouchDataList.Count;

        for (int i = 0; i < TouchDataList.Count; i++)
        {
            if (TouchDataList[i].count > 0)
            {
                CurrentMonitor.SensorInputColor(TouchDataList[i].vector.x, TouchDataList[i].vector.y, Sensor.ViewColor);
            }
        }
        CurrentMonitor.OnSensorInputEnd?.Invoke();
    }
}


public struct DistanceInfo
{
    public float Angle;
    public float Distance;
}

public struct TouchData
{
    public Vector2 vector;
    public float dist;
    public int count;
    public int index;
}

[System.Serializable]
public class LidarViewData
{
    public bool IsUse = false;

    public float MinDist = 20;

    public float Angle = 90;
    public float Scale = 1f;

    public int GroupAngle = 10;
    public int GroupMaxCount = 0;

    public bool IsXFlip = true;
    public bool IsYFlip = false;

    public float TouchRight = 100;
    public float TouchBottom = -80;
    public float TouchLeft = -100;
    public float TouchTop = 80;


    public float ScreenTop;
    public float ScreenRight;
    public float ScreenBottom;
    public float ScreenLeft;

    public float SizeWidthRate = 1f;
    public float SizeHeightRate = 1f;

    public List<Rect> IgnoreList = new List<Rect>();


    public void Reset()
    {
        Angle = 90;
        Scale = 1f;

        IsXFlip = true;
        IsYFlip = false;

        TouchRight = 100;
        TouchBottom = -80;
        TouchLeft = -100;
        TouchTop = 80;


        ScreenTop = 0;
        ScreenRight = 0;
        ScreenBottom = 0;
        ScreenLeft = 0;

        SizeWidthRate = 1f;
        SizeHeightRate = 1f;
    }
}

/// <summary>
/// 저장 용도는 아니고 좌표 계산에만 사용
/// </summary>
[System.Serializable]
public class SensorScreenData
{
    public float TouchScreenStartX;
    public float TouchScreenStartY;
    public float TouchScreenWidth;
    public float TouchScreenHeight;
}
