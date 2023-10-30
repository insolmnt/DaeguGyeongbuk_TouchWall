using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoManager : MonoBehaviour
{
    static public PhotoManager Instance;

    [Header("대기")]
    public CanvasGroup WaitGroup;
    public Image WaitBigPhoto;
    public Image[] WaitSmallPhoto;

    [Header("비행기")]
    public CanvasGroup AirplaneGroup;

    [Header("철도")]
    public CanvasGroup TrainGroup;

    [Header("Data")]
    public PhotoData Data;


    private void Awake()
    {
        Instance = this;
    }

    public void ShowWait()
    {
        AirplaneGroup.DOKill();
        TrainGroup.DOKill();
        WaitGroup.DOKill();


        AirplaneGroup.DOFade(0, Data.FadeTime);
        TrainGroup.DOFade(0, Data.FadeTime);
        WaitGroup.DOFade(1, Data.FadeTime);
    }
    public void ShowAirplane(bool isShow)
    {
        AirplaneGroup.DOKill();
        TrainGroup.DOKill();
        WaitGroup.DOKill();

        if (isShow)
        {
            AirplaneGroup.DOFade(1, Data.FadeTime);
            TrainGroup.DOFade(0, Data.FadeTime);
            WaitGroup.DOFade(0, Data.FadeTime);
        }
        else
        {
            AirplaneGroup.DOFade(0, Data.FadeTime);
            WaitGroup.DOFade(1, Data.FadeTime);
        }
    }
    public void ShowTrain(bool isShow)
    {
        AirplaneGroup.DOKill();
        TrainGroup.DOKill();
        WaitGroup.DOKill();

        if (isShow)
        {
            TrainGroup.DOFade(1, Data.FadeTime);
            AirplaneGroup.DOFade(0, Data.FadeTime);
            WaitGroup.DOFade(0, Data.FadeTime);
        }
        else
        {
            TrainGroup.DOFade(0, Data.FadeTime);
            WaitGroup.DOFade(1, Data.FadeTime);
        }
    }


    private void Start()
    {
        AirplaneGroup.alpha = 0;
        TrainGroup.alpha = 0;


        Load();
        StartCoroutine(ShowWaitPhoto());
    }

    IEnumerator ShowWaitPhoto()
    {
        WaitBigPhoto.gameObject.SetActive(false);
        foreach(var image in WaitSmallPhoto)
        {
            image.gameObject.SetActive(false);
        }

        while (true)
        {
            ShowPhoto(WaitBigPhoto);
            yield return new WaitForSeconds(Data.FadeTime);
            yield return new WaitForSeconds(Data.WaitBigPhotoShowTime);

            HidePhoto(WaitBigPhoto);

            for (int i=0; i< WaitSmallPhoto.Length; i += 2)
            {
                ShowPhoto(WaitSmallPhoto[i]);
                ShowPhoto(WaitSmallPhoto[i+1]);

                yield return new WaitForSeconds(Data.FadeTime);
                yield return new WaitForSeconds(Data.WaitSmallPhotoShowTime);

                HidePhoto(WaitSmallPhoto[i]);
                HidePhoto(WaitSmallPhoto[i + 1]);
            }
        }
    }
    private void ShowPhoto(Image image)
    {
        image.rectTransform.anchoredPosition = new Vector2(700, image.rectTransform.anchoredPosition.y);
        image.color = new Color(1, 1, 1, 0);
        image.gameObject.SetActive(true);
        image.DOColor(Color.white, Data.FadeTime);
        image.rectTransform.DOLocalMoveX(0, Data.FadeTime);
    }
    private void HidePhoto(Image image)
    {
        image.DOColor(new Color(1, 1, 1, 0), Data.FadeTime);
        image.rectTransform.DOLocalMoveX(-700, Data.FadeTime).onComplete = () =>
        {
            image.gameObject.SetActive(false);
        };
    }


    public void Save()
    {

    }
    public void Load()
    {
        Data = DataManager.GetData<PhotoData>("Photo");
        if(Data == null)
        {
            Data = new PhotoData();
        }
    }

}


[System.Serializable]
public class PhotoData
{
    public float WaitBigPhotoShowTime = 5f;
    public float WaitSmallPhotoShowTime = 2f;
    public float FadeTime = 1f;
}