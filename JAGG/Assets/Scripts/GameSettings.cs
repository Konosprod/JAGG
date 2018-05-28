using UnityEngine;

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

    public GameSettings()
    {
        colorTrail = Color.red;
    }
}
