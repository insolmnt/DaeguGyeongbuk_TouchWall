using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.IO;
using System.Diagnostics;

public class ShutdownManager : MonoBehaviour
{
    public bool IsKeyCodeUse = false;
    public KeyCode KeyCode;

    public GameObject ShutdownSettingPanel;

    public Toggle UseToggle;
    public GameObject UsePanel;

    [Header("일월화수목금토 순서")]
    public InputField[] HourFieldList;
    public InputField[] MinuteFieldList; 

    public Button SaveButton;

    public GameObject ShutdownPanel;
    public Text ShutdownStateText;

    private float CurrentTime = 0;
    private bool mIsShutdownCacel = false;
    private DateTime TargetDateTime;
    private bool mIsLoadEnd = false;

    private bool mIsSettingView = false;

    private bool beforCursorVisible;

    public ShutdownData Data;


    void Start()
    {
        ShutdownSettingPanel.gameObject.SetActive(false);
        Load();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// PC 종료 설정 버튼을 눌렀을 때 호출
    /// </summary>
    /// <param name="isShow"></param>
    public void ShowSettingPanel(bool isShow)
    {
        mIsSettingView = isShow;

        ShutdownSettingPanel.gameObject.SetActive(mIsSettingView);
        if (mIsSettingView)
        {
            beforCursorVisible = Cursor.visible;
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = beforCursorVisible;
        }
    }

    public void OnUseToggleChange()
    {
        UsePanel.SetActive(!UseToggle.isOn);
        Data.Use = UseToggle.isOn;
    }

    public void Shutdown()
    {
        ShutdownPanel.SetActive(true);
        ShutdownStateText.text = "컴퓨터를 종료합니다.";
        UnityEngine.Debug.Log("컴퓨터를 종료합니다.");
#if UNITY_EDITOR
        IsShutdownCanel = true;
#else
        Process.Start("shutdown.exe", "-s -t 2");
#endif
    }

    bool IsShutdownCanel = false;
    IEnumerator ShowShutdownPanel()
    {
        beforCursorVisible = Cursor.visible;
        Cursor.visible = true;
        IsShutdownCanel = false;
        ShutdownPanel.SetActive(true);
        for(int i=30; i>= 0; i--)
        {
            ShutdownStateText.text = string.Format("<color=#ff0>{0}</color>초 뒤 컴퓨터를 자동 종료합니다.", i);
            yield return new WaitForSeconds(1f);
            if (IsShutdownCanel)
            {
                yield break;
            }
        }

        Shutdown();
    }

    public void OnShutdownCacelButtonClick()
    {
        IsShutdownCanel = true;
        ShutdownPanel.SetActive(false);
    }


    void Update()
    {
        if (IsKeyCodeUse && Input.GetKeyDown(KeyCode))
        {
            ShowSettingPanel(true);
        }

        CurrentTime += Time.deltaTime;
        if(CurrentTime < 0.2f)
        {
            return;
        }
        CurrentTime -= 0.2f;

        if(mIsLoadEnd && mIsShutdownCacel == false && UseToggle.isOn && (TargetDateTime - DateTime.Now).TotalSeconds < 0)
        {
            mIsShutdownCacel = true;
            StartCoroutine(ShowShutdownPanel());
        }
    }
    

    private void SetTargetdate()
    {
        TargetDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Data.HourList[(int)DateTime.Now.DayOfWeek], Data.MinuteList[(int)DateTime.Now.DayOfWeek], 0);
        mIsShutdownCacel = false;
    }

    public void OnCancelButtonClick()
    {
        Cursor.visible = beforCursorVisible;
        mIsShutdownCacel = true;
        ShowSettingPanel(false);
        Load();
    }


    private bool isDataChange = false;
    public void OnInputFieldChange()
    {
        if (isDataChange || mIsLoadEnd == false)
        {
            return;
        }

        isDataChange = true;
        for (int i = 0; i < HourFieldList.Length; i++)
        {
            if (int.TryParse(HourFieldList[i].text, out Data.HourList[i]))
            {
                Data.HourList[i] = Mathf.Clamp(Data.HourList[i], 0, 23);
            }
            HourFieldList[i].text = Data.HourList[i].ToString();


            if (int.TryParse(MinuteFieldList[i].text, out Data.MinuteList[i]))
            {
                Data.MinuteList[i] = Mathf.Clamp(Data.MinuteList[i], 0, 59);
            }
            MinuteFieldList[i].text = Data.MinuteList[i].ToString();
        }
        isDataChange = false;
    }


    public void Load()
    {
        mIsLoadEnd = false;

        Data = DataManager.GetData< ShutdownData>("Shutdown");
        if(Data == null)
        {
            Data = new ShutdownData();

            Data.Use = false;
            Data.HourList = new int[] { 20, 20, 20, 20, 20, 20, 20 };
            Data.MinuteList = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        }


        UseToggle.isOn = Data.Use;

        for (int i = 0; i < HourFieldList.Length; i++)
        {
            HourFieldList[i].text = Data.HourList[i].ToString();
            MinuteFieldList[i].text = Data.MinuteList[i].ToString();
        }

        OnUseToggleChange();
        SetTargetdate();

        mIsLoadEnd = true;
    }

    public void OnSaveButtonClick()
    {
        ShowSettingPanel(false);

        DataManager.SetData("Shutdown", Data);
        SetTargetdate();
    }

    public string GetDay(DateTime dt)
    {
        switch (dt.DayOfWeek)
        {
            case DayOfWeek.Monday:
                return "월요일";
            case DayOfWeek.Tuesday:
                return "화요일";
            case DayOfWeek.Wednesday:
                return "수요일";
            case DayOfWeek.Thursday:
                return "목요일";
            case DayOfWeek.Friday:
                return "금요일";
            case DayOfWeek.Saturday:
                return "토요일";
            case DayOfWeek.Sunday:
                return "일요일";
        }
        return "";
    }

    [System.Serializable]
    public class ShutdownData
    {
        public bool Use = false;

        /// <summary>
        /// 일월화수목금토 순서
        /// </summary>
        public int[] HourList;
        public int[] MinuteList;
    }
}