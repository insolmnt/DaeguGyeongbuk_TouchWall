using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using InsolDefaultProject;
using UnityEngine.Networking;
using System.Linq;

public class LidarSenserSetting : MonoBehaviour {
    public Text TitleText;

    public RectTransform ViewRect;
    public RectTransform[] ArrowList;

    public RectTransform[] TouchRectList;


    public Image pt;
    private Image[] PtList;
    public Image DefaultPt;
    private List<Image> DefaultPtList = new List<Image>();

    public SliderCtr ScaleSlider;
    public SliderCtr AngleSlider;

    private RectTransform MoveObject;

    public VerticalLayoutGroup Layout_X;
    public HorizontalLayoutGroup Layout_Y;


    public Toggle FlipXToggle;
    public Toggle FlipYToggle;


    public bool IsClientReceive = true;
    public bool IsSeverReceive = true;


    public SliderCtr GroupCountSlider;
    public SliderCtr GroupIndexSlider;

    [Header("실제 화면 사이즈 출력")]
    public GameObject SizePanel;
    public Text ScreenSizeText;
    public SliderCtr SizeWidthRate;
    public SliderCtr SizeHeightRate;


    /// <summary>
    /// 센서 영역이 작아서 화면을 다 채울 수 없을 경우 사용하는 노란색 막대 4개
    /// </summary>
    [Header("화면 사용영역 설정")] 
    public RectTransform[] ScreenPointBarList;
    public RectTransform[] ScreenPointRedRectList;
    private int ScreenPointChangeIndex = -1;
    private float ScreenPointStartData = 0;
    private float ScreenPointStartMousePosition = 0;


    private int TouchPointChangeIndex = -1;
    private float TouchPointStartData = 0;
    private float TouchPointStartMousePosition = 0;

    internal int MoveArrowIndex = -1;
    private Vector2 MoveArrowStartMousePosition;
    private Vector2 MoveArrowStartData;



    [Header("자동설정 관련")]
    public GameObject AutoSetupPanel;
    public Text AutoSetupStateText;

    public Button AutoSetupLeftButton;
    public Button AutoSetutRightButton;
    public Button AutoSetupUpButton;
    public Button AutoSetupDownButton;

    public Image BlackPanel;
    public Color BlackPanelNoTouchColor = new Color(1, 0.5f, 0, 0.5f);
    public Color BlackPanelTouchColor = new Color(0.5f, 0.5f, 0, 0.5f);
    //화면에 터치해야할 위치 표시
    public Image LeftTouchImage;
    public Image RightTouchImage;
    public Image UpTouchImage;
    public Image DownTouchImage;
    public UILineRenderer DownLine;

    //그래프에 표시되는 포인트 이미지
    public Image LeftPointImage;
    public Image RightPointImage;
    public Image UpPointImage;
    public Image DownPointImagePrefab;
    private List<Image> DownPointImageList = new List<Image>();
    private List<DistanceInfo> DownPointDataList = new List<DistanceInfo>();

    public GameObject AutoSetupDetailPanel;
    public SliderCtr DistCheckSlider;
    public SliderCtr DownDistCheckSlider;
    public SliderCtr MinDistCheckSlider;

    public AudioClip CountSound;
    public AudioClip CountEndSound;

    [Header("자동설정 Data")]
    public AutoSetupState AutoState = AutoSetupState.없음;
    public List<DistanceInfo> AutoSetupIgnoreList = new List<DistanceInfo>();
    private DistanceInfo LeftPointData;
    private DistanceInfo RightPointData;
    private DistanceInfo UpPointData;
    private Dictionary<int, DistanceInfo> DefaultDist = new Dictionary<int, DistanceInfo>();
    private Dictionary<int, Image> DefaultDistImage = new Dictionary<int, Image>();
    private bool CheckDownLeft = false;
    private bool CheckDownRight = false;
    public bool Is3PointType = true;
    public bool IsSaveCheck = false;
    private float SideCheckDistRate = 0.05f;
    private float[] SideZeroPosition = null;
    public bool ScreenCheckTop = false;
    public bool ScreenCheckRight = false;
    public bool ScreenCheckBottom = false;
    public bool ScreenCheckLeft = false;


    [Header("무시")]
    public IgnoeSettingItem IgnoeItem;
    public List<IgnoeSettingItem> IgnoeItemList = new List<IgnoeSettingItem>();

    [Header("Data")]
    public LidarSensor Sensor;
    public LidarView CurrentView;
    public Monitor CurrentScreen;


    private bool mIsStart = false;
    private void Start()
    {
        PtList = new Image[360 * 8 + 50];
        for (int i = 0; i < PtList.Length; i++)
        {
            PtList[i] = Instantiate(pt, pt.transform.parent);
        }


        ClearAutoSetup();

        mIsStart = true;
    }


    public void ShowSetting(LidarView view)
    {
        Sensor = view.Sensor;
        CurrentView = view;
        CurrentScreen = view.CurrentMonitor;
        //CurrentScreen.ShowSensorView(true);

        if (CurrentView == null)
        {
            MsgBox.Show("센서 설정을 열 수 없습니다."); //
            return;
        }

        MinDistCheckSlider.Val = CurrentView.Data.MinDist;
        SetUI();

        SettingManager.Instance.SettingPanel.gameObject.SetActive(false);
        SensorManager.Instance.ShowDebugMode(true);
        gameObject.SetActive(true);
        Sensor.OnReadData += ReadData;
    }

    private void OnDisable()
    {
        Sensor.OnReadData -= ReadData;
        SettingManager.Instance.SettingPanel.gameObject.SetActive(true);
        SensorManager.Instance.ShowDebugMode(false);
        //if (CurrentScreen != null)
        //    CurrentScreen.ShowSensorView(false);
        CurrentView = null;
        Sensor = null;
    }


    public void OnAngleSliderChange()
    {
        CurrentView.Data.Angle = AngleSlider.Val;

        ResetDefaultDist();
        SetAutoPointToImage();
    }
    public void OnScaleSliderChange()
    {
        CurrentView.Data.Scale = ScaleSlider.Val;

        OnSizeSliderChange();

        ResetDefaultDist();
        SetAutoPointToImage();
    }
    public void ResetDefaultDist()
    {
        DefaultDist.Clear();
        DefaultDistImage.Clear();
        foreach (var image in DefaultPtList)
        {
            image.gameObject.SetActive(false);
        }
    }


    public void OnSaveButtonClick()
    {
        Sensor.Save();
        gameObject.SetActive(false);
    }
    public void OnResetButtonClick()
    {
        CurrentView.Data.Reset();
        //CurrentSensor.SetData();
        //CurrentSensor.SetScreenData();
        SetUI();
    }
    public void OnReLoadButtonClick()
    {
        Sensor.Load();
        SetUI();
    }
    public void OnGroupSettingChange()
    {
        if (mIsLoad == false)
        {
            return;
        }
        CurrentView.Data.GroupMaxCount = (int)GroupCountSlider.Val;
        CurrentView.Data.GroupAngle = (int)GroupIndexSlider.Val;
    }






    private float ViewScale = 1;
    private void ChangeViewScale(float scale)
    {
        ViewScale = Mathf.Clamp(scale, 0.3f, 5f);
        ViewRect.localScale = new Vector3(ViewScale, ViewScale, 1);
    }


