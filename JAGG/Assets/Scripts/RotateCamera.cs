using UnityEngine;

public class RotateCamera : MonoBehaviour {

    public float angle = 20.0f;
    public float speed = 10.0f;

    public bool onPoint = false;

    public GameObject target;
    public Vector3 point;

    private Vector3 pivot;

	// Use this for initialization
	void Start () {

		if(onPoint)
        {
            pivot = point;
        }
        else
        {
            pivot = target.transform.position;
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(pivot);
        transform.RotateAround(pivot, Vector3.up, speed * angle * Time.deltaTime);
	}
}