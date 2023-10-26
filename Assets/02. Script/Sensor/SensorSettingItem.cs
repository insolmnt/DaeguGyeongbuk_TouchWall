using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorSettingItem : MonoBehaviour
{
    public SettingSensor Setting;

    public Text NameText;
    public Text MemoText;

    public Image ViewColorImage;

    public RawImage PreviewRawImage;
    public Toggle[] UseToggle;
    public Button[] SettingButton;

    [Header("Data")]
    public Sensor Sensor;
    private bool mIsLoad = false;
    private bool BeforeConnect = false;


    private void Update()
    {
        if (Sensor == null)
        {
            return;
        }

        if (Sensor.IsConnect != BeforeConnect)
        {
            BeforeConnect = Sensor.IsConnect;
            SetText();
        }

        if (PreviewRawImage.texture == null)
        {
            PreviewRawImage.texture = Sensor.GetPreviewImage(true);
        }
    }
    private void SetText()
    {
        NameText.text = Sensor.SensorName;
        NameText.color = Sensor.IsConnect ? new Color(0.34f, 0.54f, 1f, 1) : Color.red;

        if (BeforeConnect == false)
        {
            NameText.text += "<size=13>  (Not connected)</size>";
        }
    }

    public void SetData(Sensor sensor)
    {
        Sensor = sensor;

        NameText.text = sensor.SensorName;
        BeforeConnect = sensor.IsConnect;

        SetText();
        SetMemo();
        SetColor();

        //Group.interactable = isConnect;

        gameObject.SetActive(true);
        SetUseToggle();
    }

    private void OnEnable()
    {
        PreviewRawImage.texture = Sensor.GetPreviewImage(true);

        UseToggle[0].transform.parent.gameObject.SetActive(true);
        //UseToggle[1].transform.parent.gameObject.SetActive(ScreenManager.Instance.LeftDisplay >= 0);
        UseToggle[1].transform.parent.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        Sensor.GetPreviewImage(false);
    }

    public void SetMemo()
    {
        MemoText.text = Sensor.Memo;
    }

    public void SetColor()
    {
        ViewColorImage.color = Sensor.ViewColor;
    }

    public void OnUseToggleChange()
    {
        if(mIsLoad == false)
        {
            return;
        }

        switch (Sensor.SensorId.Model)
        {
            case SensorModel.LidarJp40:
            case SensorModel.LidarG4:
                ((LidarSensor)Sensor).Data.LeftData.IsUse = UseToggle[0].isOn;
                ((LidarSensor)Sensor).Data.RightData.IsUse = UseToggle[1].isOn;
                break;
            case SensorModel.Astra:
            case SensorModel.Kinect:
                ((DepthSensor)Sensor).Data.LeftBackview.IsUse = UseToggle[0].isOn;
                ((DepthSensor)Sensor).Data.RightBackview.IsUse = UseToggle[1].isOn;
                break;
        }
        SetUseToggle();
    }
    private void SetUseToggle()
    {
        mIsLoad = false;

        Debug.Log("토글 " + Sensor.SensorId.Model);
        switch (Sensor.SensorId.Model)
        {
            case SensorModel.LidarJp40:
            case SensorModel.LidarG4:
                UseToggle[0].isOn = ((LidarSensor)Sensor).Data.LeftData.IsUse;
                UseToggle[1].isOn = ((LidarSensor)Sensor).Data.RightData.IsUse;
                break;
            case SensorModel.Astra:
            case SensorModel.Kinect:
                UseToggle[0].isOn = ((DepthSensor)Sensor).Data.LeftBackview.IsUse;
                UseToggle[1].isOn = ((DepthSensor)Sensor).Data.RightBackview.IsUse;
                break;
        }


        SettingButton[0].interactable = UseToggle[0].isOn;
        SettingButton[1].interactable = UseToggle[1].isOn;

        mIsLoad = true;
    }

    public void OnSettingButtonClick(int screen)
    {
        switch (Sensor.SensorId.Model)
        {
            case SensorModel.LidarJp40:
            case SensorModel.LidarG4:
                switch ((ScreenType)screen)
                {
                    case ScreenType.지도:
                        Setting.LidarSetting.ShowSetting(((LidarSensor)Sensor).LeftView);
                        break;

                    case ScreenType.사진:
                        Setting.LidarSetting.ShowSetting(((LidarSensor)Sensor).RightView);
                        break;
                }
                break;

            case SensorModel.Astra:
            case SensorModel.Kinect:
                var depthSensor = (DepthSensor)Sensor;
                switch ((ScreenType)screen)
                {
                    case ScreenType.지도:
                        if (depthSensor.Data.LeftTopview.IsUse)
                        {
                            Setting.TopviewSetting.ShowSetting(depthSensor.LeftTopView);
                        }
                        else
                        {
                            Setting.BackviewSetting.ShowSetting(depthSensor.LeftBackView);
                        }
                        break;

                    case ScreenType.사진:
                        if (depthSensor.Data.RightTopview.IsUse)
                        {
                            Setting.TopviewSetting.ShowSetting(depthSensor.RightTopView);
                        }
                        else
                        {
                            Setting.BackviewSetting.ShowSetting(depthSensor.RightBackView);
                        }
                        break;
                }
                break;
        }
    }
}
