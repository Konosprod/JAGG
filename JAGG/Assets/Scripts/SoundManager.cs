using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType {
    MainMenu,
    TestLevel,
    ButtonHover
};

public enum SFXType
{
    WoodHit
}

public class SoundManager : MonoBehaviour {

    private static AudioSource audioSrc;
    private static Dictionary<SoundType, AudioClip> audioClips;
    private static Dictionary<SFXType, AudioClip> sfxClips;

	// Use this for initialization
	void Awake () {

        audioSrc = GetComponent<AudioSource>();

        audioClips = new Dictionary<SoundType, AudioClip>();
        sfxClips = new Dictionary<SFXType, AudioClip>();

        foreach(SoundType t in Enum.GetValues(typeof(SoundType)))
        {
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

    public static void PlayMusic(SoundType type)
    {
        audioSrc.PlayOneShot(audioClips[type]);
    }

    public static void PlayMusic(string name)
    {
        SoundType t = (SoundType) Enum.Parse(typeof(SoundType), name);
        PlayMusic(t);
    }

    public static void PlaySFX(SFXType type)
    {
        audioSrc.PlayOneShot(sfxClips[type]);
    }

    public static void PlaySFX(string name)
    {
        SFXType t = (SFXType)Enum.Parse(typeof(SFXType), name);
        PlaySFX(t);
    }
}
