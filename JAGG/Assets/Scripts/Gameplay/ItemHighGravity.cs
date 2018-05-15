﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemHighGravity : Item {

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
        StartCoroutine(ChangeGravity(time));
    }

    private IEnumerator ChangeGravity(float time)
    {
        player.ChangeGravity(GravityType.High);
        yield return new WaitForSeconds(time);
        player.ResetGravity();
        Destroy(this.gameObject);
    }
}
