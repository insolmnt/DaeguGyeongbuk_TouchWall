using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PolygonColliderToUILine : MonoBehaviour
{
    public PolygonCollider2D Collider;
    public UILineRenderer Line;

    private void OnEnable()
    {
        //ShowLine();
    }

    [ContextMenu("라인")]
    public void ShowLine()
    {
        Line.Points = new Vector2[Collider.points.Length + 1];
        for(int i=0; i<Collider.points.Length; i++)
        {
            Line.Points[i] = Collider.points[i];
        }
        Line.Points[Line.Points.Length - 1] = Collider.points[0];
        Line.SetAllDirty();
    }
}
