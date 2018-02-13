using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    private int layerPlane;
    private int layerFloor;
    private int layerWall;

    // Specific pieces that go on top of other pieces and/or have a special layer due to their behaviour
    // Only instance to date are booster pads
    private int layerBoosterPad;

    // Which layers to use our Raycasts on when we waznt to select pieces
    private int layerMaskPieceSelection;


    public GameObject scrollviewContent;
    public GameObject grid;
    public GameObject plane;
    private static GameObject gridGO;
    private static GameObject planeGO;

    private static GameObject[] prefabs;

    private GameObject currentPiece = null; // Piece that the player wants to place
    private static GameObject originPiece = null;  // Piece that is used as the origin for the grid
    public static List<GameObject> selectedPiecesInPlace; // Piece that was placed that the player wants to edit

    private List<GameObject> piecesInPlace; // List of pieces placed

    private UndoRedoStack<CommandParams> undoRedoStack = new UndoRedoStack<CommandParams>();
    private CommandParams currParams = null;

    private Vector3 targetNormal = Vector3.up;

    private static float
        offsetGridX = 0f,
        offsetGridY = 0f,
        offsetGridZ = 0f;

    // Use this for initialization
    void Start()
    {
        piecesInPlace = new List<GameObject>();
        selectedPiecesInPlace = new List<GameObject>();

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

        // GameObjects
        gridGO = grid;
        planeGO = plane;

        // Setup our layerMask
        layerPlane = LayerMask.NameToLayer("LevelEditor");
        layerFloor = LayerMask.NameToLayer("Floor");
        layerWall = LayerMask.NameToLayer("Wall");
        layerBoosterPad = LayerMask.NameToLayer("BoosterPad");
        layerMaskPieceSelection = (1 << layerFloor | 1 << layerWall | 1 << layerBoosterPad);
    }

    // Update is called once per frame
    void Update()
    {

        // Handle ctrl + Z
        // Unity editor will catch the ctrl + Z so Z alone will be the input for the editor
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z))
#else
        if((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Z))
#endif
        {
            currParams = undoRedoStack.Undo(currParams);
        }

        // Handle ctrl + Y
        // Unity editor will catch the ctrl + Y so Y alone will be the input for the editor
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Y))
#else
        if((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Y))
