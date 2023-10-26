using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCtr : MonoBehaviour {
    public ParticleSystem[] Particles;


    public float HideToUnActiveTime = 5f;

    private float CurrentTime = 0;
    private float ShowTime = 0;
    private bool IsShow = false;

    private Action OnShowEnd;

    public void Show(float showTime = 0, Action onShowEnd = null)
    {
        foreach (var pa in Particles)
        {
            var main = pa.main;
            main.loop = true;
        }
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        OnShowEnd = onShowEnd;
        CurrentTime = 0;
        ShowTime = showTime;
        IsShow = true;
    }

    public void Hide()
    {
        foreach(var pa in Particles)
        {
            var main = pa.main;
            main.loop = false;
        }

        CurrentTime = 0;
        ShowTime = HideToUnActiveTime;
        IsShow = false;

        if(OnShowEnd != null)
        {
            OnShowEnd.Invoke();
        }
    }

    private void Update()
    {
        if(ShowTime == 0)
        {
            return;
        }

        CurrentTime += Time.deltaTime;

        if (CurrentTime >= ShowTime)
        {
            if (IsShow)
            {
                Hide();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
