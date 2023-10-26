using StartPage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataClass
{
    static public LicenseDetailResponse LicenseData = null;

    private const string FileName = "Setting.ini";
    static public INIParser IniParser = new INIParser();
    static public bool mIsLoad = false;

    static public bool IsMonitorAuto = true;
    static public int MonitorX;
    static public int MonitorY;
    static public int MonitorWidth;
    static public int MonitorHeight;

    static public bool IsUpdate = false;

    static public bool Test = false;
    static public float Timeout = 1f;

    static public void Load()
    {
        if (mIsLoad)
        {
            return;
        }

        Open();

        IniParser.WriteValue("App", "Type", Config.AppType);
        IniParser.WriteValue("App", "Version", Config.AppVersion);
        IsUpdate = IniParser.ReadValue("App", "Update", false);
        if (IsUpdate)
        {
            IniParser.KeyDelete("App", "Update");
        }

        Timeout = (float)IniParser.ReadValue("App", "Timeout", 1);

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
        Test = true;
#else
        Debug.unityLogger.logEnabled = IniParser.ReadValue("App", "Log", false);
        Test = IniParser.ReadValue("App", "Test", false);
#endif

        IsMonitorAuto = IniParser.ReadValue("Monitor", "Auto", true);
        MonitorX = IniParser.ReadValue("Monitor", "X", 0);
        MonitorY = IniParser.ReadValue("Monitor", "Y", 0);
        MonitorWidth = IniParser.ReadValue("Monitor", "Width", Display.main.systemWidth);
        MonitorHeight = IniParser.ReadValue("Monitor", "Height", Display.main.systemHeight);

        Close();
        mIsLoad = true;
    }


    static private int OpenCount = 0;
    static public void Open()
    {
        OpenCount++;

        if (OpenCount > 1)
        {
            return;
        }

        try
        {
            Debug.Log("DataClass Open");
            IniParser.Open(FileName);
        }
        catch (Exception e)
        {
            Debug.LogError("DataOpen Error : " + e.Message);
        }
    }

    static public void Close()
    {
        OpenCount--;

        if (OpenCount > 0)
        {
            return;
        }

        try
        {
            Debug.Log("DataClass Close");
            IniParser.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("DataClose Error : " + e.Message);
        }
    }
}
