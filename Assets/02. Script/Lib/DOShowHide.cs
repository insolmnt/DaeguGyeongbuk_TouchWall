using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOShowHide : MonoBehaviour
{
    [Header("Show 관련"), Space(20)]
    public AnimationType ShowType = AnimationType.스케일변경;
    public Animator ShowActionAnimator;
    public string ShowAnimationTrigger;

    public float ShowAnimationTime = 0.5f;
    [Space(10)]
    public Vector3 ShowStartPosition;
    public Vector3 ShowStartScale = new Vector3(1, 0, 1);
    public Vector3 ShowStartRotation;
    [Space(10)]
    public Vector3 ShowTargetPosition;
    public Vector3 ShowTargetScale = new Vector3(1, 1, 1);
    public Vector3 ShowTargetRotation;


    [Header("Hide 관련"), Space(20)]
    public AnimationType HideType = AnimationType.스케일변경;
    public Animator HideActionAnimator;
    public string HideAnimationTrigger;

    public float HideAnimationTime = 0.5f;
    //[Space(10)]
    //public Vector3 HideStartPosition;
    //public Vector3 HideStartScale;
    //public Vector3 HideStartRotation;
    [Space(10)]
    public Vector3 HideTargetPosition;
    public Vector3 HideTargetScale = new Vector3(1, 0, 1);
    public Vector3 HideTargetRotation;


    [Space(20)]
    public bool IsShow = false;


    public void Show(Action onComplete = null)
    {
        if (IsShow)
        {
            return;
        }
        IsShow = true;

        switch (ShowType)
        {
            case AnimationType.사용안함:
                return;

            case AnimationType.단순OnOff:
                gameObject.SetActive(false);
                gameObject.SetActive(true);
                return;

            case AnimationType.스케일변경:
                gameObject.SetActive(false);
                gameObject.SetActive(true);
                transform.DOKill();

                transform.localScale = ShowStartScale;

                if (onComplete != null)
                {
                    transform.DOScale(ShowTargetScale, ShowAnimationTime).onComplete += () =>
                    {
                        onComplete();
                    };
                }
                else
                {
                    transform.DOScale(ShowTargetScale, ShowAnimationTime);
                }

                break;

            case AnimationType.여러가지변경:
                gameObject.SetActive(false);
                gameObject.SetActive(true);
                transform.DOKill();

                transform.localEulerAngles = ShowStartRotation;
                transform.localPosition = ShowStartPosition;
                transform.localScale = ShowStartScale;

                transform.DOLocalMove(ShowTargetPosition, ShowAnimationTime);
                transform.DOLocalRotate(ShowTargetRotation, ShowAnimationTime);

                if (onComplete != null)
                {
                    transform.DOScale(ShowTargetScale, ShowAnimationTime).onComplete += () =>
                    {
                        onComplete();
                    };
                }
                else
                {
                    transform.DOScale(ShowTargetScale, ShowAnimationTime);
                }
                break;

            case AnimationType.애니메이션재생:
                ShowActionAnimator.SetTrigger(ShowAnimationTrigger);
                return;

            default:
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns>AnimationTime</returns>
    public float Hide(Action onComplete = null)
    {
        if (IsShow == false)
        {
            return HideAnimationTime;
        }
        IsShow = false;

        switch (HideType)
        {
            case AnimationType.사용안함:
                return HideAnimationTime;

            case AnimationType.단순OnOff:
                gameObject.SetActive(false);
                return 0;

            case AnimationType.스케일변경:
                transform.DOKill();
                transform.DOScale(HideTargetScale, HideAnimationTime).onComplete += () =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                    transform.gameObject.SetActive(false);
                };
                break;

            case AnimationType.여러가지변경:
                transform.DOKill();

                transform.DOLocalMove(HideTargetPosition, HideAnimationTime);
                transform.DOLocalRotate(HideTargetRotation, HideAnimationTime);
                transform.DOScale(HideTargetScale, HideAnimationTime).onComplete += () =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                    transform.gameObject.SetActive(false);
                };
                break;

            case AnimationType.애니메이션재생:
                HideActionAnimator.SetTrigger(HideAnimationTrigger);
                break;

            default:
                break;
        }

        return HideAnimationTime;
    }


    public void ShowStartObjectToData()
    {
        ShowStartPosition = transform.localPosition;
        ShowStartRotation = transform.localEulerAngles;
        ShowStartScale = transform.localScale;
    }
    public void ShowStartDataToObject()
    {
        transform.localPosition = ShowStartPosition;
        transform.localEulerAngles = ShowStartRotation;
        transform.localScale = ShowStartScale;
    }
    public void ShowTargetObjectToData()
    {
        ShowTargetPosition = transform.localPosition;
        ShowTargetRotation = transform.localEulerAngles;
        ShowTargetScale = transform.localScale;
    }
    public void ShowTargetDataToObject()
    {
        transform.localPosition = ShowTargetPosition;
        transform.localEulerAngles = ShowTargetRotation;
        transform.localScale = ShowTargetScale;
    }

    public void HideTargetObjectToData()
    {
        HideTargetPosition = transform.localPosition;
        HideTargetRotation = transform.localEulerAngles;
        HideTargetScale = transform.localScale;
    }
    public void HideTargetDataToObject()
    {
        transform.localPosition = HideTargetPosition;
        transform.localEulerAngles = HideTargetRotation;
        transform.localScale = HideTargetScale;
    }
}



public enum AnimationType
{
    사용안함, 단순OnOff, 스케일변경, 여러가지변경, 애니메이션재생
}