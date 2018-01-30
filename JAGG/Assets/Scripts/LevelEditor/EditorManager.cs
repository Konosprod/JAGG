using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{

    private int layerFloor;
    private int layerWall;

    // Specific pieces that go on top of other pieces and/or have a special layer due to their behaviour
    // Only instance to date are booster pads
    private int layerBoosterPad;

    // Which layers to use our Raycasts on when we waznt to select pieces
    private int layerMaskPieceSelection;


    public GameObject scrollviewContent;

    private static GameObject[] prefabs;

    private GameObject currentPiece = null; // Piece that the player wants to place
    public static List<GameObject> selectedPiecesInPlace; // Piece that was placed that the player wants to edit

    private List<GameObject> piecesInPlace; // List of pieces placed

    private UndoRedoStack<CommandParams> undoRedoStack = new UndoRedoStack<CommandParams>();
    private CommandParams currParams = null;

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


        // Setup our layerMask
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
                            currParams = undoRedoStack.Do(new AddPieceCommand(currentPiece, pos, currentPiece.transform.rotation), currParams);
                            // GameObject newPiece = Instantiate(currentPiece, pos, currentPiece.transform.rotation);
                            // Enable all colliders so that Raycasts do hit the piece
                            SetAllCollidersStatus(true, currParams.result);
                            piecesInPlace.Add(currParams.result);
                        }

                    }
                }
            }

            // Use R to rotate the piece 90 degrees clockwise
            if(Input.GetKeyDown(KeyCode.R))
            {
                currentPiece.transform.Rotate(new Vector3(0f, 90f, 0f));
            }

            // Right-click allows to destroy the current piece
            if (Input.GetMouseButtonDown(1))
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
                /*foreach(GameObject go in selectedPiecesInPlace)
                {
                    Destroy(go);
                }
                selectedPiecesInPlace.Clear();*/
                currParams = undoRedoStack.Do(new DeletePiecesCommand(), currParams);
            }


            // Use R to rotate the selected pieces 90 degrees clockwise
            if (Input.GetKeyDown(KeyCode.R))
            {
                currParams = undoRedoStack.Do(new RotateSelectedPiecesCommand(), currParams);
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
                    while (piece.transform.parent != null)
                    {
                        piece = piece.transform.parent.gameObject;
                    }

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
                    if(  !(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) )
                    {
                        currParams = undoRedoStack.Do(new DeselectAllPiecesCommand(), currParams);
                    }
                }
            }

        }
        else
        {
            // No piece selected, the player can click pieces in place to modify them or a piece in the menu to start placing them

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
                    while (piece.transform.parent != null)
                    {
                        piece = piece.transform.parent.gameObject;
                    }

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
        RaycastHit rayHitPiece;
        Ray rayPiece = Camera.main.ScreenPointToRay(pos);
        if (Physics.Raycast(rayPiece, out rayHitPiece, Mathf.Infinity, layerMaskPieceSelection))
        {
            res = false;
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
    public static void SetHighlight(bool active, GameObject go)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            OutlineRend outl = r.gameObject.GetComponent<OutlineRend>();
            if (outl == null)
            {
                outl = r.gameObject.AddComponent<OutlineRend>();
                outl.color = 2;
            }
            outl.enabled = active;
        }
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
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;

        // AddPieceCommand (stores the created piece)
        // SelectPieceCommand (parameter)
        // SelectSinglePieceCommand (parameter)
        // DeselectSinglePieceCommand (parameter)
        public GameObject result;

        // DeletePieceCommand (stores the pieces to restore if undo / delete if out of the redo stack)
        // RotateSelectedPiecesCommand
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
            foreach(GameObject go in selectedPiecesInPlace)
            {
                _CP.selectedPieces.Add(go);
            }
        }

        public CommandParams Do(CommandParams input = null)
        {
            foreach (GameObject go in _CP.selectedPieces)
            {
                go.SetActive(false);
                SetHighlight(false, go);
            }
            selectedPiecesInPlace.Clear();
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            foreach (GameObject go in _CP.selectedPieces)
            {
                go.SetActive(true);
                selectedPiecesInPlace.Add(go);
                SetHighlight(true, go);
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
            selectedPiecesInPlace.Add(_CP.result);
            SetHighlight(true, _CP.result);
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
            foreach(GameObject go in selectedPiecesInPlace)
            {
                _CP.selectedPieces.Add(go);
            }
        }

        public CommandParams Do(CommandParams input = null)
        {
            // Remove other pieces from the selection
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(false, go);
            }
            selectedPiecesInPlace.Clear();

            // Add the new piece to the selection
            selectedPiecesInPlace.Add(_CP.result);
            SetHighlight(true, _CP.result);
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
            selectedPiecesInPlace.Add(_CP.result);
            SetHighlight(true, _CP.result);
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
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(false, go);
            }
            selectedPiecesInPlace.Clear();
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



}
