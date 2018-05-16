using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSliderSpeed : Item {

    public bool inUse;
    public float time;
    public int sliderSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void Do()
    {
        inUse = true;
        StartCoroutine(ChangeSliderSpeed(sliderSpeed, time));
    }

    public IEnumerator ChangeSliderSpeed(int sliderSpeed, float time)
    {
        player.ChangeSliderSpeed(sliderSpeed, time);
        yield return new WaitForEndOfFrame();
        Destroy(this.gameObject);
    }
}
