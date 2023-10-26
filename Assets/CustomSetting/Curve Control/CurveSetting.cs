using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class CurveSetting : MonoBehaviour {
    static public CurveSetting Instance;

    public Canvas UICanvas;
    public int Detaile = 100;

    public Text TitleText;
    public Toggle RealTimeRefreshToggle;
    public Image CurveBackground;
    public Image DotPrefab;
    public UILineRenderer DotLint;

    public CurveSettingKeyItem KeyItemPrefab;
    public List<CurveSettingKeyItem> KeyItemList = new List<CurveSettingKeyItem>();
    private System.Action<AnimationCurve> OnResult = null;
    private System.Action OnClose = null;

    public Image EmptySubPanel;
    public Image KeyFrameSubPanel;

    [Header("Data")]
    //public CurveData Data;
    public AnimationCurve Curve;
    public float CurveBgWidth = 200;
    public float CurveBgHeight = 100;

    private bool mIsInit = false;

    private void Awake()
    {
        Instance = this;
    }
    private void Init()
    {
        if (mIsInit)
        {
            return;
        }
        mIsInit = true;

        CurveBgWidth = CurveBackground.rectTransform.sizeDelta.x;
        CurveBgHeight = CurveBackground.rectTransform.sizeDelta.y;

        DotPrefab.gameObject.SetActive(false);

        KeyItemPrefab.gameObject.SetActive(false);

        //for (int i = 0; i < DotList.Length; i++)
        //{
        //    DotList[i] = Instantiate(DotPrefab, DotPrefab.transform.parent);
        //    DotList[i].gameObject.SetActive(true);
        //}
    }

    private void Update()
    {
        for(int i=0; i< KeyItemList.Count; i++)
        {
            if(KeyItemList[i].gameObject.activeSelf == false)
            {
                continue;
            }

            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                KeyItemList[i].IsKeyDown = true;
                KeyItemList[i].OnPointDown(false);
            }
            if (Input.GetKeyUp(KeyCode.Alpha1 + i))
            {
                foreach(var it in KeyItemList)
                {
                    //KeyItemList[i].IsKeyDown = false;
                    it.IsKeyDown = false;
                }
            }


            if (KeyItemList[i].IsKeyDown)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Q))
                {
                    KeyItemList[i].OnLeftPointDown(false);
                }
                else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.W))
                {
                    KeyItemList[i].OnRightPointDown(false);
                }
                else if (Input.GetMouseButtonUp(0) 
                    || Input.GetMouseButtonUp(1)
                    || Input.GetKeyUp(KeyCode.Q)
                    || Input.GetKeyUp(KeyCode.W))
                {
                    KeyItemList[i].OnPointDown(false);
                }
            }
        }
    }

    public void ShowCurveSetting(string title, AnimationCurve curve, System.Action<AnimationCurve> onResult, bool isRealTimeRefreshToggle = false, System.Action onClose = null)
    {
        Init();

        OnResult = onResult;
        OnClose = onClose;
        Curve = curve;

        TitleText.text = title;
        Make();

        RealTimeRefreshToggle.isOn = isRealTimeRefreshToggle;

        gameObject.SetActive(true);
    }
    public void OnCloseButtonClick()
    {
        gameObject.SetActive(false);
        if (OnResult != null)
        {
            OnResult.Invoke(Curve);
        }
        OnResult = null;

        if(OnClose != null)
        {
            OnClose.Invoke();
        }
        OnClose = null;
    }

    private void Make()
    {
        foreach (var item in KeyItemList)
        {
            item.gameObject.SetActive(false);
        }

        //Curve = Data.GetAnimationCurve();
        for (int i = 0; i < Curve.keys.Length; i++)
        {
            var item = GetIdleItem();
            item.SetData(Curve.keys[i]);
            item.gameObject.SetActive(true);
            item.Index = i;
        }

        Draw();
    }

    private void Draw()
    {
        for (int i = 0; i < Curve.keys.Length; i++)
        {
            KeyItemList[i].SetUI();
        }



        DotLint.Points = new Vector2[Detaile];
        for (int i = 0; i < Detaile; i++)
        {
            //DotList[i].rectTransform.anchoredPosition = new Vector2(i * (CurveBgWidth / DotList.Length), Curve.Evaluate((float)i / DotList.Length) * CurveBgHeight);

            DotLint.Points[i] = new Vector2(i * (CurveBgWidth / Detaile), Curve.Evaluate((float)i / Detaile) * CurveBgHeight);
        }


    }


    public void OnCurveDataChange(bool isResult, bool isChagne = false)
    {
        int count = 0;
        KeyItemList.Sort((a, b) => { return (int)(a.Data.time * 100 - b.Data.time * 100); });
        for(int i=0; i<KeyItemList.Count; i++)
        {
            KeyItemList[i].Index = i;
            if (KeyItemList[i].gameObject.activeSelf)
            {
                count++;
            }
        }

        Curve.keys = new Keyframe[0];
        foreach(var key in KeyItemList)
        {
            if (key.gameObject.activeSelf)
            {
                Curve.AddKey(new Keyframe(key.Data.time, key.Data.value, key.Data.inTangent, key.Data.outTangent));
            }
        }

        //Curve = Data.GetAnimationCurve();

        Draw();
        if(((RealTimeRefreshToggle.isOn && isChagne) || isResult) && OnResult != null)
        {
            OnResult.Invoke(Curve);
        }
    }

    public void OnEmptyPointDown()
    {
        for (int i = 0; i < KeyItemList.Count; i++)
        {
            if(KeyItemList[i].gameObject.activeSelf == false)
            {
                continue;
            }

            if (Input.GetKey(KeyCode.Alpha0 + i))
            {
                return;
            }
        }


        MousePosition = GetMousePosition();
        KeyFrameSubPanel.gameObject.SetActive(false);

        if (Input.GetMouseButtonDown(0))
        {
            EmptySubPanel.gameObject.SetActive(false);
        }
        if (Input.GetMouseButtonDown(1))
        {
            //RectTransformUtility.Scr(rectTransform, mousePosition, canvas.worldCamera, out localPosition);


            EmptySubPanel.rectTransform.anchoredPosition = GetMousePosition() - new Vector2(UICanvas.pixelRect.width * 0.5f, UICanvas.pixelRect.height * 0.5f)/* + new Vector2(53.5f, 56)*/;
            EmptySubPanel.gameObject.SetActive(true);
        }
    }
    private Vector2 GetMousePosition()
    {
        var v2 = (Vector2)Input.mousePosition;
        return new Vector2(v2.x / Screen.width * UICanvas.pixelRect.width, v2.y / Screen.height * UICanvas.pixelRect.height);
    }
    private Vector2 MousePosition;
    private CurveSettingKeyItem CurrentKeyFrame;
    public void OnKeyframePointRightDown(CurveSettingKeyItem item)
    {
        EmptySubPanel.gameObject.SetActive(false);
        CurrentKeyFrame = item;




        KeyFrameSubPanel.rectTransform.anchoredPosition = GetMousePosition() - new Vector2(UICanvas.pixelRect.width * 0.5f, UICanvas.pixelRect.height * 0.5f)/* + new Vector2(53.5f, 56)*/;
        KeyFrameSubPanel.gameObject.SetActive(true);
    }

    public void OnSubPanelResetButtonClick()
    {
        EmptySubPanel.gameObject.SetActive(false);
        KeyFrameSubPanel.gameObject.SetActive(false);

        CurrentKeyFrame.Data.value = (float)CurrentKeyFrame.Index / (Curve.keys.Length - 1);
        CurrentKeyFrame.Data.time = CurrentKeyFrame.Data.value;
        CurrentKeyFrame.Data.inTangent = 1;
        CurrentKeyFrame.Data.outTangent = 1;
        OnCurveDataChange(true);
    }
    public void OnSubPanelAddFrameButtonClick()
    {
        EmptySubPanel.gameObject.SetActive(false);
        KeyFrameSubPanel.gameObject.SetActive(false);

        float x = MousePosition.x - UICanvas.pixelRect.width * 0.5f + CurveBgWidth * 0.5f - CurveBackground.rectTransform.anchoredPosition.x;
        float y = MousePosition.y - UICanvas.pixelRect.height * 0.5f + CurveBgHeight * 0.5f - CurveBackground.rectTransform.anchoredPosition.y;
        Curve.AddKey(new Keyframe(x / CurveBgWidth, y / CurveBgHeight, 1, 1));
        Make();
        OnCurveDataChange(true);
    }
    public void OnSubPanelDeleteFrameButtonClick()
    {
        EmptySubPanel.gameObject.SetActive(false);
        KeyFrameSubPanel.gameObject.SetActive(false);

        Curve.RemoveKey(CurrentKeyFrame.Index);
        Make();
        OnCurveDataChange(true);
    }


    private CurveSettingKeyItem GetIdleItem()
    {
        foreach(var item in KeyItemList)
        {
            if(item.gameObject.activeSelf == false)
            {
                return item;
            }
        }

        var curveItem = Instantiate(KeyItemPrefab, KeyItemPrefab.transform.parent);
        KeyItemList.Add(curveItem);
        return curveItem;
    }

    public void SetDefaultData()
    {
        Curve.keys = new Keyframe[]
        {
            new Keyframe()
            {
                time = 0,
                value = 0,
                inTangent = 1,
                outTangent = 1
            },
            new Keyframe()
            {
                time = 0.5f,
                value = 0.5f,
                inTangent = 1,
                outTangent = 1
            },
            new Keyframe()
            {
                time = 1,
                value = 1,
                inTangent = 1,
                outTangent = 1
            }
        };

        Make();
        if(OnResult != null)
        {
            OnResult.Invoke(Curve);
        }
    }
}



