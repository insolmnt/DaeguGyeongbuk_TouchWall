using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepthSensorTopViewSetting : MonoBehaviour
{
    public RectTransform ViewRect;

    public RectTransform[] ArrowList;
    public RectTransform[] TouchRectList;

    public RectTransform pt;
    private RectTransform[] PtList;

    [Header("조작")]
    public Toggle FlipXToggle;
    public Toggle FlipYToggle;

    public SliderCtr ScaleSlider;
    public SliderCtr AngleSlider;

    public SliderCtr YLeftSlider;
    public SliderCtr YRightSlider;
    public SliderCtr PositionXSlider;
    public SliderCtr PositionYSlider;

    public SliderCtr GroupCountSlider;
    public SliderCtr GroupIndexSlider;

    public Text StateText;

    [Header("스크린 영역 설정")]
    public RectTransform[] ScreenPointBarList;
    public RectTransform[] ScreenPointRedRectList;
    private int ScreenPointChangeIndex = -1;
    private float ScreenPointStartData = 0;
    private float ScreenPointStartMousePosition = 0;


    [Header("우하단 Depth 카메라 표시")]
    public RawImage DepthViewRawImage;
    private Texture2D DepthViewTexture;
    private Color[] DepthViewColors;
    private int DepthViewDist = 2;
    private bool mIsDepthViewBigShow = false;

    [Header("추가메뉴")]
    public GameObject PlusMenuPanel;

    [Header("Data")]
    public DepthSensorTopView View;
    public DepthSensorTopViewData Data;
    public int DepthWidth = 640;
    public int DepthHeight = 480;





    private int TouchPointChangeIndex = -1;
    private float TouchPointStartData = 0;
    private float TouchPointStartMousePosition = 0;

    private int MoveArrowIndex = -1;
    private Vector2 MoveArrowStartMousePosition;
    private Vector2 MoveArrowStartData;

    private bool mIsLoad = false;


    private float ViewScale = 1;
    private void ChangeViewScale(float scale)
    {
        ViewScale = Mathf.Clamp(scale, 0.3f, 5f);
        ViewRect.localScale = new Vector3(ViewScale, ViewScale, 1);
    }




    private void Awake()
    {
        MakePtList();
    }
    void MakePtList()
    {
        if (PtList != null)
        {
            return;
        }

        PtList = new RectTransform[640];
        for (int i = 0; i < PtList.Length; i++)
        {
            PtList[i] = Instantiate(pt, pt.transform.parent);
            PtList[i].gameObject.SetActive(false);
        }
    }


    public void OnTouchPointRectPointDown(int index)
    {
        TouchPointChangeIndex = index;
        switch (index)
        {
            case 0: //top
                TouchPointStartMousePosition = Input.mousePosition.y;
                TouchPointStartData = Data.TouchTop;
                break;
            case 1: //right
                TouchPointStartMousePosition = Input.mousePosition.x;
                TouchPointStartData = Data.TouchRight;
                break;
            case 2: //bottom
                TouchPointStartMousePosition = Input.mousePosition.y;
                TouchPointStartData = Data.TouchBottom;
                break;
            case 3: //left
                TouchPointStartMousePosition = Input.mousePosition.x;
                TouchPointStartData = Data.TouchLeft;
                break;
        }
    }
    
    private void UpdateTouchPointRectChange()
    {
        if (TouchPointChangeIndex < 0)
        {
            return;
        }

        switch (TouchPointChangeIndex)
        {
            case 0: //top
                Data.TouchTop = TouchPointStartData + (Input.mousePosition.y - TouchPointStartMousePosition) / ViewScale;
                break;
            case 1: //right
                Data.TouchRight = TouchPointStartData + (Input.mousePosition.x - TouchPointStartMousePosition) / ViewScale;
                break;
            case 2: //bottom
                Data.TouchBottom = TouchPointStartData + (Input.mousePosition.y - TouchPointStartMousePosition) / ViewScale;
                break;
            case 3: //left
                Data.TouchLeft = TouchPointStartData + (Input.mousePosition.x - TouchPointStartMousePosition) / ViewScale;
                break;
        }

        DataToArrowUI();
        if (Input.GetMouseButton(0) == false)
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
                Data.TouchRight = ArrowList[MoveArrowIndex].anchoredPosition.x;
                Data.TouchTop = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;

            case 1:
                Data.TouchRight = ArrowList[MoveArrowIndex].anchoredPosition.x;
                Data.TouchBottom = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;

            case 2:
                Data.TouchLeft = ArrowList[MoveArrowIndex].anchoredPosition.x;
                Data.TouchBottom = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;

            case 3:
                Data.TouchLeft = ArrowList[MoveArrowIndex].anchoredPosition.x;
                Data.TouchTop = ArrowList[MoveArrowIndex].anchoredPosition.y;
                break;
        }



        DataToArrowUI();
        if (Input.GetMouseButton(0) == false
            && Input.GetKey(KeyCode.Q) == false
            && Input.GetKey(KeyCode.W) == false
            && Input.GetKey(KeyCode.A) == false
            && Input.GetKey(KeyCode.S) == false)
        {
            MoveArrowIndex = -1;
        }
    }




    public void DataToArrowUI()
    {
        Data.TouchTop = Mathf.Clamp(Data.TouchTop, -400, 400);
        Data.TouchRight = Mathf.Clamp(Data.TouchRight, -690, 690);
        Data.TouchBottom = Mathf.Clamp(Data.TouchBottom, -400, 400);
        Data.TouchLeft = Mathf.Clamp(Data.TouchLeft, -690, 690);


        ArrowList[0].anchoredPosition = new Vector2(Data.TouchRight, Data.TouchTop);
        ArrowList[1].anchoredPosition = new Vector2(Data.TouchRight, Data.TouchBottom);
        ArrowList[2].anchoredPosition = new Vector2(Data.TouchLeft, Data.TouchBottom);
        ArrowList[3].anchoredPosition = new Vector2(Data.TouchLeft, Data.TouchTop);

        //LineRenderer.Points = new Vector2[]
        //{
        //    ArrowList[0].anchoredPosition,
        //    ArrowList[1].anchoredPosition,
        //    ArrowList[2].anchoredPosition,
        //    ArrowList[3].anchoredPosition,
        //    ArrowList[0].anchoredPosition
        //};

        float touchWidth = Data.TouchRight - Data.TouchLeft;
        float touchHeight = Data.TouchTop - Data.TouchBottom;

        TouchRectList[0].sizeDelta = new Vector2(touchWidth, 15f);
        TouchRectList[1].sizeDelta = new Vector2(15f, touchHeight);
        TouchRectList[2].sizeDelta = new Vector2(touchWidth, 15f);
        TouchRectList[3].sizeDelta = new Vector2(15f, touchHeight);

        TouchRectList[0].anchoredPosition = new Vector2(Data.TouchLeft + touchWidth * 0.5f, Data.TouchTop);
        TouchRectList[1].anchoredPosition = new Vector2(Data.TouchRight, Data.TouchBottom + touchHeight * 0.5f);
        TouchRectList[2].anchoredPosition = new Vector2(Data.TouchLeft + touchWidth * 0.5f, Data.TouchBottom);
        TouchRectList[3].anchoredPosition = new Vector2(Data.TouchLeft, Data.TouchBottom + touchHeight * 0.5f);

        View.SetData();
    }


    public void OnScreenPointRectPointDown(int index)
    {
        ScreenPointChangeIndex = index;
        switch (index)
        {
            case 0: //Top
                ScreenPointStartMousePosition = Input.mousePosition.y;
                ScreenPointStartData = Data.ScreenTop;
                break;
            case 1: //Right
                ScreenPointStartMousePosition = Input.mousePosition.x;
                ScreenPointStartData = Data.ScreenRight;
                break;
            case 2: //Bottom
                ScreenPointStartMousePosition = Input.mousePosition.y;
                ScreenPointStartData = Data.ScreenBottom;
                break;
            case 3: //Left
                ScreenPointStartMousePosition = Input.mousePosition.x;
                ScreenPointStartData = Data.ScreenLeft;
                break;
        }
    }
    public void ChangeScreenRect()
    {
        View.SetData();

        float width = GetComponent<RectTransform>().rect.width;
        float height = GetComponent<RectTransform>().rect.height;

        float size = width * 0.01f;
        ScreenPointBarList[0].sizeDelta = new Vector2(width, size);
        ScreenPointBarList[1].sizeDelta = new Vector2(size, height);
        ScreenPointBarList[2].sizeDelta = new Vector2(width, size);
        ScreenPointBarList[3].sizeDelta = new Vector2(size, height);

        ScreenPointBarList[0].anchoredPosition = new Vector2(0, -height * Data.ScreenTop);
        ScreenPointBarList[1].anchoredPosition = new Vector2(-width * Data.ScreenRight, 0);
        ScreenPointBarList[2].anchoredPosition = new Vector2(0, height * Data.ScreenBottom);
        ScreenPointBarList[3].anchoredPosition = new Vector2(width * Data.ScreenLeft, 0);

        ScreenPointRedRectList[0].sizeDelta = new Vector2(size * 3f, size * 3f);
        ScreenPointRedRectList[1].sizeDelta = new Vector2(size * 3f, size * 3f);
        ScreenPointRedRectList[2].sizeDelta = new Vector2(size * 3f, size * 3f);
        ScreenPointRedRectList[3].sizeDelta = new Vector2(size * 3f, size * 3f);

        ScreenPointRedRectList[4].sizeDelta = new Vector2(size * 2f, size * 2f);
        ScreenPointRedRectList[5].sizeDelta = new Vector2(size * 2f, size * 2f);
        ScreenPointRedRectList[6].sizeDelta = new Vector2(size * 2f, size * 2f);
        ScreenPointRedRectList[7].sizeDelta = new Vector2(size * 2f, size * 2f);

        ScreenPointRedRectList[0].anchoredPosition = new Vector2(-width * Data.ScreenRight - size * 0.5f, -height * Data.ScreenTop - size * 0.5f);
        ScreenPointRedRectList[1].anchoredPosition = new Vector2(-width * Data.ScreenRight - size * 0.5f, height * Data.ScreenBottom + size * 0.5f);
        ScreenPointRedRectList[2].anchoredPosition = new Vector2(width * Data.ScreenLeft + size * 0.5f, height * Data.ScreenBottom + size * 0.5f);
        ScreenPointRedRectList[3].anchoredPosition = new Vector2(width * Data.ScreenLeft + size * 0.5f, -height * Data.ScreenTop - size * 0.5f);

        ScreenPointRedRectList[4].localPosition = new Vector2(ScreenPointRedRectList[0].localPosition.x, (ScreenPointRedRectList[0].localPosition.y + ScreenPointRedRectList[1].localPosition.y) / 2);
        ScreenPointRedRectList[5].localPosition = new Vector2((ScreenPointRedRectList[1].localPosition.x + ScreenPointRedRectList[2].localPosition.x) / 2, ScreenPointRedRectList[1].localPosition.y);
        ScreenPointRedRectList[6].localPosition = new Vector2(ScreenPointRedRectList[2].localPosition.x, (ScreenPointRedRectList[2].localPosition.y + ScreenPointRedRectList[3].localPosition.y) / 2);
        ScreenPointRedRectList[7].localPosition = new Vector2((ScreenPointRedRectList[3].localPosition.x + ScreenPointRedRectList[0].localPosition.x) / 2, ScreenPointRedRectList[3].localPosition.y);


        //LidarManager.SetData(Data, Manager.ScreenData);
    }
    private void UpdateChangeScreenRect()
    {
        if (ScreenPointChangeIndex >= 0)
        {
            switch (ScreenPointChangeIndex)
            {
                case 0: //Top
                    Data.ScreenTop = Mathf.Clamp(ScreenPointStartData - (Input.mousePosition.y - ScreenPointStartMousePosition) / View.CurrentMonitor.ScreenHeight, 0, 1 - Data.ScreenBottom - 0.05f);
                    break;
                case 1: //Right
                    Data.ScreenRight = Mathf.Clamp(ScreenPointStartData - (Input.mousePosition.x - ScreenPointStartMousePosition) / View.CurrentMonitor.ScreenWidth, 0, 1 - Data.ScreenLeft - 0.05f);
                    break;
                case 2: //Bottom
                    Data.ScreenBottom = Mathf.Clamp(ScreenPointStartData + (Input.mousePosition.y - ScreenPointStartMousePosition) / View.CurrentMonitor.ScreenHeight, 0, 1 - Data.ScreenTop - 0.05f);
                    break;
                case 3: //Left
                    Data.ScreenLeft = Mathf.Clamp(ScreenPointStartData + (Input.mousePosition.x - ScreenPointStartMousePosition) / View.CurrentMonitor.ScreenWidth, 0, 1 - Data.ScreenRight - 0.05f);
                    break;
                default:
                    break;
            }

            ChangeScreenRect();


            if (Input.GetMouseButton(0) == false)
            {
                ScreenPointChangeIndex = -1;
            }
        }
    }



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
            ViewRect.anchoredPosition = MoveCameraStartPosition + ((Vector2)Input.mousePosition - MoveCameraStartMousePosition);

            if (Input.GetMouseButton(0) == false)
            {
                IsCameraViewMove = false;
            }
        }
    }

    public void OnXFlipToggleChange()
    {
        Data.IsXFlip = FlipXToggle.isOn;
    }

    public void OnYFlipToggleChange()
    {
        Data.IsYFlip = FlipYToggle.isOn;
    }


    public void OnAngleSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }
        Data.Angle = AngleSlider.Val;
        View.SetDepthCamera();
    }
    public void OnScaleSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }
        Data.Scale = ScaleSlider.Val;
    }

    public void OnYSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }
        Data.YLeft = (int)YLeftSlider.Val;
        Data.YRight = (int)YRightSlider.Val;
        View.SetYData();
    }

    public void OnGroupSettingChange()
    {
        if (mIsLoad == false)
        {
            return;
        }
        Data.GroupMaxCount = (int)GroupCountSlider.Val;
        Data.GroupAngle = (int)GroupIndexSlider.Val;
    }

    public void OnCmeraPositionSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }
        Data.CameraPositionX = PositionXSlider.Val;
        Data.CameraPositionY = PositionYSlider.Val;
        View.SetDepthCamera();
    }

    public void OnDepthViewClick()
    {
        mIsDepthViewBigShow = !mIsDepthViewBigShow;
        if (mIsDepthViewBigShow)
        {
            DepthViewRawImage.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        }
        else
        {
            DepthViewRawImage.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        }
    }

    //public void OnXSliderChange()
    //{
    //    if (mIsLoad == false)
    //    {
    //        return;
    //    }
    //    Data.StartX = (int)XSlider.MinValue;
    //    Data.EndX = (int)XSlider.MaxValue;

    //    if (PtList == null)
    //    {
    //        MakePtList();
    //    }

    //    for (int i = 0; i < PtList.Length; i++)
    //    {
    //        var pt = PtList[i].GetComponent<Image>();
    //        if (pt != null)
    //        {
    //            pt.color = (i >= Data.StartX && i <= Data.EndX) ? Color.red : Color.blue;
    //        }
    //    }
    //}

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            ChangeViewScale(ViewScale + scroll);
        }

        if (MoveArrowIndex < 0)
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


        UpdateMoveCamera();

        UpdateArrowMove();
        UpdateTouchPointRectChange();

        UpdateChangeScreenRect();

        StateText.text = "Input Point : " + View.InputIndex + "\nTouch Count : " + View.TouchCount;
    }






    private void OnDisable()
    {
        //if (Screen != null)
        //    Screen.ShowSensorView(false);
        if (View != null)
        {
            View.Sensor.OnDepthRead -= OnDepthChange;
            View = null;
        }

        SensorManager.Instance.ShowDebugMode(false);
    }
    public void ShowSetting(DepthSensorTopView view)
    {
        SensorManager.Instance.ShowDebugMode(true);
        if (View != null)
        {
            View.Sensor.Save();
            View.Sensor.OnDepthRead -= OnDepthChange;
        }

        PlusMenuPanel.gameObject.SetActive(false);

        gameObject.SetActive(true);

        View = view;
        Data = view.Data;
        View.Sensor.OnDepthRead += OnDepthChange;

        DepthWidth = view.Sensor.DepthWidth;
        DepthHeight = view.Sensor.DepthHeight;

        mIsLoad = false;

        AngleSlider.Val = Data.Angle;
        ScaleSlider.Val = Data.Scale;

        FlipXToggle.isOn = Data.IsXFlip;
        FlipYToggle.isOn = Data.IsYFlip;


        YLeftSlider.mSlider.minValue = -View.Sensor.DepthHeight;
        YLeftSlider.mSlider.maxValue = View.Sensor.DepthHeight * 2 - 1;

        YRightSlider.mSlider.minValue = -View.Sensor.DepthHeight;
        YRightSlider.mSlider.maxValue = View.Sensor.DepthHeight * 2 - 1;

        YLeftSlider.Val = Data.YLeft;
        YRightSlider.Val = Data.YRight;

        GroupCountSlider.Val = Data.GroupMaxCount;
        GroupIndexSlider.Val = Data.GroupAngle;

        //XSlider.MinValue = XSlider.LimitMinValue;
        //XSlider.MaxValue = XSlider.LimitMaxValue;


        //XSlider.MinValue = Data.StartX;
        //XSlider.LimitMaxValue = View.Sensor.DepthWidth - 1;
        //XSlider.MaxValue = Data.EndX;

        PositionXSlider.Val = Data.CameraPositionX;
        PositionYSlider.Val = Data.CameraPositionY;

        mIsLoad = true;

        DataToArrowUI();
        ChangeScreenRect();

        if (DepthViewTexture != null)
        {
            Destroy(DepthViewTexture);
        }

        DepthViewTexture = new Texture2D(
            DepthWidth / DepthViewDist, View.Sensor.DepthHeight / DepthViewDist, TextureFormat.ARGB32, false);
        DepthViewColors = new Color[DepthViewTexture.width * DepthViewTexture.height];
        DepthViewRawImage.texture = DepthViewTexture;


        //OnXSliderChange();
    }

    [Range(0, 639)]
    public int Check = 1;
    public void OnDepthChange(ushort[] depthList )
    {
        //foreach (var pt in PtList)
        //{
        //    pt.gameObject.SetActive(false);
        //}

        for (int i = 0; i < DepthWidth; i++)
        {
            int y = View.GetY(i);
            if (y < 0 || y >= View.Sensor.DepthHeight)
            {
                PtList[i].gameObject.SetActive(false);
                continue;
            }
            int depthIndex = i + y * DepthWidth;

            if (depthList[depthIndex] == 0)
            {
                PtList[i].gameObject.SetActive(false);
                continue;
            }

            PtList[i].gameObject.SetActive(true);
            PtList[i].anchoredPosition = View.GetPositon(i, y, depthIndex);

            //if (i == Check)
            //{
            //    Debug.Log("" + i + " -> " + y + " / "+ PtList[i].anchoredPosition);
            //}
        }

        for (int x = 0; x < DepthWidth / DepthViewDist; x++)
        {
            int _y = (View.GetY(x * DepthViewDist) / DepthViewDist);

            for (int y = 0; y < DepthHeight / DepthViewDist; y++)
            {
                int depthIndex = x * DepthViewDist + (y * DepthViewDist) * DepthWidth;
                int colorIndex = x + y * DepthWidth / DepthViewDist;

                //if (y == Data.Y / DepthViewDist)
                if (y == _y)//Sensor.Data.Y
                {
                    DepthViewColors[colorIndex] = Color.red;
                }
                else if (depthList[depthIndex] == 0)
                {
                    DepthViewColors[colorIndex] = Color.green;
                }
                else
                {
                    DepthViewColors[colorIndex] = new Color(
                        depthList[depthIndex] / (5000f),
                        depthList[depthIndex] / (2000f),
                        depthList[depthIndex] / (1000f), 1);
                }

                //if (x * DepthViewDist < Data.StartX || x * DepthViewDist > Data.EndX)
                //{
                //    DepthViewColors[colorIndex] -= new Color(0.2f, 0.2f, 0.2f, 0f);
                //}
            }
        }

        DepthViewTexture.SetPixels(DepthViewColors);
        DepthViewTexture.Apply();
    }



    public void OnPlusButtonClick()
    {
        PlusMenuPanel.gameObject.SetActive(!PlusMenuPanel.gameObject.activeSelf);
    }
    public void OnModeChangeButtonClick()
    {
        MsgBox.Show("BackView 모드로 변경하시겠습니까?").SetButtonType(MsgBoxButtons.OK_CANCEL).OnResult((result) =>
        {
            switch (result)
            {
                case DialogResult.YES_OK:
                    Data.IsUse = false;
                    switch (View.Screen)
                    {
                        case ScreenType.지도:
                            SettingSensor.Instance.BackviewSetting.ShowSetting(View.Sensor.LeftBackView);
                            break;
                        case ScreenType.사진:
                            SettingSensor.Instance.BackviewSetting.ShowSetting(View.Sensor.RightBackView);
                            break;
                    }
                    gameObject.SetActive(false);
                    break;
            }
        });
    }
}
