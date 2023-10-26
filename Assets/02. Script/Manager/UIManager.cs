using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static public UIManager Instance;

    public Canvas UiCanvas;

    public Text MsgText;

    private float CurrentMsgTime = 0;


    public Image[] FadeImageList;

    [Header("UI 캔버스 디스플레이 이동")]
    public GameObject TargetDisplayPanelObject;
    public CustomButton[] TargetDisplayButtonList;
    public int CurrentUiDisplay = 0;

    [Header("로딩")]
    public CanvasGroup LoadingObject;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        Application.wantsToQuit += ApplicationQuit;
    }



    public void FadeIn(float time)
    {
        foreach (var image in FadeImageList)
        {
            image.DOKill();
            image.DOColor(new Color(0, 0, 0, 0), time).onComplete = () =>
            {
                image.gameObject.SetActive(false);
            };
        }
    }
    public void FadeOut(float time)
    {
        foreach (var image in FadeImageList)
        {
            if (image.gameObject.activeSelf == false)
            {
                image.color = new Color(0, 0, 0, 0);
                image.gameObject.SetActive(true);
            }

            image.DOKill();
            image.DOColor(Color.black, time);
        }
    }


    private bool ApplicationQuit()
    {
        Quit();
        return false;
    }
    public void Quit()
    {
        foreach (var sensor in SensorManager.Instance.SensorList)
        {
            sensor.DisConnect();
        }
        ShowMessage("프로그램을 종료합니다.");
        Invoke("Kill", 1f);
    }
    void Kill()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }

    private void Start()
    {
        ShowCursor(false);


        SetTargetDisplay(0);
    }
    public void SetTargetDisplay(int display)
    {
        for (int i = 0; i < TargetDisplayButtonList.Length; i++)
        {
            TargetDisplayButtonList[i].Select(i == display);
        }

        UiCanvas.targetDisplay = display;

        CurrentUiDisplay = display;

        if (display < Display.displays.Length)
        {
            Display.displays[display].Activate();
        }
    }



    public void ShowLoading(bool isShow)
    {
        LoadingObject.DOKill();

        if (isShow)
        {
            if (LoadingObject.gameObject.activeSelf == false)
            {
                LoadingObject.alpha = 0;
                LoadingObject.gameObject.SetActive(true);
            }

            LoadingObject.DOFade(1, 0.5f);
        }
        else
        {
            LoadingObject.DOFade(0, 0.5f).onComplete = () =>
            {
                LoadingObject.gameObject.SetActive(false);
            };
        }
    }


    public void ShowMessage(string msg, float time = 5f)
    {
        MsgText.text = msg;
        CurrentMsgTime = time;
        MsgText.transform.parent.gameObject.SetActive(true);
    }

    public void ShowWarringMessage(string msg, float time = 3f)
    {
        //WarningMsgText.text = msg;
        //CurrentWarningMsgTime = time;
        //WarningMsgText.transform.parent.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(CurrentMsgTime > 0)
        {
            CurrentMsgTime -= Time.deltaTime;
            if(CurrentMsgTime <= 0)
            {
                MsgText.transform.parent.gameObject.SetActive(false);
            }
        }


        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1 + i))
            {
                SetTargetDisplay(i);
            }
        }

        //마우스 관련
        float dist = (Input.mousePosition - beforeMousePosition).sqrMagnitude;
        beforeMousePosition = Input.mousePosition;
        if (MouseVisibleTime > 0)
        {
            if (dist > 10 || Input.GetMouseButtonDown(0))
            {
                ShowCursor(true);
            }
        }
        else
        {
            if (dist > 5000 || Input.GetMouseButtonDown(0))
            {
                ShowCursor(true);
            }
        }

        if (MouseVisibleTime > 0)
        {
            MouseVisibleTime -= Time.deltaTime;
            if (MouseVisibleTime <= 0)
            {
                ShowCursor(false);
            }
        }

    }


    private float MouseVisibleTime = 0;
    private Vector3 beforeMousePosition;
    public void ShowCursor(bool isShow)
    {
        if(isShow)
        {
            MouseVisibleTime = 10f;
        }
        else
        {
            MouseVisibleTime = 0;
        }
        Cursor.visible = isShow;
    }

}
