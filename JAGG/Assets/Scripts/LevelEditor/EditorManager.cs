using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour {


    public GameObject scrollviewContent;

    private GameObject[] prefabs;

    private GameObject currentPiece = null;
    private List<GameObject> piecesInPlace;
    private List<Vector3> usedPos;

	// Use this for initialization
	void Start () {
        piecesInPlace = new List<GameObject>();
        usedPos = new List<Vector3>();

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
    }
	
	// Update is called once per frame
	void Update () {

        int layerPlane = 1 << 30;
        RaycastHit rayHitPlane;
        Ray rayPlane = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayPlane, out rayHitPlane, Mathf.Infinity, layerPlane))
        {
            //Debug.Log("Hit the plane");
            // Only move the piece on the grid, ignore if the cursor is on any UI element
            if (currentPiece != null && !EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("hitPoint : " + rayHitPlane.point);
                // Round to the lowest even integer in order to move correctly on the grid
                int x = (int)Mathf.Floor(rayHitPlane.point.x + 1); // + 1 because of grid offset
                x = (x % 2 == 0) ? x : x-1;
                int z = (int)Mathf.Floor(rayHitPlane.point.z + 1);
                z = (z % 2 == 0) ? z : z-1;
                Vector3 pos = new Vector3(x, 0f, z);
                //Debug.Log("pos : " + pos);
                currentPiece.transform.position = pos;

                if (Input.GetMouseButtonDown(0) && !usedPos.Contains(pos))
                {
                    GameObject newPiece = Instantiate(currentPiece, pos, currentPiece.transform.rotation);
                    piecesInPlace.Add(newPiece);
                    usedPos.Add(pos);
                }
            }
        }

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
            }
            else
                Debug.LogError("Piece not found : " + pieceName);
        }
        else
            Debug.LogError("Empty string call");
    }
}
