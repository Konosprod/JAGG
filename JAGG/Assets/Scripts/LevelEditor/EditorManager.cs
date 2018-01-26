using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour {

    private int layerFloor;
    private int layerWall;

    // Specific pieces that go on top of other pieces and/or have a special layer due to their behaviour
    // Only instance to date are booster pads
    private int layerBoosterPad;

    // Which layers to use our Raycasts on when we waznt to select pieces
    private int layerMaskPieceSelection;


    public GameObject scrollviewContent;

    private GameObject[] prefabs;

    private GameObject currentPiece = null; // Piece that the player wants to place
    private GameObject selectedPieceInPlace = null; // Piece that was placed that the player wants to edit

    private List<GameObject> piecesInPlace; // List of pieces placed
    private List<Vector3> usedPos;          // List of positions used

	// Use this for initialization
	void Start () {
        piecesInPlace = new List<GameObject>();
        usedPos = new List<Vector3>();

        // Grab all prefabs previews
        prefabs = Resources.LoadAll<GameObject>("Prefabs/Terrain");
        foreach (GameObject pref in prefabs)
        {
            //Debug.Log(pref.name);
            //string preview = Application.dataPath + "/Resources/Previews/" + pref.name + "Preview.png";
            GameObject previewImage = new GameObject(pref.name);

            previewImage.AddComponent<RectTransform>();
            previewImage.AddComponent<LayoutElement>();

            Image pi_im = previewImage.AddComponent<Image>();
            pi_im.sprite = Resources.Load<Sprite>("Previews/" + pref.name + "Preview");

            UIPieceHandler uiph = previewImage.AddComponent<UIPieceHandler>();
            uiph.editorMan = this;

            previewImage.transform.SetParent(scrollviewContent.transform);
        }


        // Setup our layerMask
        layerFloor = LayerMask.NameToLayer("Floor");
        layerWall = LayerMask.NameToLayer("Wall");
        layerBoosterPad = LayerMask.NameToLayer("BoosterPad");
        layerMaskPieceSelection = (1 << layerFloor | 1 << layerWall | 1 << layerBoosterPad);
    }
	
	// Update is called once per frame
	void Update () {

        if (currentPiece != null)
        {
            // A piece was selected in the menu, the player can place copies

            // We use a raycast to find the plane (layer 30)
            RaycastHit rayHitPlane;
            Ray rayPlane = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayPlane, out rayHitPlane, Mathf.Infinity))
            {
                if (rayHitPlane.transform.gameObject.layer == 30)
                {
                    //Debug.Log("Hit the plane");

                    // Only move the piece on the grid, ignore if the cursor is on any UI element
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        //Debug.Log("hitPoint : " + rayHitPlane.point);
                        // Round to the lowest even integer in order to move correctly on the grid
                        int x = (int)Mathf.Floor(rayHitPlane.point.x + 1); // + 1 because of grid offset
                        x = (x % 2 == 0) ? x : x - 1;
                        int z = (int)Mathf.Floor(rayHitPlane.point.z + 1);
                        z = (z % 2 == 0) ? z : z - 1;
                        Vector3 pos = new Vector3(x, 0f, z);
                        //Debug.Log("pos : " + pos);
                        currentPiece.transform.position = pos;

                        // If you left-click and the position is free, place the piece
                        if (Input.GetMouseButtonDown(0) && isPositionValid(pos, currentPiece))
                        {
                            GameObject newPiece = Instantiate(currentPiece, pos, currentPiece.transform.rotation);
                            // Enable all colliders so that Raycasts do hit the piece
                            SetAllCollidersStatus(true, newPiece);
                            piecesInPlace.Add(newPiece);
                            usedPos.Add(pos);
                        }

                    }
                }
            }

            // Right-click allows to destroy the current piece
            if (Input.GetMouseButtonDown(1))
            {
                Destroy(currentPiece);
            }
        }
        else if (selectedPieceInPlace != null)
        {
            // The player clicked on a piece that was placed, he can edit it

        }
        else
        {
            // No piece selected, the player can click pieces in place to modify them or a piece in the menu to start placing them

            // We use a raycast to find the pieces
            RaycastHit rayHitPiece;
            Ray rayPiece = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayPiece, out rayHitPiece, Mathf.Infinity, layerMaskPieceSelection))
            {
                //Debug.Log("Hit a piece : " + rayHitPiece.transform.gameObject.name);
                GameObject piece = rayHitPiece.transform.gameObject;
                while (piece.transform.parent != null)
                {
                    piece = piece.transform.parent.gameObject;
                }

            }
        }
    }

    // Returns true if the piece can be placed on the specific position
    private bool isPositionValid(Vector3 pos, GameObject piece = null)
    {
        bool res = true;

        // Check if the position is empty (there are some exceptions like barrel roll that covers sevral spaces)
        res &= !usedPos.Contains(pos);

        return res;
    }

    public void clickOnPiece(string pieceName)
    {
        if (pieceName != "")
        {
            //Debug.Log(pieceName);

            GameObject piece = null;

            foreach(GameObject pref in prefabs)
            {
                if (pref.name == pieceName)
                    piece = pref;
            }

            if (piece != null)
            {
                Destroy(currentPiece);
                currentPiece = Instantiate(piece);
                // Disable all colliders so that Raycasts can go through the currentPiece
                SetAllCollidersStatus(false, currentPiece);
            }
            else
                Debug.LogError("Piece not found : " + pieceName);
        }
        else
            Debug.LogError("Empty string call");
    }

    // Enable / disable all colliders of gameobject and its children
    private void SetAllCollidersStatus(bool active, GameObject go)
    {
        foreach (Collider c in go.GetComponentsInChildren<Collider>())
        {
            c.enabled = active;
        }
    }
}
