using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace InsolDefaultProject
{
    public class SettingSound : Setting
    {
        public SoundManager Manager;

        public SliderCtr BgWaitSlider;
        public SliderCtr BgPlaySlider;
        public SliderCtr EffectSlider;


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
                BgWaitSlider.Val = Manager.Data.BgWaitVolume;
                BgPlaySlider.Val = Manager.Data.BgPlayVolume;
                EffectSlider.Val = Manager.Data.EffectVolume;
                IsShow = true;
            }
        }




        public void CheckEffectSoundButtonClick()
        {
            SoundManager.Instance.Play(ContentsManager.Instance.TouchSound);
        }


        public void OnSliderChange()
        {
            if(IsShow == false)
            {
                return;
            }
            
            Manager.Data.BgWaitVolume = BgWaitSlider.Val;
            Manager.SetData(true);
        }
        public void OnBgPlayChange()
        {
            if (IsShow == false)
            {
                return;
            }

            Manager.Data.BgPlayVolume = BgPlaySlider.Val;
            Manager.Data.EffectVolume = EffectSlider.Val;

            Manager.SetData(false);
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