//[System.Serializable]
//public class CurveData
//{
//    public List<CurveKeyData> keys = new List<CurveKeyData>()
//    {
//            new CurveKeyData()
//            {
//                time = 0,
//                value = 0,
//                inTangent = 1,
//                outTangent = 1
//            },
//            new CurveKeyData()
//            {
//                time = 0.5f,
//                value = 0.5f,
//                inTangent = 1,
//                outTangent = 1
//            },
//            new CurveKeyData()
//            {
//                time = 1,
//                value = 1,
//                inTangent = 1,
//                outTangent = 1
//            }
//    };

//    public AnimationCurve GetAnimationCurve()
//    {
//        var curve = new AnimationCurve();
//        for (int i = 0; i < keys.Count; i++)
//        {
//            curve.AddKey(new Keyframe()
//            {
//                time = keys[i].time,
//                value = keys[i].value,
//                inTangent = keys[i].inTangent,
//                outTangent = keys[i].outTangent,
//                inWeight = keys[i].inWeight,
//                outWeight = keys[i].outWeight
//            });
//        }

//        return curve;
//    }
//}
//[System.Serializable]
//public class CurveKeyData
//{
//    public float time;
//    public float value;
//    public float inTangent = 1;
//    public float outTangent = 1;
//    public float inWeight = 1 / 3f;
//    public float outWeight = 1 / 3f;
//    public WeightedMode weightedMode;
//}
