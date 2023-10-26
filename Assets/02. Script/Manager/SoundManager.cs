using DG.Tweening;
using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static public SoundManager Instance;

    public SettingSound Setting;

    public AudioSource[] BgmAudioSource;
    public AudioSource EffectOnePlayeAudioSource;
    public AudioSource[] EffectAudioSource;

    [Header("Sound")]
    public AudioClip[] BgmList;
    public AudioClip TouchSound;


    [Header("Data")]
    public SoundData Data;
    private int BgmPlayIndex = 0;
    private int BgmSourceIndex = 0;
    private bool Bgmchanging = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
    }

    public void PlayTouchSound()
    {
        Play(TouchSound);
    }

    public void Play(AudioClip clip)
    {
        if (clip != null)
            EffectOnePlayeAudioSource.PlayOneShot(clip);
    }
    public AudioSource PlayEffect(AudioClip clip)
    {
        foreach (var source in EffectAudioSource)
        {
            if (source.isPlaying == false)
            {
                source.clip = clip;
                source.Play();
                return source;
            }
        }
        EffectAudioSource[0].clip = clip;
        EffectAudioSource[0].Play();
        return EffectAudioSource[0];
    }


    private void Update()
    {
        if (BgmList == null || BgmList.Length == 0)
        {
            return;
        }

        if (Bgmchanging)
        {
            ChagneTime += Time.deltaTime;
            if (ChagneTime > 5)
            {
                Bgmchanging = false;
                ChagneTime = 0;
            }
        }

        //if (Input.GetKeyDown(KeyCode.PageDown))
        //{
        //    BgmNext();
        //}
        if (Bgmchanging == false)
        {
            if (BgmAudioSource[BgmSourceIndex].time > BgmAudioSource[BgmSourceIndex].clip.length - Data.EffectVolume)
            {
                BgmNext();
            }
        }
    }

    public float ChagneTime = 0;
    public float FadeVolume = 0;
    public void BgmFade(float volume, float time)
    {
        Debug.Log("[BgmFade] " + volume + " / " + time);
        BgmAudioSource[BgmSourceIndex].DOKill();
        BgmAudioSource[BgmSourceIndex].DOFade(volume, time);

        FadeVolume = volume;
    }
    public void BgmNext()
    {
        if (BgmList == null || BgmList.Length == 0)
        {
            return;
        }

        Bgmchanging = true;
        ChagneTime = 0;

        var nextBgm = BgmList[0];
        if (BgmList.Length > 1)
        {
            if (Data.IsBgRandom)
            {
                for (int i = 0; i < 20; i++)
                {
                    int next = Random.Range(0, BgmList.Length);
                    if (next != BgmPlayIndex)
                    {
                        nextBgm = BgmList[next];
                        break;
                    }
                }
            }
            else
            {
                BgmPlayIndex = (BgmPlayIndex + 1) % BgmList.Length;
                nextBgm = BgmList[BgmPlayIndex];
            }
        }

        var currentSource = BgmAudioSource[BgmSourceIndex];


        currentSource.DOKill();
        currentSource.DOFade(0, Data.BgFadeTime).onComplete = () =>
        {
            currentSource.Stop();
        };


        BgmSourceIndex = (BgmSourceIndex + 1) % BgmAudioSource.Length;
        Setting.BgmIndexText.text = (BgmPlayIndex + 1).ToString() + " / " + BgmList.Length;
        var nextSource = BgmAudioSource[BgmSourceIndex];

        nextSource.DOKill();
        nextSource.clip = nextBgm;
        nextSource.volume = 0;
        nextSource.DOFade(FadeVolume, Data.BgFadeTime).onComplete = () =>
        {
            Bgmchanging = false;
        };
        nextSource.Play();
    }

    public void SetData()
    {
        if (Bgmchanging)
        {
            Bgmchanging = false;
            BgmAudioSource[BgmSourceIndex].DOKill();
        }

        BgmAudioSource[BgmSourceIndex].volume = Data.BgVolume;

        foreach (var source in EffectAudioSource)
        {
            source.volume = Data.EffectVolume;
        }
        EffectOnePlayeAudioSource.volume = Data.EffectVolume;
    }



    public void Save()
    {
        DataManager.SetData("Sound", Data);
    }

    private bool isFirstLoad = true;
    public void Load()
    {
        Data = DataManager.GetData<SoundData>("Sound");
        if (Data == null)
        {
            Data = new SoundData();
        }
        SetData();
        //BgmNext();
        if (isFirstLoad)
        {
            isFirstLoad = false;

            BgmSourceIndex = 0;

            if(BgmList ==null || BgmList.Length == 0)
            {
                return;
            }

            Setting.BgmIndexText.text = (BgmSourceIndex + 1).ToString() + " / " + BgmList.Length;

            BgmPlayIndex = Random.Range(0, BgmList.Length);
            BgmAudioSource[BgmSourceIndex].clip = BgmList[BgmPlayIndex];
            BgmAudioSource[BgmSourceIndex].Play();
            BgmAudioSource[BgmSourceIndex].volume = 0;
            BgmAudioSource[BgmSourceIndex].DOFade(Data.BgVolume, Data.BgFadeTime);
            Setting.BgmIndexText.text = (BgmPlayIndex + 1).ToString() + " / " + BgmList.Length;

            FadeVolume = Data.BgVolume;
        }
    }
}

[System.Serializable]
public class SoundData
{
    public bool IsBgRandom = true;
    public float BgVolume = 0.5f;
    public float BgFadeTime = 3;
    public float EffectVolume = 1;
}

