using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageColorChange : MonoBehaviour {
    public Color[] ColorList;

    [ContextMenu("SetColor")]
    void SetColor()
    {
        if(ColorList.Length == 0)
        {
            Debug.LogError("컬러 없음!");
            return;
        }

        var list = GetComponentsInChildren<Image>();

        for (int i = 0; i < list.Length; i++)
        {
            list[i].color = ColorList[i % ColorList.Length];
        }

        var rawList = GetComponentsInChildren<RawImage>();

        for (int i = 0; i < rawList.Length; i++)
        {
            rawList[i].color = ColorList[i % ColorList.Length];
        }
    }
}
