using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorTest : MonoBehaviour
{
    static public SensorTest Instance;

    public GameObject TestPanel;
    public GridLayoutGroup Grid;
    public SensorTestBlock BlockPrefab;

    public Text StateText;
    [Header("Data")]
    public bool IsShow = false;
    public int GridXCount = 16;
    public int GridYCount = 10;
    public List<SensorTestBlock> BlockList = new List<SensorTestBlock>();
    public int OutCheckCount
    {
        set
        {
            outCheckCount = value;
            StateText.text = "OutCheckCount : " + outCheckCount;
        }
        get
        {
            return outCheckCount;
        }
    }
    private int outCheckCount = 0;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void ShowTest(bool isShow)
    {
        IsShow = isShow;
        TestPanel.gameObject.SetActive(IsShow);

        if (isShow)
        {
            if(BlockList.Count == 0)
            {
                BlockPrefab.gameObject.SetActive(false);
                SetGridSize(GridXCount, GridYCount);
            }

            ScreenManager.Instance.ScreenList[0].OnSensorInputEnd += OnTouchEnd;
        }
        else
        {
            ScreenManager.Instance.ScreenList[0].OnSensorInputEnd -= OnTouchEnd;
        }
    }
    private void OnTouchEnd()
    {
        foreach(var block in BlockList)
        {
            block.OnTouchEnd();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ShowTest(!IsShow);
        }

        if(IsShow == false)
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //if (Input.GetKey(KeyCode.Alpha0))
            //{
            //    InCheckCount = 0;
            //}
            //for (int i = 0; i < 9; i++)
            //{
            //    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            //    {
            //        InCheckCount = i + 1;
            //    }
            //}
        }
        else
        {
            if (Input.GetKey(KeyCode.Alpha0))
            {
                OutCheckCount = 0;
            }
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    OutCheckCount = i + 1;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(var bl in BlockList)
            {
                bl.Clear();
            }
        }

        if(SettingSensor.Instance.LidarSetting.gameObject.activeSelf == false 
            || SettingSensor.Instance.LidarSetting.MoveArrowIndex < 0)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SetGridSize(GridXCount + 1, GridYCount);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SetGridSize(GridXCount - 1, GridYCount);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SetGridSize(GridXCount, GridYCount + 1);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SetGridSize(GridXCount, GridYCount - 1);
            }
        }
    }
    private void SetGridSize(int x, int y)
    {
        GridXCount = Mathf.Max(x, 1);
        GridYCount = Mathf.Max(y, 1);

        UIManager.Instance.ShowMessage("" + GridXCount + ", " + GridYCount);

        var rect = Grid.GetComponent<RectTransform>().rect;

        Grid.cellSize = new Vector2(rect.width / GridXCount - Grid.spacing.x, rect.height / GridYCount - Grid.spacing.y);

        foreach(var block in BlockList)
        {
            block.gameObject.SetActive(false);
        }

        for(int i = 0; i < GridXCount; i++)
        {
            for (int j = 0; j < GridYCount; j++)
            {
                var block = GetIdleBlock();
                block.gameObject.SetActive(true);
                block.SetSize();
                block.Clear();
            }
        }
    }

    public void OnTouch(Camera camera, float viewport_x, float viewport_y)
    {
        Ray ray = camera.ViewportPointToRay(new Vector3(viewport_x, viewport_y));

        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red, 3f);

        //RaycastHit hit;
        var hitList = Physics.RaycastAll(ray, 1000);
        if (hitList != null && hitList.Length > 0)
        {
            foreach (var hit in hitList)
            {
                var touch = hit.collider.GetComponent<TouchInput>();
                if (touch == null)
                {
                    continue;
                }

                var result = touch.OnTouch(hit.point);
            }
        }

    }
    private SensorTestBlock GetIdleBlock()
    {
        foreach(var bl in BlockList)
        {
            if(bl.gameObject.activeSelf == false)
            {
                return bl;
            }
        }


        var block = Instantiate(BlockPrefab, BlockPrefab.transform.parent);
        BlockList.Add(block);
        return block;
    }
}
