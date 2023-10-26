using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingContents : Setting
{
    public ContentsManager Manager;


    public Image BgColor;

    public Image AirplaneIconColor;
    public Image AirplaneLineColor;

    public Image RailroadIconColor;
    public Image ArrowLineColor;
    public Image RailroadLineColor;


    public Image DaeguCircleImageType2Color;
    public Image MegaCircleImageType2Color;

    public override void Init()
    {
    }

    public override void Load()
    {
        Manager.Load();
    }

    public override void Save()
    {
        Manager.Save();
    }

    public void OnColorChangeButtonClick(int index)
    {
        if(index == 0)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.BgColor, true, (color) =>
            {
                Manager.Data.BgColor = color;
                BgColor.color = Manager.Data.BgColor;
                Manager.SetData();
            });
        }
        if (index == 1)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.AirplaneIconColor, true, (color) =>
            {
                Manager.Data.AirplaneIconColor = color;
                AirplaneIconColor.color = Manager.Data.AirplaneIconColor;
                Manager.SetData();
            });
        }
        if (index == 2)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.AirplaneLineColor, true, (color) =>
            {
                Manager.Data.AirplaneLineColor = color;
                AirplaneLineColor.color = Manager.Data.AirplaneLineColor;
                Manager.SetData();
            });
        }
        if (index == 3)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.RailroadIconColor, true, (color) =>
            {
                Manager.Data.RailroadIconColor = color;
                RailroadIconColor.color = Manager.Data.RailroadIconColor;
                Manager.SetData();
            });
        }
        if (index == 4)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.ArrowLineColor, true, (color) =>
            {
                Manager.Data.ArrowLineColor = color;
                ArrowLineColor.color = Manager.Data.ArrowLineColor;
                Manager.SetData();
            });
        }
        if (index == 5)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.RailroadLineColor, true, (color) =>
            {
                Manager.Data.RailroadLineColor = color;
                RailroadLineColor.color = Manager.Data.RailroadLineColor;
                Manager.SetData();
            });
        }
        if (index == 6)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.DaeguCircleImageColor, true, (color) =>
            {
                Manager.Data.DaeguCircleImageColor = color;
                DaeguCircleImageType2Color.color = Manager.Data.DaeguCircleImageColor;
                Manager.SetData();
            });
        }
        if (index == 7)
        {
            ColorSetting.Instance.ShowSetting("", Manager.Data.MegaCircleImageColor, true, (color) =>
            {
                Manager.Data.MegaCircleImageColor = color;
                MegaCircleImageType2Color.color = Manager.Data.MegaCircleImageColor;
                Manager.SetData();
            });
        }
    }

    public override void Show(bool isShow)
    {
        if (isShow)
        {
            BgColor.color = Manager.Data.BgColor;

            AirplaneIconColor.color = Manager.Data.AirplaneIconColor;
            AirplaneLineColor.color = Manager.Data.AirplaneLineColor;

            RailroadIconColor.color = Manager.Data.RailroadIconColor;
            ArrowLineColor.color = Manager.Data.ArrowLineColor;
            RailroadLineColor.color = Manager.Data.RailroadLineColor;


            DaeguCircleImageType2Color.color = Manager.Data.DaeguCircleImageColor;
            MegaCircleImageType2Color.color = Manager.Data.MegaCircleImageColor;
        }
    }
}
