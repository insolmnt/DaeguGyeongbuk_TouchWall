//Created by PoqXert (poqxert@gmail.com)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MsgBox : MonoBehaviour
{
    public RectTransform canvasRect;
    public RectTransform mainPanel;
    public GameObject blockPanel;
    //public Text captionText;
    public Text mainText;

    //public Text YesTimeText;
    public Text BtnYesText;
    public Text BtnNoText;
    public Text BtnCancelText;

    public Image backgroundImg;
    public Image btnYesImg;
    public Image btnNoImg;
    public Image btnCancelImg;
    public GameObject eventer;

    public List<MSGBoxStyle> boxStyles;

    public static MsgBox _lastMsgBox;

    private DialogResultMethod calledMethod;

    static private float CurrentShowTime = 0;
    static private float TargetShowTime = 0;

    [System.Serializable]
    public class MSGBoxStyle
    {
        public string name;

        public Color background;
        public Color btnYesColor;
        public Color btnNoColor;
        public Color btnCancelColor;

        [Space(10)]
        public Color mainTextColor = Color.black;
        public Color btnYesTextColor = Color.black;
        public Color btnNoTextColor = Color.black;
        public Color btnCancelTextColor = Color.black;
    }

    public void ButtonClickEvent(int btn)
    {
        MsgBox.Close();
        if (calledMethod != null)
            calledMethod((DialogResult)btn);
    }

    public void BuildMessageBox(string msg, MsgBoxButtons btns = MsgBoxButtons.OK)
    {
        calledMethod = null;
        blockPanel.SetActive(false);

        mainText.text = msg;


        //YesTimeText.gameObject.SetActive(false);

        SetStyle(MsgBoxStyle.Information);

        //this.iconImg.sprite = boxStyles[styleId].icon;

        SetButtons(btns);
    }

    private void SetButtons(MsgBoxButtons btns)
    {
        btnNoImg.gameObject.SetActive(true);
        btnCancelImg.gameObject.SetActive(true);
        switch (btns)
        {
            case MsgBoxButtons.OK:
                BtnYesText.text = "확인";
                btnNoImg.gameObject.SetActive(false);
                btnCancelImg.gameObject.SetActive(false);
                break;

            case MsgBoxButtons.OK_CANCEL:
                BtnYesText.text = "확인";
                BtnCancelText.text = "취소";
                btnNoImg.gameObject.SetActive(false);
                break;

            case MsgBoxButtons.YES_NO:
                BtnYesText.text = "예";
                BtnNoText.text = "아니요";
                btnCancelImg.gameObject.SetActive(false);
                break;

            case MsgBoxButtons.YES_NO_CANCEL:
                BtnYesText.text = "예";
                BtnNoText.text = "아니요";
                BtnCancelText.text = "취소";
                break;
        }
    }

    /// <summary>
    /// MsgBox.Close()는 자동 호출됨
    /// </summary>
    public static MsgBox Show(string msg)
    {
        Close();

        CurrentShowTime = 0;
        TargetShowTime = 0;

        var boxObject = (GameObject)Instantiate(Resources.Load("MSG"));

        _lastMsgBox = boxObject.GetComponent<MsgBox>();
        _lastMsgBox.BuildMessageBox(msg);
        if (EventSystem.current == null)
            _lastMsgBox.eventer.SetActive(true);
        IsShow = true;

        return _lastMsgBox;
    }



    public MsgBox SetButtonType(MsgBoxButtons buttons)
    {
        SetButtons(buttons);
        return this;
    }
    public MsgBox OnResult(DialogResultMethod method)
    {
        calledMethod = method;
        return this;
    }


    public MsgBox SetButtonText(string btnYes = "", string btnNo = "", string btnCancel = "")
    {
        if (string.IsNullOrEmpty(btnYes))
        {
            BtnYesText.text = btnYes;
        }
        if (string.IsNullOrEmpty(btnNo))
        {
            BtnNoText.text = btnNo;
        }
        if (string.IsNullOrEmpty(btnCancel))
        {
            BtnCancelText.text = btnCancel;
        }
        return this;
    }

    /// <summary>
    /// 스타일 변경 (색상)
    /// </summary>
    public MsgBox SetStyle(MsgBoxStyle style)
    {
        int styleId = (int)style;

        mainText.color = boxStyles[styleId].mainTextColor;
        //YesTimeText.color = boxStyles[styleId].mainTextColor;
        BtnYesText.color = boxStyles[styleId].btnYesTextColor;
        BtnNoText.color = boxStyles[styleId].btnNoTextColor;
        BtnCancelText.color = boxStyles[styleId].btnCancelTextColor;

        backgroundImg.color = boxStyles[styleId].background;
        btnYesImg.color = boxStyles[styleId].btnYesColor;
        btnNoImg.color = boxStyles[styleId].btnNoColor;
        btnCancelImg.color = boxStyles[styleId].btnCancelColor;
        return this;
    }

    [ContextMenu("스타일 : Info")]
    private void SetStI()
    {
        SetStyle(MsgBoxStyle.Information);
    }
    [ContextMenu("스타일 : Question")]
    private void SetStQ()
    {
        SetStyle(MsgBoxStyle.Question);
    }
    [ContextMenu("스타일 : Warning")]
    private void SetStW()
    {
        SetStyle(MsgBoxStyle.Warning);
    }
    [ContextMenu("스타일 : Error")]
    private void SetStE()
    {
        SetStyle(MsgBoxStyle.Error);
    }
    [ContextMenu("스타일 : Custom")]
    private void SetStC()
    {
        SetStyle(MsgBoxStyle.Custom);
    }


    /// <summary>
    /// 일정 시간 후 자동 Close (확인 버튼 클릭 처리)
    /// </summary>
    public MsgBox SetAutoCloseTime(float time)
    {
        //YesTimeText.gameObject.SetActive(true);
        TargetShowTime = time;
        return this;
    }

    /// <summary>
    /// 배경 검은색 페이드 처리 여부 설정
    /// </summary>
    public MsgBox SetBackgroundBlack(bool isBlack)
    {
        _lastMsgBox.blockPanel.SetActive(isBlack);
        return this;
    }




    static public bool IsShow = false;

    private void Update()
    {
        if (TargetShowTime > 0)
        {
            CurrentShowTime += Time.deltaTime;

            //YesTimeText.text = "(" + (TargetShowTime - CurrentShowTime).ToString("F0") + ")";
            if (CurrentShowTime >= TargetShowTime)
            {
                TargetShowTime = 0;
                CurrentShowTime = 0;
                ButtonClickEvent(0);
            }
        }
    }

    public static void Close()
    {
        if (_lastMsgBox != null)
        {
            DestroyImmediate(_lastMsgBox.gameObject);
        }
        TargetShowTime = 0;
        CurrentShowTime = 0;
        IsShow = false;
    }
}

public delegate void DialogResultMethod(DialogResult result);

public enum DialogResult
{
    YES_OK = 0,
    NO = 1,
    CANCEL = 2
}

public enum MsgBoxStyle
{
    Information = 0,
    Question = 1,
    Warning = 2,
    Error = 3,
    Custom = 4
}

public enum MsgBoxButtons
{
    OK = 0,
    OK_CANCEL = 1,
    YES_NO = 2,
    YES_NO_CANCEL = 3,
}