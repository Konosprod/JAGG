using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Fader : MonoBehaviour {

    public CanvasGroup canvasGroup;

    public void FadeIn(float time = 0.5f, UnityAction callback = null)
    {
        StartCoroutine(Fade(canvasGroup, 0, 1, time, callback));
    }

    public void FadeOut(float time = 0.5f, UnityAction callback = null)
    {
        StartCoroutine(Fade(canvasGroup, 1, 0, time, callback));
    }

    public IEnumerator Fade(CanvasGroup cg, float start, float end, float time = 0.5f, UnityAction callback = null)
    {
        float startTime = Time.time;
        float timeSinceStarted = Time.time - startTime;
        float percentage = timeSinceStarted / time;

        while(true)
        {
            timeSinceStarted = Time.time - startTime;
            percentage = timeSinceStarted / time;

            float currentValue = Mathf.Lerp(start, end, percentage);

            cg.alpha = currentValue;

            if (currentValue >= 1) break;

            yield return new WaitForEndOfFrame();
        }

        if(callback != null)
            callback();
    }
}
