using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorManager : MonoBehaviour
{
    static public SensorManager Instance;

    public SensorAstra AstraPrefab;
    public SensorG4 LidarG4Prefab;
    public SensorUST10 LidarUstPrefab;
    public SensorKinect KinectPrefab;

    public Text DebugModeText;

    [Header("센서 연결 UI")]
    public GameObject StartSensorConnectPanel;
    private List<SensorConnectPanelItem> StartSensorConnectTextList = new List<SensorConnectPanelItem>();
    public SensorConnectPanelItem StartSensorConnectTextPrefab;
    private bool IsStartSensorConnetEnd = false;
    private bool IsChecking = false;
    private int CheckingCount = -1;
    private float SensorCheckTime = 0;

    [Header("Data")]
    public SensorManagerData Data;
    public List<Sensor> SensorList = new List<Sensor>();
    public bool IsShowDebugMode = false;
    private float FpsTime = 0;
    private int Fps = 0;
    private int ShowFps = 0;

    public float DebugShowTime = 1f;

    public bool CheckTouch
    {
        get
        {
            return true;
        }
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12)
            && Input.GetKey(KeyCode.LeftShift) == false
            && Input.GetKey(KeyCode.LeftControl) == false)
        {
            ShowDebugMode(!IsShowDebugMode);
        }
        if (IsShowDebugMode)
        {
            Fps++;
            FpsTime += Time.deltaTime;
            if(FpsTime >= 1)
            {
                FpsTime -= 1;
                ShowFps = Fps;
                Fps = 0;
            }
            DebugModeText.text = "FPS : " + ShowFps + "\n";
            foreach(var sensor in SensorList)
            {
                DebugModeText.text += sensor.GetDebugMsg() + "\n";
            }
        }
        if (IsChecking == false)
        {
            SensorCheckTime += Time.deltaTime;
            if (SensorCheckTime > 5f)
            {
                SensorCheckTime -= 5f;
                StartCoroutine(SensorConnect());
            }
        }
    }
    public void ShowDebugMode(bool isDebugMode)
    {
        IsShowDebugMode = isDebugMode;

        DebugModeText.gameObject.SetActive(isDebugMode);
    }
    private IEnumerator SensorConnect()
    {
        if (IsChecking)
        {
            yield break;
        }
        IsChecking = true;

        foreach (var sensor in SensorList)
        {
            if (sensor.IsConnect == false)
            {
                sensor.Connect();
            }
        }

        CheckingCount++;
        IsChecking = false;

        yield return null;
    }

    private IEnumerator StartSensorConnectCheck()
    {
        IsStartSensorConnetEnd = false;
        StartSensorConnectPanel.SetActive(true);

        foreach (var sensor in SensorList)
        {
            var item = Instantiate(StartSensorConnectTextPrefab, StartSensorConnectTextPrefab.transform.parent);
            item.gameObject.SetActive(true);
            item.SensorMemoText.text = "[ " + sensor.Memo + " ]";
            item.StateText.text = "센서 연결 시도중";
            StartSensorConnectTextList.Add(item);
        }

        while (IsStartSensorConnetEnd == false)
        {
            yield return null;

            for (int i = 0; i < SensorList.Count; i++)
            {
                var sensor = SensorList[i];
                if (sensor.IsConnect)
                {
                    StartSensorConnectTextList[i].StateText.color = Color.green;
                    StartSensorConnectTextList[i].StateText.text = "센서가 연결되었습니다.";
                }
            }

            if (IsChecking == false)
            {
                bool check = false;
                for (int i = 0; i < SensorList.Count; i++)
                {
                    var sensor = SensorList[i];
                    if (sensor.IsConnect == false)
                    {
                        StartSensorConnectTextList[i].CountText.text = "( " + CheckingCount + " )";
                        if (CheckingCount < 2)
                        {
                            StartSensorConnectTextList[i].StateText.color = Color.white;
                            StartSensorConnectTextList[i].StateText.text ="센서 연결 시도중입니다.";
                        }
                        else if (CheckingCount < 4)
                        {
                            StartSensorConnectTextList[i].StateText.color = Color.yellow;
                            StartSensorConnectTextList[i].StateText.text = "센서 연결에 실패했습니다. 재시도 중입니다.";
                        }
                        else if(CheckingCount < 6)
                        {
                            StartSensorConnectTextList[i].StateText.color = new Color(1, 0.5f, 0, 1f);
                            StartSensorConnectTextList[i].StateText.text = "센서 연결에 실패했습니다. USB를 뺏다 꽂아주세요.";
                        }
                        else
                        {
                            OnStartSensorCheckCloseButtonClick();
                        }
                        check = true;
                    }
                }

                if (check == false) //모든 센서 연결됨
                {
                    yield return new WaitForSeconds(1f);
                    IsStartSensorConnetEnd = true;
                }
            }
        }

        StartSensorConnectPanel.SetActive(false);


        //StartCoroutine(ContentsManager.Instance.UiManager.AutoStart());
    }

    internal void RemoveSensor(Sensor sensor)
    {
        SensorList.Remove(sensor);
        Destroy(sensor.gameObject);

        Data.SensorList.Remove(sensor.SensorId.Id);
    }

    public void OnStartSensorCheckCloseButtonClick()
    {
        IsStartSensorConnetEnd = true;
    }




    public Sensor GetSensor(string id)
    {
        foreach (var sensor in SensorList)
        {
            if (sensor.SensorId.Id == id)
            {
                return sensor;
            }
        }

        return null;
    }


    public Sensor AddSensor(string id, bool isSearch)
    {
        Debug.Log("[AddSensor] " + id);

        if (string.IsNullOrEmpty(id))
        {
            Debug.Log("Id 없음");
            return null;
        }

        var sensorData = new SensorData(id);
        var sensor = GetSensor(id);

        if (sensor == null)
        {
            switch (sensorData.Model)
            {
                case SensorModel.LidarG4:
                    sensor = Instantiate(LidarG4Prefab, LidarG4Prefab.transform.parent);
                    break;
                case SensorModel.LidarJp40:
                    sensor = Instantiate(LidarUstPrefab, LidarUstPrefab.transform.parent);
                    sensor.SensorId = sensorData;
                    break;
                case SensorModel.Astra:
                    sensor = Instantiate(AstraPrefab, AstraPrefab.transform.parent);
                    sensor.SensorId = sensorData;
                    break;
                case SensorModel.Kinect:
                    sensor = KinectPrefab;
                    sensor.SensorId = sensorData;
                    break;
                default:
                    Debug.Log("관련된 센서 찾을 수 없음 : " + id);
                    return null;
            }

            sensor.gameObject.SetActive(true);
            SensorList.Add(sensor);
        }

        Debug.Log("Sensor Name " + sensor.name);

        sensor.SensorId = sensorData;
        sensor.Load();

        SettingSensor.Instance.AddSensorItem(sensor);

        if (Data.SensorList.Contains(sensor.SensorId.Id) == false)
        {
            Data.SensorList.Add(sensor.SensorId.Id);
        }
        SensorCheckTime = 5;

        return sensor;
    }


    public void Save()
    {
        DataManager.SetData("Sensor", Data);
        foreach (var sensor in SensorList)
        {
            sensor.Save();
        }
    }


    private bool mIsInit = false;
    public void Init()
    {
        Debug.Log("[SensorManager] Init");
        if (mIsInit)
        {
            return;
        }

        mIsInit = true;
        if (Data.SensorList.Count == 0)
        {
            //StartCoroutine(ContentsManager.Instance.UiManager.AutoStart());
            return;
        }

        StartCoroutine(SensorConnect());
        StartCoroutine(StartSensorConnectCheck());
    }
    public void Load()
    {
        Debug.Log("Sensor Manager Load");
        Data = DataManager.GetData<SensorManagerData>("Sensor");
        if(Data == null)
        {
            Data = new SensorManagerData();
        }

        foreach (var id in Data.SensorList)
        {
            AddSensor(id, false);
        }

        Init();
    }
}


