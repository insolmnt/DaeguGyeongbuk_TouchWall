using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchInput : MonoBehaviour {
    public string Tag;
    public float TouchDelay = 0f;
    public UnityEvent<Vector3> OnTouchEvent;

    private float LastTouchTime = 0;

    public bool OnTouch(Vector3 v3)
    {
        if(LastTouchTime < TouchDelay)
        {
            return false;
        }

        LastTouchTime = 0;

        if(OnTouchEvent != null)
        {
            OnTouchEvent.Invoke(v3);
        }

        return true;
    }

    void Update()
    {
        LastTouchTime += Time.deltaTime;
    }
}
