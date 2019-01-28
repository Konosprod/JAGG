using UnityEngine;
using System.Collections.Generic;

public enum KeyAction {
    AccurateShoot,
    Pause,
    ItemUse,
    Reset
}

public class GameSettings
{
    public bool Fullscreen;
    public int TextureQuality;
    public int Antialiasing;
    public int VSync;
    public int Resolution;
    public float BGMAudioVolume;
    public float SFXAudioVolume;
    public float Sensibility;
    public float AccurateSensibility;
    public int AccurateMode;
    public Color colorTrail;
    public Dictionary<KeyAction, KeyCode> Keys;

    public GameSettings()
    {
        colorTrail = Color.red;
        Keys = new Dictionary<KeyAction, KeyCode>();

        Keys[KeyAction.AccurateShoot] = KeyCode.A;
        Keys[KeyAction.Pause] = KeyCode.P;
        Keys[KeyAction.ItemUse] = KeyCode.Space;
        Keys[KeyAction.Reset] = KeyCode.R;
    }
}
