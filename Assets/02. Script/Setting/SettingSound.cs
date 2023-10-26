using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace InsolDefaultProject
{
    public class SettingSound : Setting
    {
        public SoundManager Manager;

        public SliderCtr BgSlider;
        public SliderCtr EffectSlider;
        public Text BgmIndexText;


        public override void Init()
        {
            Load();
        }

        public override void Load()
        {
            Manager.Load();
        }

        public override void Save()
        {
            Manager.Save();
        }

        private bool IsShow = false;
        public override void Show(bool isShow)
        {
            IsShow = false;
            if (isShow)
            {
                BgSlider.Val = Manager.Data.BgVolume;
                EffectSlider.Val = Manager.Data.EffectVolume;
                IsShow = true;
            }
        }




        public void CheckEffectSoundButtonClick()
        {
            SoundManager.Instance.PlayTouchSound();
        }


        public void OnSliderChange()
        {
            if(IsShow == false)
            {
                return;
            }
            
            Manager.Data.BgVolume = BgSlider.Val;
            Manager.Data.EffectVolume = EffectSlider.Val;

            Manager.SetData();
        }

    }

    [System.Serializable]
    public class SoundObject
    {
        public string Title;
        public int Index;
        public AudioSource[] AudioList;
    }
}