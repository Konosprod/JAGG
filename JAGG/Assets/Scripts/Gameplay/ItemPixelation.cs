using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPixelation : Item {

    public bool inUse = false;
    public float time;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void Do()
    {
        inUse = true;
        StartCoroutine(Pixelation(time));
    }

    private IEnumerator Pixelation(float time)
    {
        player.Pixelation(time);
        yield return new WaitForEndOfFrame();
        Destroy(this.gameObject);
    }
}
