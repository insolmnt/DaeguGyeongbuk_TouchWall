using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIDrag : MonoBehaviour {
    private float x;
    private float y;

    private bool mMounseDown = false;

    public GameObject MoveObject;
    public RectTransform objectRectTransform;
    public RectTransform CanvasRectTransform;

    void Start(){
        if (objectRectTransform == null)
        {
            objectRectTransform = GetComponent<RectTransform>();
        }
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 v3 = transform.position;

            float width = objectRectTransform.sizeDelta.x * Screen.width / CanvasRectTransform.rect.width;
            float height = objectRectTransform.sizeDelta.y * Screen.height / CanvasRectTransform.rect.height;
            Rect rect = new Rect(v3.x - width * objectRectTransform.pivot.x, v3.y - height * objectRectTransform.pivot.y, width, height);
            
            if (rect.Contains(Input.mousePosition))
            {
                x = MoveObject.transform.position.x - Input.mousePosition.x;
                y = MoveObject.transform.position.y - Input.mousePosition.y;
                mMounseDown = true;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (mMounseDown)
            {
                MoveObject.transform.position = new Vector3(
                    Mathf.Clamp(x + Input.mousePosition.x, 0, Screen.width),
                    Mathf.Clamp(y + Input.mousePosition.y, 0, Screen.height),
                    0);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            mMounseDown = false;
        }
	}
}
