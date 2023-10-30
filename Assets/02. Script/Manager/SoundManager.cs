using DG.Tweening;
using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static public SoundManager Instance;

    public SettingSound Setting;

    public AudioSource BgmAudioSource;
    public AudioSource EffectOnePlayeAudioSource;


    [Header("Data")]
    public SoundData Data;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public void Play(AudioClip clip)
    {
        if (clip != null)
            EffectOnePlayeAudioSource.PlayOneShot(clip);
    }


    public void SetData(bool isWait)
    {
        if (isWait)
        {
            BgmAudioSource.volume = Data.BgWaitVolume;
        }
        else
        {
            BgmAudioSource.volume = Data.BgPlayVolume;
        }
        EffectOnePlayeAudioSource.volume = Data.EffectVolume;
    }



    public void Save()
    {
        DataManager.SetData("Sound", Data);
    }

    public void Load()
    {
        Data = DataManager.GetData<SoundData>("Sound");
        if (Data == null)
        {
            Data = new SoundData();
        }
        SetData(true);
    }
}

[System.Serializable]
public class SoundData
{
    public float BgWaitVolume = 0.5f;
    public float BgPlayVolume = 0.5f;
    public float EffectVolume = 1;
}

