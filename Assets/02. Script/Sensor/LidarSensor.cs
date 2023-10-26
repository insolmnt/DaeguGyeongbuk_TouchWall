using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidarSensor : Sensor
{
    public Action<List<DistanceInfo>> OnReadData;
    [Header("View")]
    public LidarView LeftView;
    public LidarView RightView;

    [Header("Data")]
    public LidarData Data;
    [System.NonSerialized]
    public List<DistanceInfo> LastReadData = new List<DistanceInfo>();
    private int LastFrame = 0;
    private int CurrentFrame = 0;
    public bool IsShowPreview = false;
    private Texture2D PreviewTexture;
    private int ShowFps = 0;
    internal int Fps = 0;
    private float FpsCurrentTime = 0;

    public override void Connect()
    {
    }

    public override void DisConnect()
    {
    }

    protected override string GetMemo()
    {
        if (string.IsNullOrEmpty(Data.Memo))
        {
            return SensorName + "_" + SensorId.Port;
        }
        return Data.Memo;
    }
    public override string GetDebugMsg()
    {
        var str = SensorName + "_" + SensorId.Port + " FPS : " + ShowFps;

        if (Data.LeftData.IsUse)
        {
            str += "\nLeft - Input : " + LeftView.PointCount + " / Touch : " + LeftView.TouchCount;
        }
        if (Data.RightData.IsUse)
        {
            str += "\nRight - Input : " + RightView.PointCount + " / Touch : " + RightView.TouchCount;
        }

        return str;

    }

    public override Texture2D GetPreviewImage(bool isStart)
    {
        IsShowPreview = isStart;
        if (PreviewTexture == null)
        {
            PreviewTexture = new Texture2D(640, 480, TextureFormat.ARGB32, false);
        }
        return PreviewTexture;
    }

    public override void Load()
    {
        Data = DataManager.GetData<LidarData>(SensorId.Id);
        if (Data == null)
        {
            Data = new LidarData();
        }

        LeftView.Data = Data.LeftData;
        LeftView.SetData();
        LeftView.SetScreenData();

        RightView.Data = Data.RightData;
        RightView.SetData();
        RightView.SetScreenData();

    }

    public override void Save()
    {
        DataManager.SetData(SensorId.Id, Data);
    }

    [Range(1, 50)]
    public int Detail = 5;
    [Range(0.1f, 2f)]
    public float Size = 1f;

    public virtual void Update()
    {
        FpsCurrentTime += Time.deltaTime;
        if(FpsCurrentTime >= 1)
        {
            FpsCurrentTime -= 1;
            ShowFps = Fps;
            Fps = 0;
        }


        if (LastFrame < CurrentFrame)
        {
            LastFrame = CurrentFrame;
            //Fps++;

            if (Data.LeftData.IsUse)
            {
                LeftView.ReadSensorData(LastReadData);
            }

            if (Data.RightData.IsUse)
            {
                RightView.ReadSensorData(LastReadData);
            }

            OnReadData?.Invoke(LastReadData);

            if (IsShowPreview)
            {
                List<MatOfPoint> data = new List<MatOfPoint>();
                MatOfPoint pointData = new MatOfPoint();
                List<Point> list = new List<Point>();
                for (int i = 0; i < LastReadData.Count; i += Detail)
                {
                    var dot = LastReadData[i];
                    int angle = (int)((dot.Angle + 720) % 360) * 10;
                    float x = (LidarView.SinData[angle] * dot.Distance * Size) + 320;
                    float y = (LidarView.CosData[angle] * dot.Distance * Size) + 240;
                    //if(x > 0 && x < 640 && y > 0 && y < 480)
                    list.Add(new Point(x, y));
                }
                pointData.fromList(list);
                data.Add(pointData);
                var previewMat = new Mat(PreviewTexture.height, PreviewTexture.width, CvType.CV_8UC3, new Scalar(0, 0, 0, 0));
                Imgproc.fillPoly(previewMat, data, new Scalar(255, 255, 0));
                Utils.matToTexture2D(previewMat, PreviewTexture);
                previewMat.Dispose();
            }
            //OpenCVForUnity.ImgprocModule.Imgproc.fillPoly
        }
    }



    protected override Color GetColor()
    {
        return Data.BasicColor;
    }

    protected override void SetColor(Color color)
    {
        Data.BasicColor = color;
    }

    protected override void SetMemo(string memo)
    {
        Data.Memo = memo;
    }

    public void ReadData(List<DistanceInfo> list)
    {
        LastReadData = list;
        //if(list != null)
        //{
        //
        //}
        //if(LastReadData.Count != list.Count)
        {
           // Debug.Log("새로! : " + LastReadData.Count + " -> " + list.Count);

            //LastReadData.Clear();
            //foreach (var data in list)
            //{
            //    LastReadData.Add(data);
            //}
        }
        //else
        //{
        //    for(int i=0; i<list.Count; i++)
        //    {
        //        LastReadData[i] = list[i];
        //    }
        //}
        CurrentFrame++;
    }
}


[System.Serializable]
public class LidarData
{
    public string Addr;
    public string Memo;
    public Color BasicColor = Color.magenta;

    public LidarViewData LeftData = new LidarViewData();
    public LidarViewData RightData = new LidarViewData();

    public string SerialNo;
}