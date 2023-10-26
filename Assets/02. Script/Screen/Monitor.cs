using Coffee.UIExtensions;
using Fenderrio.ImageWarp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monitor : MonoBehaviour
{
    public ScreenManager Manager;

    public Action OnSensorInputStart;
    public Action OnSensorInputEnd;

    public ScreenType Type;

    public RawImageWarp MainOutImagePrefab;
    public List<RawImageWarp> MainOutImage;
    public RawImage[] SubRawImageList;
    //public RawImageWarp[] ImageList;

    public Canvas MonitorCanvas;
    public Canvas RealSizeCanvas;

    public Camera RawCamera;
    public Camera KeystoneCamera;
    public Camera OutCamera;

    public RenderTexture RawRenderTexture;
    public RenderTexture OutputRenderTexture;
    public RenderTexture DefaultRawRenderTexture;
    public RenderTexture DefaultOutputRenderTexture;

    public KeystoneSetting Keystone;
    public MonitorMask Mask;

    [Header("UI")]
    public Canvas UICanvas;
    public Camera UICamera;
    public RawImageWarp UIOutRawIamge;

    [Header("센서 관련")]
    public GameObject SensorSettingObject;
    public RawImage TouchViewImage;
    public SensorViewDot SensorViewDotPrefab;
    private List<SensorViewDot> SensorViewDotList = new List<SensorViewDot>();


    [Header("Data")]
    public MonitorData Data;
    public MonitorSplitData SplitData;
    public int DisplayIndex
    {
        get
        {
            return Data.Display;
        }
    }
    public Display CurrentDisplay;
    public int ScreenWidth = 1280;
    public int ScreenHeight = 800;
    public bool IsShowKeystoneLine = false;


    private void Awake()
    {
        DefaultRawRenderTexture = RawRenderTexture;
        DefaultOutputRenderTexture = OutputRenderTexture;
    }


    private void Start()
    {
        //RawImageWidth = RawRenderTexture.width;
        //RawImageHeight = RawRenderTexture.height;

        ScreenWidth = OutputRenderTexture.width;
        ScreenHeight = OutputRenderTexture.height;

        ScreenManager.OnDisplayChange += SetData;
        ScreenManager.OnScreenSizeChange += ScreenSizeChange;
    }
    private void OnDestroy()
    {
        ScreenManager.OnDisplayChange -= SetData;
        ScreenManager.OnScreenSizeChange -= ScreenSizeChange;
    }

    public void SensorInputColor(float x, float y, Color color)
    {
        if (SensorManager.Instance.IsShowDebugMode)
        {
            var dot = GetIdleDotImage();
            dot.DotImage.color = color;
            dot.DotImage.rectTransform.anchoredPosition = new Vector2(x, y);
            dot.ShowTime = SensorManager.Instance.DebugShowTime;
            dot.gameObject.SetActive(true);
        }
        if (SensorTest.Instance.IsShow)
        {
            SensorTest.Instance.OnTouch(RawCamera, x / RawRenderTexture.width, y / RawRenderTexture.height);
        }


        Ray ray = RawCamera.ScreenPointToRay(new Vector3(x, y));

        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000, Color.red, 1f);

        //RaycastHit hit;
        var hitList = Physics.RaycastAll(ray, 2000);
        if (hitList != null && hitList.Length > 0)
        {
            foreach (var hit in hitList)
            {
                var touch = hit.collider.GetComponent<TouchInput>();
                if (touch == null)
                {
                    continue;
                }

                //var result = touch.OnTouch(hit.point);
                var result = touch.OnTouch(hit.point);
            }
        }
    }
    private SensorViewDot GetIdleDotImage()
    {
        foreach(var image in SensorViewDotList)
        {
            if(image.gameObject.activeSelf == false)
            {
                return image;
            }
        }

        var item = Instantiate(SensorViewDotPrefab, SensorViewDotPrefab.transform.parent);
        SensorViewDotList.Add(item);
        return item;
    }

    public void ScreenSizeChange()
    {
        ScreenWidth = ScreenManager.Instance.Data.ScreenWidth;
        ScreenHeight = ScreenManager.Instance.Data.ScreenHeight;

        if (ScreenWidth != RawRenderTexture.width || ScreenHeight != RawRenderTexture.height)
        {
            Debug.Log("" + name + " 렌더텍스쳐 사이즈 변경 : (" + RawRenderTexture.width + ", " + RawRenderTexture.height + " -> (" + ScreenWidth + ", " + ScreenHeight + ")");

            var rawRender = new RenderTexture(RawRenderTexture);
            rawRender.width = ScreenWidth;
            rawRender.height = ScreenHeight;
            foreach (var image in SubRawImageList)
            {
                image.texture = rawRender;
            }
            RawCamera.targetTexture = rawRender;
            KeystoneCamera.targetTexture = rawRender;
            if (RawRenderTexture.Equals(DefaultRawRenderTexture) == false)
            {
                DestroyImmediate(RawRenderTexture);
            }
            RawRenderTexture = rawRender;

            var outRender = new RenderTexture(OutputRenderTexture);
            outRender.width = ScreenWidth;
            outRender.height = ScreenHeight;
            MainOutImagePrefab.texture = outRender;
            OutCamera.targetTexture = outRender;
            if (OutputRenderTexture.Equals(DefaultOutputRenderTexture) == false)
            {
                DestroyImmediate(OutputRenderTexture);
            }
            OutputRenderTexture = outRender;

            var uiRaw = new RenderTexture(RawRenderTexture);
            uiRaw.width = ScreenWidth;
            uiRaw.height = ScreenHeight;
            UICamera.targetTexture = uiRaw;
            UIOutRawIamge.texture = uiRaw;
        }

        for (int i = 0; i < MainOutImage.Count; i++)
        {
            MainOutImage[i].texture = OutputRenderTexture;
            int x = i % Data.SplitCountX;
            int y = i / Data.SplitCountX;

            MainOutImage[i].uvRect = new Rect(SplitData.Split[x], SplitData.SplitY[y], SplitData.Split[x + 1] - SplitData.Split[x], SplitData.SplitY[y + 1] - SplitData.SplitY[y]);

            SetKeystone(i, false);
        }


        KeystoneCamera.orthographicSize = (float)ScreenHeight / ScreenWidth * 16 * 20;
    }


    public void ShowKeystoneLine(bool isShow)
    {
        IsShowKeystoneLine = isShow;
        KeystoneCamera.gameObject.SetActive(isShow);
        if (RawCamera != null)
            RawCamera.gameObject.SetActive(!isShow);
    }



    public void ShowKeystoneSetting(bool isShow)
    {
        Keystone.ShowKeystoneSetting(isShow);
    }

    public void SetColorData()
    {
        foreach (var image in MainOutImage)
        {
            var gradiant = image.GetComponent<UIGradient>();
            gradiant.color1 = Data.ScreenColor[0];
            gradiant.color2 = Data.ScreenColor[1];
            gradiant.color3 = Data.ScreenColor[2];
            gradiant.color4 = Data.ScreenColor[3];
        }
    }

    public void SetData()
    {
        if (DisplayIndex == -1) //사용안함
        {
            CurrentDisplay = null;

            RealSizeCanvas.gameObject.SetActive(false);
            MonitorCanvas.gameObject.SetActive(false);
            KeystoneCamera.gameObject.SetActive(false);
            if (RawCamera != null)
                RawCamera.gameObject.SetActive(false);
            Mask.SetData();

            UICanvas.gameObject.SetActive(false);
            return;
        }

        if (Display.displays.Length > DisplayIndex)
        {
            CurrentDisplay = Display.displays[DisplayIndex];
        }
        else
        {
#if UNITY_EDITOR
            CurrentDisplay = Display.displays[0];
#endif
        }

        RealSizeCanvas.gameObject.SetActive(true);
        MonitorCanvas.targetDisplay = DisplayIndex;
        MonitorCanvas.gameObject.SetActive(true);

        KeystoneCamera.gameObject.SetActive(IsShowKeystoneLine);
        if (RawCamera != null)
            RawCamera.gameObject.SetActive(!IsShowKeystoneLine);


        UICanvas.gameObject.SetActive(true);

        UICanvas.targetDisplay = DisplayIndex;

        SetRenderSize();

        Mask.SetData(false);
    }
    public void SetKeystone(int index, bool isDouble)
    {
        var image = MainOutImage[index];
        var data = SplitData.Keystone[index];

        image.cornerOffsetTR = data.PointList[0];
        image.cornerOffsetBR = data.PointList[1];
        image.cornerOffsetBL = data.PointList[2];
        image.cornerOffsetTL = data.PointList[3];

        float screenWidth = ScreenManager.Instance.Data.ScreenWidth;
        float screenHeight = ScreenManager.Instance.Data.ScreenHeight;

        float canvasWidth = 800f / MonitorCanvas.renderingDisplaySize.y * MonitorCanvas.renderingDisplaySize.x;
        float canvasHeight = 800f;
        //Debug.Log("캔버스 : " + canvasWidth + ", " + canvasHeight);

        //Top A왼 B오
        //Right A위 B아래
        //Bottom A오 B왼
        //Left A아래  B위

        var top = new Vector3(
            (canvasWidth + image.cornerOffsetTR.x - image.cornerOffsetTL.x) / 3f,
            (image.cornerOffsetTR.y - image.cornerOffsetTL.y) / 3f,
            0);
        var left = new Vector3(
            (image.cornerOffsetTL.x - image.cornerOffsetBL.x) / 3f,
            (canvasHeight + image.cornerOffsetTL.y - image.cornerOffsetBL.y) / 3f,
            0);
        var right = new Vector3(
            (image.cornerOffsetBR.x - image.cornerOffsetTR.x) / 3f,
            -(canvasHeight + image.cornerOffsetTR.y - image.cornerOffsetBR.y) / 3f,
            0);
        var bottom = new Vector3(
            -(canvasWidth + image.cornerOffsetBR.x - image.cornerOffsetBL.x) / 3f,
            -(image.cornerOffsetBR.y - image.cornerOffsetBL.y) / 3f,
            0);


        if (Data.isOnltyStraight)
        {
            image.topBezierHandleA = (Vector2)top * data.StraightDataList[0];
            image.topBezierHandleB = -(Vector2)top * data.StraightDataList[1];

            image.leftBezierHandleA = (Vector2)left * data.StraightDataList[6];
            image.leftBezierHandleB = -(Vector2)left * data.StraightDataList[7];

            image.rightBezierHandleA = (Vector2)right * data.StraightDataList[2];
            image.rightBezierHandleB = -(Vector2)right * data.StraightDataList[3];

            image.bottomBezierHandleA = (Vector2)bottom * data.StraightDataList[4];
            image.bottomBezierHandleB = -(Vector2)bottom * data.StraightDataList[5];
        }
        else
        {
            image.topBezierHandleA = (Vector2)top + data.BezierList[0];
            image.topBezierHandleB = -(Vector2)top + data.BezierList[1];

            image.leftBezierHandleA = (Vector2)left + data.BezierList[6];
            image.leftBezierHandleB = -(Vector2)left + data.BezierList[7];

            image.rightBezierHandleA = (Vector2)right + data.BezierList[2];
            image.rightBezierHandleB = -(Vector2)right + data.BezierList[3];

            image.bottomBezierHandleA = (Vector2)bottom + data.BezierList[4];
            image.bottomBezierHandleB = -(Vector2)bottom + data.BezierList[5];

        }

        if (isDouble)
        {
            int x = index % Data.SplitCountX;
            int y = index / Data.SplitCountX;

            if (x > 0) //왼쪽에 이미지 있으면
            {
                var leftKeystone = SplitData.Keystone[index - 1];


                leftKeystone.PointList[0] = new Vector2(-canvasWidth + data.PointList[3].x, data.PointList[3].y);
                leftKeystone.PointList[1] = new Vector2(-canvasWidth + data.PointList[2].x, data.PointList[2].y);

                leftKeystone.StraightDataList[2] = data.StraightDataList[7];
                leftKeystone.StraightDataList[3] = data.StraightDataList[6];

                leftKeystone.BezierList[2] = data.BezierList[7];
                leftKeystone.BezierList[3] = data.BezierList[6];

                SetKeystone(index - 1, false);
            }

            if (x < Data.SplitCountX - 1) //오른쪽에 이미지 있으면
            {
                var rightKeystone = SplitData.Keystone[index + 1];

                rightKeystone.PointList[2] = new Vector2(canvasWidth + data.PointList[1].x, data.PointList[1].y);
                rightKeystone.PointList[3] = new Vector2(canvasWidth + data.PointList[0].x, data.PointList[0].y);

                rightKeystone.StraightDataList[7] = data.StraightDataList[2];
                rightKeystone.StraightDataList[6] = data.StraightDataList[3];


                rightKeystone.BezierList[7] = data.BezierList[2];
                rightKeystone.BezierList[6] = data.BezierList[3];


                SetKeystone(index + 1, false);
            }


            if (y < Data.SplitCountY - 1) //위에 이미지 있으면
            {
                var upKeystone = SplitData.Keystone[index + Data.SplitCountX];

                upKeystone.PointList[1] = new Vector2(data.PointList[0].x, +canvasHeight + data.PointList[0].y);
                upKeystone.PointList[2] = new Vector2(data.PointList[3].x, +canvasHeight + data.PointList[3].y);

                upKeystone.StraightDataList[5] = data.StraightDataList[0];
                upKeystone.StraightDataList[4] = data.StraightDataList[1];


                upKeystone.BezierList[5] = data.BezierList[0];
                upKeystone.BezierList[4] = data.BezierList[1];

                SetKeystone(index + Data.SplitCountX, false);
            }

            if (y > 0) //아래에 이미지 있으면
            {
                var upKeystone = SplitData.Keystone[index - Data.SplitCountX];

                upKeystone.PointList[0] = new Vector2(data.PointList[1].x, -canvasHeight + data.PointList[1].y);
                upKeystone.PointList[3] = new Vector2(data.PointList[2].x, -canvasHeight + data.PointList[2].y);

                upKeystone.StraightDataList[0] = data.StraightDataList[5];
                upKeystone.StraightDataList[1] = data.StraightDataList[4];

                upKeystone.BezierList[0] = data.BezierList[5];
                upKeystone.BezierList[1] = data.BezierList[4];

                SetKeystone(index - Data.SplitCountX, false);
            }

            if (x < Data.SplitCountX - 1 && y < Data.SplitCountY - 1) //우측 위 이미지 있으면
            {
                var keystone = SplitData.Keystone[index + Data.SplitCountX + 1];
                keystone.PointList[2] = new Vector2(+canvasWidth + data.PointList[0].x, +canvasHeight + data.PointList[0].y);

                SetKeystone(index + Data.SplitCountX + 1, false);
            }

            if (x < Data.SplitCountX - 1 && y > 0) //우측 아래 이미지 있으면
            {
                var keystone = SplitData.Keystone[index - Data.SplitCountX + 1];
                keystone.PointList[3] = new Vector2(+canvasWidth + data.PointList[1].x, -canvasHeight + data.PointList[1].y);

                SetKeystone(index - Data.SplitCountX + 1, false);
            }

            if (x > 0 && y > 0) //좌측 아래
            {
                var keystone = SplitData.Keystone[index - Data.SplitCountX - 1];
                keystone.PointList[0] = new Vector2(-canvasWidth + data.PointList[2].x, -canvasHeight + data.PointList[2].y);

                SetKeystone(index - Data.SplitCountX - 1, false);
            }

            if (x > 0 && y < Data.SplitCountY - 1) //좌측 위
            {
                var keystone = SplitData.Keystone[index + Data.SplitCountX - 1];
                keystone.PointList[1] = new Vector2(-canvasWidth + data.PointList[3].x, +canvasHeight + data.PointList[3].y);

                SetKeystone(index + Data.SplitCountX - 1, false);
            }
        }


    }

    internal void ShowSensorView(bool isShow)
    {
    }

    public void SetRenderSize()
    {
        //var texture = new RenderTexture(QualityManager.Instance.Data.GetFirstRenderSize, QualityManager.Instance.Data.GetFirstRenderSize, RenderTexture.depth, RenderTexture.graphicsFormat, RenderTexture.mipmapCount);
        //texture.name = name + "_RenderTexture";
        //TargetCamera.targetTexture = texture;
        //TargetImage.texture = texture;

        //try
        //{
        //if (RenderTexture != null)
        //{
        //DestroyImmediate(RenderTexture);
        //}
        //}
        //catch (System.Exception e)
        //{
        //Debug.LogError("Destroy(1) Err : " + e.Message);
        //}


        //RenderTexture = texture;
        for (int i = 0; i < MainOutImage.Count; i++)
        {
            SetKeystone(i, false);
        }
    }

    public void Save()
    {
        DataManager.SetData(Type.ToString(), Data);
        DataManager.SetData(Type.ToString() + "_" + Data.SplitCountX + "_" + Data.SplitCountY, SplitData);
    }
    public void Load()
    {
        Data = DataManager.GetData<MonitorData>(Type.ToString());
        if (Data == null)
        {
            Data = new MonitorData();
            Debug.Log(Type.ToString() + " 데이터 없음");
        }

        if (Data.Display > 0 && Display.displays.Length > Data.Display)
        {
            Display.displays[Data.Display].Activate();
        }

        LoadSplitData();

        //SetColorData();
        //ScreenSizeChange();
        SetData();
    }
    public void LoadSplitData()
    {
        SplitData = DataManager.GetData<MonitorSplitData>(Type.ToString() + "_" + Data.SplitCountX + "_" + Data.SplitCountY);
        if (SplitData == null)
        {
            SplitData = new MonitorSplitData();
        }
        SetSplitCountData();
    }

    private int beforeSplitCountX = 0;
    private int beforeSplitCountY = 0;
    public void SetSplitCountData()
    {
        if (Data.SplitCountX == beforeSplitCountX && Data.SplitCountY == beforeSplitCountY && MainOutImage != null && MainOutImage.Count != Data.SplitCountX * Data.SplitCountY)
        {
            return;
        }
        beforeSplitCountX = Data.SplitCountX;
        beforeSplitCountY = Data.SplitCountY;


        float canvasWidth = 800f / MonitorCanvas.renderingDisplaySize.y * MonitorCanvas.renderingDisplaySize.x;
        float canvasHeight = 800f;

        if (SplitData.Split == null || SplitData.Split.Length != Data.SplitCountX + 1)
        {
            SplitData.Split = new float[Data.SplitCountX + 1];
            for (int i = 0; i < SplitData.Split.Length; i++)
            {
                SplitData.Split[i] = (float)i / (Data.SplitCountX);
            }
        }


        if (SplitData.SplitY == null || SplitData.SplitY.Length != Data.SplitCountY + 1)
        {
            SplitData.SplitY = new float[Data.SplitCountY + 1];
            for (int i = 0; i < SplitData.SplitY.Length; i++)
            {
                SplitData.SplitY[i] = (float)i / (Data.SplitCountY);
            }
        }

        int count = Data.SplitCountX * Data.SplitCountY;
        if (SplitData.Keystone == null || SplitData.Keystone.Length != count)
        {
            SplitData.Keystone = new MonitorKeystoneData[count];

            for (int i = 0; i < count; i++)
            {
                int x = i % Data.SplitCountX;
                int y = i / Data.SplitCountX;

                SplitData.Keystone[i] = new MonitorKeystoneData();
                SplitData.Keystone[i].PointList = new Vector2[]
                {
                    new Vector2((canvasWidth * SplitData.Split[x + 1]) - canvasWidth, (canvasHeight * SplitData.SplitY[y + 1]) - canvasHeight),
                    new Vector2((canvasWidth * SplitData.Split[x + 1]) - canvasWidth, canvasHeight * SplitData.SplitY[y]),
                    new Vector2(canvasWidth * SplitData.Split[x], canvasHeight * SplitData.SplitY[y]),
                    new Vector2(canvasWidth * SplitData.Split[x], (canvasHeight * SplitData.SplitY[y + 1]) - canvasHeight)
                };
            }
        }

        if (MainOutImage == null || MainOutImage.Count != count)
        {
            for (int i = MainOutImage.Count - 1; i >= 0; i--)
            {
                Destroy(MainOutImage[i].gameObject);
            }
            MainOutImage.Clear();

            MainOutImagePrefab.gameObject.SetActive(false);
            for (int i = 0; i < Data.SplitCountX * Data.SplitCountY; i++)
            {
                var image = Instantiate(MainOutImagePrefab, MainOutImagePrefab.transform.parent);
                image.transform.SetSiblingIndex(i + 1);
                image.gameObject.SetActive(true);
                MainOutImage.Add(image);
            }
        }

        SetColorData();
        ScreenSizeChange();
    }
}



