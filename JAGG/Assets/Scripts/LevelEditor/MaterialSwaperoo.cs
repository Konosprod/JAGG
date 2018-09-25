using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwaperoo : MonoBehaviour
{

    private Material swapMaterial;
    private Material[] originalMats;

    private MeshRenderer meshRend;

    // Use this for initialization
    void Awake()
    {
        swapMaterial = Resources.Load("Materials/GreyLevelEditor", typeof(Material)) as Material;
        meshRend = GetComponent<MeshRenderer>();
        originalMats = meshRend.materials;
    }

    public void SwapToGrey(bool grey)
    {
        if (grey)
        {
            Material[] mats = null;
            try
            {
                mats = meshRend.materials;
            }
            catch(System.NullReferenceException e)
            {
                Debug.Log("Exception : " + e.Message + ", error on object : " + gameObject.name + ", parent : " + transform.parent.gameObject.name);
                Debug.Break();
            }

            if (mats != null)
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = swapMaterial;
                }
                meshRend.materials = mats;
            }
        }
        else
        {
            meshRend.materials = originalMats;
        }

    }

    void OnDestroy()
    {
        foreach(Material m in meshRend.materials)
        {
            Destroy(m);
        }
        foreach (Material m in originalMats)
        {
            Destroy(m);
        }
    }

}
