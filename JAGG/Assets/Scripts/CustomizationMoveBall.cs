using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationMoveBall : MonoBehaviour
{

    private Vector3 p1, p2, p3, p4, start, end;
    float i, rate;

    void Start()
    {
        /*p1 = new Vector3(-2.5f, 0.055f, 3f);
        p2 = new Vector3(3f, 0.055f, 3f);*/
        p1 = new Vector3(2f, 0.055f, -1.5f);
        p2 = new Vector3(-2f, 0.055f, -1.5f);
        p3 = new Vector3(2f, 0.055f, -1.5f);
        p4 = new Vector3(-2f, 0.055f, -1.5f);
        start = p1;
        end = p2;
    }

    // Update is called once per frame
    void Update()
    {
        if(i>=1.0f)
        {
            if (start == p1)
            {
                start = p2;
                end = p3;
            }
            else if (start == p2)
            {
                start = p3;
                end = p4;
            }
            else if (start == p3)
            {
                start = p4;
                end = p1;
            }
            else
            {
                start = p1;
                end = p2;
            }
            i = 0f;
        }
        rate = 1.0f / 2.0f;
        i += Time.deltaTime * rate;
        transform.position = Vector3.Lerp(start, end, i);
    }

}
