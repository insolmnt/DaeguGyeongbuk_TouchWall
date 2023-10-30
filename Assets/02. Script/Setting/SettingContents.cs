using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingContents : Setting
{
    public ContentsManager Manager;


    public Image BgColor;

    //public Image AirplaneIconColor;
    public Image AirplaneLineColor;
    public Image AirplaneColor;

    //public Image RailroadIconColor;
    public Image ArrowLineColor;
    public Image RailroadLineColor;
    public Image TrainColor;


    public Image DaeguCircleImageType2Color;
    public Image MegaCircleImageType2Color;

    public NumberInputField ContentsWaitTime;
    public NumberInputField EndWaitTime;


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
            ColorSetting.Instance.ShowSetting("", Manager.Data.AirplaneColor, true, (color) =>
            {
                Manager.Data.AirplaneColor = color;
                AirplaneColor.color = Manager.Data.AirplaneColor;
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
            ColorSetting.Instance.ShowSetting("", Manager.Data.TrainColor, true, (color) =>
            {
                Manager.Data.TrainColor = color;
                TrainColor.color = Manager.Data.TrainColor;
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

    public void OnInputChange()
    {
        if(mIsLoad == false)
        {
            return;
        }


        Manager.Data.MegaEndWaitTime = EndWaitTime.Val;
        Manager.Data.TouchEndWaitTime = ContentsWaitTime.Val;
    }

    private bool mIsLoad = false;
    public override void Show(bool isShow)
    {
        if (isShow)
        {
            mIsLoad = false;

            BgColor.color = Manager.Data.BgColor;

            AirplaneColor.color = Manager.Data.AirplaneColor;
            AirplaneLineColor.color = Manager.Data.AirplaneLineColor;

            TrainColor.color = Manager.Data.TrainColor;
            ArrowLineColor.color = Manager.Data.ArrowLineColor;
            RailroadLineColor.color = Manager.Data.RailroadLineColor;


            DaeguCircleImageType2Color.color = Manager.Data.DaeguCircleImageColor;
            MegaCircleImageType2Color.color = Manager.Data.MegaCircleImageColor;

            EndWaitTime.Val = Manager.Data.MegaEndWaitTime;
            ContentsWaitTime.Val = Manager.Data.TouchEndWaitTime;
            mIsLoad = true;
        }
    }
}