[System.Serializable]
public class MonitorData
{
    /// <summary>
    /// 디스플레이 번호 (0 : 메인 디스플레이 -> 1로 표시됨)
    /// </summary>
    public int Display;

    public Color[] ScreenColor = new Color[] { Color.white, Color.white, Color.white, Color.white};

    public float CutTop = 0;
    public float CutRight = 0;
    public float CutBottom = 0;
    public float CutLeft = 0;

    public float FadeTop = 0;
    public float FadeRight = 0;
    public float FadeBottom = 0;
    public float FadeLeft = 0;

    public bool isOnltyStraight = true;


    public int SplitCountX = 1;
    public int SplitCountY = 1;

    public AnimationCurve CurveTop = new AnimationCurve { keys = new Keyframe[] { new Keyframe() { time = 0, value = 0, inTangent = 1, outTangent = 1 }, new Keyframe() { time = 1, value = 1, inTangent = 1, outTangent = 1 } } };
    public AnimationCurve CurveRight = new AnimationCurve { keys = new Keyframe[] { new Keyframe() { time = 0, value = 0, inTangent = 1, outTangent = 1 }, new Keyframe() { time = 1, value = 1, inTangent = 1, outTangent = 1 } } };
    public AnimationCurve CurveBottom = new AnimationCurve { keys = new Keyframe[] { new Keyframe() { time = 0, value = 0, inTangent = 1, outTangent = 1 }, new Keyframe() { time = 1, value = 1, inTangent = 1, outTangent = 1 } } };
    public AnimationCurve CurveLeft = new AnimationCurve { keys = new Keyframe[] { new Keyframe() { time = 0, value = 0, inTangent = 1, outTangent = 1 }, new Keyframe() { time = 1, value = 1, inTangent = 1, outTangent = 1 } } };
}


[System.Serializable]
public class MonitorSplitData
{
    public float[] Split = new float[] { 0, 0.3f, 0.5f, 1f };
    public float[] SplitY = new float[] { 0, 1 };
    public MonitorKeystoneData[] Keystone;
}


[System.Serializable]
public class MonitorKeystoneData
{
    /// <summary>
    /// 키스톤 사용을 위한 좌표
    /// </summary>
    public Vector2[] PointList = new Vector2[]
    {
        Vector2.zero,
        Vector2.zero,
        Vector2.zero,
        Vector2.zero
    };

    /// <summary>
    /// 키스톤 곡선 사용을 위한 좌표
    /// </summary>
    public Vector2[] BezierList = new Vector2[]
    {
        Vector2.zero,
        Vector2.zero,
        Vector2.zero,
        Vector2.zero,
        Vector2.zero,
        Vector2.zero,
        Vector2.zero,
        Vector2.zero
    };


    public float[] StraightDataList = new float[]
    {
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1
    };
}