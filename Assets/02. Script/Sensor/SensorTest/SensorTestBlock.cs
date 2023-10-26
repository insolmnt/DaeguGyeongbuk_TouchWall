using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class SensorTestBlock : MonoBehaviour
{
    public SensorTest Manager;
    public ProceduralImage LineImage;
    public Text StateText;
    public BoxCollider Collider;

    public Color[] ColorList;

    [Header("Data")]
    public int TouchCount = 0;
    public int TouchDownCount = 0;
    public int BeforeTouchCount = 0;
    public int CurrentTouchCount = 0;
    public int OutCount = 0;
    public void OnTouch()
    {
        if (OutCount >= Manager.OutCheckCount 
            && BeforeTouchCount == 0
            && CurrentTouchCount == 0)
        {
            OutCount = 0;
            TouchDownCount++;
            LineImage.color = ColorList[TouchDownCount % ColorList.Length];
        }


        LineImage.BorderWidth = 10;
        TouchCount++;
        CurrentTouchCount++;
        StateText.text = "" + TouchCount + "\n" + TouchDownCount;

    }

    public void SetSize()
    {
        Collider.size = new Vector3(LineImage.rectTransform.rect.width, LineImage.rectTransform.rect.height, 1);
    }


    public void OnTouchEnd()
    {
        if(CurrentTouchCount == 0)
        {
            OutCount++;
            if(OutCount >= Manager.OutCheckCount)
            {
                LineImage.BorderWidth = 4;
            }
        }
        BeforeTouchCount = CurrentTouchCount;
        CurrentTouchCount = 0;
    }


    public void Clear()
    {
        LineImage.BorderWidth = 4;
        BeforeTouchCount = 0;
        CurrentTouchCount = 0;
        OutCount = 0;

        TouchCount = 0;
        TouchDownCount = 0;
        LineImage.color = Color.white;
        StateText.text = "" + TouchCount + "\n" + TouchDownCount;
    }

    private void Update()
    {
    }
}
