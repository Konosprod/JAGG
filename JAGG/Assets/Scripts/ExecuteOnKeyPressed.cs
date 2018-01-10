using UnityEngine;

public class ExecuteOnKeyPressed : MonoBehaviour {

    public KeyCode keyPressed;
    public UnityEngine.Events.UnityEvent callbacks;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(keyPressed))
        {
            callbacks.Invoke();
        }
	}
}
