using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType {
    MainMenu,
    TestLevel,
    ButtonHover
};

public class SoundManager : MonoBehaviour {

    private static AudioSource audioSrc;
    private static Dictionary<SoundType, AudioClip> audioClips;

	// Use this for initialization
	void Awake () {

        audioSrc = GetComponent<AudioSource>();

        audioClips = new Dictionary<SoundType, AudioClip>();

        foreach(SoundType t in Enum.GetValues(typeof(SoundType)))
        {
            audioClips[t] = Resources.Load<AudioClip>("Sounds/" + t.ToString());
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
        audioSrc.PlayOneShot(audioClips[t]);
    }
}
