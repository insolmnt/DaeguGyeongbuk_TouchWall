using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTest : MonoBehaviour
{
    public RectTransform[] ImageList;
    public float Speed = 50;

    [Header("Data")]
    public bool IsMove = true;
    public bool IsUp = true;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IsMove = !IsMove;
        }

        if(IsMove == false)
        {
            return;
        }
        if (IsUp)
        {
            foreach(var image in ImageList)
            {
                image.anchoredPosition += Time.deltaTime * Speed * Vector2.up;
            }
            if(ImageList[0].anchoredPosition.y > 400)
            {
                IsUp = false;
            }
        }
        else
        {
            foreach (var image in ImageList)
            {
                image.anchoredPosition += Time.deltaTime * Speed * Vector2.down;
            }
            if (ImageList[0].anchoredPosition.y < -400)
            {
                IsUp = true;
            }
        }
    }
}
