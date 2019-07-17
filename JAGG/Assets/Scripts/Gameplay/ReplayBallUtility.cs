using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayBallUtility : MonoBehaviour
{
    public Text playerNameText;
    public ParticleSystem trail;
    public BallPhysics physics;

    public void SetReplayBallParams(string steamName, Color trailColor)
    {
        playerNameText.text = steamName;
        trail.GetComponent<Renderer>().material.SetColor("_TintColor", trailColor);
    }

    public void MoveBall(Vector3 position)
    {
        if (gameObject.activeSelf)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = false;
        }

        transform.position = position;

        if (gameObject.activeSelf)
            StartCoroutine(EnableTrail());
    }

    IEnumerator EnableTrail()
    {
        yield return null;
        ParticleSystem.EmissionModule em = trail.emission;
        em.enabled = true;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