    public void OnTouchPointRectPointDown(int index)
    {
        TouchPointChangeIndex = index;
        switch ((ScreenRectPosition)index)
        {
            case ScreenRectPosition.Top:
                TouchPointStartMousePosition = Input.mousePosition.y;
                TouchPointStartData = CurrentView.Data.TouchTop;
                break;
            case ScreenRectPosition.Right:
                TouchPointStartMousePosition = Input.mousePosition.x;
                TouchPointStartData = CurrentView.Data.TouchRight;
                break;
            case ScreenRectPosition.Bottom:
                TouchPointStartMousePosition = Input.mousePosition.y;
                TouchPointStartData = CurrentView.Data.TouchBottom;
                break;
            case ScreenRectPosition.Left:
                TouchPointStartMousePosition = Input.mousePosition.x;
                TouchPointStartData = CurrentView.Data.TouchLeft;
                break;
        }
    }
    private void UpdateTouchPointRectChange()
    {
        if (TouchPointChangeIndex < 0)
        {
            return;
        }

        switch ((ScreenRectPosition)TouchPointChangeIndex)
        {
            case ScreenRectPosition.Top:
                CurrentView.Data.TouchTop = TouchPointStartData + (Input.mousePosition.y - TouchPointStartMousePosition) / ViewScale;
                break;
            case ScreenRectPosition.Right:
                CurrentView.Data.TouchRight = TouchPointStartData + (Input.mousePosition.x - TouchPointStartMousePosition) / ViewScale;
                break;
            case ScreenRectPosition.Bottom:
                CurrentView.Data.TouchBottom = TouchPointStartData + (Input.mousePosition.y - TouchPointStartMousePosition) / ViewScale;
                break;
            case ScreenRectPosition.Left:
                CurrentView.Data.TouchLeft = TouchPointStartData + (Input.mousePosition.x - TouchPointStartMousePosition) / ViewScale;
                break;
        }

        SetUI();
        if (Input.GetMouseButton(0) == false || Input.touchCount > 1)
        {
            TouchPointChangeIndex = -1;
        }
    }


    public void OnArrowDown(int index)
    {
        MoveArrowIndex = index;
        MoveArrowStartData = ArrowList[index].anchoredPosition;
        MoveArrowStartMousePosition = Input.mousePosition;
    }
    private void UpdateArrowMove()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnArrowDown(3);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnArrowDown(0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            OnArrowDown(1);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnArrowDown(2);
        }


        if (MoveArrowIndex < 0)
        {
            return;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveArrowStartMousePosition += new Vector2(-10, 0) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveArrowStartMousePosition += new Vector2(10, 0) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveArrowStartMousePosition += new Vector2(0, -10) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveArrowStartMousePosition += new Vector2(0, 10) * Time.deltaTime;
        }


        ArrowList[MoveArrowIndex].anchoredPosition = MoveArrowStartData + (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - MoveArrowStartMousePosition) / ViewScale;

        switch (MoveArrowIndex)
        {
            case 0:
                CurrentView.Data.TouchRight = ArrowList[MoveArrowIndex].anchoredPosition.x;
                CurrentView.Data.TouchTop = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;

            case 1:
                CurrentView.Data.TouchRight = ArrowList[MoveArrowIndex].anchoredPosition.x;
                CurrentView.Data.TouchBottom = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;

            case 2:
                CurrentView.Data.TouchLeft = ArrowList[MoveArrowIndex].anchoredPosition.x;
                CurrentView.Data.TouchBottom = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;

            case 3:
                CurrentView.Data.TouchLeft = ArrowList[MoveArrowIndex].anchoredPosition.x;
                CurrentView.Data.TouchTop = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;
        }



