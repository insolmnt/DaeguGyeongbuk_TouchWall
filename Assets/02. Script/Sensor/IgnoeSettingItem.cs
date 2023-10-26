using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IgnoeSettingItem : MonoBehaviour
{
    public LidarSenserSetting Setting;
    public Image RectImage;

    [Header("Data")]
    public Rect Rect;
    public Vector2 PointDownMousePosition;
    public Vector2 PointDownVal;
    public int PointDown = -1;

    public void SetRect(Rect rect)
    {
        Rect = rect;

        RectImage.rectTransform.anchoredPosition = rect.center;
        RectImage.rectTransform.sizeDelta = rect.size;
    }

    public void OnIgnoeRectChangePointDonw(int index)
    {
        PointDown = index;
        PointDownMousePosition = Input.mousePosition;

        switch (PointDown)
        {
            case 0: //이동
                PointDownVal = Rect.center;
                break;

            case 1: //우상
                PointDownVal = new Vector2(Rect.xMax, Rect.yMax);
                break;

            case 2: //우하
                PointDownVal = new Vector2(Rect.xMax, Rect.yMin);
                break;

            case 3: //좌하
                PointDownVal = new Vector2(Rect.xMin, Rect.yMin);
                break;

            case 4: //좌상
                PointDownVal = new Vector2(Rect.xMin, Rect.yMax);
                break;
        }
    }

    private void Update()
    {
        if (PointDown >= 0)
        {
            switch (PointDown)
            {
                case 0: //이동
                    Rect.center = PointDownVal + ((Vector2)Input.mousePosition - PointDownMousePosition);
                    break;

                case 1:
                    Rect.xMax = PointDownVal.x + (((Vector2)Input.mousePosition).x - PointDownMousePosition.x);
                    Rect.yMax = PointDownVal.y + (((Vector2)Input.mousePosition).y - PointDownMousePosition.y);
                    break;

                case 2:
                    Rect.xMax = PointDownVal.x + (((Vector2)Input.mousePosition).x - PointDownMousePosition.x);
                    Rect.yMin = PointDownVal.y + (((Vector2)Input.mousePosition).y - PointDownMousePosition.y);
                    break;

                case 3:
                    Rect.xMin = PointDownVal.x + (((Vector2)Input.mousePosition).x - PointDownMousePosition.x);
                    Rect.yMin = PointDownVal.y + (((Vector2)Input.mousePosition).y - PointDownMousePosition.y);
                    break;

                case 4:
                    Rect.xMin = PointDownVal.x + (((Vector2)Input.mousePosition).x - PointDownMousePosition.x);
                    Rect.yMax = PointDownVal.y + (((Vector2)Input.mousePosition).y - PointDownMousePosition.y);
                    break;
            }

            Setting.IgnoeItemToData();
            SetRect(Rect);
            if (Input.GetMouseButton(0) == false)
            {
                PointDown = -1;
            }
        }
    }


    public void OnDeleteButtonClick()
    {
        gameObject.SetActive(false);
        Setting.IgnoeItemToData();
    }
}
