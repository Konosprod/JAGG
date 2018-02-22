using UnityEngine.UI;
using UnityEngine;

public class PanelJoin : MonoBehaviour {

    public PanelCreate panelCreate;
    public Button buttonJoin;
    public Button buttonReturn;
    public Button buttonCreate;
    public PortInputField portInputCreate;
    public IPInputField ipInput;
    public PortInputField portInput;
    public Text statusText;

    public bool shouldCheckInputs = true;

    public float animationSpeed = 3f;
    private float elapsed = 0f;

    private bool isConnecting = false;
    private int nbDots = 0;

	// Use this for initialization
	void Start () {
        shouldCheckInputs = true;
        buttonJoin.interactable = false;
	}
	
	// Update is called once per frame
	void Update () {
  
        if(isConnecting)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= animationSpeed)
            {
                string dots = "";

                if (nbDots == 4)
                {
                    dots = "";
                    nbDots = 0;
                }
                else
                {
                    for (int i = 0; i < nbDots; i++)
                    {
                        dots += ".";
                    }

                    nbDots++;
                }

                statusText.text = "Connecting" + dots;

                elapsed = 0f;
            }
        }
        else
        {
            if(shouldCheckInputs)
                buttonJoin.interactable = (ipInput.isValid && portInput.isValid);
        }
	}

    void OnEnable()
    {
        statusText.text = "";
        isConnecting = false;
        nbDots = 0;
        elapsed = 0f;

        buttonReturn.interactable = true;
        buttonCreate.interactable = true;

        ipInput.inputField.interactable = true;
        portInput.inputField.interactable = true;
        portInputCreate.inputField.interactable = true;
    }

    public void Connecting()
    {
        panelCreate.shouldCheckInputs = false;
        buttonJoin.interactable = false;
        buttonReturn.interactable = false;
        buttonCreate.interactable = false;

        ipInput.inputField.interactable = false;
        portInput.inputField.interactable = false;
        portInputCreate.inputField.interactable = false;

        SetStatus("Connecting", Color.white);

        isConnecting = true;
    }

    public void Error()
    {
        panelCreate.shouldCheckInputs = true;
        buttonJoin.interactable = true;
        buttonReturn.interactable = true;
        buttonCreate.interactable = true;

        ipInput.inputField.interactable = true;
        portInput.inputField.interactable = true;
        portInputCreate.inputField.interactable = true;

        SetStatus("Error while connecting the host", Color.red);

        isConnecting = false;
    }

    public void SetStatus(string status, Color color)
    {
        statusText.color = color;
        statusText.text = status;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