        SetUI();
        if (Input.GetMouseButton(0) == false
            && Input.GetKey(KeyCode.Q) == false
            && Input.GetKey(KeyCode.W) == false
            && Input.GetKey(KeyCode.A) == false
            && Input.GetKey(KeyCode.S) == false)
        {
            MoveArrowIndex = -1;
        }
    }


    #region 카메라 화면 이동 관련
    private bool IsCameraViewMove = false;
    private Vector2 MoveCameraStartMousePosition;
    private Vector2 MoveCameraStartPosition;

    public void OnMoveCameraPointDown()
    {
        IsCameraViewMove = true;

        MoveCameraStartMousePosition = Input.mousePosition;
        MoveCameraStartPosition = ViewRect.anchoredPosition;
    }

    private void UpdateMoveCamera()
    {
        if (IsCameraViewMove)
        {
            ViewRect.anchoredPosition = MoveCameraStartPosition + ((Vector2)Input.mousePosition - MoveCameraStartMousePosition) * 1080 / Screen.height;

            if (Input.GetMouseButton(0) == false || Input.touchCount > 1)
            {
                IsCameraViewMove = false;
            }
        }
    }
    #endregion


    public void OnXFlipToggleChange()
    {
        CurrentView.Data.IsXFlip = FlipXToggle.isOn;

        ResetDefaultDist();
        SetAutoPointToImage();
    }

    public void OnYFlipToggleChange()
    {
        CurrentView.Data.IsYFlip = FlipYToggle.isOn;

        ResetDefaultDist();
        SetAutoPointToImage();
    }



    private float StartDist = 0;

    private void Update()
    {
        //if (mIsSetting == false)
        //{
        //    return;
        //}

        if (Input.GetKeyDown(KeyCode.F1) && Input.GetKey(KeyCode.LeftShift) == false && Input.GetKey(KeyCode.LeftControl) == false)
        {
            SizePanel.SetActive(!SizePanel.activeSelf);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            ChangeViewScale(ViewScale + scroll);
        }

        if(StartDist <= 0)
        {
            if(Input.touchCount == 2)
            {
                StartDist = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            }
        }
        else
        {
            if (Input.touchCount == 2)
            {
                var dist = Vector2.Distance(Input.touches[0].position, Input.touches[1].position) - StartDist;

                ChangeViewScale(ViewScale + dist * 0.001f);
            }
            else
            {
                StartDist = 0;
            }
        }

        UpdateMoveCamera();

        UpdateArrowMove();
        UpdateTouchPointRectChange();

        UpdateChangeScreenRect();


        if (SensorTest.Instance.IsShow == false && MoveArrowIndex < 0)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                AngleSlider.Val += Time.deltaTime * 2f;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                AngleSlider.Val += Time.deltaTime * -2f;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                AngleSlider.Val += Time.deltaTime * 5f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                AngleSlider.Val += Time.deltaTime * -5f;
            }
        }

        //StateText.text = "Input Point : " + CurrentView.InputIndex + "\nTouch Count : " + View.TouchCount;

        #region 자동 조절
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (BlackPanel.gameObject.activeSelf)
            {
                StopAutoSetup();
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Is3PointType = true;
                ClearAutoSetup();
                StartAutoSetupWizard();
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Is3PointType = true;
                AutoSetupClearLeft();
                StartAutoSetupWizard();
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Is3PointType = true;
                AutoSetupClearRight();
                StartAutoSetupWizard();
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Is3PointType = true;
                AutoSetupClearUp();
                StartAutoSetupWizard();
            }



            if (Input.GetKeyDown(KeyCode.F5))
            {
                Is3PointType = false;
                ClearAutoSetup();
                StartAutoSetupWizard();
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Is3PointType = false;
                AutoSetupClearLeft();
                StartAutoSetupWizard();
            }
            if (Input.GetKeyDown(KeyCode.F7))
            {
                Is3PointType = false;
                AutoSetupClearRight();
                StartAutoSetupWizard();
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Is3PointType = false;
                AutoSetupClearUp();
                StartAutoSetupWizard();
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Is3PointType = false;
                AutoSetupClearDown();
                StartAutoSetupWizard();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                AutoSetupDetailPanel.gameObject.SetActive(!AutoSetupDetailPanel.gameObject.activeSelf);
            }
        }

        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            bool isShow = !RightTouchImage.gameObject.activeSelf;

            RightTouchImage.gameObject.SetActive(isShow);
            UpTouchImage.gameObject.SetActive(isShow);
            LeftTouchImage.gameObject.SetActive(isShow);
            DownTouchImage.gameObject.SetActive(isShow);
        }
        #endregion
    }
    #region 스크린 영역 설정 (노란색 막대)
    public void OnScreenPointRectPointDown(int index)
    {
        ScreenPointChangeIndex = index;
        switch ((ScreenRectPosition)index)
        {
            case ScreenRectPosition.Top:
                ScreenPointStartMousePosition = Input.mousePosition.y;
                ScreenPointStartData = CurrentView.Data.ScreenTop;
                break;
            case ScreenRectPosition.Right:
                ScreenPointStartMousePosition = Input.mousePosition.x;
                ScreenPointStartData = CurrentView.Data.ScreenRight;
                break;
            case ScreenRectPosition.Bottom:
                ScreenPointStartMousePosition = Input.mousePosition.y;
                ScreenPointStartData = CurrentView.Data.ScreenBottom;
                break;
            case ScreenRectPosition.Left:
                ScreenPointStartMousePosition = Input.mousePosition.x;
                ScreenPointStartData = CurrentView.Data.ScreenLeft;
                break;
        }
    }
    public void ChangeScreenRect()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float width = rect.rect.width;
        float height = 800;

        //Debug.Log("플레이화면 : " + display.systemWidth + ", " + display.systemHeight + " / " + width + ", " + height);
        CurrentView.ScreenData.TouchScreenWidth = (width * (1f - CurrentView.Data.ScreenRight - CurrentView.Data.ScreenLeft));
        CurrentView.ScreenData.TouchScreenHeight = (height * (1f - CurrentView.Data.ScreenTop - CurrentView.Data.ScreenBottom));
        CurrentView.ScreenData.TouchScreenStartX = (width * CurrentView.Data.ScreenLeft);
        CurrentView.ScreenData.TouchScreenStartY = (height * CurrentView.Data.ScreenBottom);

        float size = width * 0.01f;
        ScreenPointBarList[0].sizeDelta = new Vector2(width, size);
        ScreenPointBarList[1].sizeDelta = new Vector2(size, height);
        ScreenPointBarList[2].sizeDelta = new Vector2(width, size);
        ScreenPointBarList[3].sizeDelta = new Vector2(size, height);

        ScreenPointBarList[0].anchoredPosition = new Vector2(0, -height * CurrentView.Data.ScreenTop);
        ScreenPointBarList[1].anchoredPosition = new Vector2(-width * CurrentView.Data.ScreenRight, 0);
        ScreenPointBarList[2].anchoredPosition = new Vector2(0, height * CurrentView.Data.ScreenBottom);
        ScreenPointBarList[3].anchoredPosition = new Vector2(width * CurrentView.Data.ScreenLeft, 0);

        ScreenPointRedRectList[0].sizeDelta = new Vector2(size * 3f, size * 3f);
        ScreenPointRedRectList[1].sizeDelta = new Vector2(size * 3f, size * 3f);
        ScreenPointRedRectList[2].sizeDelta = new Vector2(size * 3f, size * 3f);
        ScreenPointRedRectList[3].sizeDelta = new Vector2(size * 3f, size * 3f);

        ScreenPointRedRectList[4].sizeDelta = new Vector2(size * 2f, size * 2f);
        ScreenPointRedRectList[5].sizeDelta = new Vector2(size * 2f, size * 2f);
        ScreenPointRedRectList[6].sizeDelta = new Vector2(size * 2f, size * 2f);
        ScreenPointRedRectList[7].sizeDelta = new Vector2(size * 2f, size * 2f);

        ScreenPointRedRectList[0].anchoredPosition = new Vector2(-width * CurrentView.Data.ScreenRight - size * 0.5f, -height * CurrentView.Data.ScreenTop - size * 0.5f);
        ScreenPointRedRectList[1].anchoredPosition = new Vector2(-width * CurrentView.Data.ScreenRight - size * 0.5f, height * CurrentView.Data.ScreenBottom + size * 0.5f);
        ScreenPointRedRectList[2].anchoredPosition = new Vector2(width * CurrentView.Data.ScreenLeft + size * 0.5f, height * CurrentView.Data.ScreenBottom + size * 0.5f);
        ScreenPointRedRectList[3].anchoredPosition = new Vector2(width * CurrentView.Data.ScreenLeft + size * 0.5f, -height * CurrentView.Data.ScreenTop - size * 0.5f);

        ScreenPointRedRectList[4].localPosition = new Vector2(ScreenPointRedRectList[0].localPosition.x, (ScreenPointRedRectList[0].localPosition.y + ScreenPointRedRectList[1].localPosition.y) / 2);
        ScreenPointRedRectList[5].localPosition = new Vector2((ScreenPointRedRectList[1].localPosition.x + ScreenPointRedRectList[2].localPosition.x) / 2, ScreenPointRedRectList[1].localPosition.y);
        ScreenPointRedRectList[6].localPosition = new Vector2(ScreenPointRedRectList[2].localPosition.x, (ScreenPointRedRectList[2].localPosition.y + ScreenPointRedRectList[3].localPosition.y) / 2);
        ScreenPointRedRectList[7].localPosition = new Vector2((ScreenPointRedRectList[3].localPosition.x + ScreenPointRedRectList[0].localPosition.x) / 2, ScreenPointRedRectList[3].localPosition.y);


        //CurrentSensor.SetData(CurrentSensor.Data, CurrentSensor.ScreenData);

        CurrentView.SetData();
        CurrentView.SetScreenData();

    }
    private void UpdateChangeScreenRect()
    {
        if (ScreenPointChangeIndex >= 0)
        {
            switch ((ScreenRectPosition)(ScreenPointChangeIndex))
            {
                case ScreenRectPosition.Top:
                    CurrentView.Data.ScreenTop = Mathf.Clamp(ScreenPointStartData - (Input.mousePosition.y - ScreenPointStartMousePosition) / Screen.height, 0, 1 - CurrentView.Data.ScreenBottom - 0.05f);
                    break;
                case ScreenRectPosition.Right:
                    CurrentView.Data.ScreenRight = Mathf.Clamp(ScreenPointStartData - (Input.mousePosition.x - ScreenPointStartMousePosition) / Screen.width, 0, 1 - CurrentView.Data.ScreenLeft - 0.05f);
                    break;
                case ScreenRectPosition.Bottom:
                    CurrentView.Data.ScreenBottom = Mathf.Clamp(ScreenPointStartData + (Input.mousePosition.y - ScreenPointStartMousePosition) / Screen.height, 0, 1 - CurrentView.Data.ScreenTop - 0.05f);
                    break;
                case ScreenRectPosition.Left:
                    CurrentView.Data.ScreenLeft = Mathf.Clamp(ScreenPointStartData + (Input.mousePosition.x - ScreenPointStartMousePosition) / Screen.width, 0, 1 - CurrentView.Data.ScreenRight - 0.05f);
                    break;
                default:
                    break;
            }

            ChangeScreenRect();


            if (Input.GetMouseButton(0) == false || Input.touchCount > 1)
            {
                ScreenPointChangeIndex = -1;
            }

            OnSizeSliderChange();
        }
    }
    #endregion

    private int beforeCount = 0;
    private void ReadData(List<DistanceInfo> distanceList)
    {
        if(mIsStart == false)
        {
            return;
        }

        //foreach (var pt in PtList)
        //{
        //    pt.gameObject.SetActive(false);
        //}

        //try
        {                                            //distanceList.AddRange(Manager.GetSensorData());
            if (distanceList == null || CurrentView == null || CurrentView.Data == null)
            {
                return;
            }

            #region 자동설정
            DistanceInfo minDist = new DistanceInfo()
            {
                Angle = 0,
                Distance = 0
            };
            Image minImage = null;

            float w = CurrentView.TouchWidth * SideCheckDistRate;
            float h = CurrentView.TouchHeight * SideCheckDistRate;
            var rect = new Rect(CurrentView.Data.TouchLeft - w, CurrentView.Data.TouchBottom - h, CurrentView.TouchWidth + w * 2, CurrentView.TouchHeight + h * 2);
            float[] dist_list = new float[] { 0, 0, 0, 0 };
            int[] count_list = new int[] { 0, 0, 0, 0 };
            #endregion


            for (int i = 0; i < distanceList.Count; i++)
            {
                var info = distanceList[i];

                float len = info.Distance * CurrentView.Data.Scale;
                int angle = (int)((info.Angle + CurrentView.Data.Angle + 720) % 360 * 10);

                float x = LidarView.SinData[angle] * len;
                float y = LidarView.CosData[angle] * len;

                PtList[i].rectTransform.anchoredPosition = new Vector2(CurrentView.Data.IsXFlip ? -x : x, CurrentView.Data.IsYFlip ? -y : y);
                PtList[i].gameObject.SetActive(true);


                #region 자동설정
                PtList[i].color = Color.yellow;

                int index = Mathf.RoundToInt(info.Angle * 5);
                if (DefaultDist.ContainsKey(index) == false)
                {
                    DefaultDist.Add(index, info);
                    var image = GetIdleDefaultPt();
                    DefaultDistImage.Add(index, image);
                    image.gameObject.SetActive(true);
                    image.rectTransform.anchoredPosition = PtList[i].rectTransform.anchoredPosition;

                    //if(minImage == null || info.Distance < minDist.Distance)
                    //{
                    //    minDist = info;
                    //    minImage = PtList[i];
                    //}
                }
                else
                {
                    var data = DefaultDist[index];

                    if (data.Distance < info.Distance)
                    {
                        DefaultDist[index] = info;
                        var image = DefaultDistImage[index];
                        image.rectTransform.anchoredPosition = PtList[i].rectTransform.anchoredPosition;
                    }
                    else if (data.Distance > info.Distance + DistCheckSlider.Val)
                    {
                        bool check = false;
                        foreach (var ignore in AutoSetupIgnoreList)
                        {
                            //Debug.Log("igrnore " + ignore.Angle + " / " + GetDefaultPosition(ignore));
                            //Debug.Log("info " + info.Angle + " / " + GetDefaultPosition(info));
                            if (Vector2.Distance(GetDefaultPosition(ignore), GetDefaultPosition(info)) < MinDistCheckSlider.Val)
                            {
                                PtList[i].color = Color.cyan;
                                check = true;
                                continue;
                            }
                        }
                        if (check)
                        {
                            continue;
                        }


                        for(int j = index - 10; j <= index + 10; j++)
                        {
                            if (DefaultDist.ContainsKey(j))
                            {
                                if (Mathf.Abs(DefaultDist[j].Distance - info.Distance) < DistCheckSlider.Val)
                                {
                                    check = true;
                                    break;
                                }
                            }
                        }

                        if (check)
                        {
                            continue;
                        }

                        PtList[i].color = Color.green;

                        if (info.Distance > MinDistCheckSlider.Val && (minImage == null || info.Distance < minDist.Distance))
                        {
                            minDist = info;
                            minImage = PtList[i];
                        }
                    }
                }

                if (info.Distance < MinDistCheckSlider.Val)
                {
                    PtList[i].color = Color.cyan;
                }
                else if (PtList[i].color.Equals(Color.green) == false)
                {
                    //상 : 빨강
                    //우 : 주황
                    //하 : 하양
                    //좌 : 파랑
                    if (rect.Contains(PtList[i].rectTransform.anchoredPosition))
                    {
                        var pt_x = PtList[i].rectTransform.anchoredPosition.x;
                        var pt_y = PtList[i].rectTransform.anchoredPosition.y;
                        //상
                        float min_dist = rect.yMax - pt_y;
                        int way = 0;

                        //하
                        float dist = pt_y - rect.yMin;
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            way = 2;
                        }

                        //우
                         dist = rect.xMax - pt_x;
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            way = 1;
                        }

                        //좌
                        dist = pt_x - rect.xMin;
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            way = 3;
                        }

                        switch (way)
                        {
                            case 0:
                                PtList[i].color = Color.red;
                                break;

                            case 1:
                                PtList[i].color = new Color(1, 0.5f, 0, 1f);
                                break;

                            case 2:
                                PtList[i].color = Color.white;
                                break;

                            case 3:
                                PtList[i].color = new Color(0f, 0.5f, 1f, 1f);
                                break;
                        }
                        count_list[way]++;
                        if (dist_list[way] == 0 || (dist_list[way] < min_dist && dist_list[way] > 0))
                        {
                            dist_list[way] = min_dist;
                        }

                    }
                }
            }

            for(int i=distanceList.Count; i< beforeCount; i++)
            {
                PtList[i].gameObject.SetActive(false);
            }
            beforeCount = distanceList.Count;

            switch (AutoState)
            {
                case AutoSetupState.조절중_스크린범위:
                    if (SideZeroPosition == null || SideZeroPosition.Length < 4)
                    {
                        break;
                    }

                    if (dist_list[0] > (SideZeroPosition[0] - SideZeroPosition[2]) * 0.4f
                        || dist_list[2] > (SideZeroPosition[0] - SideZeroPosition[2]) * 0.4f
                        || dist_list[1] > (SideZeroPosition[1] - SideZeroPosition[3]) * 0.4f
                        || dist_list[3] > (SideZeroPosition[1] - SideZeroPosition[3]) * 0.4f)
                    {
                        AutoState = AutoSetupState.오류;
                        break;
                    }

                    int way = -1;
                    int maxCount = 0;
                    for(int i=0; i<4; i++)
                    {
                        if(count_list[i] > maxCount)
                        {
                            way = i;
                            maxCount = count_list[i];
                        }
                    }

                    float side_w = SideZeroPosition[1] - SideZeroPosition[3];
                    float side_h = SideZeroPosition[0] - SideZeroPosition[2];

                    if (way == 0 && dist_list[0] > 0) //상
                    {
                        CurrentView.Data.TouchTop = CurrentView.Data.TouchTop - dist_list[0];// - h
                        CurrentView.Data.ScreenTop = (SideZeroPosition[0] - CurrentView.Data.TouchTop) / side_h;
                        //Debug.Log("상 : " + CurrentSensor.Data.ScreenTop + " / " + SideZeroPosition[0] + " / " + CurrentSensor.Data.TouchTop + " / " + side_h);
                        SetUI();
                    }
                    if (way == 1 && dist_list[1] > 0) //우
                    {
                        CurrentView.Data.TouchRight = CurrentView.Data.TouchRight - dist_list[1]; // - w
                        CurrentView.Data.ScreenRight = (SideZeroPosition[1] - CurrentView.Data.TouchRight) / side_w;
                        //Debug.Log("우 : " + CurrentSensor.Data.ScreenRight + " / " + SideZeroPosition[1] + " / " + CurrentSensor.Data.TouchRight + " / " + side_w);
                        SetUI();
                    }
                    if (way == 2 && dist_list[2] > 0) //하
                    {
                        CurrentView.Data.TouchBottom = CurrentView.Data.TouchBottom + dist_list[2]; // + h
                        CurrentView.Data.ScreenBottom = (CurrentView.Data.TouchBottom - SideZeroPosition[2]) / side_h;
                        //Debug.Log("하 : " + CurrentSensor.Data.ScreenBottom + " / " + SideZeroPosition[2] + " / " + CurrentSensor.Data.TouchBottom + " / " + side_h);
                        SetUI();
                    }
                    if (way == 3 && dist_list[3] > 0) //좌
                    {
                        CurrentView.Data.TouchLeft = CurrentView.Data.TouchLeft + dist_list[3]; // + w
                        CurrentView.Data.ScreenLeft = (CurrentView.Data.TouchLeft - SideZeroPosition[3]) / side_w;
                        //Debug.Log("좌 : " + CurrentSensor.Data.ScreenLeft + " / " + SideZeroPosition[3] + " / " + CurrentSensor.Data.TouchLeft + " / " + side_w);
                        SetUI();
                    }
                    break;
            }


            //if (IsSaveCheck)
            //{
            //    for (int i = 0; i < distanceList.Count; i++)
            //    {
            //        var data = distanceList[i];
            //        int index = Mathf.RoundToInt(data.Angle * 5);
            //        Debug.Log(i + " : " + index + " / " + data.Angle + " / " + data.Distance);
            //    }
            //    IsSaveCheck = false;
            //}

            if (minImage != null)
            {
                switch (AutoState)
                {
                    case AutoSetupState.없음:
                        break;

                    case AutoSetupState.대기: //대기중에 입력이 들어오면 해당 위치는 무시
                        AutoSetupIgnoreList.Add(minDist);
                        break;

                    case AutoSetupState.좌측터치:
                        //if (LeftPointData.Distance > 0)
                        //{
                        //    break;
                        //}
                        LeftPointImage.rectTransform.position = minImage.rectTransform.position;
                        LeftPointData = minDist;
                        SetAutoPointToImage();
                        break;

                    case AutoSetupState.우측터치:
                        //if (RightPointData.Distance > 0)
                        //{
                        //    break;
                        //}
                        RightPointImage.rectTransform.position = minImage.rectTransform.position;
                        RightPointData = minDist;
                        SetAutoPointToImage();
                        break;

                    case AutoSetupState.상단터치:
                        if (UpPointData.Distance > 0)
                        {
                            break;
                        }
                        UpPointImage.rectTransform.position = minImage.rectTransform.position;
                        UpPointData = minDist;
                        SetAutoPointToImage();
                        break;

                    case AutoSetupState.하단터치:
                        var image = GetIdleDownImage();
                        image.gameObject.SetActive(true);
                        image.rectTransform.position = minImage.rectTransform.position;

                        DownPointDataList.Add(minDist);
                        break;


                    case AutoSetupState.조절중:
                        break;
                }
            }
            #endregion

        }
        //catch (Exception ex)
        //{
        //    Debug.Log(ex);
        //}
    }


    bool mIsLoad = false;
    public void SetUI()
    {
        mIsLoad = false;
        TitleText.text = Sensor.Id + " (" + Sensor.Data.SerialNo + ")";

        AngleSlider.Val = CurrentView.Data.Angle;
        ScaleSlider.Val = CurrentView.Data.Scale;

        FlipXToggle.isOn = CurrentView.Data.IsXFlip;
        FlipYToggle.isOn = CurrentView.Data.IsYFlip;

        //CurrentSensor.Data.TouchTop = Mathf.Clamp(CurrentSensor.Data.TouchTop, -400, 400);
        //CurrentSensor.Data.TouchRight = Mathf.Clamp(CurrentSensor.Data.TouchRight, -690, 690);
        //CurrentSensor.Data.TouchBottom = Mathf.Clamp(CurrentSensor.Data.TouchBottom, -400, 400);
        //CurrentSensor.Data.TouchLeft = Mathf.Clamp(CurrentSensor.Data.TouchLeft, -690, 690);


        ArrowList[0].anchoredPosition = new Vector2(CurrentView.Data.TouchRight, CurrentView.Data.TouchTop);
        ArrowList[1].anchoredPosition = new Vector2(CurrentView.Data.TouchRight, CurrentView.Data.TouchBottom);
        ArrowList[2].anchoredPosition = new Vector2(CurrentView.Data.TouchLeft, CurrentView.Data.TouchBottom);
        ArrowList[3].anchoredPosition = new Vector2(CurrentView.Data.TouchLeft, CurrentView.Data.TouchTop);

        //LineRenderer.Points = new Vector2[]
        //{
        //    ArrowList[0].anchoredPosition,
        //    ArrowList[1].anchoredPosition,
        //    ArrowList[2].anchoredPosition,
        //    ArrowList[3].anchoredPosition,
        //    ArrowList[0].anchoredPosition
        //};

        float touchWidth = CurrentView.Data.TouchRight - CurrentView.Data.TouchLeft;
        float touchHeight = CurrentView.Data.TouchTop - CurrentView.Data.TouchBottom;

        TouchRectList[0].sizeDelta = new Vector2(touchWidth, 15f);
        TouchRectList[1].sizeDelta = new Vector2(15f, touchHeight);
        TouchRectList[2].sizeDelta = new Vector2(touchWidth, 15f);
        TouchRectList[3].sizeDelta = new Vector2(15f, touchHeight);

        TouchRectList[0].anchoredPosition = new Vector2(CurrentView.Data.TouchLeft + touchWidth * 0.5f, CurrentView.Data.TouchTop);
        TouchRectList[1].anchoredPosition = new Vector2(CurrentView.Data.TouchRight, CurrentView.Data.TouchBottom + touchHeight * 0.5f);
        TouchRectList[2].anchoredPosition = new Vector2(CurrentView.Data.TouchLeft + touchWidth * 0.5f, CurrentView.Data.TouchBottom);
        TouchRectList[3].anchoredPosition = new Vector2(CurrentView.Data.TouchLeft, CurrentView.Data.TouchBottom + touchHeight * 0.5f);


        Layout_X.spacing = (Layout_X.GetComponent<RectTransform>().rect.height / 20) - 1;
        Layout_Y.spacing = (Layout_Y.GetComponent<RectTransform>().rect.width / 30) - 1;


        SizeWidthRate.Val = CurrentView.Data.SizeWidthRate;
        SizeHeightRate.Val = CurrentView.Data.SizeHeightRate;

        GroupCountSlider.Val = CurrentView.Data.GroupMaxCount;
        GroupIndexSlider.Val = CurrentView.Data.GroupAngle;


        mIsLoad = true;

        OnSizeSliderChange();

        //CurrentSensor.SetData(Data, ScreenData);
        //CurrentSensor.SetData();

        ChangeScreenRect();

        CurrentView.SetData();
        CurrentView.SetScreenData();

        DrawIgnoe();
    }

    public void OnSizeSliderChange()
    {
        if(mIsLoad == false)
        {
            return;
        }
        CurrentView.Data.SizeWidthRate = SizeWidthRate.Val;
        CurrentView.Data.SizeHeightRate = SizeHeightRate.Val;

        float width = TouchRectList[1].anchoredPosition.x - TouchRectList[3].anchoredPosition.x;
        float height = TouchRectList[0].anchoredPosition.y - TouchRectList[2].anchoredPosition.y;
        width = width * SizeWidthRate.Val / CurrentView.Data.Scale;
        height = height * SizeHeightRate.Val / CurrentView.Data.Scale;


        var w2 = width / (1 - CurrentView.Data.ScreenLeft - CurrentView.Data.ScreenRight);
        var h2 = height / (1 - CurrentView.Data.ScreenTop - CurrentView.Data.ScreenBottom);
        //ScreenSizeText.text = "Width : " + w2.ToString("F1") + "\nHeight : " + h2.ToString("F1");
        ScreenSizeText.text = 
            "Width : " + w2.ToString("F1") 
            + "\nHeight : " + h2.ToString("F1")
            + "\n16 : " + (h2 / (w2 / 16f)).ToString("F1");
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }




    #region 자동설정

    public void OnAutoSetupButtonClick()
    {
        AutoSetupPanel.gameObject.SetActive(!AutoSetupPanel.gameObject.activeSelf);
    }
    public void OnAutoSetupStartButtonClick(int type)
    {
        if (type == 0) //전체시작
        {
            Is3PointType = false;
            ClearAutoSetup();
            StartAutoSetupWizard();
        }
        if (type == 1) //좌측만새로
        {
            Is3PointType = false;
            AutoSetupClearLeft();
            StartAutoSetupWizard();
        }
        if (type == 2) //우측만새로
        {
            Is3PointType = false;
            AutoSetupClearRight();
            StartAutoSetupWizard();
        }
        if (type == 3) //상단만 새로
        {
            Is3PointType = false;
            AutoSetupClearUp();
            StartAutoSetupWizard();
        }
        if (type == 4) //하단만 새로
        {
            Is3PointType = false;
            AutoSetupClearDown();
            StartAutoSetupWizard();
        }
    }

    public void OnMinDistSliderChange()
    {
        if(CurrentView != null)
        {
            CurrentView.Data.MinDist = MinDistCheckSlider.Val;
        }
    }

    private void SetAutoPointToImage()
    {
        if (LeftPointData.Distance > 0)
        {
            float left_len = LeftPointData.Distance * CurrentView.Data.Scale;
            float angle = (LeftPointData.Angle + CurrentView.Data.Angle + 720) % 360;
            float left_x = LidarView.SinData[(int)(angle * 10)] * left_len;
            float left_y = LidarView.CosData[(int)(angle * 10)] * left_len;
            var left_v2 = new Vector2(CurrentView.Data.IsXFlip ? -left_x : left_x, CurrentView.Data.IsYFlip ? -left_y : left_y);

            LeftPointImage.gameObject.SetActive(true);
            LeftPointImage.rectTransform.anchoredPosition = left_v2;
        }
        else
        {
            LeftPointImage.gameObject.SetActive(false);
        }

        if (RightPointData.Distance > 0)
        {
            float right_len = RightPointData.Distance * CurrentView.Data.Scale;
            float angle = (RightPointData.Angle + CurrentView.Data.Angle + 720) % 360;
            float right_x = LidarView.SinData[(int)(angle * 10)] * right_len;
            float right_y = LidarView.CosData[(int)(angle * 10)] * right_len;
            var right_v2 = new Vector2(CurrentView.Data.IsXFlip ? -right_x : right_x, CurrentView.Data.IsYFlip ? -right_y : right_y);

            RightPointImage.gameObject.SetActive(true);
            RightPointImage.rectTransform.anchoredPosition = right_v2;
        }
        else
        {
            RightPointImage.gameObject.SetActive(false);
        }

        if (UpPointData.Distance > 0)
        {
            float up_len = UpPointData.Distance * CurrentView.Data.Scale;
            float angle = (UpPointData.Angle + CurrentView.Data.Angle + 720) % 360;
            float up_x = LidarView.SinData[(int)(angle * 10)] * up_len;
            float up_y = LidarView.CosData[(int)(angle * 10)] * up_len;
            var up_v2 = new Vector2(CurrentView.Data.IsXFlip ? -up_x : up_x, CurrentView.Data.IsYFlip ? -up_y : up_y);

            UpPointImage.gameObject.SetActive(true);
            UpPointImage.rectTransform.anchoredPosition = up_v2;
        }
        else
        {
            UpPointImage.gameObject.SetActive(false);
        }


        foreach (var image in DownPointImageList)
        {
            image.gameObject.SetActive(false);
        }

        foreach (var data in DownPointDataList)
        {
            var image = GetIdleDownImage();
            image.gameObject.SetActive(true);

            float down_len = data.Distance * CurrentView.Data.Scale;
            float angle = (data.Angle + CurrentView.Data.Angle + 720) % 360;
            float down_x = LidarView.SinData[(int)(angle * 10)] * down_len;
            float down_y = LidarView.CosData[(int)(angle * 10)] * down_len;
            var down_v2 = new Vector2(CurrentView.Data.IsXFlip ? -down_x : down_x, CurrentView.Data.IsYFlip ? -down_y : down_y);
            image.rectTransform.anchoredPosition = down_v2;
        }

        AutoSetupMakeDown();
    }

    private Coroutine AutoSetupWizardCoroutine = null;
    private Coroutine AutoSetupCoroutine = null;
    private void ClearAutoSetupUI()
    {
        UpTouchImage.gameObject.SetActive(false);
        RightTouchImage.gameObject.SetActive(false);
        LeftTouchImage.gameObject.SetActive(false);
        DownTouchImage.gameObject.SetActive(false);

        BlackPanel.gameObject.SetActive(false);
        AutoSetupStateText.gameObject.SetActive(false);
    }
    private void ClearAutoSetup()
    {
        ClearAutoSetupUI();
        AutoState = AutoSetupState.없음;

        AutoSetupClearLeft();
        AutoSetupClearRight();
        AutoSetupClearUp();
        AutoSetupClearDown();

        AutoSetupLeftButton.interactable = false;
        AutoSetutRightButton.interactable = false;
        AutoSetupUpButton.interactable = false;
        AutoSetupDownButton.interactable = false;

        ScreenCheckTop = false;
        ScreenCheckRight = false;
        ScreenCheckBottom = false;
        ScreenCheckLeft = false;
    }
    private void AutoSetupClearLeft()
    {
        LeftPointImage.gameObject.SetActive(false);
        LeftTouchImage.gameObject.SetActive(false);
        LeftTouchImage.color = Color.white;
        LeftPointData = new DistanceInfo();
    }
    private void AutoSetupClearRight()
    {
        RightPointImage.gameObject.SetActive(false);
        RightTouchImage.gameObject.SetActive(false);
        RightTouchImage.color = Color.white;
        RightPointData = new DistanceInfo();
    }
    private void AutoSetupClearUp()
    {
        UpPointImage.gameObject.SetActive(false);
        UpTouchImage.gameObject.SetActive(false);
        UpPointData = new DistanceInfo();
    }
    private void AutoSetupClearDown()
    {
        CheckDownLeft = false;
        CheckDownRight = false;
        foreach (var image in DownPointImageList)
        {
            image.gameObject.SetActive(false);
        }
        DownTouchImage.gameObject.SetActive(false);
        DownPointDataList.Clear();

        DownLine.gameObject.SetActive(false);
    }

    public void StopAutoSetup()
    {
        ClearAutoSetup();

        if (AutoSetupWizardCoroutine != null)
            StopCoroutine(AutoSetupWizardCoroutine);


        if (AutoSetupCoroutine != null)
        {
            StopCoroutine(AutoSetupCoroutine);
        }
    }

    private void StartAutoSetupWizard()
    {
        if (AutoSetupWizardCoroutine != null)
            StopCoroutine(AutoSetupWizardCoroutine);

        AutoSetupWizardCoroutine = StartCoroutine(AutoSetupWizard());
    }
    private IEnumerator AutoSetupWizard()
    {
        AutoSetupPanel.gameObject.SetActive(false);

        BlackPanel.gameObject.SetActive(true);
        AutoSetupStateText.gameObject.SetActive(true);
        AutoSetupStateText.text = "";
        //LeftTouchImage.rectTransform.anchoredPosition = new Vector2(-(Screen.width / 8f * 2f), -(Screen.height / 8f * 2f));
        //RightTouchImage.rectTransform.anchoredPosition = new Vector2((Screen.width / 8f * 2f), -(Screen.height / 8f * 2f));
        //UpTouchImage.rectTransform.anchoredPosition = new Vector2(0, (Screen.height / 8f * 2f));
        //DownTouchImage.rectTransform.anchoredPosition = new Vector2(0, -(Screen.height / 8f * 2f));
        //DownTouchImage.rectTransform.sizeDelta = new Vector2(
        //    RightTouchImage.rectTransform.anchoredPosition.x - LeftTouchImage.rectTransform.anchoredPosition.x,
        //    DownTouchImage.rectTransform.sizeDelta.y);

        AutoState = AutoSetupState.없음;
        AutoSetupIgnoreList.Clear();
        AutoSetupStateText.text = "잠시 기다려주세요.";
        BlackPanel.color = BlackPanelNoTouchColor;


        FlipXToggle.isOn = true;
        FlipYToggle.isOn = true;
        AngleSlider.Val = 45;

        yield return new WaitForSeconds(1f);

        AutoState = AutoSetupState.대기;
        yield return new WaitForSeconds(2f);

        bool delayCheck = false;

        AutoState = AutoSetupState.좌측터치;
        AutoSetupStateText.text = "해당 지점에 3초간 공을 대어주세요.";
        BlackPanel.color = BlackPanelTouchColor;
        LeftTouchImage.gameObject.SetActive(true);
        while (LeftPointData.Distance == 0)
        {
            delayCheck = true;
            yield return null;
        }
        if (delayCheck)
        {
            for (int i = 3; i > 0; i--)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.Play(CountSound);
                }
                AutoSetupStateText.text = (i.ToString());
                yield return new WaitForSeconds(1f);
            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play(CountEndSound);
            }
        }

        AutoState = AutoSetupState.조절중;
        BlackPanel.color = BlackPanelNoTouchColor;
        AutoSetupStateText.text = "잠시 기다려주세요.";
        LeftTouchImage.gameObject.SetActive(false);



        if (delayCheck)
            yield return new WaitForSeconds(1f);
        delayCheck = false;

        AutoState = AutoSetupState.우측터치;
        AutoSetupStateText.text = "해당 지점에 3초간 공을 대어주세요.";
        BlackPanel.color = BlackPanelTouchColor;
        RightTouchImage.gameObject.SetActive(true);
        while (RightPointData.Distance == 0)
        {
            delayCheck = true;
            yield return null;
        }
        if (delayCheck)
        {
            for (int i = 3; i > 0; i--)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.Play(CountSound);
                }
                AutoSetupStateText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play(CountEndSound);
            }
        }
        AutoState = AutoSetupState.조절중;



        BlackPanel.color = BlackPanelNoTouchColor;
        var dist_w = Vector2.Distance(GetDefaultPosition(LeftPointData), GetDefaultPosition(RightPointData));
        Debug.Log("좌우 정규화 거리 : " + dist_w);
        if (dist_w < 30)
        {
            AutoSetupStateText.text = "좌표를 정상적으로 인식하지 못했습니다.\n잠시 후 재시도 해주세요.";
            yield return new WaitForSeconds(3f);

            ClearAutoSetupUI();
            yield break;
        }


        AutoSetupStateText.text = "잠시 기다려주세요.";
        RightTouchImage.gameObject.SetActive(false);

        if (delayCheck)
            yield return new WaitForSeconds(1f);
        delayCheck = false;

        AutoState = AutoSetupState.상단터치;
        AutoSetupStateText.text = "해당 지점에 공을 정확히 맞혀주세요";
        BlackPanel.color = BlackPanelTouchColor;
        UpTouchImage.gameObject.SetActive(true);
        while (UpPointData.Distance == 0)
        {
            delayCheck = true;
            yield return null;
        }
        if (CheckDownLeft == false && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(CountEndSound);
        }


        BlackPanel.color = BlackPanelNoTouchColor;
        var dist_h = Vector2.Distance(GetDefaultPosition(UpPointData), GetDefaultPosition(new DistanceInfo()
        {
            Angle = (LeftPointData.Angle + RightPointData.Angle) * 0.5f,
            Distance = (LeftPointData.Distance + RightPointData.Distance) * 0.5f
        }));
        Debug.Log("상하 정규화 거리 : " + dist_h);
        if (dist_h < 30)
        {
            AutoSetupStateText.text = "좌표를 정상적으로 인식하지 못했습니다.\n잠시 후 재시도 해주세요.";
            yield return new WaitForSeconds(3f);

            ClearAutoSetupUI();
            yield break;
        }


        AutoSetupStateText.text = "잠시 기다려주세요.";
        UpTouchImage.gameObject.SetActive(false);



        if (delayCheck)
            yield return new WaitForSeconds(1f);
        delayCheck = false;

        if (Is3PointType)
        {
        }
        else
        {
            AutoState = AutoSetupState.하단터치;

            AutoSetupStateText.text = "하단 선을 따라서\n공으로 천천히 터치하며 이동해주세요."; //
            BlackPanel.color = BlackPanelTouchColor;

            DownTouchImage.gameObject.SetActive(true);

            LeftTouchImage.gameObject.SetActive(true);
            RightTouchImage.gameObject.SetActive(true);

            LeftTouchImage.color = Color.gray;
            RightTouchImage.color = Color.gray;
            while (CheckDownLeft == false || CheckDownRight == false)
            {
                yield return null;

                foreach (var data in DownPointDataList)
                {
                    if (Vector2.Distance(GetDefaultPosition(LeftPointData), GetDefaultPosition(data)) < DownDistCheckSlider.Val)
                    {
                        if (CheckDownLeft == false && SoundManager.Instance != null)
                        {
                            SoundManager.Instance.Play(CountEndSound);
                        }
                        CheckDownLeft = true;
                        LeftTouchImage.color = Color.white;
                    }
                    if (Vector2.Distance(GetDefaultPosition(RightPointData), GetDefaultPosition(data)) < DownDistCheckSlider.Val)
                    {
                        if (CheckDownRight == false && SoundManager.Instance != null)
                        {
                            SoundManager.Instance.Play(CountEndSound);
                        }
                        CheckDownRight = true;
                        RightTouchImage.color = Color.white;
                    }
                }

                AutoSetupMakeDown();
                delayCheck = true;
            }
            BlackPanel.color = BlackPanelNoTouchColor;
            AutoSetupStateText.text = "잠시 기다려주세요.";
            if (delayCheck)
                yield return new WaitForSeconds(1f);
            DownTouchImage.gameObject.SetActive(false);
            LeftTouchImage.gameObject.SetActive(false);
            RightTouchImage.gameObject.SetActive(false);
        }

        if (AutoSetupCoroutine != null)
        {
            StopCoroutine(AutoSetupCoroutine);
        }
        AutoSetupCoroutine = StartCoroutine(AutoSetup());
        yield return AutoSetupCoroutine;
    }
    private void AutoSetupMakeDown()
    {
        int count = 0;
        float sum_x = 0;
        float sum_y = 0;

        foreach (var dot in DownPointImageList)
        {
            if (dot.gameObject.activeSelf == false)
            {
                continue;
            }

            count++;

            float x = dot.rectTransform.anchoredPosition.x;
            sum_x += x;
            sum_y += dot.rectTransform.anchoredPosition.y;
        }

        //Debug.Log("Count : " + count);
        if (count < 2)
        {
            DownLine.gameObject.SetActive(false);
            return;
        }

        float avg_x = sum_x / count;
        float avg_y = sum_y / count;

        float a1 = 0;
        float a2 = 0;

        foreach (var dot in DownPointImageList)
        {
            if (dot.gameObject.activeSelf == false)
            {
                continue;
            }

            a1 += (dot.rectTransform.anchoredPosition.x - avg_x) * (dot.rectTransform.anchoredPosition.y - avg_y);
            a2 += (dot.rectTransform.anchoredPosition.x - avg_x) * (dot.rectTransform.anchoredPosition.x - avg_x);
        }

        float a = a1 / a2;
        float b = avg_y - (avg_x * a);

        float left_x = LeftPointImage.rectTransform.anchoredPosition.x;
        float right_x = RightPointImage.rectTransform.anchoredPosition.x;

        DownLine.Points = new Vector2[]
        {
            new Vector2(left_x, a * left_x + b),
            new Vector2(right_x, a * right_x + b)
        };
        DownLine.SetAllDirty();
        DownLine.gameObject.SetActive(true);
    }

    private IEnumerator AutoSetup()
    {
        Debug.Log("자동 설정");
        AutoSetupStateText.text = "잠시 기다려주세요.";
        Debug.Log("LefDown " + LeftPointData.Angle + " / " + LeftPointData.Distance);
        Debug.Log("RightDown " + RightPointData.Angle + " / " + RightPointData.Distance);
        Debug.Log("Up " + UpPointData.Angle + " / " + UpPointData.Distance);

        SetAutoPointToImage();

        if (LeftPointData.Distance <= 0)
        {
            Debug.Log("좌측 데이터 없음");
            ClearAutoSetup();
            yield break;
        }
        if (RightPointData.Distance <= 0)
        {
            Debug.Log("우측 데이터 없음");
            ClearAutoSetup();
            yield break;
        }
        if (UpPointData.Distance <= 0)
        {
            Debug.Log("상단 데이터 없음");
            ClearAutoSetup();
            yield break;
        }

        AutoSetupLeftButton.interactable = true;
        AutoSetutRightButton.interactable = true;
        AutoSetupUpButton.interactable = true;
        AutoSetupDownButton.interactable = true;


        AutoState = AutoSetupState.조절중;


        CurrentView.Data.ScreenTop = 0;
        CurrentView.Data.ScreenRight = 0;
        CurrentView.Data.ScreenBottom = 0;
        CurrentView.Data.ScreenLeft = 0;
        ChangeScreenRect();
        yield return null;


        //각도 조절
        var left_v2 = LeftPointImage.rectTransform.anchoredPosition;
        var right_v2 = RightPointImage.rectTransform.anchoredPosition;

        var new_angle = CurrentView.Data.Angle;

        if (Is3PointType == false && DownLine.gameObject.activeSelf)
        {
            left_v2 = DownLine.Points[0];
            right_v2 = DownLine.Points[1];
        }

        Debug.Log("left : " + left_v2 + "/ right : " + right_v2);
        var angle = Mathf.Atan2(right_v2.y - left_v2.y, right_v2.x - left_v2.x) * Mathf.Rad2Deg;

        if (CurrentView.Data.IsXFlip && CurrentView.Data.IsYFlip)
        {
            new_angle = CurrentView.Data.Angle + angle;
        }
        else
        {
            new_angle = CurrentView.Data.Angle - angle;
        }

        if (new_angle < -180)
        {
            new_angle += 360;
        }
        if (new_angle > 180)
        {
            new_angle -= 360;
        }
        Debug.Log("각도 : " + angle + " / " + CurrentView.Data.Angle + " -> " + new_angle);
        AngleSlider.Val = new_angle;
        yield return null;



        //좌우반전 조절
        left_v2 = LeftPointImage.rectTransform.anchoredPosition;
        right_v2 = RightPointImage.rectTransform.anchoredPosition;

        if (left_v2.x > right_v2.x)
        {
            FlipXToggle.isOn = !FlipXToggle.isOn;
        }
        yield return null;


        //상하반전 조절
        left_v2 = LeftPointImage.rectTransform.anchoredPosition;
        right_v2 = RightPointImage.rectTransform.anchoredPosition;
        var up_v2 = UpPointImage.rectTransform.anchoredPosition;
        if (left_v2.y > up_v2.y)
        {
            FlipYToggle.isOn = !FlipYToggle.isOn;
        }
        yield return null;



        //스케일 조절
        left_v2 = LeftPointImage.rectTransform.anchoredPosition;
        right_v2 = RightPointImage.rectTransform.anchoredPosition;
        var dist = Vector2.Distance(left_v2, right_v2);
        var targetDist = 200;

        ScaleSlider.Val *= targetDist / dist;
        ChangeViewScale(1);

        yield return null;



        //뷰 포트 위치 및 크기조절
        left_v2 = LeftPointImage.rectTransform.anchoredPosition;
        right_v2 = RightPointImage.rectTransform.anchoredPosition;
        up_v2 = UpPointImage.rectTransform.anchoredPosition;

        dist = Vector2.Distance(left_v2, right_v2);
        var center = (left_v2 + right_v2 + up_v2) / 3f;
        ViewRect.anchoredPosition = -new Vector2(center.x, center.y * 1.1f);


        //터치영역 사각형 조절
        float width = right_v2.x - left_v2.x;
        float height = up_v2.y - ((left_v2.y + right_v2.y) * 0.5f);

        CurrentView.Data.TouchLeft = left_v2.x - width / 2f;
        CurrentView.Data.TouchRight = right_v2.x + width / 2f;
        CurrentView.Data.TouchTop = up_v2.y + height / 2f;
        CurrentView.Data.TouchBottom = left_v2.y - height / 2f;

        SetUI();
        yield return null;

        SideZeroPosition = new float[]
        {
            CurrentView.Data.TouchTop,
            CurrentView.Data.TouchRight,
            CurrentView.Data.TouchBottom,
            CurrentView.Data.TouchLeft
        };

        yield return new WaitForSeconds(2f);
        AutoState = AutoSetupState.조절중_스크린범위;
        float time = 0;
        while (time < 3)
        {
            time += Time.deltaTime;
            yield return null;

            switch (AutoState)
            {
                case AutoSetupState.오류:
                    AutoSetupStateText.text = "좌표를 정상적으로 인식하지 못했습니다.\n잠시 후 재시도 해주세요.";
                    yield return new WaitForSeconds(3f);

                    ClearAutoSetupUI();
                    yield break;
            }
        }

        BlackPanel.gameObject.SetActive(false);
        AutoSetupStateText.gameObject.SetActive(false);
        AutoState = AutoSetupState.없음;

        MsgBox.Show("설정이 완료됐습니다.\n공을 던져 좌표를 확인해보세요").SetAutoCloseTime(5f);//
        //ClearAutoSetup();
    }
    private void MsgBoxClose()
    {
        MsgBox.Close();
    }
    private Image GetIdleDownImage()
    {
        foreach (var image in DownPointImageList)
        {
            if (image.gameObject.activeSelf == false)
            {
                return image;
            }
        }

        var item = Instantiate(DownPointImagePrefab, DownPointImagePrefab.transform.parent);
        DownPointImageList.Add(item);
        return item;
    }


    private Vector2 GetDefaultPosition(DistanceInfo info)
    {
        float angle = (info.Angle + 720) % 360;
        float x = LidarView.SinData[(int)(angle * 10)] * info.Distance;
        float y = LidarView.CosData[(int)(angle * 10)] * info.Distance;

        return new Vector2(x, y);
    }


    private Image GetIdleDefaultPt()
    {
        foreach (var p in DefaultPtList)
        {
            if (p.gameObject.activeSelf == false)
            {
                return p;
            }
        }

        var item = Instantiate(DefaultPt, DefaultPt.transform.parent);
        DefaultPtList.Add(item);
        return item;
    }
    #endregion





    public void OnIgnoeAddButtonClick()
    {
        CurrentView.Data.IgnoreList.Add(new Rect(-50, -50, 100, 100));
        DrawIgnoe();
    }

    public void IgnoeItemToData()
    {
        CurrentView.Data.IgnoreList.Clear();
        foreach (var item in IgnoeItemList)
        {
            if (item.gameObject.activeSelf)
            {
                CurrentView.Data.IgnoreList.Add(item.Rect);
            }
        }
    }


    private void DrawIgnoe()
    {
        foreach(var item in IgnoeItemList)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var ignoe in CurrentView.Data.IgnoreList)
        {
            var item = GetIgnoeItem();
            item.SetRect(ignoe);
            item.gameObject.SetActive(true);
        }
    }

    private IgnoeSettingItem GetIgnoeItem()
    {
        foreach (var item in IgnoeItemList)
        {
            if (item.gameObject.activeSelf == false)
            {
                return item;
            }
        }

        var ignoe = Instantiate(IgnoeItem, IgnoeItem.transform.parent);
        IgnoeItemList.Add(ignoe);
        return ignoe;
    }




    public enum ScreenRectPosition
    {
        Top, Right, Bottom, Left
    }
    public enum ArrowPosition
    {
        TopRight, BottomRIght, BottomLeft, TopLeft
    }




    public enum AutoSetupState
    {
        없음, 대기, 좌측터치, 우측터치, 상단터치, 하단터치, 조절중, 조절중_스크린범위, 오류
    }
}


