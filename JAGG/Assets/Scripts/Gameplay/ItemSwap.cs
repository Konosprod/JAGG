using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSwap : Item {

    public bool inUse = false;
    public int target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void Do()
    {
        StartCoroutine(SwapPlayers(target));
    }

    private IEnumerator SwapPlayers(int target)
    {
        player.SwapPlayers(target);
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
}
