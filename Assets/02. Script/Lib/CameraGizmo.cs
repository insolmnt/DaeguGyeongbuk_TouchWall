using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraGizmo : MonoBehaviour {
    public Camera GizmoCamera;
    public Color GizmoColor = Color.blue;

    public bool IsMiddle = false;
    public bool IsCenter = false;
    public bool IsTopDown = true;

    private void OnDrawGizmos()
    {
        if(GizmoCamera == null)
        {
            return;
        }

        Gizmos.color = GizmoColor;
        if (!GizmoCamera.orthographic)
        {
            var lb = GizmoCamera.ViewportToWorldPoint(new Vector3(0, 0, GizmoCamera.farClipPlane));
            var lt = GizmoCamera.ViewportToWorldPoint(new Vector3(0, 1, GizmoCamera.farClipPlane));
            var rb = GizmoCamera.ViewportToWorldPoint(new Vector3(1, 0, GizmoCamera.farClipPlane));
            var rt = GizmoCamera.ViewportToWorldPoint(new Vector3(1, 1, GizmoCamera.farClipPlane));
            if (IsCenter)
            {
                var center = GizmoCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, GizmoCamera.farClipPlane));
                Gizmos.DrawLine(GizmoCamera.transform.position, center);
                Gizmos.DrawLine(lb, rt);
                Gizmos.DrawLine(lt, rb);
            }
            if (IsMiddle)
            {
                Gizmos.DrawLine(GizmoCamera.transform.position, GizmoCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, GizmoCamera.farClipPlane)));
                Gizmos.DrawLine(GizmoCamera.transform.position, GizmoCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, GizmoCamera.farClipPlane)));
            }

            if (IsTopDown)
            {
                //Gizmos.DrawFrustum(GizmoCamera.transform.position, GizmoCamera.fieldOfView, GizmoCamera.farClipPlane, GizmoCamera.nearClipPlane, GizmoCamera.aspect);
                Gizmos.DrawLine(GizmoCamera.transform.position, lb);
                Gizmos.DrawLine(GizmoCamera.transform.position, lt);
                Gizmos.DrawLine(GizmoCamera.transform.position, rb);
                Gizmos.DrawLine(GizmoCamera.transform.position, rt);

                Gizmos.DrawLine(rb, lb);
                Gizmos.DrawLine(lb, lt);
                Gizmos.DrawLine(rt, rb);
                Gizmos.DrawLine(lt, rt);
            }
        }
        else
        {
            if (IsCenter)
            {
                Gizmos.DrawLine(GizmoCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)), GizmoCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, GizmoCamera.farClipPlane)));
            }

            if (IsMiddle)
            {
                Gizmos.DrawLine(GizmoCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)), GizmoCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, GizmoCamera.farClipPlane)));
                Gizmos.DrawLine(GizmoCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)), GizmoCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, GizmoCamera.farClipPlane)));
            }

            if (IsTopDown)
            {
                Gizmos.DrawLine(GizmoCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)), GizmoCamera.ViewportToWorldPoint(new Vector3(0, 0, GizmoCamera.farClipPlane)));
                Gizmos.DrawLine(GizmoCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)), GizmoCamera.ViewportToWorldPoint(new Vector3(0, 1, GizmoCamera.farClipPlane)));
                Gizmos.DrawLine(GizmoCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)), GizmoCamera.ViewportToWorldPoint(new Vector3(1, 0, GizmoCamera.farClipPlane)));
                Gizmos.DrawLine(GizmoCamera.ViewportToWorldPoint(new Vector3(1, 1, 0)), GizmoCamera.ViewportToWorldPoint(new Vector3(1, 1, GizmoCamera.farClipPlane)));
            }
        }

    }
}