[System.Serializable]
public class SensorManagerData
{
    public List<string> SensorList = new List<string>();


    public bool UseKeepInTouch = true;
    public float InputDelay = 0.3f;


    public bool IsLidarMaxHz = false;

    public float EmptyNoShowSqrDist = 1f;
}

[System.Serializable]
public class SensorData
{
    public SensorModel Model;
    public string Port = "0";
    public int PortNumber = 0;
    public string Id
    {
        get
        {
            return Model + "_" + Port;
        }
    }

    public override bool Equals(object obj)
    {
        return Model.Equals(((SensorData)obj).Model) && Port.Equals(((SensorData)obj).Port);
    }

    public override int GetHashCode()
    {
        var hashCode = -1114337584;
        hashCode = hashCode * -1521134295 + Model.GetHashCode();
        hashCode = hashCode * -1521134295 + Port.GetHashCode();
        return hashCode;
    }

    //public SensorData()
    //{

    //}

    public SensorData(SensorModel model, string port)
    {
        Model = model;
        Port = port;

        int.TryParse(Port, out PortNumber);
    }
    public SensorData(SensorModel model, int portNum = 0)
    {
        Model = model;
        PortNumber = portNum;
        Port = PortNumber.ToString("0");
    }


    public SensorData(string id)
    {
        var list = id.Split('_');
        Model = (SensorModel)System.Enum.Parse(typeof(SensorModel), list[0]);

        if (list.Length > 1 && string.IsNullOrEmpty(list[1]) == false)
        {
            Port = list[1];
            int.TryParse(Port, out PortNumber);
        }
        else
        {
            Port = "0";
            PortNumber = 0;
        }
    }
}

public enum SensorModel
{
    None = -1,
    Touch = 0,
    LidarCh10 = 1,
    LidarJp10 = 2,
    LidarJp40 = 3,
    Kinect = 4,
    Astra = 5,
    IRCamera = 6,
    WebCamera = 7,
    RealSense415 = 8,
    RealSense435 = 9,
    LidarA1M8 = 10,
    LidarG4 = 11
}