using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class SensorKinect : DepthSensor
{
    public KinectManager Kinect;



    private float LastFrameTime = 0;
    static public bool IsInit = false;

    private float FpsTime = 0;
    private int Fps = 0;

    private long _lastDepthFrameIndex = -1;


    private void Awake()
    {
        if (IsInit == false)
        {
            IsInit = true;
        }
    }


    public override void Connect()
    {
        Debug.Log("<color=white>[Astra] Open " + Id + "</color>");

        IsConnect = false;

        Kinect.gameObject.SetActive(true);

        try
        {

            Kinect.StartKinect();

            if(KinectSensor.GetDefault() == null || KinectSensor.GetDefault().IsAvailable == false || KinectSensor.GetDefault().IsOpen == false)
            {
                DisConnect();
                SettingSensor.Instance.AddSensorItem(this);
                return;
            }


            DepthData = Kinect.GetRawDepthMap();

            IsConnect = true;

            SettingSensor.Instance.AddSensorItem(this);
        }
        catch (System.Exception err)
        {
            Debug.LogError("[Astra] - OnAstraInitializing Err " + err.ToString());
            DisConnect();
            SettingSensor.Instance.AddSensorItem(this);
            //UIManager.Instance.ShowMessage("" + sensor.Id + " 센서 연결에 실패했습니다.");
        }
    }
    public override void DisConnect()
    {
        Debug.Log("[Astra] UninitializeStreams");

        if (IsConnect == false)
        {
            return;
        }

        Kinect.gameObject.SetActive(false);
        IsConnect = false;


        Kinect.Disconnect();
    }

    void Update()
    {
        if (IsConnect == false)
        {
            return;
        }

        FpsTime += Time.deltaTime;
        if (FpsTime >= 1)
        {
            FpsTime -= 1;
            ShowFps = Fps;
            Fps = 0;
        }

      // if (SensorData.lastDepthFrameTime <= lastDepthFrameTime)
      // {
      //     return;
      // }

        Fps++;

        if(DepthData == null || DepthData.Length == 0)
        {
            DepthData = Kinect.GetRawDepthMap();
        }

        //lastDepthFrameTime = SensorData.lastDepthFrameTime;
        DepthChange();


        LastFrameTime += Time.deltaTime;
        //if (LastFrameTime > 5)
        //{
        //    DisConnect();
        //}
    }




    public override Texture2D GetColorImage(bool isStart)
    {
        base.GetColorImage(isStart);

        //if (isStart)
        //{
        //    Kinect.computeColorMap = true;
        //}
        //else
        //{
        //    Kinect.computeColorMap = false;
        //}
        return Kinect.GetUsersClrTex2D();
    }

    public ushort[] GetRawDepthMap()
    {
        return DepthData;
    }


    public override void Load()
    {
        base.Load();

        SensorName = "Kinect V2";
    }
}

