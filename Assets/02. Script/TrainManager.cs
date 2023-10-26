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

    public void SetColor(Color lineColor)
    {
        Line.Line.color = lineColor;
    }


    [ContextMenu("Show")]
    public void Show()
    {
        StartCoroutine(ShowEvent());
    }
    private IEnumerator ShowEvent()
    {
        Group.alpha = 1;
        Clear();

        
        DOTween.To(() => Line.LineCurrentDistVal, x => Line.LineCurrentDistVal = x, 1 , 3f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(0.5f);
        for (int i=0; i<DotImageList.Length; i++)
        {
            yield return new WaitForSeconds(0.3f - i * 0.005f);
            ShowImage(DotImageList[i]);
            ShowImage(NameImageList[i]);
        }
        yield return new WaitForSeconds(0.5f);

        DOTween.To(() => TrainTVal, x => TrainTVal = x, 1 + TrainDist * 2, 7f).SetEase(Ease.Linear);

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

            TrainImage1.color = new Color(1, 1, 1, TrainAlpha.Evaluate(TrainTVal));
            TrainImage1.transform.parent.localPosition = TrainPath.GetPosition(TrainTVal, out q);
            var next = (Vector3)TrainPath.GetPosition(TrainTVal + 0.01f, out q);
            var dir = TrainImage1.transform.parent.localPosition - next;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            TrainImage1.transform.parent.rotation = TrainTVal == 0 ? 
                Quaternion.AngleAxis(angle, Vector3.forward) 
                : Quaternion.Lerp(TrainImage1.transform.parent.rotation, Quaternion.AngleAxis(angle, Vector3.forward), RotationSpeed * Time.deltaTime);


            var t = (TrainTVal - TrainDist) ;
            TrainImage2.color = new Color(1, 1, 1, TrainAlpha.Evaluate(t));
            //TrainImage2.transform.parent.localPosition = TrainPath.line.GetPosition(TrainTVal - TrainDist);
            //next = (Vector3)TrainPath.line.GetPosition(TrainTVal - TrainDist + 0.01f);
            //dir = TrainImage2.transform.parent.localPosition - next;
            //angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //TrainImage2.transform.parent.rotation = TrainTVal == 0 ? 
            //    Quaternion.AngleAxis(angle, Vector3.forward)
            //    : Quaternion.Lerp(TrainImage2.transform.parent.rotation, Quaternion.AngleAxis(angle, Vector3.forward), RotationSpeed * Time.deltaTime);


            t = (TrainTVal - TrainDist - TrainDist) ;
            TrainImage3.color = new Color(1, 1, 1, TrainAlpha.Evaluate(t));
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
