using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

[ExecuteInEditMode]
public class UILineObjectPath : MonoBehaviour
{
    public UILineRenderer Line;
    //public bool IsReflash = false;

    public RectTransform[] ObjectList;

    public bool IsRight = false;

    [Header("Data")]
    public List<float> LineDistList = new List<float>();
    public float LineTotalDist = 0;
    [Range(0, 1)]
    public float LineCurrentDistVal = 0;
    public float LineBeforeDistVal;

    private void Update()
    {
        if (Line == null)
        {
            return;
        }

        DrawLine(LineCurrentDistVal * LineTotalDist);
    }

    private void DrawLine(float len)
    {
        if (len == LineBeforeDistVal)
        {
            return;
        }
        LineBeforeDistVal = len;

        float remainingDist = len;
        List<Vector2> points = new List<Vector2>();

        points.Add(ObjectList[0].anchoredPosition);
        for (int i = 0; i < LineDistList.Count; i++)
        {
            if (remainingDist >= LineDistList[i])
            {
                points.Add(ObjectList[i + 1].anchoredPosition);
                remainingDist -= LineDistList[i];
            }
            else
            {
                points.Add(Vector2.Lerp(ObjectList[i].anchoredPosition, ObjectList[i + 1].anchoredPosition, remainingDist / LineDistList[i]));
                break;
            }
        }

        Line.Points = points.ToArray();
    }



    [ContextMenu("Make")]
    private void Make()
    {
        //var list = transform.GetComponentsInChildren<RectTransform>();

        //ObjectList = new RectTransform[list.Length - 1];
        //for (int i = 0; i < ObjectList.Length; i++)
        //{
        //    ObjectList[i] = list[i + 1];
        //}

        //Line.Points = new Vector2[ObjectList.Length];
        //for (int i = 0; i < Line.Points.Length; i++)
        //{
        //    Line.Points[i] = ObjectList[i].anchoredPosition;
        //}

        //Line.SetAllDirty();


        LineTotalDist = 0;
        LineDistList.Clear();
        for (int i = 0; i < ObjectList.Length - 1; i++)
        {
            var dist = Vector2.Distance(ObjectList[i].anchoredPosition, ObjectList[i + 1].anchoredPosition);
            LineDistList.Add(dist);
            LineTotalDist += dist;
        }

    }


    public Vector2 GetPosition(float t, out Quaternion rotation)
    {
        float dist = t * LineTotalDist;
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < LineDistList.Count; i++)
        {
            if (dist >= LineDistList[i])
            {
                dist -= LineDistList[i];
            }
            else
            {
                var position = Vector2.Lerp(ObjectList[i].anchoredPosition, ObjectList[i + 1].anchoredPosition, dist / LineDistList[i]);
                var dir = ObjectList[i + 1].anchoredPosition - position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                rotation = Quaternion.AngleAxis(angle, Vector3.back);
                return position;
            }
        }


        rotation = Quaternion.identity;
        return ObjectList[ObjectList.Length - 1].anchoredPosition;
    }
}
