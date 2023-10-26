using InsolDefaultProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class WarpMaskImageKeystoneSetting : MonoBehaviour
{
    public MaskObjectManager Manager;

    [Header("우상, 우하, 좌하, 좌상")]
    public RectTransform[] KeystoneArrowList;
    public RectTransform[] BezierHandleList;
    public UILineRenderer[] LineList;

    public Toggle OnlyRectangleToggle;

    [Header("Data")]
    public WarpMaskImage CurrentSettingImage = null;
    private bool mIsLoad = false;
    private bool keyCheck = false;
    private float keyDownTime = 0;

    private int KeystonePointDown = -1;
    private Vector2 PointDownMousePosition;
    private Vector2 PointDownArrowPosition;
    private int KeystoneBezierPointDown = -1;
    private Vector2 BezierPointDownMousePosition;
    private Vector2 BezierPointDownData;
    public MonitorKeystoneData Data
    {
        get
        {
            return CurrentSettingImage.Data.Keystone;
        }
    }

    public void SettingMaskImage(WarpMaskImage image)
    {
        if (CurrentSettingImage != null)
        {
            CurrentSettingImage.WarpImage.color = CurrentSettingImage.Data.ImageColor;
            if (CurrentSettingImage == image)
            {
                Hide();
                return;
            }
        }

        CurrentSettingImage = image;
        CurrentSettingImage.WarpImage.color = new Color(0.5f, 1f, 0, 1f);

        Show();
    }

    private void Show()
    {
        SettingManager.Instance.SettingPanel.gameObject.SetActive(false);
        keyCheck = false;
        keyDownTime = 0;

        KeystonePointDown = -1;
        KeystoneBezierPointDown = -1;


        mIsLoad = false;

        OnlyRectangleToggle.isOn = CurrentSettingImage.Data.isOnltyRectangle;
        ShowHandleImage();
        SetUI();
        mIsLoad = true;

        transform.SetParent(CurrentSettingImage.transform);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
        gameObject.SetActive(true);

    }
    public void ShowHandleImage()
    {
        foreach(var handle in BezierHandleList)
        {
            handle.gameObject.SetActive(!CurrentSettingImage.Data.isOnltyRectangle);
        }
    }
    private void SetUI()
    {
        for (int i = 0; i < KeystoneArrowList.Length; i++)
        {
            KeystoneArrowList[i].anchoredPosition = CurrentSettingImage.Data.Keystone.PointList[i];
        }

        BezierHandleList[0].anchoredPosition = CurrentSettingImage.WarpImage.topBezierLocalPositionHandleA;
        BezierHandleList[1].anchoredPosition = CurrentSettingImage.WarpImage.topBezierLocalPositionHandleB;
        BezierHandleList[2].anchoredPosition = CurrentSettingImage.WarpImage.rightBezierLocalPositionHandleA;
        BezierHandleList[3].anchoredPosition = CurrentSettingImage.WarpImage.rightBezierLocalPositionHandleB;
        BezierHandleList[4].anchoredPosition = CurrentSettingImage.WarpImage.bottomBezierLocalPositionHandleA;
        BezierHandleList[5].anchoredPosition = CurrentSettingImage.WarpImage.bottomBezierLocalPositionHandleB;
        BezierHandleList[6].anchoredPosition = CurrentSettingImage.WarpImage.leftBezierLocalPositionHandleA;
        BezierHandleList[7].anchoredPosition = CurrentSettingImage.WarpImage.leftBezierLocalPositionHandleB;


        if (LineList != null && LineList.Length > 0)
        {
            LineList[0].Points = new Vector2[] { BezierHandleList[0].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionTL };
            LineList[1].Points = new Vector2[] { BezierHandleList[1].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionTR };
            LineList[2].Points = new Vector2[] { BezierHandleList[2].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionTR };
            LineList[3].Points = new Vector2[] { BezierHandleList[3].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionBR };
            LineList[4].Points = new Vector2[] { BezierHandleList[4].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionBR };
            LineList[5].Points = new Vector2[] { BezierHandleList[5].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionBL };
            LineList[6].Points = new Vector2[] { BezierHandleList[6].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionBL };
            LineList[7].Points = new Vector2[] { BezierHandleList[7].anchoredPosition, CurrentSettingImage.WarpImage.cornerLocalPositionTL };
        }
    }

    public void OnStraightToggleChange()
    {
        if (CurrentSettingImage == false)
        {
            return;
        }
        CurrentSettingImage.Data.isOnltyRectangle = OnlyRectangleToggle.isOn;
        ShowHandleImage();
        //CurrentSettingImage.SetKeystone();
        //Invoke("SetData", 0.1f);
    }


    internal void Hide()
    {
        SettingManager.Instance.SettingPanel.gameObject.SetActive(true);
        keyCheck = false;
        keyDownTime = 0;

        KeystonePointDown = -1;
        KeystoneBezierPointDown = -1;


        if (CurrentSettingImage != null)
        {
            CurrentSettingImage.WarpImage.color = CurrentSettingImage.Data.ImageColor;
            CurrentSettingImage = null;
        }

        transform.SetParent(Manager.WarpMaskImageParent[0]);
        gameObject.SetActive(false);
    }




    public void OnBezierKeystoneArrowPointDown(int index)
    {
        keyCheck = false;
        keyDownTime = 0;
        if (Input.GetMouseButton(0))
        {
            KeystoneBezierPointDown = index;
            BezierPointDownMousePosition = Input.mousePosition;

            BezierPointDownData = Data.BezierList[index];
        }
    }


    public void OnKeystoneArrowPointDown(int index)
    {
        keyCheck = false;
        keyDownTime = 0;
        if (Input.GetMouseButton(0))
        {
            KeystonePointDown = index;
            PointDownMousePosition = Input.mousePosition;
            PointDownArrowPosition = KeystoneArrowList[index].anchoredPosition;
        }
    }


    private void Update()
    {
        if(CurrentSettingImage == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            if (ColorSetting.IsShowSetting)
            {
                ColorSetting.Instance.HideSetting();
            }
            else
            {
                ColorSetting.Instance.ShowSetting("색상 설정", CurrentSettingImage.Data.ImageColor, true, (result) =>
                {
                    CurrentSettingImage.Data.ImageColor = result;
                    CurrentSettingImage.WarpImage.color = result;
                });
            }
        }

        for (int i = 0; i < Manager.WarpMaskImageParent.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.F1 + i))
            {
                CurrentSettingImage.SetDisplay(i);
            }
        }


        float speed = 0.005f;


        if (Input.GetKeyDown(KeyCode.Slash))
        {
            OnlyRectangleToggle.isOn = !OnlyRectangleToggle.isOn;
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


            KeystoneArrowList[KeystonePointDown].anchoredPosition =
                PointDownArrowPosition +
                (-PointDownMousePosition + new Vector2(Input.mousePosition.x, Input.mousePosition.y)) * (Input.GetKey(KeyCode.LeftShift) ? 0.3f : 1); ;

            if (CurrentSettingImage.Data.isOnltyRectangle)
            {
                switch (KeystonePointDown)
                {
                    case 0: //우상
                        KeystoneArrowList[1].anchoredPosition = new Vector2(KeystoneArrowList[KeystonePointDown].anchoredPosition.x, KeystoneArrowList[1].anchoredPosition.y);
                        KeystoneArrowList[3].anchoredPosition = new Vector2(KeystoneArrowList[3].anchoredPosition.x, KeystoneArrowList[KeystonePointDown].anchoredPosition.y);
                        break;

                    case 1: //우하
                        KeystoneArrowList[0].anchoredPosition = new Vector2(KeystoneArrowList[KeystonePointDown].anchoredPosition.x, KeystoneArrowList[0].anchoredPosition.y);
                        KeystoneArrowList[2].anchoredPosition = new Vector2(KeystoneArrowList[2].anchoredPosition.x, KeystoneArrowList[KeystonePointDown].anchoredPosition.y);
                        break;

                    case 2: //좌하
                        KeystoneArrowList[3].anchoredPosition = new Vector2(KeystoneArrowList[KeystonePointDown].anchoredPosition.x, KeystoneArrowList[3].anchoredPosition.y);
                        KeystoneArrowList[1].anchoredPosition = new Vector2(KeystoneArrowList[1].anchoredPosition.x, KeystoneArrowList[KeystonePointDown].anchoredPosition.y);
                        break;

                    case 3: //좌상
                        KeystoneArrowList[2].anchoredPosition = new Vector2(KeystoneArrowList[KeystonePointDown].anchoredPosition.x, KeystoneArrowList[2].anchoredPosition.y);
                        KeystoneArrowList[0].anchoredPosition = new Vector2(KeystoneArrowList[0].anchoredPosition.x, KeystoneArrowList[KeystonePointDown].anchoredPosition.y);
                        break;
                }

            }

            for(int i=0; i<4; i++)
            {
                Data.PointList[i] = KeystoneArrowList[i].anchoredPosition;
            }

            CurrentSettingImage.SetKeystone();
            SetUI();

            if (keyCheck == false && Input.GetMouseButton(0) == false)
            {
                KeystonePointDown = -1;
            }
        }

        //if (CurrentSettingImage.isOnltyStraight == false)
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


            Data.BezierList[KeystoneBezierPointDown] =
                BezierPointDownData +
                (-BezierPointDownMousePosition + new Vector2(Input.mousePosition.x, Input.mousePosition.y)) / 1f * (Input.GetKey(KeyCode.LeftShift) ? 0.3f : 1);

            CurrentSettingImage.SetKeystone();
            SetUI();

            if (keyCheck == false && Input.GetMouseButton(0) == false)
            {
                KeystoneBezierPointDown = -1;
            }
        }
    }
}