#endif
        {
            currParams = undoRedoStack.Redo(currParams);
        }


        if (currentPiece != null)
        {
            // A piece was selected in the menu, the player can place copies
            float x,y,z = 0f;

            // If the selected is a prefab or a simple floor
            if (currentPiece.layer != layerBoosterPad && currentPiece.layer != layerWall)
            {
                // We use a raycast to find the plane (layerPlane)
                RaycastHit rayHitPlane;
                Ray rayPlane = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(rayPlane, out rayHitPlane, Mathf.Infinity))
                {
                    if (rayHitPlane.transform.gameObject.layer == layerPlane)
                    {
                        //Debug.Log("Hit the plane");

                        // Only move the piece on the grid, ignore if the cursor is on any UI element
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            //Debug.Log("hitPoint : " + rayHitPlane.point);
                            // Round to the lowest even integer in order to move correctly on the grid
                            x = Mathf.Floor(rayHitPlane.point.x + 1 - offsetGridX); // + 1 because of grid offset (It is placed on (-1;0;-1))
                            x = (x % 2 == 0) ? x : x - 1;
                            x += offsetGridX;
                            y = rayHitPlane.point.y;
                            z = Mathf.Floor(rayHitPlane.point.z + 1 - offsetGridZ);
                            z = (z % 2 == 0) ? z : z - 1;
                            z += offsetGridZ;
                            Vector3 pos = new Vector3(x, y, z);
                            //Debug.Log("pos : " + pos);
                            targetNormal = Vector3.up;
                            currentPiece.transform.eulerAngles = new Vector3(0f, currentPiece.transform.eulerAngles.y, 0f);
                            currentPiece.transform.position = pos;

                            // If you left-click and the position is free, place the piece
                            if (Input.GetMouseButtonDown(0) && isPositionValid(pos, currentPiece))
                            {
                                currParams = undoRedoStack.Do(new AddPieceCommand(currentPiece, pos, currentPiece.transform.rotation), currParams);
                                // GameObject newPiece = Instantiate(currentPiece, pos, currentPiece.transform.rotation);
                                // Enable all colliders so that Raycasts do hit the piece
                                SetAllCollidersStatus(true, currParams.result);
                                piecesInPlace.Add(currParams.result);
                            }

                        }
                    }
                }
            }
            else if (currentPiece.layer == layerWall)
            {

            }
            else if (currentPiece.layer == layerBoosterPad)
            {
                // The booster pads go on top of the floor pieces so we use a raycast to find them
                RaycastHit rayHit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out rayHit, Mathf.Infinity))
                {
                    if (rayHit.transform.gameObject.layer == layerFloor) // If we hit a floor we put the BoosterPad on top
                    {
                        //Debug.Log("Hit a floor");

                        // Only move the piece on the grid, ignore if the cursor is on any UI element
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            //Debug.Log("hitPoint : " + rayHitPlane.point);
                            // Round to the lowest even integer in order to move correctly on the grid
                            x = Mathf.Floor(rayHit.point.x + 1 - offsetGridX); // + 1 because of grid offset (It is placed on (-1;0;-1))
                            x = (x % 2 == 0) ? x : x - 1;
                            x += offsetGridX;
                            y = rayHit.point.y + 0.001f;
                            z = Mathf.Floor(rayHit.point.z + 1 - offsetGridZ);
                            z = (z % 2 == 0) ? z : z - 1;
                            z += offsetGridZ;
                            Vector3 pos = new Vector3(x, rayHit.point.y+0.001f, z);
                            if (rayHit.transform.gameObject.name == "Slope")
                            {
                                y = 0.391f;
                                pos.y = 0.391f;
                            }

                            // Align the booster pad alongside the piece
                            if (rayHit.normal != targetNormal)
                            {
                                targetNormal = rayHit.normal;
                                currentPiece.transform.rotation = Quaternion.FromToRotation(Vector3.up, rayHit.normal);
                            }
                            //Debug.Log("pos : " + pos);
                            currentPiece.transform.position = pos;

                           
                            // If you left-click and the position is free, place the piece
                            if (Input.GetMouseButtonDown(0) && isPositionValid(pos, currentPiece))
                            {
                                currParams = undoRedoStack.Do(new AddPieceCommand(currentPiece, pos, currentPiece.transform.rotation), currParams);
                                // GameObject newPiece = Instantiate(currentPiece, pos, currentPiece.transform.rotation);
                                // Enable all colliders so that Raycasts do hit the piece
                                SetAllCollidersStatus(true, currParams.result);
                                piecesInPlace.Add(currParams.result);
                            }

                        }
                    }
                    else if (rayHit.transform.gameObject.layer == layerPlane) // If we hit the plane we move along the grid
                    {
                        // Only move the piece on the grid, ignore if the cursor is on any UI element
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            //Debug.Log("hitPoint : " + rayHitPlane.point);
                            // Round to the lowest even integer in order to move correctly on the grid
                            x = Mathf.Floor(rayHit.point.x + 1 - offsetGridX); // + 1 because of grid offset (It is placed on (-1;0;-1))
                            x = (x % 2 == 0) ? x : x - 1;
                            x += offsetGridX;
                            y = rayHit.point.y;
                            z = Mathf.Floor(rayHit.point.z + 1 - offsetGridZ);
                            z = (z % 2 == 0) ? z : z - 1;
                            z += offsetGridZ;
                            Vector3 pos = new Vector3(x, y, z);
                            //Debug.Log("pos : " + pos);
                            targetNormal = Vector3.up;
                            currentPiece.transform.eulerAngles = new Vector3(0f, currentPiece.transform.eulerAngles.y, 0f);
                            currentPiece.transform.position = pos;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Unknown layer : " + currentPiece.layer + ", for piece : " +currentPiece.name);
            }

            // Use R to rotate the piece 90 degrees clockwise
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentPiece.transform.Rotate(currentPiece.transform.up, 90f, Space.World);
            }

            // The D allows to destroy the current piece
            if (Input.GetKeyDown(KeyCode.D))
            {
                Destroy(currentPiece);
            }
        }
        else if (selectedPiecesInPlace.Count > 0)
        {
            // The player clicked on a piece that was placed, he can edit it
            // He can also select other pieces or select a part of the piece (like a single wall to disable or stuff)

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                currParams = undoRedoStack.Do(new DeletePiecesCommand(), currParams);
            }


            // Use R to rotate the selected pieces 90 degrees clockwise
            if (Input.GetKeyDown(KeyCode.R))
            {
                currParams = undoRedoStack.Do(new RotateSelectedPiecesCommand(), currParams);
            }

            // Use O to define the piece as the origin of the grid (offset the grid to align the pieces correctly)
            if(selectedPiecesInPlace.Count == 1)
            {
                if (Input.GetKeyDown(KeyCode.O))
                {
                    currParams = undoRedoStack.Do(new UsePieceAsOriginCommand(), currParams);
                }
            }

            // Click alone selects a single piece while deselecting other pieces
            // Use shift + click to select additional pieces
            if (Input.GetMouseButtonDown(0))
            {
                // We use a raycast to find the pieces
                RaycastHit rayHitPiece;
                Ray rayPiece = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(rayPiece, out rayHitPiece, Mathf.Infinity, layerMaskPieceSelection))
                {
                    //Debug.Log("Hit a piece : " + rayHitPiece.transform.gameObject.name);
                    GameObject piece = rayHitPiece.transform.gameObject;

                    // Holding ctrl allows to select a specific part of the prefab while a simple click will select the parent prefab GameObject
                    //if (!(Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
                    //{
                    while (piece.transform.parent != null)
                    {
                        piece = piece.transform.parent.gameObject;
                    }
                    //}

                    // Debug.Log(piece.name);

                    // Add to the selection or remove from the selection if the piece was already selected
                    if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
                    {
                        if (selectedPiecesInPlace.Find(x => x.Equals(piece)))
                        {
                            currParams = undoRedoStack.Do(new DeselectSinglePieceCommand(piece), currParams);
                        }
                        else
                        {
                            currParams = undoRedoStack.Do(new SelectPieceCommand(piece), currParams);
                        }
                    }
                    else
                    {
                        currParams = undoRedoStack.Do(new SelectSinglePieceCommand(piece), currParams);
                    }
                }
                else
                {
                    // Click on an empty space => deselect all pieces
                    // If the player is shifting, we tolerate clicks on empty spaces (to avoid ruining multi-selection)
                    if (!(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
                    {
                        currParams = undoRedoStack.Do(new DeselectAllPiecesCommand(), currParams);
                    }
                }
            }

        }
        else
        {
            // No piece selected, the player can click pieces in place to modify them or a piece in the menu to start placing them


            // Use O to define the piece as the origin of the grid (offset the grid to align the pieces correctly)
            if (Input.GetKeyDown(KeyCode.O))
            {
                currParams = undoRedoStack.Do(new ResetOriginCommand(), currParams);
            }


            // Use left-click to select
            if (Input.GetMouseButtonDown(0))
            {
                // We use a raycast to find the pieces
                RaycastHit rayHitPiece;
                Ray rayPiece = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(rayPiece, out rayHitPiece, Mathf.Infinity, layerMaskPieceSelection))
                {
                    //Debug.Log("Hit a piece : " + rayHitPiece.transform.gameObject.name);
                    GameObject piece = rayHitPiece.transform.gameObject;

                    // Holding ctrl allows to select a specific part of the prefab while a simple click will select the parent prefab GameObject
                    //if (!(Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
                    //{
                    while (piece.transform.parent != null)
                    {
                        piece = piece.transform.parent.gameObject;
                    }
                    //}

                    // Debug.Log(piece.name);

                    // We highlight the selected piece
                    currParams = undoRedoStack.Do(new SelectPieceCommand(piece), currParams);
                }
            }
        }
    }

    // Returns true if the piece can be placed on the specific position
    private bool isPositionValid(Vector3 pos, GameObject piece = null)
    {
        bool res = true;

        // Check if the position is empty
        // We use a raycast to find the pieces
        if (piece.layer != layerBoosterPad && piece.layer != layerWall)
        {
            RaycastHit rayHitPiece;
            Ray rayPiece = Camera.main.ScreenPointToRay(pos);
            if (Physics.Raycast(rayPiece, out rayHitPiece, Mathf.Infinity, layerMaskPieceSelection))
            {
                res = false;
            }
        }

        return res;
    }

    public void clickOnPiece(string pieceName)
    {
        if (pieceName != "")
        {
            //Debug.Log(pieceName);

            GameObject piece = null;

            foreach (GameObject pref in prefabs)
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
    public static void SetAllCollidersStatus(bool active, GameObject go)
    {
        foreach (Collider c in go.GetComponentsInChildren<Collider>())
        {
            c.enabled = active;
        }
    }

    // Activate/Disable the outline effect on object (will add the component if the object doesn't have it)
    // As a side effect of selection handling it is essential to respect the following rules :
    //      1) If you SetHighlight to true, you must add the GameObject to SelectedPiecesInPlace AFTER the call to SetHighlight
    //      2) When you SetHighlight to false, it's the other way around so you must remove the GameObject from SelectedPiecesInPlace BEFORE the call to SetHighlight
    //      If a piece isn't/stays selected when it should/n't then there is probably a call going in the wrong order
    // Side note on highlight colors : 
    //      1) We can only have 3 different colors at once
    //      2) Color 2 is for a selected piece (blue)
    //      3) Color 1 is for a piece that is both selected and the origin for the grid (pink/purple)
    //      4) Color 0 is for a non-selected piece that is the origin (red)
    public static void SetHighlight(bool active, GameObject go, int outlColor = 2)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            // Only change the highlight if the subpiece is not part of selectedPiecesInPlace (which means it has been individually selected then selected as part of its prefab)
            if (!selectedPiecesInPlace.Find(x => x.Equals(r.gameObject)))
            {
                OutlineRend outl = r.gameObject.GetComponent<OutlineRend>();
                if (outl == null)
                {
                    // Add the component when needed
                    outl = r.gameObject.AddComponent<OutlineRend>();
                }

                if (originPiece == go)
                {
                    // Special cases for the origin
                    // If active = false, it means we are deselecting the origin piece => we keep the highlight and change to color 0
                    // else, it means we are selecting the origin piece (individually or not) => we change from color 0 to 1
                    outl.color = (active)?1:0;
                }
                else
                {
                    outl.color = outlColor;
                    outl.enabled = active;
                }
            }
        }
    }

    // Moves the grid to align with the end of the selected piece
    // It takes the calculated offset as parameter and moves the grid accordingly
    // The planeGO is what go beneath the grid to catch RayCasts
    private static void SetGridOffset(float offX = 0f, float offY = 0f, float offZ = 0f)
    {
        gridGO.transform.position += new Vector3(offX - offsetGridX, offY - offsetGridY, offZ - offsetGridZ);
        planeGO.transform.position += new Vector3(offX - offsetGridX, offY - offsetGridY, offZ - offsetGridZ);
        offsetGridX = offX;
        offsetGridY = offY;
        offsetGridZ = offZ;
    }

    #region Undo/Redo Stack
    public interface ICommand<T>
    {
        T Do(T input);
        T Undo(T input);
    }

    public class CommandParams
    {
        // AddPieceCommand (parameters)
        // UsePieceAsOriginCommand (position stores the offset to reach, prefab stores the piece for outline effect)
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;

        // UsePieceAsOriginCommand (position stores the previous offset for the undo) 
        public Vector3 offset;

        // AddPieceCommand (stores the created piece)
        // SelectPieceCommand (parameter)
        // SelectSinglePieceCommand (parameter)
        // DeselectSinglePieceCommand (parameter)
        // UsePieceAsOriginCommand (stores the previous origin piece, can be null)
        public GameObject result;

        // DeletePieceCommand (stores the pieces to restore if undo / delete if out of the redo stack)
        // RotateSelectedPiecesCommand (parameter)
        // SelectSinglePieceCommand (stores the pieces that were deselected)
        // DeselectAllPiecesCommand (stores the pieces that were deselected)
        public List<GameObject> selectedPieces = new List<GameObject>();
    }

    // Adding a piece
    public class AddPieceCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public AddPieceCommand(GameObject pr, Vector3 pos, Quaternion rot)
        {
            // Remove the (Clone) from the name
            string prName = pr.name.Split('(')[0];
            // We must find the reference in prefabs because the pr from the parameters is an instance used temporarily to display at the mouse cursor position (currentPiece)
            // This way the ctrl + Z will work even if the player selects another piece
            foreach (GameObject pref in prefabs)
            {
                if (pref.name == prName)
                {
                    _CP.prefab = pref;
                }
            }
            _CP.position = pos;
            _CP.rotation = rot;
            _CP.result = null;
        }

        public CommandParams Do(CommandParams input = null)
        {
            if (_CP.result == null)
            {
                _CP.result = Instantiate(_CP.prefab, _CP.position, _CP.rotation);
            }
            else
            {
                _CP.result.SetActive(true);
            }
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            _CP.result.SetActive(false);
            return _CP;
        }
    }

    // Deletes the selected pieces
    public class DeletePiecesCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public DeletePiecesCommand()
        {
            foreach (GameObject go in selectedPiecesInPlace)
            {
                _CP.selectedPieces.Add(go);
            }
        }

        public CommandParams Do(CommandParams input = null)
        {
            selectedPiecesInPlace.Clear();
            foreach (GameObject go in _CP.selectedPieces)
            {
                go.SetActive(false);
                SetHighlight(false, go);
            }
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            foreach (GameObject go in _CP.selectedPieces)
            {
                go.SetActive(true);
                SetHighlight(true, go);
                selectedPiecesInPlace.Add(go);
            }
            return _CP;
        }
    }

    // Add a piece to the selected pieces
    public class SelectPieceCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public SelectPieceCommand(GameObject sel)
        {
            _CP.result = sel;
        }

        public CommandParams Do(CommandParams input = null)
        {
            SetHighlight(true, _CP.result);
            selectedPiecesInPlace.Add(_CP.result);
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            selectedPiecesInPlace.Remove(_CP.result);
            SetHighlight(false, _CP.result);
            return _CP;
        }
    }

    // Select a single piece (deselects other pieces)
    public class SelectSinglePieceCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public SelectSinglePieceCommand(GameObject sel)
        {
            _CP.result = sel;
            foreach (GameObject go in selectedPiecesInPlace)
            {
                _CP.selectedPieces.Add(go);
            }
        }

        public CommandParams Do(CommandParams input = null)
        {
            // Remove other pieces from the selection
            selectedPiecesInPlace.Clear();
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(false, go);
            }

            // Add the new piece to the selection
            SetHighlight(true, _CP.result);
            selectedPiecesInPlace.Add(_CP.result);
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            // Remove the piece from the selection
            selectedPiecesInPlace.Remove(_CP.result);
            SetHighlight(false, _CP.result);

            // Put the former pieces back in the selection
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(true, go);
                selectedPiecesInPlace.Add(go);
            }
            return _CP;
        }
    }

    // Deselect a single piece
    public class DeselectSinglePieceCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public DeselectSinglePieceCommand(GameObject sel)
        {
            _CP.result = sel;
        }

        public CommandParams Do(CommandParams input = null)
        {
            // Remove the piece from the selection
            selectedPiecesInPlace.Remove(_CP.result);
            SetHighlight(false, _CP.result);
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            // Add the piece to the selection
            SetHighlight(true, _CP.result);
            selectedPiecesInPlace.Add(_CP.result);
            return _CP;
        }
    }

    // Deselect all pieces
    public class DeselectAllPiecesCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public DeselectAllPiecesCommand()
        {
            foreach (GameObject go in selectedPiecesInPlace)
            {
                _CP.selectedPieces.Add(go);
            }
        }

        public CommandParams Do(CommandParams input = null)
        {
            // Remove all pieces from the selection
            selectedPiecesInPlace.Clear();
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(false, go);
            }
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            // Put the pieces back in the selection
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(true, go);
                selectedPiecesInPlace.Add(go);
            }
            return _CP;
        }
    }

    // Rotate the selected pieces (clockwise 90 degrees)
    public class RotateSelectedPiecesCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public RotateSelectedPiecesCommand()
        {
            foreach (GameObject go in selectedPiecesInPlace)
            {
                _CP.selectedPieces.Add(go);
            }
        }

        public CommandParams Do(CommandParams input = null)
        {
            foreach (GameObject go in _CP.selectedPieces)
            {
                go.transform.Rotate(new Vector3(0f, 90f, 0f));
            }
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            foreach (GameObject go in _CP.selectedPieces)
            {
                go.transform.Rotate(new Vector3(0f, -90f, 0f));
            }
            return _CP;
        }
    }

    // Use the selected piece as origin for the grid
    public class UsePieceAsOriginCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public UsePieceAsOriginCommand()
        {
            GameObject referencePiece = selectedPiecesInPlace[0];
            _CP.prefab = referencePiece;
            _CP.result = originPiece;

            float offX = Mathf.Repeat(referencePiece.transform.position.x, 2f), 
                  offY = referencePiece.transform.position.y, 
                  offZ = Mathf.Repeat(referencePiece.transform.position.z, 2f);

            // Remove the (Clone) from the name
            string pName = referencePiece.name.Split('(')[0];

            Vector3 off = new Vector3();

            // Pieces with specific offset
            if (pName == "BarrelRoll" || pName == "StartBarrelRoll")
            {
                off = new Vector3(-1.38f, 0f, 1.3f);
            }
            else if (pName == "Looping" || pName == "AirReceptionBoosterLooping")
            {
                off = new Vector3(0f, 0f, 0.4f);
            }
            else if (pName == "Slope" || pName == "SlopeNoWall")
            {
                off = new Vector3(0f, 0.57875f, 0f);
            }

            if (off != Vector3.zero)
            {
                // Apply the rotation of the piece to get the right offset
                off = Quaternion.Euler(referencePiece.transform.eulerAngles) * off;
                offX += off.x;
                offY += off.y;
                offZ += off.z;
            }

            _CP.position = new Vector3(offX, offY, offZ);
            _CP.offset = new Vector3(offsetGridX, offsetGridY, offsetGridZ);
        }

        public CommandParams Do(CommandParams input = null)
        {
            originPiece = _CP.prefab;
            SetGridOffset(_CP.position.x, _CP.position.y, _CP.position.z);
            SetHighlight(true, _CP.prefab, 1);
            if(_CP.result != null)
                SetHighlight(false, _CP.result);
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            SetGridOffset(_CP.offset.x, _CP.offset.y, _CP.offset.z);
            if (_CP.result != null)
                SetHighlight(true, _CP.result, 0);
            originPiece = _CP.result;
            SetHighlight(true, _CP.prefab);
            return _CP;
        }
    }

    // Reset the origin of the grid
    public class ResetOriginCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public ResetOriginCommand()
        {
            _CP.offset = new Vector3(offsetGridX, offsetGridY, offsetGridZ);
            _CP.prefab = originPiece;
        }

        public CommandParams Do(CommandParams input = null)
        {
            // Default call will reset the grid
            SetGridOffset();
            originPiece = null;
            SetHighlight(false, _CP.prefab);
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            SetGridOffset(_CP.offset.x, _CP.offset.y, _CP.offset.z);
            SetHighlight(true, _CP.prefab, 0);
            originPiece = _CP.prefab;
            return _CP;
        }
    }


    public class UndoRedoStack<T>
    {
        private Stack<ICommand<T>> _Undo;
        private Stack<ICommand<T>> _Redo;

        public int UndoCount
        {
            get
            {
                return _Undo.Count;
            }
        }
        public int RedoCount
        {
            get
            {
                return _Redo.Count;
            }
        }

        public UndoRedoStack()
        {
            Reset();
        }
        public void Reset()
        {
            _Undo = new Stack<ICommand<T>>();
            _Redo = new Stack<ICommand<T>>();
        }

        public T Do(ICommand<T> cmd, T input)
        {
            T output = cmd.Do(input);
            _Undo.Push(cmd);
            _Redo.Clear(); // Once we issue a new command, the redo stack clears
            return output;
        }
        public T Undo(T input)
        {
            if (_Undo.Count > 0)
            {
                ICommand<T> cmd = _Undo.Pop();
                T output = cmd.Undo(input);
                _Redo.Push(cmd);
                return output;
            }
            else
            {
                return input;
            }
        }
        public T Redo(T input)
        {
            if (_Redo.Count > 0)
            {
                ICommand<T> cmd = _Redo.Pop();
                T output = cmd.Do(input);
                _Undo.Push(cmd);
                return output;
            }
            else
            {
                return input;
            }
        }

    }



    #endregion


    private float ClampAngle(float angle, float min = 0f, float max = 360f)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }


}
