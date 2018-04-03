using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class PanelReport : MonoBehaviour {

    public Canvas canvasUi;
    public Image imagePreview;
    public InputField inputMail;

    private SmtpClient smtpClient;

	// Use this for initialization
	void Start () {

        smtpClient = new SmtpClient("mail.konosprod.fr")
        {
            Port = 587,
            Credentials = new System.Net.NetworkCredential("jagg@konosprod.fr", "testpass35") as ICredentialsByHost,
            EnableSsl = true
        };

        ServicePointManager.ServerCertificateValidationCallback = 
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
    }
	
    void OnEnable()
    {
        StartCoroutine(TakeScreenShot());
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cancel();
        }
    }

    public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

    public void SendMail()
    {
        string mailContent = inputMail.text;
        MailMessage message = new MailMessage("bug_report@konosprod.fr", "jagg@konosprod.fr");

        byte[] imageData = imagePreview.sprite.texture.EncodeToJPG();

        MemoryStream ms = new MemoryStream(imageData);
        ms.Seek(0, SeekOrigin.Begin);

        message.Body = mailContent;
        message.Subject = "Bug";
        message.Attachments.Add(new Attachment(ms, "preview.jpg", "image/jpeg"));


        smtpClient.Send(message);

        Cancel();

    }

    IEnumerator TakeScreenShot()
    {
        yield return null;
        canvasUi.enabled = false;
        yield return new WaitForEndOfFrame();

        Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();

        texture.Apply();
        imagePreview.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0f, 0f));
        canvasUi.enabled = true;
    }
}
