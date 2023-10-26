using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class DepthSensorBackViewSetting : MonoBehaviour
{
    public UnityEngine.UI.Text TitleText; // 상단에 센서 이름

    [Header("중앙")]
    public RectTransform ViewRect;
    public RawImage DepthViewImage; // 뎁스 이미지
    public RawImage ColorMapRawImage; // 칼라 이미지
    public RawImage TouchViewImage
    {
        get
        {
            return Screen.TouchViewImage;
        }
    }
    // 인식 범위 설정하는 화살표 4개와 선
    public RectTransform[] ViewPortPointList;
    public Text[] ArrowText;
    public UILineRenderer DepthLineRenderer;

    private bool IsShowColorCamera = true;


    [Header("상단")]
    public Slider ScaleSlider; // 뎁스, 칼라 이미지 크기
    public Toggle XFlipToggle; // x 반전
    public Toggle YFlipToggle; // y 반전

    public SliderCtr MinDistSlider; // 인식을 위한 z 범위 최대
    public SliderCtr MaxDistSlider; // 인식을 위한 z 범위 최소
    public Toggle MaxDistUseToggle; // 최대값 무재한

    public SliderCtr MinAreaSlider; // 인식 최소 크기
    public SliderCtr MaxAreaSlider; // 인식 최대 크기
    public Toggle MaxAreaUseToggle; // 최대값 무재한

    public Slider ErosionSlider; // 침식
    public Slider ExpansionSlider; // 팽창
    public Toggle ErosionNextExpansionToggle; // 침식 팽창 우선 순위 변경


    public SliderCtr BackgroundRemoveCheckCountSlider; // 아스트라가 시간이 지나면 틀어지는데 이 현상을 해당 프레임마다 보정해줌
    public SliderCtr BackgroundRemoveMaxSlider; // 보정을 해당 범위 안의 값에만 적용함

    public Toggle[] PointTouchTypeToggle;
    public SliderCtr OutlinePointSlider;

    [Header("하단")]
    public Text CameraChangeButtonText;
    public GameObject DepthViewSliderPanel;
    // 뎁스 이미지 색상 값 조정
    public SliderCtr DepthCheckSizeSlider;
    public SliderCtr DepthCheckDistSlider;
    public Slider AlphaSlider;

    public Text DistText;// 거리값을 표현하기 위한 텍스트
    public Text AreaListText; // 입력된 범위 로그
    private List<string> AreaList = new List<string>();



    //4개 화살표 움직이기
    private int MoveArrowIndex = -1;
    private Vector2 MoveArrowStartMousePosition;
    private Vector2 MoveArrowStartData;
    public Texture2D TouchViewSampleImage;




    [Header("스크린 설정")]
    /// <summary>
    /// 센서 영역이 작아서 화면을 다 채울 수 없을 경우 사용하는 노란색 막대 4개
    /// </summary>
    public RectTransform[] ScreenPointBarList;
    public RectTransform[] ScreenPointRedRectList;
    private int ScreenPointChangeIndex = -1;
    private float ScreenPointStartData = 0;
    private float ScreenPointStartMousePosition = 0;


    [Header("추가메뉴")]
    public GameObject PlusMenuPanel;


    [Header("Data")]
    public Monitor Screen;
    public DepthSensor Sensor;
    public DepthSensorBackView BackView;

    private ushort[] DepthRawList;
    private Texture2D DepthTexture;
    private Color[] DepthColors;
    private Texture2D TouchTexture;
    private Color[] ColorDefaultPack;
    private Mat TouchMat;


    private float DepthTextureScale = 1;


    private float minDist = 9999999999999;
    private float MaxDist = 0;
    private float SumDist = 0;
    private int SumDistCount = 0;
    private int RecognitionCount = 0;

    private int DepthTextureDetail = 2;

    private bool mIsLoad = false;
    public void ShowSetting(DepthSensorBackView view)
    {
        SensorManager.Instance.ShowDebugMode(true);
        if (BackView != null)
        {
            TouchViewImage.gameObject.SetActive(false);
            BackView.Sensor.Save();
            BackView.Sensor.OnDepthRead -= OnDepthChange;
            BackView.Sensor.GetColorImage(false);
        }

        Screen = view.CurrentMonitor;
        TouchViewImage.gameObject.SetActive(true);

        TitleText.text = view.Sensor.Id + " (" + view.Sensor.Memo + ") " + Screen.Type;
        ColorMapRawImage.texture = view.Sensor.GetColorImage(true);

        mIsLoad = false;
        gameObject.SetActive(true);


        Sensor = view.Sensor;
        BackView = view;
        BackView.Sensor.OnDepthRead += OnDepthChange;
        //Screen.SensorSettingObject.gameObject.SetActive(true);
        Screen.TouchViewImage.gameObject.SetActive(true);


        if (DepthTexture == null || DepthTexture.width != Sensor.DepthWidth / DepthTextureDetail || DepthTexture.height != Sensor.DepthHeight / DepthTextureDetail)
        {
            DepthTexture = new Texture2D(Sensor.DepthWidth / DepthTextureDetail, Sensor.DepthHeight / DepthTextureDetail);
            DepthColors = new Color[DepthTexture.width * DepthTexture.height];

            Color defaultColor = new Color(0, 0, 0, 0.1f);
        }

        if (ColorDefaultPack == null || ColorDefaultPack.Length != Sensor.DepthWidth * Sensor.DepthHeight)
        {
            ColorDefaultPack = new Color[Sensor.DepthWidth * Sensor.DepthHeight];
            Color defaultColor = new Color(0, 0, 0, 0.1f);
            for (int i = 0; i < ColorDefaultPack.Length; i++)
            {
                ColorDefaultPack[i] = defaultColor;
            }
        }

        DepthViewImage.transform.localScale = Sensor.SettingDepthImageScale;
        ColorMapRawImage.transform.localScale = Sensor.SettingColorImageScale;

        DepthViewImage.texture = DepthTexture;


        MinDistSlider.Val = BackView.Data.MinDist;
        MaxDistSlider.Val = BackView.Data.MaxDist;
        MaxDistUseToggle.isOn = BackView.Data.IsMaxDistUse;

        MinAreaSlider.Val = BackView.Data.MinArea;
        MaxAreaSlider.Val = BackView.Data.MaxArea;
        MaxAreaUseToggle.isOn = BackView.Data.IsMaxAreaUse;

        MaxDistSlider.interactable = MaxDistUseToggle.isOn;
        MaxAreaSlider.interactable = MaxAreaUseToggle.isOn;

        XFlipToggle.isOn = BackView.Data.IsXFlip;
        YFlipToggle.isOn = BackView.Data.IsYFlip;
        ScaleSlider.value = DepthTextureScale;

        ErosionSlider.value = BackView.Data.ErosionVal;
        ExpansionSlider.value = BackView.Data.ExpansionVal;
        ErosionNextExpansionToggle.isOn = BackView.Data.IsErosionNextExpansion;
        OnErosionNextExpansionToggleChange();

        BackgroundRemoveCheckCountSlider.Val = BackView.Data.ScreenCheckRate;
        BackgroundRemoveMaxSlider.Val = BackView.Data.ScreenCheckMaxDist;


        PointTouchTypeToggle[0].isOn = false;
        PointTouchTypeToggle[1].isOn = false;
        PointTouchTypeToggle[2].isOn = false;

        if (BackView.Data.IsTouchCenter)
        {
            PointTouchTypeToggle[0].isOn = true;
        }
        else if(BackView.Data.IsOutlineAllTouch)
        {
            PointTouchTypeToggle[2].isOn = true;
        }
        else
        {
            PointTouchTypeToggle[1].isOn = true;
        }
        OutlinePointSlider.gameObject.SetActive(PointTouchTypeToggle[2].isOn);
        OutlinePointSlider.Val = BackView.Data.OutlineAllTouchInterval;

        SetViewPoint();
        ChangeScreenRect();

        OnAlphaSliderChange();
        mIsLoad = true;

        OnScalseSliderChange();
        ColorMapViewChange(true);
    }
    private void OnDisable()
    {
        //if (Screen != null)
        //    Screen.ShowSensorView(false);
        if (BackView != null)
        {
            BackView.Sensor.OnDepthRead -= OnDepthChange;
            BackView.Sensor.GetColorImage(false);
            BackView = null;
        }

        if(Screen != null)
        {
            //Screen.SensorSettingObject.gameObject.SetActive(false);
            Screen.TouchViewImage.gameObject.SetActive(false);
        }

        PlusMenuPanel.gameObject.SetActive(false);

        Sensor = null;

        SensorManager.Instance.ShowDebugMode(false);
    }
    public void OnAlphaSliderChange()
    {
        TouchViewImage.color = new Color(1, 1, 1, 1 - AlphaSlider.value);
    }


    private DepthViewStateData DepthViewState;
    public enum DepthViewStateData
    {
        실시간데이터, 스크린데이터, 수정된스크린데이터
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnSetDefaultFloorButtonClick();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnColorMapViewButtonClick();
        }
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            DepthViewState = (DepthViewStateData)(((int)DepthViewState + 1) % 3);

            switch (DepthViewState)
            {
                case DepthViewStateData.실시간데이터:
                    UIManager.Instance.ShowMessage("실시간 깊이 데이터 확인");
                    break;
                case DepthViewStateData.스크린데이터:
                    UIManager.Instance.ShowMessage("저장된 깊이 데이터 확인");
                    break;
                case DepthViewStateData.수정된스크린데이터:
                    UIManager.Instance.ShowMessage("저장 + 스크린 체크 깊이 데이터 확인");
                    break;
                default:
                    break;
            }
        }



        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            ScaleSlider.value += scroll;
        }


        //카메라 전체 화면 이동
        UpdateMoveCamera();


        //화면 출력 범위(터치) 설정 (노란색 사각형)
        UpdateChangeScreenRect();


        //노란색 화살표 이동
        UpdateArrowMove();
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
            ViewRect.anchoredPosition = MoveCameraStartPosition + ((Vector2)Input.mousePosition - MoveCameraStartMousePosition);

            if (Input.GetMouseButton(0) == false)
            {
                IsCameraViewMove = false;
            }
        }
    }
    #endregion


    public void OnSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }

        BackView.Data.MinDist = (int)MinDistSlider.Val;
        BackView.Data.MaxDist = (int)MaxDistSlider.Val;
        BackView.Data.IsMaxDistUse = MaxDistUseToggle.isOn;
        MaxDistSlider.interactable = MaxDistUseToggle.isOn;

        BackView.Data.MinArea = (int)MinAreaSlider.Val;
        BackView.Data.MaxArea = (int)MaxAreaSlider.Val;
        BackView.Data.IsMaxAreaUse = MaxAreaUseToggle.isOn;
        MaxAreaSlider.interactable = MaxAreaUseToggle.isOn;

        BackView.Data.ErosionVal = (int)ErosionSlider.value;
        BackView.Data.ExpansionVal = (int)ExpansionSlider.value;


        BackView.Data.ScreenCheckRate = (int)BackgroundRemoveCheckCountSlider.Val;
        BackView.Data.ScreenCheckMaxDist = (int)BackgroundRemoveMaxSlider.Val;

    }
    public void OnScalseSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }

        DepthTextureScale = ScaleSlider.value;
        BackView.Data.IsXFlip = XFlipToggle.isOn;
        BackView.Data.IsYFlip = YFlipToggle.isOn;

        ViewRect.transform.localScale = new Vector3(DepthTextureScale, DepthTextureScale, 1);

        ViewRect.transform.localScale = new Vector3(
            BackView.Data.IsXFlip ? -DepthTextureScale : DepthTextureScale,
            BackView.Data.IsYFlip ? -DepthTextureScale : DepthTextureScale,
            1);

        //DepthViewImage.transform.localScale = new Vector3(
        //    BackView.Data.IsXFlip ? -1 : 1,
        //    BackView.Data.IsYFlip ? -1 : 1,
        //    1);

        //ColorMapRawImage.transform.localScale = new Vector3(
        //    BackView.Data.IsXFlip ? -1 : 1,
        //    BackView.Data.IsYFlip ? -1 : 1,
        //    1);

        TouchViewImage.transform.localScale = new Vector3(
            BackView.Data.IsXFlip ? -1 : 1,
            BackView.Data.IsYFlip ? -1 : 1,
            1);


        foreach (var text in ArrowText)
        {
            text.rectTransform.localScale = new Vector3(BackView.Data.IsXFlip ? -1 : 1, BackView.Data.IsYFlip ? -1 : 1, 1);
        }
        if (BackView.Data.IsXFlip)
        {
            if (BackView.Data.IsYFlip)
            {
                ArrowText[0].text = "RB";
                ArrowText[1].text = "LB";
                ArrowText[2].text = "LT";
                ArrowText[3].text = "RT";
            }
            else
            {
                ArrowText[0].text = "RT";
                ArrowText[1].text = "LT";
                ArrowText[2].text = "LB";
                ArrowText[3].text = "RB";
            }
        }
        else
        {
            if (BackView.Data.IsYFlip)
            {
                ArrowText[0].text = "LB";
                ArrowText[1].text = "RB";
                ArrowText[2].text = "RT";
                ArrowText[3].text = "LT";
            }
            else
            {
                ArrowText[0].text = "LT";
                ArrowText[1].text = "RT";
                ArrowText[2].text = "RB";
                ArrowText[3].text = "LB";
            }
        }


    }

    public void OnSetDefaultFloorButtonClick()
    {
        Sensor.SetSaveFloorDefault();
        UIManager.Instance.ShowMessage("깊이 데이터 저장 완료");
    }


    public void OnErosionNextExpansionToggleChange()
    {
        BackView.Data.IsErosionNextExpansion = ErosionNextExpansionToggle.isOn;

        Vector3 topPosition = ErosionSlider.transform.localPosition;
        Vector3 bottomPosition = ExpansionSlider.transform.localPosition;
        if (ExpansionSlider.transform.localPosition.y > ErosionSlider.transform.localPosition.y)
        {
            topPosition = ExpansionSlider.transform.localPosition;
            bottomPosition = ErosionSlider.transform.localPosition;
        }

        if (BackView.Data.IsErosionNextExpansion)
        {
            ErosionSlider.transform.localPosition = topPosition;
            ExpansionSlider.transform.localPosition = bottomPosition;
        }
        else
        {
            ErosionSlider.transform.localPosition = bottomPosition;
            ExpansionSlider.transform.localPosition = topPosition;
        }
    }


    public void OnColorMapViewButtonClick()
    {
        //ON - 컬러맵
        ColorMapViewChange(!IsShowColorCamera);
    }
    void ColorMapViewChange(bool isColorView)
    {
        IsShowColorCamera = isColorView;

        ColorMapRawImage.gameObject.SetActive(isColorView);
        DepthViewImage.gameObject.SetActive(!isColorView);

        if (isColorView)
        {
            CameraChangeButtonText.text = "깊이 카메라 보기";
        }
        else
        {
            DepthCheckDistSlider.OnSliderValChange();
            CameraChangeButtonText.text = "컬러 카메라 보기";
        }

        DepthViewSliderPanel.SetActive(!isColorView);
    }


    private void OnDepthChange(ushort[] depthList)
    {
        DepthRawList = depthList;
        if (ColorDefaultPack == null || ColorDefaultPack.Length < BackView.ViewPortDepthWidth * BackView.ViewPortDepthHeight)
        {
            ColorDefaultPack = new Color[BackView.ViewPortDepthWidth * BackView.ViewPortDepthHeight];
            Color defaultColor = new Color(0, 0, 0, 0.1f);
            for (int i = 0; i < ColorDefaultPack.Length; i++)
            {
                ColorDefaultPack[i] = defaultColor;
            }
            //Debug.Log(ColorDefaultPack.Length + "/" + BackView.ViewPortDepthWidth * BackView.ViewPortDepthHeight);
            //return;
        }

        try
        {
            TouchTexture.SetPixels(0, 0, BackView.ViewPortDepthWidth, BackView.ViewPortDepthHeight, ColorDefaultPack);
            if (Sensor.DepthHeight * Sensor.DepthWidth > DepthRawList.Length)
            {
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("[OnDepthImageChange] Err  : " + e.Message);
            return;
        }


        minDist = float.MaxValue;
        MaxDist = 0;
        SumDist = 0;
        SumDistCount = 0;
        RecognitionCount = 0;


        if (IsShowColorCamera == false)
        {
            var depthShowList = DepthRawList;

            switch (DepthViewState)
            {
                case DepthViewStateData.실시간데이터:
                    break;
                case DepthViewStateData.스크린데이터:
                    depthShowList = Sensor.SaveFloorDefault;
                    break;
                case DepthViewStateData.수정된스크린데이터:
                    depthShowList = new ushort[Sensor.SaveFloorDefault.Length];
                    for (int i = 0; i < depthShowList.Length; i++)
                    {
                        depthShowList[i] = (ushort)BackView.ScreenCheckList[i].checkScreen;
                    }
                    break;
            }

            float r = DepthCheckSizeSlider.Val * 20;
            float g = DepthCheckSizeSlider.Val * 10;
            float b = DepthCheckSizeSlider.Val;

            for (int x = 0; x < DepthTexture.width; x++)
            {
                for (int y = 0; y < DepthTexture.height; y++)
                {
                    int depthIndex = (x * DepthTextureDetail) + (y * DepthTextureDetail) * Sensor.DepthWidth;
                    int textureIndex = x + y * DepthTexture.width;

                    if (textureIndex >= DepthColors.Length)
                    {
                        continue;
                    }

                    float depth = depthShowList[depthIndex];
                    if (depth == 0)
                    {
                        DepthColors[textureIndex] = Color.green;
                        continue;
                    }


                    float realDist = Mathf.Abs(depthShowList[depthIndex] - DepthCheckDistSlider.Val);
                    DepthColors[textureIndex] = new Color(
                        (realDist < r) ? ((r - realDist) / r) : 0,
                        (realDist < g) ? ((g - realDist) / g) : 0,
                        (realDist < b) ? ((b - realDist) / b) : 0,
                        1);
                }
            }



            //for (int x = 0; x < BackView.Data.DepthWidth; x++)
            //{
            //    for (int y = 0; y < BackView.Data.DepthHeight; y++)
            //    {
            //        int index = y * BackView.Data.DepthWidth + x;

            //        float depth = DepthRawList[index] - DepthViewDefaultSlider.Val;
            //        float depthData = BackView.Data.CheckFloorDefault[index] - DepthRawList[index];

            //    }
            //}

            DepthTexture.SetPixels(DepthColors);
            DepthTexture.Apply();
        }



        for (int x = BackView.StartX; x <= BackView.EndX; x++)
        {
            for (int y = BackView.StartY; y <= BackView.EndY; y++)
            {
                int index = y * Sensor.DepthWidth + x;
                bool isOut = x < 0 || x >= Sensor.DepthWidth || y < 0 || y >= Sensor.DepthHeight;

                if (isOut)
                {
                    TouchTexture.SetPixel(x - BackView.StartX, y - BackView.StartY, Color.red);
                    continue;
                }
                //스크린 전체에 표시되는 인식 범위 표시 (녹색, 파란색)
                if (DepthRawList[index] == 0) //범위를 벗어난거나 값이 0인 경우
                {
                    TouchTexture.SetPixel(x - BackView.StartX, y - BackView.StartY, Color.green);

                    continue;
                }

                float depthData = BackView.ScreenCheckList[index].checkScreen - DepthRawList[index];

                SumDistCount++;
                SumDist += DepthRawList[index];
                if (DepthRawList[index] > MaxDist)
                {
                    MaxDist = DepthRawList[index];
                }
                if (DepthRawList[index] < minDist)
                {
                    minDist = DepthRawList[index];
                }

                if (depthData >= BackView.Data.MinDist && (BackView.Data.IsMaxDistUse == false || depthData <= BackView.Data.MaxDist))
                {
                    if (BackView.Data.IsTouchCenter || BackView.Data.IsOutlineAllTouch)
                    {
                        TouchTexture.SetPixel(x - BackView.StartX, y - BackView.StartY, Color.blue);
                    }
                    else
                    {
                        TouchTexture.SetPixel(x - BackView.StartX, y - BackView.StartY,
                            new Color(0, 0, 1 - depthData / (BackView.Data.MaxDist - BackView.Data.MinDist)));
                    }
                }
            }
        }



        //for (int x = 0; x < Sensor.DepthWidth; x++)
        //{
        //    for (int y = 0; y < Sensor.DepthHeight; y++)
        //    {
        //        int index = y * Sensor.DepthWidth + x;

        //        float depthData = BackView.ScreenCheckList[index].checkScreen - DepthRawList[index];

        //        //스크린 전체에 표시되는 인식 범위 표시 (녹색, 파란색)
        //        if (DepthRawList[index] == 0)
        //        {
        //            if (x >= BackView.StartX && x <= BackView.EndX && y >= BackView.StartY && y <= BackView.EndY)
        //            {
        //                TouchTexture.SetPixel(x - BackView.StartX, y - BackView.StartY, Color.green);
        //            }
        //        }
        //        else if (x >= BackView.StartX && x <= BackView.EndX && y >= BackView.StartY && y <= BackView.EndY)
        //        {
        //            SumDistCount++;
        //            SumDist += DepthRawList[index];
        //            if (DepthRawList[index] > MaxDist)
        //            {
        //                MaxDist = DepthRawList[index];
        //            }
        //            if (DepthRawList[index] < minDist)
        //            {
        //                minDist = DepthRawList[index];
        //            }

        //            if (depthData >= BackView.Data.MinDist
        //                && (BackView.Data.IsMaxDistUse == false
        //                || depthData <= BackView.Data.MaxDist))
        //            {
        //                if (BackView.Data.IsTouchCenter)
        //                {
        //                    TouchTexture.SetPixel(x - BackView.StartX, y - BackView.StartY, Color.blue);
        //                }
        //                else
        //                {
        //                    TouchTexture.SetPixel(x - BackView.StartX, y - BackView.StartY,
        //                        new Color(0, 0, 1 - depthData / BackView.Data.MaxDist));
        //                }
        //            }

        //        }
        //    }
        //}

        DistText.text = "최소 거리 : " + minDist.ToString("F0")
            + "\n최대 거리 : " + MaxDist.ToString("F0")
            + "\n평균 거리 : " + (SumDist / SumDistCount).ToString("F0") + " (" + SumDistCount + ")";

        TouchTexture.Apply();

        Utils.texture2DToMat(TouchTexture, TouchMat);

        if (BackView.Data.IsErosionNextExpansion)
        {
            if (BackView.Data.ErosionVal > 0)
                Imgproc.erode(TouchMat, TouchMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(BackView.Data.ErosionVal, BackView.Data.ErosionVal)));

            if (BackView.Data.ExpansionVal > 0)
                Imgproc.dilate(TouchMat, TouchMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(BackView.Data.ExpansionVal, BackView.Data.ExpansionVal)));

        }
        else
        {
            if (BackView.Data.ExpansionVal > 0)
                Imgproc.dilate(TouchMat, TouchMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(BackView.Data.ExpansionVal, BackView.Data.ExpansionVal)));

            if (BackView.Data.ErosionVal > 0)
                Imgproc.erode(TouchMat, TouchMat, Imgproc.getStructuringElement(Imgproc.MORPH_OPEN, new Size(BackView.Data.ErosionVal, BackView.Data.ErosionVal)));
        }



        if (BackView.ViewPortPointTransformMat != null)
        {
            Imgproc.warpPerspective(TouchMat, TouchMat, BackView.ViewPortPointTransformMat,
                new Size(BackView.ViewPortDepthWidth, BackView.ViewPortDepthHeight));
        }

        for (int i = 0; i < BackView.srcContours.Count; i++)
        {
            var area = Imgproc.contourArea(BackView.srcContours[i]);
            //Imgproc.putText(TouchMat, area.ToString("F0"), BackView.ContoursPointList[i], Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 255, 0, 255));
            //Debug.Log("넓이 : " + area);
            if (area < BackView.Data.MinArea || (BackView.Data.IsMaxAreaUse && area > BackView.Data.MaxArea))
            {
                continue;
            }

            AreaList.Add(area.ToString("F0") + "\n");
            if (AreaList.Count > 20)
            {
                AreaList.RemoveAt(0);
            }

            AreaListText.text = "";

            foreach (var str in AreaList)
            {
                AreaListText.text += str;
            }

            RecognitionCount++;
            Imgproc.drawContours(TouchMat, BackView.srcContours, i, new Scalar(255, 0, 0, 255), 1);
        }

        Utils.matToTexture2D(TouchMat, TouchTexture);
        TouchTexture.Apply();
        TouchViewImage.texture = TouchTexture;
    }








    public void OnArrowDown(int index)
    {
        MoveArrowIndex = index;
        MoveArrowStartData = ViewPortPointList[index].anchoredPosition;
        MoveArrowStartMousePosition = Input.mousePosition;
    }
    private void UpdateArrowMove()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int index = 0;

            if (BackView.Data.IsXFlip && BackView.Data.IsYFlip)
            {
                index = 2;
            }
            else if (BackView.Data.IsXFlip)
            {
                index = 1;
            }
            else if (BackView.Data.IsYFlip)
            {
                index = 3;
            }

            OnArrowDown(index);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            int index = 1;

            if (BackView.Data.IsXFlip && BackView.Data.IsYFlip)
            {
                index = 3;
            }
            else if (BackView.Data.IsXFlip)
            {
                index = 0;
            }
            else if (BackView.Data.IsYFlip)
            {
                index = 2;
            }

            OnArrowDown(index);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            int index = 3;

            if (BackView.Data.IsXFlip && BackView.Data.IsYFlip)
            {
                index = 1;
            }
            else if (BackView.Data.IsXFlip)
            {
                index = 2;
            }
            else if (BackView.Data.IsYFlip)
            {
                index = 0;
            }

            OnArrowDown(index);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            int index = 2;

            if (BackView.Data.IsXFlip && BackView.Data.IsYFlip)
            {
                index = 0;
            }
            else if (BackView.Data.IsXFlip)
            {
                index = 3;
            }
            else if (BackView.Data.IsYFlip)
            {
                index = 1;
            }

            OnArrowDown(index);
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


        BackView.Data.TouchDepthPoints[MoveArrowIndex] = MoveArrowStartData
            + (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - MoveArrowStartMousePosition) / DepthTextureScale * new Vector2(BackView.Data.IsXFlip ? -1 : 1, BackView.Data.IsYFlip ? -1 : 1);
        //BackView.Data.TouchDepthPoints[MoveArrowIndex] = new Vector2(
        //    Mathf.Clamp(BackView.Data.TouchDepthPoints[MoveArrowIndex].x, 0, Sensor.DepthWidth - 1),
        //    Mathf.Clamp(BackView.Data.TouchDepthPoints[MoveArrowIndex].y, 0, Sensor.DepthHeight - 1)
        //    );

        SetViewPoint();
        TouchViewImage.color = new Color(1, 1, 1, 0.9f);
        TouchViewImage.texture = TouchViewSampleImage;


        if (Input.GetMouseButton(0) == false
            && Input.GetKey(KeyCode.Q) == false
            && Input.GetKey(KeyCode.W) == false
            && Input.GetKey(KeyCode.A) == false
            && Input.GetKey(KeyCode.S) == false)
        {
            MoveArrowIndex = -1;

            OnAlphaSliderChange();
            TouchViewImage.texture = TouchTexture;
        }
    }

    public void OnColorCameraChangeButtonClick()
    {
        ((SensorAstra)BackView.Sensor).ColorCameraChange();
    }


    public void OnPointTouchTypeToggleChange()
    {
        if(mIsLoad == false)
        {
            return;
        }

        if (PointTouchTypeToggle[0].isOn)
        {
            BackView.Data.IsTouchCenter = true;
            BackView.Data.IsOutlineAllTouch = false;
        }
        else if (PointTouchTypeToggle[1].isOn)
        {
            BackView.Data.IsTouchCenter = false;
            BackView.Data.IsOutlineAllTouch = false;
        }
        else if (PointTouchTypeToggle[2].isOn)
        {
            BackView.Data.IsTouchCenter = false;
            BackView.Data.IsOutlineAllTouch = true;
        }

        OutlinePointSlider.gameObject.SetActive(PointTouchTypeToggle[2].isOn);
        BackView.Data.OutlineAllTouchInterval = (int)OutlinePointSlider.Val;
    }



    /// <summary>
    /// 화살표4개 위치를 바탕으로 센서 범위 설정
    /// </summary>
    private void SetViewPoint()
    {
        //512, 424
        if (BackView.Data.TouchDepthPoints == null || BackView.Data.TouchDepthPoints.Length < 4)
        {
            BackView.Data.TouchDepthPoints = new Vector2[]
            {
                new Vector2(50, 50),
                new Vector2(50, Sensor.DepthHeight - 50),
                new Vector2(Sensor.DepthWidth - 50, Sensor.DepthHeight - 50),
                new Vector2(Sensor.DepthWidth - 50, 50)
            };
        }

        for (int i = 0; i < ViewPortPointList.Length; i++)
        {
            ViewPortPointList[i].anchoredPosition = new Vector2(BackView.Data.TouchDepthPoints[i].x, BackView.Data.TouchDepthPoints[i].y);
            //ViewPortPointList[i].localScale = new Vector3(1, BackView.Data.IsYFlip ? -1 : 1, 1);
        }

        BackView.SetData();


        TouchTexture = new Texture2D(BackView.ViewPortDepthWidth, BackView.ViewPortDepthHeight, TextureFormat.ARGB32, false);
        TouchViewImage.texture = TouchTexture;
        TouchMat = new Mat(BackView.ViewPortDepthHeight, BackView.ViewPortDepthWidth, CvType.CV_8UC4);
        //TouchMat2 = new Mat(depthSensorData.ViewPortDepthHeight, depthSensorData.ViewPortDepthWidth, CvType.CV_8UC1);
        //TouchMat2 = new Mat();

        //OnDepthImageChange();


        DepthLineRenderer.Points = new Vector2[]
        {
            BackView.Data.TouchDepthPoints[0],
            BackView.Data.TouchDepthPoints[1],
            BackView.Data.TouchDepthPoints[2],
            BackView.Data.TouchDepthPoints[3],
            BackView.Data.TouchDepthPoints[0],
        };
    }





    #region 스크린 영역 설정 (노란색 막대)
    public void OnScreenPointRectPointDown(int index)
    {
        ScreenPointChangeIndex = index;
        switch (index)
        {
            case 0:
                ScreenPointStartMousePosition = Input.mousePosition.y;
                ScreenPointStartData = BackView.Data.ScreenTop;
                break;
            case 1:
                ScreenPointStartMousePosition = Input.mousePosition.x;
                ScreenPointStartData = BackView.Data.ScreenRight;
                break;
            case 2:
                ScreenPointStartMousePosition = Input.mousePosition.y;
                ScreenPointStartData = BackView.Data.ScreenBottom;
                break;
            case 3:
                ScreenPointStartMousePosition = Input.mousePosition.x;
                ScreenPointStartData = BackView.Data.ScreenLeft;
                break;
        }
    }
    public void ChangeScreenRect()
    {
        float width = TouchViewImage.transform.parent.GetComponent<RectTransform>().rect.width;
        float height = TouchViewImage.transform.parent.GetComponent<RectTransform>().rect.height;
        float size = 10;//Screen.width * 0.01f;

        float canvasWidth = width; //ScreenPointBarList[0].parent.GetComponent<RectTransform>().rect.width;
        float canvasHeight = height;//800;


        ScreenPointBarList[0].sizeDelta = new Vector2(canvasWidth, size);
        ScreenPointBarList[1].sizeDelta = new Vector2(size, canvasHeight);
        ScreenPointBarList[2].sizeDelta = new Vector2(canvasWidth, size);
        ScreenPointBarList[3].sizeDelta = new Vector2(size, canvasHeight);

        ScreenPointBarList[0].anchoredPosition = new Vector2(0, -canvasHeight * BackView.Data.ScreenTop);
        ScreenPointBarList[1].anchoredPosition = new Vector2(-canvasWidth * BackView.Data.ScreenRight, 0);
        ScreenPointBarList[2].anchoredPosition = new Vector2(0, canvasHeight * BackView.Data.ScreenBottom);
        ScreenPointBarList[3].anchoredPosition = new Vector2(canvasWidth * BackView.Data.ScreenLeft, 0);

        foreach (var point in ScreenPointRedRectList)
        {
            point.transform.localPosition = Vector3.zero;
            point.transform.localEulerAngles = Vector3.zero;
            point.transform.localScale = Vector3.one;
        }

        ScreenPointRedRectList[0].sizeDelta = new Vector2(30, 30);
        ScreenPointRedRectList[1].sizeDelta = new Vector2(30, 30);
        ScreenPointRedRectList[2].sizeDelta = new Vector2(30, 30);
        ScreenPointRedRectList[3].sizeDelta = new Vector2(30, 30);

        ScreenPointRedRectList[4].sizeDelta = new Vector2(20, 20);
        ScreenPointRedRectList[5].sizeDelta = new Vector2(20, 20);
        ScreenPointRedRectList[6].sizeDelta = new Vector2(20, 20);
        ScreenPointRedRectList[7].sizeDelta = new Vector2(20, 20);

        ScreenPointRedRectList[0].anchoredPosition = new Vector2(-canvasWidth * BackView.Data.ScreenRight - size * 0.5f, -canvasHeight * BackView.Data.ScreenTop - size * 0.5f);
        ScreenPointRedRectList[1].anchoredPosition = new Vector2(-canvasWidth * BackView.Data.ScreenRight - size * 0.5f, canvasHeight * BackView.Data.ScreenBottom + size * 0.5f);
        ScreenPointRedRectList[2].anchoredPosition = new Vector2(canvasWidth * BackView.Data.ScreenLeft + size * 0.5f, canvasHeight * BackView.Data.ScreenBottom + size * 0.5f);
        ScreenPointRedRectList[3].anchoredPosition = new Vector2(canvasWidth * BackView.Data.ScreenLeft + size * 0.5f, -canvasHeight * BackView.Data.ScreenTop - size * 0.5f);

        ScreenPointRedRectList[4].localPosition = new Vector2(ScreenPointRedRectList[0].localPosition.x, (ScreenPointRedRectList[0].localPosition.y + ScreenPointRedRectList[1].localPosition.y) / 2);
        ScreenPointRedRectList[5].localPosition = new Vector2((ScreenPointRedRectList[1].localPosition.x + ScreenPointRedRectList[2].localPosition.x) / 2, ScreenPointRedRectList[1].localPosition.y);
        ScreenPointRedRectList[6].localPosition = new Vector2(ScreenPointRedRectList[2].localPosition.x, (ScreenPointRedRectList[2].localPosition.y + ScreenPointRedRectList[3].localPosition.y) / 2);
        ScreenPointRedRectList[7].localPosition = new Vector2((ScreenPointRedRectList[3].localPosition.x + ScreenPointRedRectList[0].localPosition.x) / 2, ScreenPointRedRectList[3].localPosition.y);


        TouchViewImage.rectTransform.sizeDelta = new Vector2(
            width * (1 - BackView.Data.ScreenRight - BackView.Data.ScreenLeft),
            height * (1 - BackView.Data.ScreenTop - BackView.Data.ScreenBottom));
        TouchViewImage.rectTransform.anchoredPosition = new Vector2(
            width * (BackView.Data.ScreenLeft - BackView.Data.ScreenRight) * 0.5f,
            height * (BackView.Data.ScreenBottom - BackView.Data.ScreenTop) * 0.5f);

        //LidarManager.SetData(Manager.Data, Manager.ScreenData);
    }
    private void UpdateChangeScreenRect()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float width = rect.rect.width;
        float height = 800;

        if (ScreenPointChangeIndex >= 0)
        {
            switch (ScreenPointChangeIndex)
            {
                case 0:
                    BackView.Data.ScreenTop = Mathf.Clamp(ScreenPointStartData - (Input.mousePosition.y - ScreenPointStartMousePosition) / height, 0, 1 - BackView.Data.ScreenBottom - 0.05f);
                    break;
                case 1:
                    BackView.Data.ScreenRight = Mathf.Clamp(ScreenPointStartData - (Input.mousePosition.x - ScreenPointStartMousePosition) / width, 0, 1 - BackView.Data.ScreenLeft - 0.05f);
                    break;
                case 2:
                    BackView.Data.ScreenBottom = Mathf.Clamp(ScreenPointStartData + (Input.mousePosition.y - ScreenPointStartMousePosition) / height, 0, 1 - BackView.Data.ScreenTop - 0.05f);
                    break;
                case 3:
                    BackView.Data.ScreenLeft = Mathf.Clamp(ScreenPointStartData + (Input.mousePosition.x - ScreenPointStartMousePosition) / width, 0, 1 - BackView.Data.ScreenRight - 0.05f);
                    break;
                default:
                    break;
            }

            BackView.ChangeScreenData();
            ChangeScreenRect();


            if (Input.GetMouseButton(0) == false)
            {
                ScreenPointChangeIndex = -1;
            }
        }
    }
    #endregion




    public void OnPlusButtonClick()
    {
        PlusMenuPanel.gameObject.SetActive(!PlusMenuPanel.gameObject.activeSelf);
    }
    public void OnModeChangeButtonClick()
    {
        MsgBox.Show("TopView 모드로 변경하시겠습니까?").SetButtonType(MsgBoxButtons.OK_CANCEL).OnResult((result) =>
        {
            switch (result)
            {
                case DialogResult.YES_OK:
                    var sensor = Sensor;
                    var view = BackView;
                    gameObject.SetActive(false);
                    switch (view.Screen)
                    {
                        case ScreenType.지도:
                            sensor.Data.LeftTopview.IsUse = true;
                            SettingSensor.Instance.TopviewSetting.ShowSetting(sensor.LeftTopView);
                            break;
                        case ScreenType.사진:
                            sensor.Data.RightTopview.IsUse = true;
                            SettingSensor.Instance.TopviewSetting.ShowSetting(sensor.RightTopView);
                            break;
                    }
                    break;
            }
        });
    }
}
