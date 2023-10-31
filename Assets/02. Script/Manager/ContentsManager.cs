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

    public Image[] TouchLineImage;

    [Header("공항")]
    public AirplaneLine[] AirplaneLineList;


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

    public Image BigTextUp;
    public Image BigTextDown;
    public Image BigTextBox;


    [Header("사운드")]
    public AudioClip TrainSound;
    public AudioClip MegaCircleSound;
    public AudioClip AirplaneSound;
    public AudioClip AirplaneArrowSound;
    public AudioClip AreaLeftSound;
    public AudioClip AreaRightSound;
    public AudioClip DaeguCircleSound;
    public AudioClip TrainLineSound;
    public AudioClip TouchSound;
    public AudioClip BlueArrowSound;

    [Header("터치 아이콘")]
    public Image DaeguIcon;
    public Image AirportIcon;
    public Image RailroadIcon;
    public Image DaeguHandImage;
    public Image AirHandImage;
    public Image TrainHandImage;

    [Header("")]
    public CanvasGroup TouchImageGroup;
    public Image TouchImageAirplane;
    public Image TouchImageTrain;
    public Image TouchImageMega;

    [Header("Data")]
    public ContentsData Data;
    public bool IsShowAirplane = false;
    public bool IsShowRailroad = false;
    public bool IsShowMega = false;
    public bool IsShowArrow = false;
    public PlayState TrainState = PlayState.대기;
    public PlayState AirplaneState = PlayState.대기;
    public PlayState MegaState = PlayState.대기;
    public float MegaEndCurrentTime = 0;

    public bool IsWait = true;
    public bool IsPlay = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TestImage.gameObject.SetActive(false);
        foreach(var image in TouchLineImage)
        {
            image.enabled = false;
        }
        Load();

        ClearMega();

        StartCoroutine(TouchImageLoopAnimation());
    }

    IEnumerator TouchImageLoopAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            TouchImageAirplane.transform.DOScale(Vector3.one * 1.3f, 0.5f);
            TouchImageTrain.transform.DOScale(Vector3.one * 1.3f, 0.5f);
            TouchImageMega.transform.DOScale(Vector3.one * 1.3f, 0.5f);

            yield return new WaitForSeconds(0.5f);
            TouchImageAirplane.transform.DOScale(Vector3.one, 0.5f);
            TouchImageTrain.transform.DOScale(Vector3.one, 0.5f);
            TouchImageMega.transform.DOScale(Vector3.one, 0.5f);

            yield return new WaitForSeconds(0.5f);
            TouchImageAirplane.transform.DOScale(Vector3.one * 1.3f, 0.5f);
            TouchImageTrain.transform.DOScale(Vector3.one * 1.3f, 0.5f);
            TouchImageMega.transform.DOScale(Vector3.one * 1.3f, 0.5f);

            yield return new WaitForSeconds(0.5f);
            TouchImageAirplane.transform.DOScale(Vector3.one, 0.5f);
            TouchImageTrain.transform.DOScale(Vector3.one, 0.5f);
            TouchImageMega.transform.DOScale(Vector3.one, 0.5f);
        }
    }
    private void ShowTouchImage(bool isShow)
    {
        TouchImageGroup.DOKill();
        if (isShow)
        {
            TouchImageGroup.DOFade(1, 2f);

            if (Data.IsShowHandImage)
            {
                DaeguHandImage.DOColor(new Color(1, 1, 1, 1), 2f);
                AirHandImage.DOColor(new Color(1, 1, 1, 1), 2f);
                TrainHandImage.DOColor(new Color(1, 1, 1, 1), 2f);
            }
        }
        else
        {
            TouchImageGroup.alpha = 0;
        }
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


    public void ShowAirplane(bool isShow, bool isAutoHide)
    {
        Debug.Log("[ShowAirplane] " + isShow + " / " + isAutoHide);
        IsShowAirplane = isShow;
        PhotoManager.Instance.ShowAirplane(isShow);
        if (isShow)
        {
            //if(AirplaneState == PlayState.재생중)
            //{
            //    return;
            //}

            if (ShowCoroutine != null)
            {
                StopCoroutine(ShowCoroutine);
            }
            ShowCoroutine = StartCoroutine(ShowAirplaneEvent(isAutoHide));
        }
        else
        {
            AirplaneState = PlayState.대기;
            foreach (var image in AirplaneLineList)
            {
                image.Hide();
            }
        }
    }
    private IEnumerator ShowAirplaneEvent(bool isAutoHide)
    {
        AirplaneState = PlayState.재생중;

        AirportIcon.DOColor(new Color(1, 1, 1, 0), 0.15f);
        yield return new WaitForSeconds(0.2f);
        AirportIcon.DOColor(new Color(1, 1, 1, 1), 0.15f);
        yield return new WaitForSeconds(0.2f);
        AirportIcon.DOColor(new Color(1, 1, 1, 0), 0.15f);
        yield return new WaitForSeconds(0.2f);
        AirportIcon.DOColor(new Color(1, 1, 1, 1), 0.15f);
        yield return new WaitForSeconds(0.2f);


        foreach (var image in AirplaneLineList)
        {
            image.Show();
        }

        SoundManager.Instance.Play(AirplaneArrowSound);
        yield return new WaitForSeconds(AirplaneLineList[0].ArrowShowTime);
        SoundManager.Instance.Play(AirplaneSound);

        while (AirplaneLineList[0].IsShow)
        {
            yield return null;
        }

        EndAirplane();

        if (isAutoHide)
        {
            IsPlay = false;
            ShowTouchImage(true);

            yield return new WaitForSeconds(Data.TouchEndWaitTime);
            ShowAirplane(false, false);

            yield return new WaitForSeconds(2f);
        }
    }

    public void HideAll()
    {
        ShowAirplane(false, false);
        ShowArrow(false);
        ShowTrain(false, false);
        ShowMega(false);
    }
    public void ShowTrain(bool isShow, bool isAutoHide)
    {
        Debug.Log("[ShowTrain] " + isShow + " / " + isAutoHide);
        IsShowRailroad = isShow;
        PhotoManager.Instance.ShowTrain(isShow);
        if (isShow)
        {
            //if (TrainState == PlayState.재생중)
            //{
            //    return;
            //}
            if (ShowCoroutine != null)
            {
                StopCoroutine(ShowCoroutine);
            }
            ShowCoroutine = StartCoroutine(ShowRailroadEvent(isAutoHide));
        }
        else
        {
            TrainState = PlayState.대기;
            TrainManager.Instance.Hide();
        }
    }
    private IEnumerator ShowRailroadEvent(bool isAutoHide)
    {
        TrainState = PlayState.재생중;
        RailroadIcon.DOColor(new Color(1, 1, 1, 0), 0.15f);
        yield return new WaitForSeconds(0.2f);
        RailroadIcon.DOColor(new Color(1, 1, 1, 1), 0.15f);
        yield return new WaitForSeconds(0.2f);
        RailroadIcon.DOColor(new Color(1, 1, 1, 0), 0.15f);
        yield return new WaitForSeconds(0.2f);
        RailroadIcon.DOColor(new Color(1, 1, 1, 1), 0.15f);
        yield return new WaitForSeconds(0.2f);

        TrainManager.Instance.Show();


        while (TrainManager.Instance.IsPlay)
        {
            yield return null;
        }
        TrainState = PlayState.재생완료;

        if (isAutoHide)
        {
            IsPlay = false;
            ShowTouchImage(true);

            yield return new WaitForSeconds(Data.TouchEndWaitTime);
            ShowTrain(false, false);
            yield return new WaitForSeconds(2f);
        }
    }


    Coroutine ShowCoroutine = null;
    public void ShowMega(bool isShow)
    {
        Debug.Log("[ShowMega] " + isShow);
        IsShowMega = isShow;

        if (isShow)
        {
            //if(MegaState == PlayState.재생중)
            //{
            //    return;
            //}
            if(ShowCoroutine != null)
            {
                StopCoroutine(ShowCoroutine);
            }
            ShowCoroutine = StartCoroutine(ShowMegaEvent());
        }
        else
        {
            MegaState = PlayState.대기;

            ShowArrow(false);

            DaeguCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 0), 2f);
            MegaCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 0), 2f);

            MegaTextGroup.DOFade(0, 2f);

            AreaLeft.Hide();
            AreaRight.Hide();
        }
    }

    IEnumerator ShowMegaEvent()
    {
        MegaState = PlayState.재생중;
        //yield return new WaitForSeconds(1f);

        SoundManager.Instance.Play(BlueArrowSound);
        ShowArrow(true);
        yield return new WaitForSeconds(Data.BlueArrowShowTime + 0.5f);

        //SoundManager.Instance.Play(AreaLeftSound);
        //AreaLeft.Show();
        //yield return new WaitForSeconds(1f);
        //SoundManager.Instance.Play(AreaRightSound);
        //AreaRight.Show();
        //yield return new WaitForSeconds(3f);

        SoundManager.Instance.Play(DaeguCircleSound);
        DaeguCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 1), 2f);
        DaeguCicleUI.effectFactor = 1;
        DOTween.To(() => DaeguCicleUI.effectFactor, x => DaeguCicleUI.effectFactor = x, 0f, 2f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(1.5f);

        SoundManager.Instance.Play(MegaCircleSound);
        MegaCircleImage.DOColor(new Color(DaeguCircleImage.color.r, DaeguCircleImage.color.g, DaeguCircleImage.color.b, 1), 0.5f);
        MegaCicleUI.effectFactor = 1;
        DOTween.To(() => MegaCicleUI.effectFactor, x => MegaCicleUI.effectFactor = x, 0f, 2f).SetEase(Ease.InSine);

        yield return new WaitForSeconds(1.5f);
        MegaTextGroup.transform.localScale = Vector3.one * 1.5f;
        MegaTextGroup.transform.DOScale(Vector3.one, 0.5f);
        MegaTextGroup.alpha = 0;
        MegaTextGroup.DOFade(1, 0.5f);




        
        yield return new WaitForSeconds(1.5f);
        //메가 끝
        MegaState = PlayState.재생완료;
        MegaEndCurrentTime = 0;

        PhotoManager.Instance.ShowWait();
        IsPlay = false;
        ShowTouchImage(true);


        yield return new WaitForSeconds(Data.MegaEndWaitTime);
        HideAll();
        yield return new WaitForSeconds(2.1f);
    }

    public void ShowArrow(bool isShow)
    {
        Debug.Log("[ShowArrow] " + isShow);

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

    public void EndTrain()
    {
        TrainState = PlayState.재생완료;

        //if (AirplaneState == PlayState.재생완료)
        //{
        //    ShowMega(true);
        //}
    }
    public void EndAirplane()
    {
        AirplaneState = PlayState.재생완료;

        //if(TrainState == PlayState.재생완료)
        //{
        //    ShowMega(true);
        //}
    }


    Coroutine EventCoroutine = null;
    public void OnTouchTrain()
    {
        if (IsPlay)
        {
            return;
        }

        ShowTouchImage(false);
        IsPlay = true;
        //switch (TrainState)
        //{
        //    case PlayState.재생중:
        //        return;
        //}


        if(EventCoroutine != null)
        {
            StopCoroutine(EventCoroutine);
        }

        EventCoroutine = StartCoroutine(OnTouchTrainEvent());
    }

    IEnumerator OnTouchTrainEvent()
    {
        SoundManager.Instance.Play(TouchSound);
        if (Data.IsShowHandImage)
        {
            DaeguHandImage.DOColor(new Color(1, 1, 1, 0), 0.5f);
            AirHandImage.DOColor(new Color(1, 1, 1, 0), 0.5f);

            TrainHandImage.transform.DOLocalRotate(new Vector3(30, 0, 0), 0.3f);
            RailroadIcon.transform.DOScale(Vector3.one * Data.TouchImageScale, 0.2f);
            yield return new WaitForSeconds(0.2f);
            RailroadIcon.transform.DOScale(Vector3.one * 1f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            TrainHandImage.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
            yield return new WaitForSeconds(0.4f);
            TrainHandImage.DOColor(new Color(1, 1, 1, 0), 0.2f);
        }
        else
        {
            RailroadIcon.transform.DOScale(Vector3.one * Data.TouchImageScale, 0.2f);
            yield return new WaitForSeconds(0.2f);
            RailroadIcon.transform.DOScale(Vector3.one * 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }


        if (MegaState == PlayState.재생완료)
        {
            HideAll();
            yield return new WaitForSeconds(2.1f);
        }
        if (TrainState == PlayState.재생완료)
        {
            ShowTrain(false, false);
            yield return new WaitForSeconds(2.1f);
        }
        if (AirplaneState == PlayState.재생완료)
        {
            ShowAirplane(false, false);
            yield return new WaitForSeconds(2.1f);
        }


        ShowTrain(true, true);
        //switch (TrainState)
        //{
        //    case PlayState.재생중:
        //        yield break;

        //    case PlayState.대기:
        //        SoundManager.Instance.Play(TouchSound);
        //        ShowTrain(true);
        //        break;
        //    case PlayState.재생완료:
        //        SoundManager.Instance.Play(TouchSound);

        //        switch (MegaState)
        //        {
        //            case PlayState.대기:
        //                //Airplane Hide 후 자시 재생
        //                ShowTrain(false);
        //                yield return new WaitForSeconds(2.5f);
        //                ShowTrain(true);
        //                break;
        //            case PlayState.재생중:
        //                yield break;

        //            case PlayState.재생완료:
        //                //전체 Hide 후 Train재생
        //                HideAll();
        //                yield return new WaitForSeconds(2.5f);
        //                ShowTrain(true);
        //                break;
        //            default:
        //                break;
        //        }
        //        break;
        //}
    }


    public void OnTouchAirport()
    {
        if (IsPlay)
        {
            return;
        }
        IsPlay = true;
        ShowTouchImage(false);
        //switch (AirplaneState)
        //{
        //    case PlayState.재생중:
        //        return;
        //}

        if (EventCoroutine != null)
        {
            StopCoroutine(EventCoroutine);
        }

        EventCoroutine = StartCoroutine(OnTouchAirportEvent());
    }
    IEnumerator OnTouchAirportEvent()
    {
        SoundManager.Instance.Play(TouchSound);
        if (Data.IsShowHandImage)
        {
            DaeguHandImage.DOColor(new Color(1, 1, 1, 0), 0.5f);
            TrainHandImage.DOColor(new Color(1, 1, 1, 0), 0.5f);

            AirHandImage.transform.DOLocalRotate(new Vector3(30, 0, 0), 0.3f);
            AirportIcon.transform.DOScale(Vector3.one * Data.TouchImageScale, 0.2f);
            yield return new WaitForSeconds(0.2f);
            AirportIcon.transform.DOScale(Vector3.one * 1f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            AirHandImage.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
            yield return new WaitForSeconds(0.4f);
            AirHandImage.DOColor(new Color(1, 1, 1, 0), 0.2f);
        }
        else
        {
            AirportIcon.transform.DOScale(Vector3.one * Data.TouchImageScale, 0.2f);
            yield return new WaitForSeconds(0.2f);
            AirportIcon.transform.DOScale(Vector3.one * 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        if (MegaState == PlayState.재생완료)
        {
            HideAll();
            yield return new WaitForSeconds(2.1f);
        }
        if (TrainState == PlayState.재생완료)
        {
            ShowTrain(false, false);
            yield return new WaitForSeconds(2.1f);
        }
        if (AirplaneState == PlayState.재생완료)
        {
            ShowAirplane(false, false);
            yield return new WaitForSeconds(2.1f);
        }

        ShowAirplane(true, true);

        //switch (AirplaneState)
        //{
        //    case PlayState.재생중:
        //        yield break;

        //    case PlayState.대기:
        //        SoundManager.Instance.Play(TouchSound);
        //        ShowAirplane(true);
        //        break;
        //    case PlayState.재생완료:
        //        SoundManager.Instance.Play(TouchSound);

        //        switch (MegaState)
        //        {
        //            case PlayState.대기:
        //                //Airplane Hide 후 자시 재생
        //                ShowAirplane(false);
        //                yield return new WaitForSeconds(2.5f);
        //                ShowAirplane(true);
        //                break;
        //            case PlayState.재생중:
        //                yield break;

        //            case PlayState.재생완료:
        //                //전체 Hide 후 Train재생
        //                HideAll();
        //                yield return new WaitForSeconds(2.5f);
        //                ShowAirplane(true);
        //                break;
        //            default:
        //                break;
        //        }
        //        break;
        //}
    }
    public void OnTouchMega()
    {
        if (IsPlay)
        {
            return;
        }
        IsPlay = true;
        ShowTouchImage(false);
        if (MegaState == PlayState.재생중)
        {
            return;
        }

        SoundManager.Instance.Play(TouchSound);
        if (EventCoroutine != null)
        {
            StopCoroutine(EventCoroutine);
        }

        EventCoroutine = StartCoroutine(OnTouchMegaEvent());
    }
    IEnumerator OnTouchMegaEvent()
    {
        if (Data.IsShowHandImage)
        {
            AirHandImage.DOColor(new Color(1, 1, 1, 0), 0.5f);
            TrainHandImage.DOColor(new Color(1, 1, 1, 0), 0.5f);

            DaeguHandImage.transform.DOLocalRotate(new Vector3(30, 0, 0), 0.3f);
            DaeguIcon.transform.DOScale(Vector3.one * Data.TouchImageScale, 0.2f);
            yield return new WaitForSeconds(0.2f);
            DaeguIcon.transform.DOScale(Vector3.one * 1f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            DaeguHandImage.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
            yield return new WaitForSeconds(0.4f);
            DaeguHandImage.DOColor(new Color(1, 1, 1, 0), 0.2f);
        }
        else
        {
            DaeguIcon.transform.DOScale(Vector3.one * Data.TouchImageScale, 0.2f);
            yield return new WaitForSeconds(0.2f);
            DaeguIcon.transform.DOScale(Vector3.one * 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);

        }

        if (MegaState == PlayState.재생완료)
        {
            HideAll();
            yield return new WaitForSeconds(2.1f);
        }
        if (TrainState == PlayState.재생완료)
        {
            ShowTrain(false, false);
            yield return new WaitForSeconds(2.1f);
        }
        if (AirplaneState == PlayState.재생완료)
        {
            ShowAirplane(false, false);
            yield return new WaitForSeconds(2.1f);
        }

        MegaState = PlayState.재생중;


        ShowAirplane(true, false);
        while (AirplaneState != PlayState.재생완료)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        ShowTrain(true, false);
        while (TrainState != PlayState.재생완료)
        {
            yield return null;
        }
        ShowMega(true);

        //switch (MegaState)
        //{
        //    case PlayState.대기:
        //        SoundManager.Instance.Play(TouchSound);
        //        if (TrainState == PlayState.대기 && AirplaneState == PlayState.대기)
        //        {
        //            //비행기, 기차, 메가 순서
        //            ShowAirplane(true);
        //            while (AirplaneState != PlayState.재생완료)
        //            {
        //                yield return null;
        //            }
        //            yield return new WaitForSeconds(1f);
        //            ShowTrain(true);
        //        }
        //        else if (TrainState == PlayState.대기)
        //        {
        //            //기차, 메가 순서
        //            while (AirplaneState != PlayState.재생완료)
        //            {
        //                yield return null;
        //            }
        //            yield return new WaitForSeconds(1f);
        //            ShowTrain(true);
        //        }
        //        else if (AirplaneState == PlayState.대기)
        //        {
        //            //비행기, 메가 순서 while (AirplaneState != PlayState.재생완료)
        //            while (TrainState != PlayState.재생완료)
        //            {
        //                yield return null;
        //            }
        //            yield return new WaitForSeconds(1f);
        //            ShowAirplane(true);
        //        }
        //        break;

        //    case PlayState.재생중:
        //        yield break;

        //    case PlayState.재생완료:
        //        SoundManager.Instance.Play(TouchSound);
        //        //비행기, 기차, 메가 순서
        //        HideAll();
        //        yield return new WaitForSeconds(1f);

        //        ShowAirplane(true);
        //        while (AirplaneState != PlayState.재생완료)
        //        {
        //            yield return null;
        //        }
        //        ShowTrain(true);
        //        break;
        //}
    }


    private void Update()
    {
        //if (MegaState == PlayState.재생완료)
        //{
        //    MegaEndCurrentTime += Time.deltaTime;

        //    if (MegaEndCurrentTime >= Data.MegaEndWaitTime)
        //    {
        //        HideAll();
        //    }
        //}

        if (IsWait)
        {
            if(AirplaneState == PlayState.재생중 || TrainState == PlayState.재생중 || MegaState == PlayState.재생중)
            {
                IsWait = false;
                SoundManager.Instance.BgmAudioSource.DOKill();
                SoundManager.Instance.BgmAudioSource.DOFade(SoundManager.Instance.Data.BgPlayVolume, 3f);

            }
        }
        else
        {
            if (AirplaneState != PlayState.재생중 && TrainState != PlayState.재생중 && MegaState != PlayState.재생중)
            {
                IsWait = true;
                SoundManager.Instance.BgmAudioSource.DOKill();
                SoundManager.Instance.BgmAudioSource.DOFade(SoundManager.Instance.Data.BgWaitVolume, 3f);


            }
        }
        if (SettingManager.Instance.IsShowSetting)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.CapsLock))
        {

            foreach (var image in TouchLineImage)
            {
                image.enabled = !image.enabled;
            }
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            Time.timeScale = 5f;
        }
        else if(Input.GetKey(KeyCode.DownArrow))
        {
            Time.timeScale = 20f;
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        {
            Time.timeScale = 0.1f;
        }
        else if(Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TestImage.gameObject.SetActive(!TestImage.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            OnTouchAirport();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            OnTouchTrain();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            OnTouchMega();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            ShowAirplane(!IsShowAirplane, true);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ShowTrain(!IsShowRailroad, true);
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            ShowArrow(!IsShowArrow);
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ShowMega(!IsShowMega);
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LineImage.gameObject.SetActive(!LineImage.gameObject.activeSelf);
        }



        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            LineImage.gameObject.SetActive(!LineImage.gameObject.activeSelf);
        }
    }

    public void SetData()
    {
        BgImage.color = Data.BgColor;
        foreach (var image in AirplaneLineList)
        {
            image.SetColor(Data.AirplaneLineColor, Data.AirplaneColor);
        }

        DaeguCircleImage.color = Data.DaeguCircleImageColor;
        MegaCircleImage.color = Data.MegaCircleImageColor;

        TrainManager.Instance.SetColor(Data.RailroadLineColor, Data.TrainColor);

        foreach(var arrow in ArrowList)
        {
            arrow.SetColor(Data.ArrowLineColor);
        }

        foreach(var image in TouchLineImage)
        {
            image.transform.localScale = Vector3.one * Data.TouchAreaScale;
        }

        AirHandImage.gameObject.SetActive(Data.IsShowHandImage);
        TrainHandImage.gameObject.SetActive(Data.IsShowHandImage);
        DaeguHandImage.gameObject.SetActive(Data.IsShowHandImage);

        BigTextBox.color = Data.BigTextBoxColor;
        BigTextUp.color = Data.BigTextUpColor;
        BigTextDown.color = Data.BigTextDownColor;


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

        foreach(var air in AirplaneLineList)
        {
            air.ArrowShowTime = Data.AirplaneLineTime;
            air.ShowTime = Data.AirplaneMoveTime;
        }

        foreach(var arr in ArrowList)
        {
            arr.ShowTime = Data.BlueArrowShowTime;
        }

        SetData();
    }

}

[System.Serializable]
public class ContentsData
{
    public Color BgColor = Color.black;

    public Color AirplaneIconColor = Color.white;
    public Color AirplaneColor = Color.white;
    public Color AirplaneLineColor = Color.white;

    public Color TrainColor = Color.white;
    public Color RailroadIconColor = Color.white;
    public Color RailroadLineColor = Color.white;



    public Color ArrowLineColor = new Color(0.5f, 0.5f, 1f, 1);


    public Color DaeguCircleImageColor = Color.red;
    public Color MegaCircleImageColor = Color.red;

    public Color BigTextDownColor = Color.black;
    public Color BigTextUpColor = Color.red;
    public Color BigTextBoxColor = Color.white;

    public float MegaEndWaitTime = 20f;
    public float TouchEndWaitTime = 5f;

    public float TrainLineTime = 1f;
    public float TrainMoveTime = 5f;

    public float AirplaneLineTime = 2f;
    public float AirplaneMoveTime = 4f;

    public float TouchAreaScale = 1.5f;

    public float BlueArrowShowTime = 2.5f;

    public float TouchImageScale = 0.8f;

    public bool IsShowHandImage = true;
}

public enum PlayState
{
    대기, 재생중, 재생완료
}