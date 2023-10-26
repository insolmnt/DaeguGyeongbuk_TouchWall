using Fenderrio.ImageWarp;
using StartPage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class KeystoneSetting : MonoBehaviour
{
    public Monitor Monitor;

    public CanvasGroup KeystoneSettingPanel;


    [Header("우상, 우하, 좌하, 좌상")]
    public RectTransform[] KeystoneArrowList;
    public RectTransform[] BezierHandleList;
    public UILineRenderer[] LineList;

    public Toggle OnlyStraightToggle;

    public List<CustomButton> CurrentSettingButton;

    public NumberInputField SplitXCountSlider;
    public NumberInputField SplitYCountSlider;

    public Image[] SplitLineImageX;
    public Image[] SplitLineImageY;
    public NumberInputField[] SplitSliderX;
    public NumberInputField[] SplitSliderY;

    [Header("Data")]
    public int CurrentSetting;
    public MonitorKeystoneData Data
    {
        get
        {
            return Monitor.SplitData.Keystone[CurrentSetting];
        }
    }
    public RawImageWarp MainOutImage
    {
        get
        {
            return Monitor.MainOutImage[CurrentSetting];
        }
    }


    private void Start()
    {
        //ShowKeystoneSetting(false);
    }


    public void OnCurrentSettingScreenChange(int index)
    {
        OnCurrentSettingScreenChange(index % 3, index / 3);
    }
    public void OnCurrentSettingScreenChange(int x, int y)
    {
        if (x >= Monitor.Data.SplitCountX || y >= Monitor.Data.SplitCountY)
        {
            return;
        }
        for (int i = 0; i < CurrentSettingButton.Count; i++)
        {
            CurrentSettingButton[i].Select(i == (y * 3 + x));
        }

        CurrentSetting = y * Monitor.Data.SplitCountX + x;
        SetData();
        Invoke("SetData", 0.1f);
    }

    [Header("Data")]
    public bool IsShowKeystoneSetting = false;
    private bool mIsLoad = false;
    public void ShowKeystoneSetting(bool isShow)
    {
        keyCheck = false;
        keyDownTime = 0;

        KeystonePointDown = -1;
        KeystoneBezierPointDown = -1;
        KeystoneBezierPointDownX = -1;
        KeystoneBezierPointDownY = -1;


        //MultiDisplayManagerUI.Instance.CheckKeystoneAlpha();
        if (isShow)
        {
            mIsLoad = false;


            for (int i = 0; i < CurrentSettingButton.Count; i++)
            {
                int x = i % 3;
                int y = i / 3;

                CurrentSettingButton[i].gameObject.SetActive(x < Monitor.Data.SplitCountX && y < Monitor.Data.SplitCountY);
            }



            for (int i = 0; i < SplitSliderX.Length; i++)
            {
                if (i < Monitor.Data.SplitCountX - 1)
                {
                    SplitLineImageX[i].gameObject.SetActive(true);
                    SplitSliderX[i].gameObject.SetActive(true);
                    SplitSliderX[i].Val = Monitor.SplitData.Split[i + 1];
                }
                else
                {
                    SplitLineImageX[i].gameObject.SetActive(false);
                    SplitSliderX[i].gameObject.SetActive(false);
                }
            }


            for (int i = 0; i < SplitSliderY.Length; i++)
            {
                if (i < Monitor.Data.SplitCountY - 1)
                {
                    SplitLineImageY[i].gameObject.SetActive(true);
                    SplitSliderY[i].gameObject.SetActive(true);
                    SplitSliderY[i].Val = Monitor.SplitData.SplitY[i + 1];
                }
                else
                {
                    SplitLineImageY[i].gameObject.SetActive(false);
                    SplitSliderY[i].gameObject.SetActive(false);
                }
            }


            SplitXCountSlider.Val = Monitor.Data.SplitCountX;
            SplitYCountSlider.Val = Monitor.Data.SplitCountY;

            KeystoneSettingPanel.gameObject.SetActive(true); //Monitor.IsUse
            KeystoneSettingPanel.alpha = 1;
            //OutputRawImagePrefab.gameObject.SetActive(); //Monitor.IsUse
            OnlyStraightToggle.isOn = Monitor.Data.isOnltyStraight;
            SetButtonText();
            OnCurrentSettingScreenChange(0, 0);
            SetData();
            mIsLoad = true;
        }
        else
        {
            KeystoneSettingPanel.gameObject.SetActive(false);
        }

        IsShowKeystoneSetting = isShow;


    }

    public void OnSplitSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }


        for (int i = 0; i < Monitor.Data.SplitCountX - 1; i++)
        {
            Monitor.SplitData.Split[i + 1] = SplitSliderX[i].Val;
        }
        for (int i = 0; i < Monitor.Data.SplitCountY - 1; i++)
        {
            Monitor.SplitData.SplitY[i + 1] = SplitSliderY[i].Val;
        }

        SetSplitLinePos();
        Monitor.ScreenSizeChange();
    }

    private void SetSplitLinePos()
    {
        float canvasWidth = 800f / Monitor.MonitorCanvas.renderingDisplaySize.y * Monitor.MonitorCanvas.renderingDisplaySize.x;
        float canvasHeight = 800f;

        int count = Monitor.Data.SplitCountX * Monitor.Data.SplitCountY;

        float left = float.MaxValue;
        float right = float.MinValue;
        float up = float.MinValue;
        float down = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            int x = i % Monitor.Data.SplitCountX;
            int y = i / Monitor.Data.SplitCountX;

            if (x == 0)
            {
                float v = Mathf.Min(Monitor.MainOutImage[i].cornerOffsetTL.x, Monitor.MainOutImage[i].cornerOffsetBL.x);
                if (left > v)
                {
                    left = v;
                }
            }
            if (x == Monitor.Data.SplitCountX - 1)
            {
                float v = Mathf.Max(Monitor.MainOutImage[i].cornerOffsetTR.x, Monitor.MainOutImage[i].cornerOffsetBR.x);
                if (right < v)
                {
                    right = v;
                }
            }
            if (y == 0)
            {
                float v = Mathf.Min(Monitor.MainOutImage[i].cornerOffsetBL.y, Monitor.MainOutImage[i].cornerOffsetBR.y);
                if (down > v)
                {
                    down = v;
                }
            }
            if (y == Monitor.Data.SplitCountY - 1)
            {
                float v = Mathf.Max(Monitor.MainOutImage[i].cornerOffsetTL.y, Monitor.MainOutImage[i].cornerOffsetTR.y);
                if (up < v)
                {
                    up = v;
                }
            }
        }


        for (int i = 0; i < Monitor.Data.SplitCountX - 1; i++)
        {
            SplitLineImageX[i].rectTransform.anchoredPosition = new Vector2(left + (canvasWidth - left + right) * Monitor.SplitData.Split[i + 1], 0);
        }
        for (int i = 0; i < Monitor.Data.SplitCountY - 1; i++)
        {
            SplitLineImageY[i].rectTransform.anchoredPosition = new Vector2(0, down + (canvasHeight - down + up) * Monitor.SplitData.SplitY[i + 1]);
        }
    }

    public void OnSplitCountSliderChange()
    {
        if (mIsLoad == false)
        {
            return;
        }

        Monitor.Save();

        Monitor.Data.SplitCountX = (int)SplitXCountSlider.Val;
        Monitor.Data.SplitCountY = (int)SplitYCountSlider.Val;
        Monitor.LoadSplitData();
        Monitor.SetSplitCountData();

        ShowKeystoneSetting(true);

        Invoke("SetData", 0.1f);
    }

    public void SetData()
    {
        for (int i = 0; i < KeystoneArrowList.Length; i++)
        {
            KeystoneArrowList[i].anchoredPosition = Data.PointList[i];
        }

        BezierHandleList[0].anchoredPosition = MainOutImage.topBezierLocalPositionHandleA;
        BezierHandleList[1].anchoredPosition = MainOutImage.topBezierLocalPositionHandleB;
        BezierHandleList[2].anchoredPosition = MainOutImage.rightBezierLocalPositionHandleA;
        BezierHandleList[3].anchoredPosition = MainOutImage.rightBezierLocalPositionHandleB;
        BezierHandleList[4].anchoredPosition = MainOutImage.bottomBezierLocalPositionHandleA;
        BezierHandleList[5].anchoredPosition = MainOutImage.bottomBezierLocalPositionHandleB;
        BezierHandleList[6].anchoredPosition = MainOutImage.leftBezierLocalPositionHandleA;
        BezierHandleList[7].anchoredPosition = MainOutImage.leftBezierLocalPositionHandleB;


        if (LineList != null && LineList.Length > 0)
        {
            LineList[0].Points = new Vector2[] { BezierHandleList[0].anchoredPosition, MainOutImage.cornerLocalPositionTL };
            LineList[1].Points = new Vector2[] { BezierHandleList[1].anchoredPosition, MainOutImage.cornerLocalPositionTR };
            LineList[2].Points = new Vector2[] { BezierHandleList[2].anchoredPosition, MainOutImage.cornerLocalPositionTR };
            LineList[3].Points = new Vector2[] { BezierHandleList[3].anchoredPosition, MainOutImage.cornerLocalPositionBR };
            LineList[4].Points = new Vector2[] { BezierHandleList[4].anchoredPosition, MainOutImage.cornerLocalPositionBR };
            LineList[5].Points = new Vector2[] { BezierHandleList[5].anchoredPosition, MainOutImage.cornerLocalPositionBL };
            LineList[6].Points = new Vector2[] { BezierHandleList[6].anchoredPosition, MainOutImage.cornerLocalPositionBL };
            LineList[7].Points = new Vector2[] { BezierHandleList[7].anchoredPosition, MainOutImage.cornerLocalPositionTL };
        }

        SetSplitLinePos();
    }

    //public void SetRawImageWarp(RawImageWarp raw)
    //{
    //    if(raw == null)
    //    {
    //        return;
    //    }

    //    raw.cornerOffsetTR = OutputRawImagePrefab.cornerOffsetTR;
    //    raw.cornerOffsetBR = OutputRawImagePrefab.cornerOffsetBR;
    //    raw.cornerOffsetBL = OutputRawImagePrefab.cornerOffsetBL;
    //    raw.cornerOffsetTL = OutputRawImagePrefab.cornerOffsetTL;


    //    raw.topBezierHandleA = OutputRawImagePrefab.topBezierHandleA;
    //    raw.topBezierHandleB = OutputRawImagePrefab.topBezierHandleB;
    //    raw.leftBezierHandleA = OutputRawImagePrefab.leftBezierHandleA;
    //    raw.leftBezierHandleB = OutputRawImagePrefab.leftBezierHandleB;
    //    raw.rightBezierHandleA = OutputRawImagePrefab.rightBezierHandleA;
    //    raw.rightBezierHandleB = OutputRawImagePrefab.rightBezierHandleB;
    //    raw.bottomBezierHandleA = OutputRawImagePrefab.bottomBezierHandleA;
    //    raw.bottomBezierHandleB = OutputRawImagePrefab.bottomBezierHandleB;
    //}

    public void OnStraightToggleChange()
    {
        if (IsShowKeystoneSetting == false)
        {
            return;
        }
        Monitor.Data.isOnltyStraight = OnlyStraightToggle.isOn;
        SetButtonText();


        for (int i = 0; i < Monitor.MainOutImage.Count; i++)
        {
            Monitor.SetKeystone(i, false);
        }
        Invoke("SetData", 0.1f);
    }

    public void SetButtonText()
    {
        if (Monitor.Data.isOnltyStraight) //IJKL
        {
            BezierHandleList[0].GetComponentInChildren<Text>().text = "Q+JL";
            BezierHandleList[1].GetComponentInChildren<Text>().text = "W+JL";
            BezierHandleList[2].GetComponentInChildren<Text>().text = "W+IK";
            BezierHandleList[3].GetComponentInChildren<Text>().text = "S+IK";
            BezierHandleList[4].GetComponentInChildren<Text>().text = "S+JL";
            BezierHandleList[5].GetComponentInChildren<Text>().text = "A+JL";
            BezierHandleList[6].GetComponentInChildren<Text>().text = "A+IK";
            BezierHandleList[7].GetComponentInChildren<Text>().text = "Q+IK";
        }
        else
        {
            BezierHandleList[0].GetComponentInChildren<Text>().text = "4";
            BezierHandleList[1].GetComponentInChildren<Text>().text = "5";
            BezierHandleList[2].GetComponentInChildren<Text>().text = "T";
            BezierHandleList[3].GetComponentInChildren<Text>().text = "G";
            BezierHandleList[4].GetComponentInChildren<Text>().text = "V";
            BezierHandleList[5].GetComponentInChildren<Text>().text = "C";
            BezierHandleList[6].GetComponentInChildren<Text>().text = "D";
            BezierHandleList[7].GetComponentInChildren<Text>().text = "E";
        }
    }


    public void Reset()
    {
        float canvasWidth = 800f / Monitor.MonitorCanvas.renderingDisplaySize.y * Monitor.MonitorCanvas.renderingDisplaySize.x;
        float canvasHeight = 800f;

        int x = CurrentSetting % Monitor.Data.SplitCountX;
        int y = CurrentSetting / Monitor.Data.SplitCountX;
        Monitor.SplitData.Keystone[CurrentSetting] = new MonitorKeystoneData()
        {
            PointList = new Vector2[]
            {
                new Vector2((canvasWidth * Monitor.SplitData.Split[x + 1]) - canvasWidth, (canvasHeight * Monitor.SplitData.SplitY[y + 1]) - canvasHeight),
                new Vector2((canvasWidth * Monitor.SplitData.Split[x + 1]) - canvasWidth, canvasHeight * Monitor.SplitData.SplitY[y]),
                new Vector2(canvasWidth * Monitor.SplitData.Split[x], canvasHeight * Monitor.SplitData.SplitY[y]),
                new Vector2(canvasWidth * Monitor.SplitData.Split[x], (canvasHeight * Monitor.SplitData.SplitY[y + 1]) - canvasHeight)
            }
        };


        Data.BezierList = new Vector2[]
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
        Data.StraightDataList = new float[]
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

        Monitor.SetKeystone(CurrentSetting, true);
        Invoke("SetData", 0.1f);
    }


    private int KeystoneBezierPointDown = -1;
    private Vector2 BezierPointDownMousePosition;
    private Vector2 BezierPointDownData;
    //private Vector2 CurrentMousePoint;

    public void OnBezierKeystoneArrowPointDown(int index)
    {
        keyCheck = false;
        keyDownTime = 0;
        if (Input.GetMouseButton(0))
        {
            KeystoneBezierPointDown = index;
            BezierPointDownMousePosition = Input.mousePosition;

            if (Monitor.Data.isOnltyStraight)
            {
                BezierPointDownData = new Vector2(Data.StraightDataList[index], Data.StraightDataList[index]);
            }
            else
            {
                BezierPointDownData = Data.BezierList[index];
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (Monitor.Data.isOnltyStraight)
            {
                Data.StraightDataList[index] = 1;
            }
            else
            {
                Data.BezierList[index] = Vector2.zero;
            }
            Monitor.SetKeystone(CurrentSetting, true);
            Invoke("SetData", 0.1f);
        }
    }


    private int KeystoneBezierPointDownX = -1;
    public void OnBezierKeystoneArrowPointDownX(int index)
    {
        keyCheck = false;
        keyDownTime = 0;
        if (Input.GetMouseButton(0))
        {
            KeystoneBezierPointDownX = index;
            BezierPointDownMousePosition = Input.mousePosition;
            BezierPointDownData = Data.BezierList[index];
        }

        if (Input.GetMouseButton(1))
        {
            Data.BezierList[index] = new Vector2(0, Data.BezierList[index].y);
            Monitor.SetKeystone(CurrentSetting, true);
            Invoke("SetData", 0.1f);
        }
    }

    private int KeystoneBezierPointDownY = -1;
    public void OnBezierKeystoneArrowPointDownY(int index)
    {
        if (Input.GetMouseButton(0))
        {
            KeystoneBezierPointDownY = index;
            BezierPointDownMousePosition = Input.mousePosition;
            BezierPointDownData = Data.BezierList[index];
        }

        if (Input.GetMouseButton(1))
        {
            Data.BezierList[index] = new Vector2(Data.BezierList[index].x, 0);
            Monitor.SetKeystone(CurrentSetting, true);
            Invoke("SetData", 0.1f);
        }
    }

    private int KeystonePointDown = -1;
    private Vector2 PointDownMousePosition;
    private Vector2 PointDownArrowPosition;

    public void OnKeystoneArrowPointDown(int index)
    {
        keyCheck = false;
        keyDownTime = 0;
        if (Input.GetMouseButton(0))
        {
            KeystonePointDown = index;
            PointDownMousePosition = Input.mousePosition;
            PointDownArrowPosition = KeystoneArrowList[index].anchoredPosition;


            if (Monitor.Data.isOnltyStraight)
            {
                switch (index)
                {
                    case 0:
                        BezierPointDownData = new Vector2(Data.StraightDataList[1], Data.StraightDataList[2]);
                        break;

                    case 1:
                        BezierPointDownData = new Vector2(Data.StraightDataList[4], Data.StraightDataList[3]);
                        break;

                    case 2:
                        BezierPointDownData = new Vector2(Data.StraightDataList[5], Data.StraightDataList[6]);
                        break;

                    case 3:
                        BezierPointDownData = new Vector2(Data.StraightDataList[0], Data.StraightDataList[7]);
                        break;
                }
            }


        }

        if (Input.GetMouseButton(1))
        {
            Data.PointList[index] = Vector2.zero;
            float canvasWidth = 800f / Monitor.MonitorCanvas.renderingDisplaySize.y * Monitor.MonitorCanvas.renderingDisplaySize.x;
            float canvasHeight = 880;

            int x = CurrentSetting % Monitor.Data.SplitCountX;
            int y = CurrentSetting / Monitor.Data.SplitCountX;

            if (index == 0)
            {
                Data.PointList[index] = new Vector2((canvasWidth * Monitor.SplitData.Split[x + 1]) - canvasWidth, (canvasHeight * Monitor.SplitData.SplitY[y + 1]) - canvasHeight);
            }
            else if (index == 1)
            {
                Data.PointList[index] = new Vector2((canvasWidth * Monitor.SplitData.Split[x + 1]) - canvasWidth, canvasHeight * Monitor.SplitData.SplitY[y]);
            }
            else if (index == 2)
            {
                Data.PointList[index] = new Vector2(canvasWidth * Monitor.SplitData.Split[x], canvasHeight * Monitor.SplitData.SplitY[y]);
            }
            else if (index == 3)
            {
                Data.PointList[index] = new Vector2(canvasWidth * Monitor.SplitData.Split[x], (canvasHeight * Monitor.SplitData.SplitY[y + 1]) - canvasHeight);
            }

            Monitor.SetKeystone(CurrentSetting, true);
            Invoke("SetData", 0.1f);
        }
    }



    bool keyCheck = false;
    private float keyDownTime = 0;
    private void Update()
    {
        if (IsShowKeystoneSetting == false)
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OnCurrentSettingScreenChange(0, 1);
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                OnCurrentSettingScreenChange(1, 1);
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                OnCurrentSettingScreenChange(2, 1);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OnCurrentSettingScreenChange(0, 0);
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                OnCurrentSettingScreenChange(1, 0);
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                OnCurrentSettingScreenChange(2, 0);
            }
        }


        float speed = 0.005f;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    KeystoneSettingPanel.alpha = i / 10f;
                }
            }

            speed = 0.002f;
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            MsgBox.Show("초기화 하시겠습니까?")
                .SetButtonType(MsgBoxButtons.OK_CANCEL)
                .OnResult((result) =>
                {
                    MsgBox.Close();
                    switch (result)
                    {
                        case DialogResult.YES_OK:
                            Reset();
                            break;
                    }
                });
        }


        if (Input.GetKeyDown(KeyCode.Slash))
        {
            OnlyStraightToggle.isOn = !OnlyStraightToggle.isOn;
            if (OnlyStraightToggle.isOn)
            {
                //UIManager.Instance.ShowGameMessage("직선 보정만 사용");
            }
            else
            {
                //UIManager.Instance.ShowGameMessage("곡선 보정 사용");
            }
        }
        //키스톤
        if (Input.GetKeyDown(KeyCode.W))
        {
            KeystonePointDown = 0;
            PointDownMousePosition = Input.mousePosition;
            PointDownArrowPosition = KeystoneArrowList[0].anchoredPosition;
            keyCheck = true;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            KeystonePointDown = -1;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            KeystonePointDown = 1;
            PointDownMousePosition = Input.mousePosition;
            PointDownArrowPosition = KeystoneArrowList[1].anchoredPosition;
            keyCheck = true;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            KeystonePointDown = -1;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            KeystonePointDown = 2;
            PointDownMousePosition = Input.mousePosition;
            PointDownArrowPosition = KeystoneArrowList[2].anchoredPosition;
            keyCheck = true;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            KeystonePointDown = -1;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            KeystonePointDown = 3;
            PointDownMousePosition = Input.mousePosition;
            PointDownArrowPosition = KeystoneArrowList[3].anchoredPosition;
            keyCheck = true;
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            KeystonePointDown = -1;
        }


        //if (Input.GetKeyDown(KeyCode.PageDown))
        //{
        //    Monitor.Keystone.OnlyStraightToggle.isOn = !Monitor.Keystone.OnlyStraightToggle.isOn;
        //}


        if (KeystonePointDown >= 0)
        {
            if (keyDownTime > 0)
            {
                keyDownTime -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PointDownMousePosition -= Vector2.left;
                keyDownTime = 1f;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                PointDownMousePosition -= Vector2.right;
                keyDownTime = 1f;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                PointDownMousePosition -= Vector2.up;
                keyDownTime = 1f;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                PointDownMousePosition -= Vector2.down;
                keyDownTime = 1f;
            }

            if (keyDownTime < 0)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    PointDownMousePosition -= Vector2.left;
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    PointDownMousePosition -= Vector2.right;
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    PointDownMousePosition -= Vector2.up;
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    PointDownMousePosition -= Vector2.down;
                }
            }




            if (Monitor.Data.isOnltyStraight)
            {
                switch (KeystonePointDown)
                {
                    case 0:
                        if (Input.GetKeyDown(KeyCode.L))
                        {
                            Data.StraightDataList[1] -= speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.J))
                        {
                            Data.StraightDataList[1] += speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.I))
                        {
                            Data.StraightDataList[2] -= speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.K))
                        {
                            Data.StraightDataList[2] += speed;
                            keyDownTime = 1f;
                        }
                        //if (Input.GetKeyDown(KeyCode.Keypad5))
                        //{
                        //    Data.StraightDataList[1] = 1f;
                        //    Data.StraightDataList[2] = 1f;
                        //}

                        if (keyDownTime < 0)
                        {
                            if (Input.GetKey(KeyCode.L))
                            {
                                Data.StraightDataList[1] -= speed;
                            }
                            if (Input.GetKey(KeyCode.J))
                            {
                                Data.StraightDataList[1] += speed;
                            }
                            if (Input.GetKey(KeyCode.I))
                            {
                                Data.StraightDataList[2] -= speed;
                            }
                            if (Input.GetKey(KeyCode.K))
                            {
                                Data.StraightDataList[2] += speed;
                            }
                        }
                        break;

                    case 1:
                        //BezierPointDownData = new Vector2(Data.StraightDataList[4], Data.StraightDataList[3]);
                        if (Input.GetKeyDown(KeyCode.L))
                        {
                            Data.StraightDataList[4] -= speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.J))
                        {
                            Data.StraightDataList[4] += speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.I))
                        {
                            Data.StraightDataList[3] += speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.K))
                        {
                            Data.StraightDataList[3] -= speed;
                            keyDownTime = 1f;
                        }
                        //if (Input.GetKeyDown(KeyCode.Keypad5))
                        //{
                        //    Data.StraightDataList[3] = 1f;
                        //    Data.StraightDataList[4] = 1f;
                        //}

                        if (keyDownTime < 0)
                        {
                            if (Input.GetKey(KeyCode.L))
                            {
                                Data.StraightDataList[4] -= speed;
                            }
                            if (Input.GetKey(KeyCode.J))
                            {
                                Data.StraightDataList[4] += speed;
                            }
                            if (Input.GetKey(KeyCode.I))
                            {
                                Data.StraightDataList[3] += speed;
                            }
                            if (Input.GetKey(KeyCode.K))
                            {
                                Data.StraightDataList[3] -= speed;
                            }
                        }
                        break;

                    case 2:
                        //BezierPointDownData = new Vector2(Data.StraightDataList[5], Data.StraightDataList[6]);
                        if (Input.GetKeyDown(KeyCode.L))
                        {
                            Data.StraightDataList[5] += speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.J))
                        {
                            Data.StraightDataList[5] -= speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.I))
                        {
                            Data.StraightDataList[6] += speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.K))
                        {
                            Data.StraightDataList[6] -= speed;
                            keyDownTime = 1f;
                        }
                        //if (Input.GetKeyDown(KeyCode.Keypad5))
                        //{
                        //    Data.StraightDataList[5] = 1f;
                        //    Data.StraightDataList[6] = 1f;
                        //}

                        if (keyDownTime < 0)
                        {
                            if (Input.GetKey(KeyCode.L))
                            {
                                Data.StraightDataList[5] += speed;
                            }
                            if (Input.GetKey(KeyCode.J))
                            {
                                Data.StraightDataList[5] -= speed;
                            }
                            if (Input.GetKey(KeyCode.I))
                            {
                                Data.StraightDataList[6] += speed;
                            }
                            if (Input.GetKey(KeyCode.K))
                            {
                                Data.StraightDataList[6] -= speed;
                            }
                        }
                        break;

                    case 3:
                        if (Input.GetKeyDown(KeyCode.L))
                        {
                            Data.StraightDataList[0] += speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.J))
                        {
                            Data.StraightDataList[0] -= speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.I))
                        {
                            Data.StraightDataList[7] -= speed;
                            keyDownTime = 1f;
                        }
                        if (Input.GetKeyDown(KeyCode.K))
                        {
                            Data.StraightDataList[7] += speed;
                            keyDownTime = 1f;
                        }
                        //if (Input.GetKeyDown(KeyCode.Keypad5))
                        //{
                        //    Data.StraightDataList[7] = 1f;
                        //    Data.StraightDataList[0] = 1f;
                        //}

                        if (keyDownTime < 0)
                        {
                            if (Input.GetKey(KeyCode.L))
                            {
                                Data.StraightDataList[0] += speed;
                            }
                            if (Input.GetKey(KeyCode.J))
                            {
                                Data.StraightDataList[0] -= speed;
                            }
                            if (Input.GetKey(KeyCode.I))
                            {
                                Data.StraightDataList[7] -= speed;
                            }
                            if (Input.GetKey(KeyCode.K))
                            {
                                Data.StraightDataList[7] += speed;
                            }
                        }
                        //BezierPointDownData = new Vector2(Data.StraightDataList[0], Data.StraightDataList[7]);
                        break;
                }
            }


            KeystoneArrowList[KeystonePointDown].anchoredPosition =
                PointDownArrowPosition +
                (-PointDownMousePosition + new Vector2(Input.mousePosition.x, Input.mousePosition.y)) * (Input.GetKey(KeyCode.LeftShift) ? 0.3f : 1); ;

            KeystoneArrowList[KeystonePointDown].anchoredPosition = KeystoneArrowList[KeystonePointDown].anchoredPosition;

            Data.PointList[KeystonePointDown] = KeystoneArrowList[KeystonePointDown].anchoredPosition;

            Monitor.SetKeystone(CurrentSetting, true);
            SetData();

            if (keyCheck == false && Input.GetMouseButton(0) == false)
            {
                KeystonePointDown = -1;
            }
        }

        if (Monitor.Data.isOnltyStraight == false)
        {
            //곡선
            //"4";
            //"5";
            //"T";
            //"G";
            //"V";
            //"C";
            //"D";
            //"E";
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                KeystoneBezierPointDown = 0;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[0];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                KeystoneBezierPointDown = -1;
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                KeystoneBezierPointDown = 1;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[1];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.Alpha5))
            {
                KeystoneBezierPointDown = -1;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                KeystoneBezierPointDown = 2;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[2];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.T))
            {
                KeystoneBezierPointDown = -1;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                KeystoneBezierPointDown = 3;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[3];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.G))
            {
                KeystoneBezierPointDown = -1;
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                KeystoneBezierPointDown = 4;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[4];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.V))
            {
                KeystoneBezierPointDown = -1;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                KeystoneBezierPointDown = 5;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[5];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.C))
            {
                KeystoneBezierPointDown = -1;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                KeystoneBezierPointDown = 6;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[6];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                KeystoneBezierPointDown = -1;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                KeystoneBezierPointDown = 7;
                BezierPointDownMousePosition = Input.mousePosition;
                BezierPointDownData = Data.BezierList[7];
                keyCheck = true;
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                KeystoneBezierPointDown = -1;
            }
        }





        if (KeystoneBezierPointDown >= 0)
        {
            if (keyDownTime > 0)
            {
                keyDownTime -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                BezierPointDownMousePosition -= Vector2.left;
                keyDownTime = 1f;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                BezierPointDownMousePosition -= Vector2.right;
                keyDownTime = 1f;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                BezierPointDownMousePosition -= Vector2.up;
                keyDownTime = 1f;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                BezierPointDownMousePosition -= Vector2.down;
                keyDownTime = 1f;
            }

            if (keyDownTime < 0)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    BezierPointDownMousePosition -= Vector2.left * 0.5f;
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    BezierPointDownMousePosition -= Vector2.right * 0.5f;
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    BezierPointDownMousePosition -= Vector2.up * 0.5f;
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    BezierPointDownMousePosition -= Vector2.down * 0.5f;
                }
            }

            if (Monitor.Data.isOnltyStraight)
            {
                switch (KeystoneBezierPointDown)
                {
                    case 0: //+x
                    case 5://+x
                        Data.StraightDataList[KeystoneBezierPointDown] =
                            BezierPointDownData.x +
                            (-BezierPointDownMousePosition.x + Input.mousePosition.x) * speed;
                        break;

                    case 1: //-x
                    case 4://-x
                        Data.StraightDataList[KeystoneBezierPointDown] =
                            BezierPointDownData.x -
                            (-BezierPointDownMousePosition.x + Input.mousePosition.x) * speed;
                        break;

                    case 2://+y
                    case 7://+y
                        Data.StraightDataList[KeystoneBezierPointDown] =
                            BezierPointDownData.y -
                            (-BezierPointDownMousePosition.y + Input.mousePosition.y) * speed;
                        break;

                    case 3://-y
                    case 6://-y
                        Data.StraightDataList[KeystoneBezierPointDown] =
                            BezierPointDownData.y +
                            (-BezierPointDownMousePosition.y + Input.mousePosition.y) * speed;
                        break;
                }
            }
            else
            {
                Data.BezierList[KeystoneBezierPointDown] =
                    BezierPointDownData +
                    (-BezierPointDownMousePosition + new Vector2(Input.mousePosition.x, Input.mousePosition.y)) / 1f * (Input.GetKey(KeyCode.LeftShift) ? 0.3f : 1);
            }


            Monitor.SetKeystone(CurrentSetting, true);
            SetData();

            if (keyCheck == false && Input.GetMouseButton(0) == false)
            {
                KeystoneBezierPointDown = -1;
            }
        }

        if (KeystoneBezierPointDownX >= 0)
        {
            Data.BezierList[KeystoneBezierPointDownX] =
                BezierPointDownData + //0.7f
                (-BezierPointDownMousePosition + new Vector2(Input.mousePosition.x, BezierPointDownMousePosition.y)) / 1f * (Input.GetKey(KeyCode.LeftShift) ? 0.3f : 1);

            Monitor.SetKeystone(CurrentSetting, true);
            SetData();

            if (Input.GetMouseButton(0) == false)
            {
                KeystoneBezierPointDownX = -1;
            }
        }

        if (KeystoneBezierPointDownY >= 0)
        {
            Data.BezierList[KeystoneBezierPointDownY] =
                BezierPointDownData +
                (-BezierPointDownMousePosition + new Vector2(BezierPointDownMousePosition.x, Input.mousePosition.y)) / 1f * (Input.GetKey(KeyCode.LeftShift) ? 0.3f : 1);

            Monitor.SetKeystone(CurrentSetting, true);
            SetData();

            if (Input.GetMouseButton(0) == false)
            {
                KeystoneBezierPointDownY = -1;
            }
        }
    }
}
