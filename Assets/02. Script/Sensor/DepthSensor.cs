using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DepthSensor : Sensor
{
    public System.Action<ushort[]> OnDepthRead;


    [Header("BackView")]
    public DepthSensorBackView LeftBackView;
    public DepthSensorBackView RightBackView;

    [Header("TopView")]
    public DepthSensorTopView LeftTopView;
    public DepthSensorTopView RightTopView;

    [Header("설정 관련 데이터")]
    public int DepthWidth = 640;
    public int DepthHeight = 480;
    public Vector3 SettingColorImageScale = new Vector3(1, 1, 1);
    public Vector3 SettingDepthImageScale = new Vector3(1, 1, 1);


    [Header("Data")]
    public DepthSensorData Data;
    protected bool IsShowPreview = false;
    protected bool IsShowColorImage = false;
    static public Color32 ColorBlack = new Color32(0, 0, 0, 255);
    static public Color32 ColorWhite = new Color32(255, 255, 255, 255);
    internal int ShowFps = 0;
    private Texture2D PreviewTexture;
    private Color[] PreviewColors;
    private int PreviewDetaile = 5; //1에 가까울수록 디테일해짐


    /// <summary>
    /// 저장된 깊이 데이터
    /// </summary>
    [System.NonSerialized]
    public ushort[] SaveFloorDefault;
    [System.NonSerialized]
    public ushort[] DepthData;


    public override void Connect() 
    { 
    }

    public void DepthChange()
    {
        if (Data.LeftBackview.IsUse)
        {
            if (Data.LeftTopview.IsUse)
            {
                LeftTopView.DepthChange(DepthData);
            }
            else
            {
                LeftBackView.DepthChange(DepthData);
            }
        }
        if (Data.RightBackview.IsUse)
        {
            if (Data.RightTopview.IsUse)
            {
                RightTopView.DepthChange(DepthData);
            }
            else
            {
                RightBackView.DepthChange(DepthData);
            }
        }

        if (OnDepthRead != null)
        {
            OnDepthRead(DepthData);
        }


        if (IsShowPreview)
        {
            MakePreviewImage();
        }
    }
    private void MakePreviewImage()
    {
        if(PreviewTexture == null)
        {
            PreviewTexture = new Texture2D(DepthWidth / PreviewDetaile, DepthHeight / PreviewDetaile, TextureFormat.RGB24, false);
            PreviewColors = new Color[PreviewTexture.width * PreviewTexture.height];
        }
        float dist = 3000;
        float size = 200;

        float r = size * 20;
        float g = size * 10;
        float b = size;

        for (int x = 0; x < PreviewTexture.width; x++)
        {
            for (int y = 0; y < PreviewTexture.height; y++)
            {
                int depthIndex = (x * PreviewDetaile) + (y * PreviewDetaile) * DepthWidth;
                int textureIndex = x + y * PreviewTexture.width;

                if (textureIndex >= PreviewColors.Length)
                {
                    continue;
                }

                float depth = DepthData[depthIndex];
                if (depth == 0)
                {
                    PreviewColors[textureIndex] = Color.green;
                    continue;
                }


                float realDist = Mathf.Abs(DepthData[depthIndex] - dist);
                PreviewColors[textureIndex] = new Color(
                    (realDist < r) ? ((r - realDist) / r) : 0,
                    (realDist < g) ? ((g - realDist) / g) : 0,
                    (realDist < b) ? ((b - realDist) / b) : 0,
                    1);
            }
        }

        PreviewTexture.SetPixels(PreviewColors);
        PreviewTexture.Apply();
    }


    protected override string GetMemo()
    {
        if (string.IsNullOrEmpty(Data.Memo))
        {
            return SensorId.Id;
        }
        return Data.Memo;
    }
    protected override void SetMemo(string memo)
    {
        Data.Memo = memo;
    }
    protected override Color GetColor()
    {
        return Data.ViewColor;
    }
    protected override void SetColor(Color color)
    {
        Data.ViewColor = color;
    }

    public override void Save()
    {
        DataManager.SetData(SensorId.Id, Data);
    }
    public override void Load()
    {
        Data = DataManager.GetData<DepthSensorData>(SensorId.Id);
        if(Data == null)
        {
            Data = new DepthSensorData();
        }

        LeftBackView.Data = Data.LeftBackview;
        RightBackView.Data = Data.RightBackview;


        LeftTopView.Data = Data.LeftTopview;
        RightTopView.Data = Data.RightTopview;


        LoadFloorData();

        LeftBackView.Load();
        RightBackView.Load();

        LeftTopView.Load();
        RightTopView.Load();
    }

    public override Texture2D GetPreviewImage(bool isStart)
    {
        IsShowPreview = isStart;
        return PreviewTexture;
    }
    public virtual Texture2D GetColorImage(bool isStart)
    {
        IsShowColorImage = isStart;
        return null;
    }

    public override void DisConnect()
    {
    }


    protected void LoadFloorData()
    {
        //저장된 깊이 파일 불러오기
        string fileName = "Setting/" + SensorId.Id + "_Default.dat";
        if (new FileInfo(fileName).Exists)
        {
            Debug.Log("" + fileName + " 파일 있음");
            System.IO.StreamReader sw = new System.IO.StreamReader(fileName);
            string data = sw.ReadToEnd();
            sw.Close();

            if (string.IsNullOrEmpty(data) == false)
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var memoryStream = new System.IO.MemoryStream(System.Convert.FromBase64String(data));

                SaveFloorDefault = (ushort[])binaryFormatter.Deserialize(memoryStream);

                if (DepthWidth * DepthHeight != SaveFloorDefault.Length)
                {
                    Debug.Log("" + fileName + " 파일 길이 다름");
                    SaveFloorDefault = new ushort[DepthWidth * DepthHeight];
                }
            }
        }
        else
        {
            Debug.Log("" + fileName + " 파일 없음");
            SaveFloorDefault = new ushort[DepthWidth * DepthHeight];
        }
    }
    public void SetSaveFloorDefault()
    {
        for (int i = 0; i < DepthData.Length; i++)
        {
            if (DepthData[i] == 0)
            {
                SaveFloorDefault[i] = SetFloorZeroData(DepthData, DepthWidth, DepthHeight, i);
            }
            else
            {
                SaveFloorDefault[i] = DepthData[i];
            }
        }

        LeftBackView.SaveToCheckScreen();
        RightBackView.SaveToCheckScreen();

        //파일로 저장
        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        var memoryStream = new System.IO.MemoryStream();
        binaryFormatter.Serialize(memoryStream, SaveFloorDefault);

        string fileName = "Setting/" + SensorId.Id + "_Default.dat";
        StreamWriter sw = new StreamWriter(fileName);
        sw.Write(System.Convert.ToBase64String(memoryStream.GetBuffer()));
        sw.Close();
    }

    ushort SetFloorZeroData(ushort[] DepthRawList, int DepthWidth, int DepthHeight, int index)
    {
        int x = index % DepthWidth;
        int y = index / DepthWidth;
        ushort min = 0;
        for (int i = 1; i < 6; i++)
        {
            if (x - i >= 0)
            {
                if (DepthRawList[y * DepthWidth + x - i] > 0 && DepthRawList[y * DepthWidth + x - i] > min)
                {
                    min = DepthRawList[y * DepthWidth + x - i];
                }
            }
            if (x + i < DepthWidth)
            {
                if (DepthRawList[y * DepthWidth + x + i] > 0 && DepthRawList[y * DepthWidth + x + i] > min)
                {
                    min = DepthRawList[y * DepthWidth + x + i];
                }
            }
            if (y - i >= 0)
            {
                if (DepthRawList[(y - i) * DepthWidth + x] > 0 && DepthRawList[(y - i) * DepthWidth + x] > min)
                {
                    min = DepthRawList[(y - i) * DepthWidth + x];
                }
            }
            if (y + i < DepthHeight)
            {
                if (DepthRawList[(y + i) * DepthWidth + x] > 0 && DepthRawList[(y + i) * DepthWidth + x] > min)
                {
                    min = DepthRawList[(y + i) * DepthWidth + x];
                }
            }
        }

        return min;
    }

    public override string GetDebugMsg()
    {
        return Memo + " DepthFps : " + ShowFps;
    }

    public virtual Vector2 GetColorArrowPointToDephtPoint(Vector2 colorPoint)
    {
        return colorPoint;
    }
}

