using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataManager
{
    static public string Path = @"Setting\";

    static private void SetJsonData(string section, string data, string key = "Data")
    {
        if (string.IsNullOrEmpty(data))
        {
            Debug.LogWarning("" + section + " 데이터 없음. 저장 X");
            return;
        }

        if (Directory.Exists(Path) == false)
        {
            Directory.CreateDirectory(Path);
        }

        var backPath = @"Setting\Backup\" + System.DateTime.Now.ToString("yyMMdd_HHmm") + @"\";
        if (Directory.Exists(backPath) == false)
        {
            Directory.CreateDirectory(backPath);
        }



        string fileName = Path + section + "_" + key + ".txt";
        string back_fileName = backPath + section + "_" + key + ".txt";
        Debug.Log("[Save] " + fileName + " / " + data);





        //PlayerPrefs.SetString(section + "_" + key, data);

        using (StreamWriter outputFile = new StreamWriter(fileName, false))
        {
            outputFile.WriteLine(data);
        }
        using (StreamWriter outputFile = new StreamWriter(back_fileName, false))
        {
            outputFile.WriteLine(data);
        }
    }

    static private string GetJsonData(string section, string defaultData, string key = "Data")
    {
        string file = Path + section + "_" + key + ".txt";
        if (File.Exists(file))
        {
            return File.ReadAllText(file);
        }
        else
        {
            return defaultData;
        }
    }


    /// <summary>
    /// [System.Serializable] 사용된 데이터 클래스 사용
    /// </summary>
    static public void SetData<T>(string key, T data)
    {
        SetJsonData(key, JsonUtility.ToJson(data, true));
    }

    /// <summary>
    /// [System.Serializable] 사용된 데이터 클래스 사용
    /// </summary>
    static public T GetData<T>(string key)
    {
        var str = GetJsonData(key, "");
        if (string.IsNullOrEmpty(str))
        {
            return default(T);
        }
        try
        {
            T data = JsonUtility.FromJson<T>(str);
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Get Data Err] " + key + " / " + e.Message);
            return default(T);
        }
    }
}
