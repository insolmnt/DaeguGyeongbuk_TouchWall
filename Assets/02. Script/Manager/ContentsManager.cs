using Coffee.UIExtensions;
using DG.Tweening;
using geniikw.DataRenderer2D;
using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ContentsManager : MonoBehaviour
{
    static public ContentsManager Instance;
    [Header("배경")]
    public Image TestImage;
    public Image BgImage;
    public Image LineImage;
    public Image TextImage;

    [Header("공항")]
    public AirplaneLine[] AirplaneLineList;
    public Image AirportIcon;

    [Header("철도")]
    public Image RailroadIcon;

    [Header("전체")]
    public UIDissolve DaeguCicleUI;
    public Image DaeguCircleImage;
    public UIDissolve MegaCicleUI;
    public Image MegaCircleImage;
    public CanvasGroup MegaTextGroup;
    public AreaColor AreaLeft;
    public AreaColor AreaRight;
    [Header("")]
    public CanvasGroup ArrowGroup;
    public ArrowLine[] ArrowList;

    [Header("Data")]
    public ContentsData Data;
    public bool IsShowAirplane = false;
    public bool IsShowRailroad = false;
    public bool IsShowMega = false;
    public bool IsShowArrow = false;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TestImage.gameObject.SetActive(false);
        Load();

        ClearMega();
    }

    void ClearMega()
    {
        ArrowGroup.alpha = 0;
        foreach (var arrow in ArrowList)
        {
            arrow.LineTVal = 0;
        }


        MegaTextGroup.alpha = 0;

        DaeguCicleUI.effectFactor = 1;
        MegaCicleUI.effectFactor = 1;

        AreaLeft.Clear();
        AreaRight.Clear();
    }

    private void ShowImage(Image image)
    {
        float fadeTime = 1f;

        image.transform.localScale = Vector3.one * 1.5f;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

        image.transform.DOScale(Vector3.one, fadeTime);
        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 1), fadeTime);
    }
    public void ShowAirplane(bool isShow)
    {
        IsShowAirplane = isShow;

        if (isShow)
        {
            StartCoroutine(ShowAirplaneEvent());
        }
        else
        {
            foreach (var image in AirplaneLineList)
            {
                image.Hide();
            }
        }
    }
    private IEnumerator ShowAirplaneEvent()
    {
        AirportIcon.DOColor(new Color(1, 1, 1, 0), 0.2f);
        yield return new WaitForSeconds(0.3f);
        AirportIcon.DOColor(new Color(1, 1, 1, 1), 0.2f);
        yield return new WaitForSeconds(0.3f);
        AirportIcon.DOColor(new Color(1, 1, 1, 0), 0.2f);
        yield return new WaitForSeconds(0.3f);
        AirportIcon.DOColor(new Color(1, 1, 1, 1), 0.2f);
        yield return new WaitForSeconds(0.3f);

        foreach (var image in AirplaneLineList)
        {
            image.Show();
        }
    }
    public void ShowRailroad(bool isShow)
    {
        IsShowRailroad = isShow;

        if (isShow)
        {
            StartCoroutine(ShowRailroadEvent());
        }
        else
        {
            TrainManager.Instance.Hide();
        }
    }
    private IEnumerator ShowRailroadEvent()
    {
        RailroadIcon.DOColor(new Color(1, 1, 1, 0), 0.2f);
        yield return new WaitForSeconds(0.3f);
        RailroadIcon.DOColor(new Color(1, 1, 1, 1), 0.2f);
        yield return new WaitForSeconds(0.3f);
        RailroadIcon.DOColor(new Color(1, 1, 1, 0), 0.2f);
        yield return new WaitForSeconds(0.3f);
        RailroadIcon.DOColor(new Color(1, 1, 1, 1), 0.2f);
        yield return new WaitForSeconds(0.3f);


        TrainManager.Instance.Show();
    }

    public void ShowMega(bool isShow)
    {
        IsShowMega = isShow;

        if (isShow)
        {
            StartCoroutine(ShowMegaEvent());
        }
        else
        {
            DaeguCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 0), 2f);
            MegaCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 0), 2f);

            MegaTextGroup.DOFade(0, 2f);

            AreaLeft.Hide();
            AreaRight.Hide();
        }
    }

    IEnumerator ShowMegaEvent()
    {
        AreaLeft.Show();
        yield return new WaitForSeconds(1f);
        AreaRight.Show();
        yield return new WaitForSeconds(3f);

        DaeguCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 1), 2f);
        DaeguCicleUI.effectFactor = 1;
        DOTween.To(() => DaeguCicleUI.effectFactor, x => DaeguCicleUI.effectFactor = x, 0f, 2f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(3f);



        MegaCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 1), 0.5f);
        MegaCicleUI.effectFactor = 1;
        DOTween.To(() => MegaCicleUI.effectFactor, x => MegaCicleUI.effectFactor = x, 0f, 2f).SetEase(Ease.InSine);

        yield return new WaitForSeconds(1.5f);
        MegaTextGroup.transform.localScale = Vector3.one * 1.5f;
        MegaTextGroup.transform.DOScale(Vector3.one, 0.5f);
        MegaTextGroup.alpha = 0;
        MegaTextGroup.DOFade(1, 0.5f);

    }

    public void ShowArrow(bool isShow)
    {
        IsShowArrow = isShow;

        if (isShow)
        {
            ArrowGroup.alpha = 0;
            ArrowGroup.DOFade(1, 0.5f);

            foreach (var arrow in ArrowList)
            {
                arrow.Show();
            }
        }
        else
        {
            ArrowGroup.DOFade(0, 2f).onComplete = () =>
            {
                foreach(var arrow in ArrowList)
                {
                    arrow.LineTVal = 0;
                }
            };
        }
    }


    private void Update()
    {
        if (SettingManager.Instance.IsShowSetting)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TestImage.gameObject.SetActive(!TestImage.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowAirplane(!IsShowAirplane);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ShowRailroad(!IsShowRailroad);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ShowArrow(!IsShowArrow);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ShowMega(!IsShowMega);
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LineImage.gameObject.SetActive(!LineImage.gameObject.activeSelf);
        }
    }

    public void SetData()
    {
        BgImage.color = Data.BgColor;
        foreach (var image in AirplaneLineList)
        {
            image.SetColor(Data.AirplaneLineColor);
        }

        DaeguCircleImage.color = Data.DaeguCircleImageColor;
        MegaCircleImage.color = Data.MegaCircleImageColor;

        TrainManager.Instance.SetColor(Data.RailroadLineColor);

        foreach(var arrow in ArrowList)
        {
            arrow.SetColor(Data.ArrowLineColor);
        }
    }
    public void Save()
    {
        DataManager.SetData("Contents", Data);
    }
    public void Load()
    {
        Data = DataManager.GetData<ContentsData>("Contents");
        if(Data == null)
        {
            Data = new ContentsData();
        }

        SetData();
    }

}

[System.Serializable]
public class ContentsData
{
    public Color BgColor = Color.black;

    public Color AirplaneIconColor = Color.white;
    public Color AirplaneLineColor = Color.white;

    public Color RailroadIconColor = Color.white;
    public Color RailroadLineColor = Color.white;



    public Color ArrowLineColor = new Color(0.5f, 0.5f, 1f, 1);


    public Color DaeguCircleImageColor = Color.red;
    public Color MegaCircleImageColor = Color.red;
}