[System.Serializable]
public class DepthSensorData
{
    public string Memo = "";
    public Color ViewColor = Color.magenta;

    public string ColorCameraName = "";

    public DepthSensorBackViewData LeftBackview = new DepthSensorBackViewData();
    public DepthSensorBackViewData RightBackview = new DepthSensorBackViewData();


    public DepthSensorTopViewData LeftTopview = new DepthSensorTopViewData();
    public DepthSensorTopViewData RightTopview = new DepthSensorTopViewData();
}


//[System.Serializable]
//public class DepthSensorScreenData
//{
//    public ScreenType Screen;

//    public DepthSensorTopViewData TopViewData = new DepthSensorTopViewData();
//    public DepthSensorBackViewData BackViewData = new DepthSensorBackViewData();
//}


[System.Serializable]
public class DepthSensorTopViewData
{
    public bool IsUse = false;

    public float Angle = 0;
    public float Scale = 1;

    public bool IsXFlip = false;
    public bool IsYFlip = false;

    public float TouchRight = 200;
    public float TouchBottom = -100;
    public float TouchLeft = -200;
    public float TouchTop = 100;


    public float ScreenTop = 0;
    public float ScreenRight = 0;
    public float ScreenBottom = 0;
    public float ScreenLeft = 0;

    public int YLeft = 160;
    public int YRight = 160;

