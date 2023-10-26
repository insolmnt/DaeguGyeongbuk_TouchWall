using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    static public ScreenManager Instance;

    static public Action OnDisplayChange;
    static public Action OnScreenSizeChange;


    public Camera[] BackgroundCamera;

    public Monitor[] ScreenList;



    [Header("Data")]
    public ScreenManagerData Data;
    public readonly AnimationCurve DefaultCurve = new AnimationCurve()
    {
        keys = new Keyframe[] {
            new Keyframe()
            {
                time = 0,
                value = 0,
                inTangent = 1,
                outTangent = 1
            },
            new Keyframe()
            {
                time = 1,
                value = 1,
                inTangent = 1,
                outTangent = 1
            }}
    };
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void DisplayChange()
    {
        foreach(var camera in BackgroundCamera)
        {
            camera.gameObject.SetActive(false);
        }
        foreach(var screen in ScreenList)
        {
            if(screen.Data.Display >= 0 && screen.Data.Display < BackgroundCamera.Length)
            {
                BackgroundCamera[screen.Data.Display].gameObject.SetActive(true);
            }
        }
        BackgroundCamera[0].gameObject.SetActive(true);

        OnDisplayChange?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.ScrollLock) || Input.GetKeyDown(KeyCode.SysReq))
        {
            foreach(var screen in ScreenList)
            {
                var rt = screen.RawRenderTexture;
                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                RenderTexture.active = null;

                var bytes = tex.EncodeToPNG();
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\캡처_" + DateTime.Now.ToString("yyMMdd_HHmmss") + "_" + screen.Type + ".png";
                System.IO.File.WriteAllBytes(path, bytes);
                Debug.Log("Saved to " + path);
            }
            
            UIManager.Instance.ShowMessage("스크린샷 저장 완료 (바탕화면)");
        }
    }

    public void Save()
    {
        DataManager.SetData("Screen", Data);

        foreach (var screen in ScreenList)
        {
            screen.Save();
        }
    }
    public void Load()
    {

        Data = DataManager.GetData<ScreenManagerData>("Screen");
        if(Data == null)
        {
            Data = new ScreenManagerData();
        }

        foreach(var screen in ScreenList)
        {
            screen.Load();
        }    
        DisplayChange();
    }
}

[System.Serializable]
public class ScreenManagerData
{
    public int ScreenWidth = 1500;
    public int ScreenHeight = 1500;
}


public enum ScreenType
{
    지도, 사진
}