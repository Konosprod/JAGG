using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTile : CustomScript {

    public List<GameObject> prefabsItems;

    void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;

        if(go.CompareTag("Player"))
        {
            GameObject item = prefabsItems[Random.Range(0, prefabsItems.Count - 1)];
            PlayerController controller = go.GetComponent<PlayerController>();

            if (controller != null)
                controller.AddItem(item);
        }
    }
}
