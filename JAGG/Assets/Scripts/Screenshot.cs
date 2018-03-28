using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Screenshot : MonoBehaviour {

    public Image image;

    public void TakeScreenShot()
    {
        Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();

        texture.Apply();
        image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0f, 0f));
    }
}
