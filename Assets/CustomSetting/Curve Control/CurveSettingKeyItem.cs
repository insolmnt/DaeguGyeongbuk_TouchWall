using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class CurveSettingKeyItem : MonoBehaviour {
    public CurveSetting Manager;


    public Image CurveImage;
    public Image CurveLeftImage;
    public Image CurveRightImage;
    public UILineRenderer CurveLine;
    public Text KeyText;

    [Header("Data")]
    public Keyframe Data;
    public int Index
    {
        set
        {
            index = value;
            if (index < 10)
            {
                KeyText.text = (index + 1).ToString();
            }
            else
            {
                KeyText.text = "";
            }
        }
        get
        {
            return index;
        }
    }
    private int index = -1;

    private Vector2 PointDownMousePosition;
    private int IsPointDown = -1;
    public bool IsKeyDown = false;
    private float ArrowDownTime = 0;
    private Vector2 Before;

    private bool IsFirst
    {
        get
        {
            return Index == 0;
        }
    }
    private bool IsLast
    {
        get
        {
            return Index >= Manager.Curve.keys.Length - 1;
        }
    }

    public void SetData(Keyframe data)
    {
        Data = data;
        SetUI();
    }

    public void SetUI()
    {
        CurveImage.rectTransform.anchoredPosition = new Vector2(Manager.CurveBgWidth * Data.time, Manager.CurveBgHeight * Data.value);

        CurveLeftImage.rectTransform.anchoredPosition = new Vector2(-1, -Data.inTangent).normalized * 30f;
        CurveRightImage.rectTransform.anchoredPosition = new Vector2(1, Data.outTangent).normalized * 30f;


        CurveLeftImage.gameObject.SetActive(!IsFirst);
        CurveRightImage.gameObject.SetActive(!IsLast);

        CurveLine.Points = new Vector2[]
        {
            CurveLeftImage.gameObject.activeSelf ? CurveLeftImage.rectTransform.anchoredPosition : Vector2.zero
            , Vector2.zero
            , CurveRightImage.gameObject.activeSelf ? CurveRightImage.rectTransform.anchoredPosition : Vector2.zero
        };
    }

    public void OnPointDown(bool isMouse)
    {
        ImageReset();
        if (Input.GetMouseButton(1) && isMouse)
        {
            Manager.OnKeyframePointRightDown(this);
        }
        //if (Input.GetMouseButton(0))
        else {
            IsPointDown = 0;
            var mouse = (Vector2)Input.mousePosition;
            PointDownMousePosition = mouse - CurveImage.rectTransform.anchoredPosition; //mouse - CurveImage.rectTransform.anchoredPosition;
            CurveImage.color = new Color(1, 0.5f, 0.5f, 1);
        }
    }
    public void OnLeftPointDown(bool isMouse)
    {
        ImageReset();
        if (Input.GetMouseButton(1) && isMouse)
        {
            Data.inTangent = 1;
            Manager.OnCurveDataChange(true);
        }
        //if (Input.GetMouseButton(0))
        else {
            IsPointDown = 1;
            var mouse = (Vector2)Input.mousePosition;
            PointDownMousePosition = mouse - CurveLeftImage.rectTransform.anchoredPosition;
            CurveLeftImage.color = new Color(1, 0.5f, 0.5f, 1);
        }
    }
    public void OnRightPointDown(bool isMouse)
    {
        ImageReset();
        if (Input.GetMouseButton(1) && isMouse)
        {
            Data.outTangent = 1;
            Manager.OnCurveDataChange(true);
        }
        //if (Input.GetMouseButton(0))
        else {
            IsPointDown = 2;
            var mouse = (Vector2)Input.mousePosition;
            PointDownMousePosition = mouse - CurveRightImage.rectTransform.anchoredPosition;
            CurveRightImage.color = new Color(1, 0.5f, 0.5f, 1);
        }
    }

    public void ImageReset()
    {
        CurveImage.color = Color.white;
        CurveLeftImage.color = new Color(0.8f, 1, 0, 1);
        CurveRightImage.color = new Color(0.8f, 1, 0, 1);
    }

    private void Update()
    {
        if(IsPointDown >= 0)
        {
            ArrowDownTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                PointDownMousePosition += new Vector2(0, -0.2f);
                ArrowDownTime = 0;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                PointDownMousePosition += new Vector2(-0.2f, 0);
                ArrowDownTime = 0;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                PointDownMousePosition += new Vector2(0, 0.2f);
                ArrowDownTime = 0;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PointDownMousePosition += new Vector2(0.2f, 0);
                ArrowDownTime = 0;
            }
            if(ArrowDownTime > 0.5f)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    PointDownMousePosition += new Vector2(0, -10) * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    PointDownMousePosition += new Vector2(-10, 0) * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    PointDownMousePosition += new Vector2(0, 10) * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    PointDownMousePosition += new Vector2(10, 0) * Time.deltaTime;
                }
            }

            var mouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var v2 = mouse - PointDownMousePosition;
            switch (IsPointDown)
            {
                case 0:
                    //if (IsFirst)
                    //{
                    //    Data.time = 0;
                    //}
                    //else if (IsLast)
                    //{
                    //    Data.time = 1;
                    //}
                    //else
                    {
                        Data.time = Mathf.Clamp((mouse.x - PointDownMousePosition.x) / Manager.CurveBgWidth, 0f, 1f);
                    }
                    Data.value = (mouse.y - PointDownMousePosition.y) / Manager.CurveBgHeight;
                    break;
                case 1:
                    Data.inTangent = (v2.y ) / (v2.x );
                    break;
                case 2:
                    Data.outTangent = (v2.y ) / (v2.x);
                    break;
            }

            Manager.OnCurveDataChange(false, Before != v2);

            Before = v2;

            if(Input.GetMouseButton(0) == false && IsKeyDown == false)
            {
                Manager.OnCurveDataChange(true);
                IsPointDown = -1;
                ImageReset();
            }
        }
    }
}
