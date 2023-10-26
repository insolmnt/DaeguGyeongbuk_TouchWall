using Astra;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SensorAstra : DepthSensor
{
    private StreamSet _streamSet;
    private Astra.StreamReader _reader1;

    private DepthStream _depthStream;

    private long _lastDepthFrameIndex = -1;

    private short[] _DepthData = new short[640 * 480];
    private Texture2D ColorTexture;
    private WebCamTexture ColorWebCamTexture = null;
    private int CurrentColorCameraIndex = -1;
    private float LastFrameTime = 0;
    static public bool IsInit = false;

    private float FpsTime = 0;
    private int Fps = 0;


    private void Awake()
    {
        DepthData = new ushort[640 * 480];


        ColorTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);

        if (IsInit == false)
        {
            Debug.Log("AstraUnityContext.Awake");
            AstraUnityContext.Instance.Initialize();
            IsInit = true;
        }
    }


    public override void Connect()
    {
        Debug.Log("<color=white>[Astra] Open " + Id + "</color>");

        IsConnect = false;

        try
        {
            AstraUnityContext.Instance.WaitForUpdate(AstraBackgroundUpdater.WaitIndefinitely);
            _streamSet = Astra.StreamSet.Open("device/sensor" + SensorId.Port);
            _reader1 = _streamSet.CreateReader();
            //_reader1.FrameReady += FrameReady;
            _depthStream = _reader1.GetStream<DepthStream>();

            var depthModes = _depthStream.AvailableModes;
            ImageMode selectedDepthMode = depthModes[0];

            foreach (var m in depthModes)
            {
                //Debug.Log("Depth 목록 : " + m.Width + ", " + m.Height + " / " + m.FramesPerSecond);
            }

            foreach (var m in depthModes)
            {
                if (m.Width == DepthWidth &&
                    m.Height == DepthHeight &&
                    m.FramesPerSecond == 30)
                {
                    selectedDepthMode = m;
                    break;
                }
            }
            _depthStream.SetMode(selectedDepthMode);
            _depthStream.Start();

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

        IsConnect = false;


        //AstraUnityContext.Instance.Initializing -= OnAstraInitializing;
        //AstraUnityContext.Instance.Terminating -= OnAstraTerminating;

        try
        {
            _depthStream.Stop();

            if (_reader1 != null)
            {
                _reader1.Dispose();
                //_reader1.FrameReady -= FrameReady;
                _reader1 = null;
            }

            if (_streamSet != null)
            {
                _streamSet.Dispose();
                _streamSet = null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("UninitializeStreams Err : " + e.Message);
        }
    }
    private void FrameReady(object sender, FrameReadyEventArgs e)
    {
        DepthFrame depthFrame = e.Frame.GetFrame<DepthFrame>();
        //Debug.Log("FrameReady " + depthFrame.FrameIndex);

        if (depthFrame != null)
        {
            if (_lastDepthFrameIndex != depthFrame.FrameIndex)
            {
                _lastDepthFrameIndex = depthFrame.FrameIndex;
                depthFrame.CopyData(ref _DepthData);
                for (int i = 0; i < _DepthData.Length; i++)
                {
                    DepthData[i] = (ushort)_DepthData[i];
                }

                if (OnDepthRead != null)
                {
                    OnDepthRead(DepthData);
                }

            }
        }
    }

    void Update()
    {
        if (IsConnect == false)
        {
            return;
        }

        FpsTime += Time.deltaTime;
        if(FpsTime >= 1)
        {
            FpsTime -= 1;
            ShowFps = Fps;
            Fps = 0;
        }

        ReaderFrame frame;
        if (_reader1.TryOpenFrame(0, out frame))
        {
            using (frame)
            {
                DepthFrame depthFrame = frame.GetFrame<DepthFrame>();

                if (depthFrame != null)
                {
                    if (_lastDepthFrameIndex != depthFrame.FrameIndex)
                    {
                        Fps++;
                        LastFrameTime = 0;
                        _lastDepthFrameIndex = depthFrame.FrameIndex;

                        depthFrame.CopyData(ref _DepthData);
                        for (int i = 0; i < _DepthData.Length; i++)
                        {
                            DepthData[i] = (ushort)_DepthData[i];
                        }

                        DepthChange();
                        //NewDepthFrameEvent.Invoke(depthFrame);
                    }
                }
            }
        }


        AstraUnityContext.Instance.UpdateAsync(() => { return true; });

        if (IsShowColorImage && ColorWebCamTexture != null && ColorWebCamTexture.didUpdateThisFrame)
        {
            ColorTexture.SetPixels(ColorWebCamTexture.GetPixels());
            ColorTexture.Apply();
            //if (OnColorReady != null)
            //{
            //    OnColorReady();
            //}
        }


        LastFrameTime += Time.deltaTime;
        //if (LastFrameTime > 5)
        //{
        //    DisConnect();
        //}
    }




    public override Texture2D GetColorImage(bool isStart)
    {
        base.GetColorImage(isStart);

        if(isStart)
        {
            OpenColorCamera();
        }
        else
        {
            CloseColorCamera();
        }
        return ColorTexture;
    }

    private void OpenColorCamera()
    {
        //if (ColorWebCamTexture != null)
        //{
        //    return;
        //}
        if (WebCamTexture.devices == null || WebCamTexture.devices.Length == 0)
        {
            UIManager.Instance.ShowMessage("Color Camera Not Found");
            return;
        }


        //if (string.IsNullOrEmpty(Data.ColorCameraName))
        //{
        //    CurrentColorCameraIndex = Mathf.Min(SensorId.PortNumber, WebCamTexture.devices.Length - 1);
        //    ColorWebCamTexture = new WebCamTexture(WebCamTexture.devices[CurrentColorCameraIndex].name, 640, 480, 30);
        //    ColorWebCamTexture.Play();
        //    return;
        //}

        CurrentColorCameraIndex = -1;
        for (int i=0; i< WebCamTexture.devices.Length; i++)
        {
            if(WebCamTexture.devices[i].name == Data.ColorCameraName)
            {
                CurrentColorCameraIndex = i;
            }
        }

        if(CurrentColorCameraIndex < 0)
        {
            CurrentColorCameraIndex = Mathf.Min(SensorId.PortNumber, WebCamTexture.devices.Length - 1);
        }

        Debug.Log("컬러 카메라 : " + CurrentColorCameraIndex + " / " + WebCamTexture.devices[CurrentColorCameraIndex].name);
        ColorWebCamTexture = new WebCamTexture(WebCamTexture.devices[CurrentColorCameraIndex].name, 640, 480, 30);
        ColorWebCamTexture.Play();
        return;
    }
    public void ColorCameraChange()
    {
        CloseColorCamera();
        CurrentColorCameraIndex = (CurrentColorCameraIndex + 1) % WebCamTexture.devices.Length;
        Data.ColorCameraName = WebCamTexture.devices[CurrentColorCameraIndex].name;
        OpenColorCamera();
    }
    private void CloseColorCamera()
    {
        if (ColorWebCamTexture == null || ColorWebCamTexture.isPlaying == false)
        {
            ColorWebCamTexture = null;
            return;
        }
        ColorWebCamTexture.Stop();
        DestroyImmediate(ColorWebCamTexture);
        ColorWebCamTexture = null;
    }

    public ushort[] GetRawDepthMap()
    {
        return DepthData;
    }


    public override void Load()
    {
        base.Load();

        SensorName = "Astra_" + SensorId.Port;

    }

}
