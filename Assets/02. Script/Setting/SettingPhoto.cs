using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPhoto : Setting
{
    public PhotoManager Manager;


    public NumberInputField PhotoBigShowTime;
    public NumberInputField PhotoSmallShowTime;
    public NumberInputField PhotoFadeTime;


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


    public void OnShowButtonClick(int i)
    {
        if(i == 0)
        {
            Manager.ShowAirplane(true);
        }
        else if(i == 1)
        {
            Manager.ShowTrain(true);
        }
        else if (i == 2)
        {
            Manager.ShowWait();
        }
    }
    private bool mIsLoad = false;
    public override void Show(bool isShow)
    {
        if (isShow)
        {
            mIsLoad = false;
            PhotoBigShowTime.Val = Manager.Data.WaitBigPhotoShowTime;
            PhotoSmallShowTime.Val = Manager.Data.WaitSmallPhotoShowTime;
            PhotoFadeTime.Val = Manager.Data.FadeTime;

            mIsLoad = true;
        }
        else
        {
            //Manager.ShowWait();
        }
    }

    public void OnSliderChange()
    {
        if(mIsLoad == false)
        {
            return;
        }

        Manager.Data.WaitBigPhotoShowTime = PhotoBigShowTime.Val;
        Manager.Data.WaitSmallPhotoShowTime = PhotoSmallShowTime.Val;
        Manager.Data.FadeTime = PhotoFadeTime.Val;
    }
}
