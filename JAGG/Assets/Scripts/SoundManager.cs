using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType {
    None,
    MainMenu,
    TestLevel,
    ButtonHover
};

public enum SFXType
{
    WoodHit
}

public class SoundManager : MonoBehaviour {

    public AudioMixer masterMixer;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    private static Dictionary<SoundType, AudioClip> audioClips;
    private static Dictionary<SFXType, AudioClip> sfxClips;

    public static SoundManager _instance;

    private SoundType actuallyPlaying;

	// Use this for initialization
	void Awake () {

        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        audioClips = new Dictionary<SoundType, AudioClip>();
        sfxClips = new Dictionary<SFXType, AudioClip>();

        foreach(SoundType t in Enum.GetValues(typeof(SoundType)))
        {
            if(t != SoundType.None)
                audioClips[t] = Resources.Load<AudioClip>("Sounds/" + t.ToString());
        }

        foreach(SFXType t in Enum.GetValues(typeof(SFXType)))   
        {
            sfxClips[t] = Resources.Load<AudioClip>("Sounds/SFX/" + t.ToString());
        }

	}

	// Update is called once per frame
	void Update () {
		
	}

    public void PlayMusic(SoundType type, bool forceReplay = false)
    {
        if (forceReplay && type == actuallyPlaying)
        {
            bgmSource.Stop();
            bgmSource.loop = true;
            bgmSource.PlayOneShot(audioClips[type]);
        }

        if (!forceReplay && type == actuallyPlaying)
            return;

        bgmSource.Stop();
        bgmSource.loop = true;
        bgmSource.PlayOneShot(audioClips[type]);

        actuallyPlaying = type;
    }

    public void PlayMusic(string name)
    {
        SoundType t = (SoundType) Enum.Parse(typeof(SoundType), name);
        PlayMusic(t);
    }

    public void PlaySFX(SFXType type)
    {
        sfxSource.PlayOneShot(sfxClips[type]);
    }

    public void PlaySFX(string name)
    {
        SFXType t = (SFXType)Enum.Parse(typeof(SFXType), name);
        PlaySFX(t);
    }

    public void SetSFXVolume(float volume)
    {
        masterMixer.SetFloat("SFXVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        masterMixer.SetFloat("BGMVolume", volume);
    }

    public void SetMasterVolume(float volume)
    {
        masterMixer.SetFloat("MasterVolume", volume);
    }
}
