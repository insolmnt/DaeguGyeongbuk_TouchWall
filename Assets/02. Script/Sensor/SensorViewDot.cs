using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorViewDot : MonoBehaviour
{
    public Image DotImage;

    public Color FistFrameColor;
    public Color DefaultColor;

    public float ShowTime = 1f;

    private void OnEnable()
    {
        DotImage.color = FistFrameColor;
        transform.localScale = Vector3.one;

        transform.DOScale(Vector3.zero, ShowTime).onComplete = () =>
        {
            gameObject.SetActive(false);
        };

        StartCoroutine(NetFrame());
    }

    IEnumerator NetFrame()
    {
        yield return null;
        DotImage.color = DefaultColor;
    }
}
