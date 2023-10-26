using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingSensor : Setting
{
    static public SettingSensor Instance;
    public SensorManager Manager;

    public SensorSettingItem SensorItemPrefab;

    public DepthSensorBackViewSetting BackviewSetting;
    public DepthSensorTopViewSetting TopviewSetting;

    public LidarSenserSetting LidarSetting;

    [Header("센서 추가")]
    public GameObject SensorAddPanel;
    public SensorSettingAddItem[] SensorAddItemList;

    [Header("Data")]
    public List<SensorSettingItem> SensorItemList = new List<SensorSettingItem>();


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void Init()
    {
        Load();
    }

    public override void Show(bool isShow)
    {
        SensorAddPanel.gameObject.SetActive(false);
    }



    public void OnSensorAddButtonClick()
    {
        SensorAddPanel.gameObject.SetActive(true);
    }

    public void OnSensorAddPanelOkButtonClick()
    {
        foreach (var item in SensorAddItemList)
        {
            if (item.IsOn)
            {
                switch (item.Type)
                {
                    case SensorModel.LidarG4:
                        StartCoroutine(SensorLidarGManager.Instance.FindLidar());
                        break;
                    case SensorModel.LidarJp40:
                        Manager.AddSensor(new SensorData(SensorModel.LidarJp40, item.SubInputField.text).Id, true);
                        break;
                    case SensorModel.Kinect:
                        Manager.AddSensor(new SensorData(SensorModel.Kinect, 0).Id, true);
                        break;
                    case SensorModel.Astra:
                        int port = (int)item.SubSlider.Val;
                        Manager.AddSensor(new SensorData(SensorModel.Astra, port).Id, true);
                        break;
                }
            }
        }
        SensorAddPanel.SetActive(false);
    }



    public void OnSensorDeleteButtonClick(SensorSettingItem item)
    {
        MsgBox.Show("<color=#91D8D2FF>" + item.MemoText.text + "</color>\n해당 센서를 제거하시겠습니까?")
            .SetButtonType(MsgBoxButtons.OK_CANCEL)
            .OnResult((result) =>
            {
                switch (result)
                {
                    case DialogResult.YES_OK:
                        if (item.Sensor != null)
                            Manager.RemoveSensor(item.Sensor);

                        SensorItemList.Remove(item);
                        Destroy(item.gameObject);
                        break;
                    case DialogResult.NO:
                    case DialogResult.CANCEL:
                        break;
                    default:
                        break;
                }

                MsgBox.Close();
            });
    }




    public void OnMemoEditButtonClick(SensorSettingItem item)
    {
        TextSetting.Instance.ShowSetting("센서 이름 설정", item.Sensor.Memo, (str) =>
        {
            item.Sensor.Memo = str;
            item.MemoText.text = str;
            item.Sensor.Save();
        });
    }


    public void OnColorEditButtonClick(SensorSettingItem item)
    {
        ColorSetting.Instance.ShowSetting("좌표 표시 색상 설정", item.Sensor.ViewColor, false, (color) =>
        {
            item.Sensor.ViewColor = color;
            item.SetColor();
            //item.Sensor.Save();
        });

    }




    public void AddSensorItem(Sensor sensor)
    {
        Debug.Log("[AddLidarSensor Item] : " + sensor.SensorId.Id + " / ");
        var item = GetDepthSensorItem(sensor);

        if (item == null)
        {
            item = Instantiate(SensorItemPrefab, SensorItemPrefab.transform.parent);
            SensorItemList.Add(item);
        }
        item.SetData(sensor);
    }
    public SensorSettingItem GetDepthSensorItem(Sensor sensor)
    {
        foreach (var item in SensorItemList)
        {
            if (item.Sensor.Id == sensor.Id)
            {
                return item;
            }
        }

        return null;
    }


    public override void Save()
    {
        Manager.Save();
    }

    public override void Load()
    {
        Manager.Load();
    }
}
