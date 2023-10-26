using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sensor : MonoBehaviour
{
    public string SensorName;

    //[Multiline(3)]
    [Header("- 연결 실패시 표시할 메시지 -")]
    public string ConnectFailMsg;


    [Header("센서 기본 Data")]
    public SensorData SensorId;
    public string Id
    {
        get
        {
            return SensorId.Id;
        }
    }
    public bool IsConnect = false;

    public string Memo
    {
        get
        {
            return GetMemo();
        }
        set
        {
            SetMemo(value);
        }
    }
    public Color ViewColor
    {
        get
        {
            return GetColor();
        }
        set
        {
            SetColor(value);
        }
    }

    public abstract void Connect();
    public abstract void DisConnect();

    public abstract string GetDebugMsg();
    protected abstract string GetMemo();
    protected abstract void SetMemo(string memo);
    protected abstract Color GetColor();
    protected abstract void SetColor(Color color);

    public abstract void Save();
    public abstract void Load();

    public abstract Texture2D GetPreviewImage(bool isStart);

    public void OnDestroy()
    {
        DisConnect();
    }
}