    public float CameraPositionX = 0;
    public float CameraPositionY = 0;

    public int GroupAngle = 10;
    public int GroupMaxCount = 0;
}

[System.Serializable]
public class DepthSensorBackViewData
{
    public bool IsUse = false;

    public bool IsXFlip = false;
    public bool IsYFlip = false;

    public Vector2[] TouchDepthPoints = new Vector2[]
    {
        new Vector2(100, 300),
        new Vector2(500, 300),
        new Vector2(500, 50),
        new Vector2(100, 50)
    };

    public int MinDist = 150;
    public int MaxDist = 250;
    public bool IsMaxDistUse = true;

    public int MinArea = 60;
    public int MaxArea = 500;
    public bool IsMaxAreaUse = true;

    public float ScreenTop;
    public float ScreenRight;
    public float ScreenBottom;
    public float ScreenLeft;

    public int ErosionVal = 0;
    public int ExpansionVal = 0;
    public bool IsErosionNextExpansion = true;


    public int ScreenCheckRate = 60;
    public int ScreenCheckMaxDist = 500;

    /// <summary>
    /// 인식되는 영역 중 가운데(또는 가장자리 중 스크린 가까운곳)에 터치 발생 여부
    /// </summary>
    public bool IsTouchCenter = true;
    public bool IsOutlineAllTouch = false;
    public int OutlineAllTouchInterval = 10;


    public AnimationCurve XCurve = new AnimationCurve() { keys = new Keyframe[] { new Keyframe() { time = 0, value = 0, inTangent = 1, outTangent = 1 }, new Keyframe() { time = 1, value = 1, inTangent = 1, outTangent = 1 } } };
    public AnimationCurve YCurve = new AnimationCurve() { keys = new Keyframe[] { new Keyframe() { time = 0, value = 0, inTangent = 1, outTangent = 1 }, new Keyframe() { time = 1, value = 1, inTangent = 1, outTangent = 1 } } };
}