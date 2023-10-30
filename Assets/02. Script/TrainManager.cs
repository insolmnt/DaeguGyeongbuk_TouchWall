using DG.Tweening;
using geniikw.DataRenderer2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class TrainManager : MonoBehaviour
{
    static public TrainManager Instance;
    public UILineObjectPath Line;

    public CanvasGroup Group;

    public Image TrainImage1;
    public Image TrainImage2;
    public Image TrainImage3;
    public AnimationCurve TrainAlpha;

    public Image[] DotImageList;
    public Image[] NameImageList;

    public UILineObjectPath TrainPath;

    public float RotationSpeed = 1f;

    [Header("Data")]
    [Range(0, 1f)]
    public float TrainTVal = 0;
    public float BeforeTrainTVal = 0;
    public float TrainDist = 0.1f;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        //DOTween.To(() => TrainTVal, x => TrainTVal = x, 1, 5f).SetEase(Ease.Linear);
        Clear();
    }

    public void SetColor(Color lineColor, Color trainColor)
    {
        Line.Line.color = lineColor;
        TrainImage1.color = trainColor;
        TrainImage2.color = trainColor;
        TrainImage3.color = trainColor;
    }


    [ContextMenu("Show")]
    public void Show()
    {
        IsPlay = true;
        StartCoroutine(ShowEvent());
    }
    public bool IsPlay = false;
    private IEnumerator ShowEvent()
    {
        Group.alpha = 1;
        Clear();

        SoundManager.Instance.Play(ContentsManager.Instance.TrainLineSound);
        DOTween.To(() => Line.LineCurrentDistVal, x => Line.LineCurrentDistVal = x, 1 , ContentsManager.Instance.Data.TrainLineTime).SetEase(Ease.InSine); //3
        yield return new WaitForSeconds(ContentsManager.Instance.Data.TrainLineTime / 6f); //0.5
        for (int i=0; i<DotImageList.Length; i++)
        {
            yield return new WaitForSeconds(ContentsManager.Instance.Data.TrainLineTime / 10f - i * 0.005f); //0.3
            ShowImage(DotImageList[i]);
            if(i < NameImageList.Length)
                ShowImage(NameImageList[i]);
        }
        yield return new WaitForSeconds(0.5f);

        SoundManager.Instance.Play(ContentsManager.Instance.TrainSound);
        DOTween.To(() => TrainTVal, x => TrainTVal = x, 1 + TrainDist * 2, ContentsManager.Instance.Data.TrainMoveTime).SetEase(Ease.Linear).onComplete = () => 
        {
            ContentsManager.Instance.EndTrain();
            IsPlay = false;
        };

    }


    private void Clear()
    {
        BeforeTrainTVal = -1;
        TrainTVal = 0;
        Line.LineCurrentDistVal = 0;
        foreach (var image in DotImageList)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }
        foreach (var image in NameImageList)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }
    }
    [ContextMenu("Hide")]
    public void Hide()
    {
        Group.DOFade(0, 2f).onComplete = () =>
        {
            Clear();
        };
    }

    private void ShowImage(Image image)
    {
        float fadeTime = 1f;

        image.transform.localScale = Vector3.one * 1.5f;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

        image.transform.DOScale(Vector3.one, fadeTime);
        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 1), fadeTime);
    }

    void Update()
    {
        if(TrainTVal != BeforeTrainTVal)
        {
            BeforeTrainTVal = TrainTVal;

            Quaternion q;

            TrainImage1.color = new Color(TrainImage1.color.r, TrainImage1.color.g, TrainImage1.color.b, TrainAlpha.Evaluate(TrainTVal));
            TrainImage1.transform.parent.localPosition = TrainPath.GetPosition(TrainTVal, out q);
            var next = (Vector3)TrainPath.GetPosition(TrainTVal + 0.01f, out q);
            var dir = TrainImage1.transform.parent.localPosition - next;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            TrainImage1.transform.parent.rotation = TrainTVal == 0 ? 
                Quaternion.AngleAxis(angle, Vector3.forward) 
                : Quaternion.Lerp(TrainImage1.transform.parent.rotation, Quaternion.AngleAxis(angle, Vector3.forward), RotationSpeed * Time.deltaTime);


            var t = (TrainTVal - TrainDist) ;
            TrainImage2.color = new Color(TrainImage2.color.r, TrainImage2.color.g, TrainImage2.color.b, TrainAlpha.Evaluate(t));
            //TrainImage2.transform.parent.localPosition = TrainPath.line.GetPosition(TrainTVal - TrainDist);
            //next = (Vector3)TrainPath.line.GetPosition(TrainTVal - TrainDist + 0.01f);
            //dir = TrainImage2.transform.parent.localPosition - next;
            //angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //TrainImage2.transform.parent.rotation = TrainTVal == 0 ? 
            //    Quaternion.AngleAxis(angle, Vector3.forward)
            //    : Quaternion.Lerp(TrainImage2.transform.parent.rotation, Quaternion.AngleAxis(angle, Vector3.forward), RotationSpeed * Time.deltaTime);


            t = (TrainTVal - TrainDist - TrainDist) ;
            TrainImage3.color = new Color(TrainImage3.color.r, TrainImage3.color.g, TrainImage3.color.b, TrainAlpha.Evaluate(t));
            //TrainImage3.transform.parent.localPosition = TrainPath.line.GetPosition(TrainTVal - TrainDist - TrainDist);
            //next = (Vector3)TrainPath.line.GetPosition(TrainTVal - TrainDist - TrainDist + 0.01f);
            //dir = TrainImage3.transform.parent.localPosition - next;
            //angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //TrainImage3.transform.parent.rotation = TrainTVal == 0 ? 
            //    Quaternion.AngleAxis(angle, Vector3.forward)
            //    : Quaternion.Lerp(TrainImage3.transform.parent.rotation, Quaternion.AngleAxis(angle, Vector3.forward), RotationSpeed * Time.deltaTime);
        }
    }
}
