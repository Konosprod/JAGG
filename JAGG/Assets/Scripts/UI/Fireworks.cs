using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireworks : MonoBehaviour {

    public ParticleSystem[] systems;
    public int maxShot;

    private int[] nbParticles;
    private int[] nbShots;


	// Use this for initialization
	void Start () {

        nbParticles = new int[systems.Length];
        nbShots = new int[systems.Length];

	}
	
	// Update is called once per frame
	void Update () {

        for(int i = 0; i < systems.Length; i++)
        {
            int count = systems[i].particleCount;

            //When died
            if(count < nbParticles[i])
            {
                nbShots[i]++;
                if (nbShots[i] < maxShot)
                    systems[i].Emit(1);
                    
                SoundManager._instance.PlaySFX(SFXType.RocketExplosion);
            }

            //When birth
            if(count > nbParticles[i])
            {
                SoundManager._instance.PlaySFX(SFXType.Rocket);
            }

            nbParticles[i] = count;
        }

	}
}
