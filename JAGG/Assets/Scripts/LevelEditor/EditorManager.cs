using cakeslice;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    private static int layerPlane;
    private static int layerFloor;
    private static int layerWall;
    private static int layerDecor;

    // Specific pieces that go on top of other pieces and/or have a special layer due to their behaviour
    // Only instance to date are booster pads
    private static int layerOverlay;

    // Which layers to use our Raycasts on when we waznt to select pieces
    private static int layerMaskPieceSelection;

    // Required things in the scene
    //public GameObject scrollviewContent;
    public ListPrefabPanel listPrefabPanel;
    public PanelHoleProperties panelHoleProperties;
    public SaveBeforeExit saveBeforeExit;
    public GameObject grid;
    public GameObject plane;
    private static GameObject gridGO;
    private static GameObject planeGO;
    public TestMode testMode;
    public Dropdown holeSelection;

    // Contains the list of terrain prefabs
    private static Dictionary<string, GameObject> prefabs;
    //private static GameObject[] prefabs;

    public GameObject prefabSpawnPoint;
    public GameObject prefabSpawnPointNoNetworkStart;
    public GameObject prefabLevelProperties;

    private static int currentHole = 0; // The hole that the player is currently editing
    private const int maxHoles = 18;

    public GameObject holesObject = null;
    private static GameObject currentHoleObject = null;

    private GameObject currentPiece = null; // Piece that the player wants to place
    private static GameObject originPiece = null;  // Piece that is used as the origin for the grid
    public static List<GameObject> selectedPiecesInPlace; // Piece that was placed that the player wants to edit

    private static GameObject[] spawnPoints = new GameObject[maxHoles]; // SpawnPoints of the holes
    private static GameObject[] levelsProperties = new GameObject[maxHoles]; // levelProperties of the holes
    private static List<GameObject>[] piecesInPlace; // List of pieces placed

    private UndoRedoStack<CommandParams> undoRedoStack = new UndoRedoStack<CommandParams>();
    private CommandParams currParams = null;

    private Vector3 targetNormal = Vector3.up;

    private static float
        offsetGridX = 0f,
        offsetGridY = 0f,
        offsetGridZ = 0f;


    [Header("Piece Information")]
    public GameObject infoPanel;
    public Text pieceNameText;
    // Position of the piece (transform.position)
    public Toggle positionToggle;
    public InputField inputPosX;
    public InputField inputPosY;
    public InputField inputPosZ;
    // Rotation of the piece (transform.quaternion)
    public Toggle rotationToggle;
    public InputField inputRotX;
    public InputField inputRotY;
    public InputField inputRotZ;
    // Spinning piece (RotatePiece)
    public Toggle spinningPieceToggle;
    public InputField inputSpinTime;
    public InputField inputSpinPauseTime;
    public InputField inputSpinNbRota;
    // Moving piece (MovingPiece)
    public Toggle movingPieceToggle;
    public InputField inputMoveDestX;
    public InputField inputMoveDestY;
    public InputField inputMoveDestZ;
    public InputField inputTravelTime;
    public InputField inputMovePauseTime;

    [Header("Gizmo")]
    public GizmoRotateScript gizmoRotate;
    public GizmoScaleScript gizmoScale;
    public GizmoTranslateScript gizmoTranslate;
    public Camera gizmoCamera;

    private static GameObject _infoPanel;
    private static Text _pieceNameText;
    // Position of the piece (transform.position)
    private static Toggle _positionToggle;
    private static InputField _inputPosX;
    private static InputField _inputPosY;
    private static InputField _inputPosZ;
    // Rotation of the piece (transform.quaternion)
    private static Toggle _rotationToggle;
    private static InputField _inputRotX;
    private static InputField _inputRotY;
    private static InputField _inputRotZ;
    // Spinning piece (RotatePiece)
    private static Toggle _spinningPieceToggle;
    private static InputField _inputSpinTime;
    private static InputField _inputSpinPauseTime;
    private static InputField _inputSpinNbRota;
    // Moving piece (MovingPiece)
    private static Toggle _movingPieceToggle;
    private static InputField _inputMoveDestX;
    private static InputField _inputMoveDestY;
    private static InputField _inputMoveDestZ;
    private static InputField _inputTravelTime;
    private static InputField _inputMovePauseTime;
    //Gizmo things
    private static GizmoRotateScript _gizmoRotate;
    private static GizmoScaleScript _gizmoScale;
    private static GizmoTranslateScript _gizmoTranslate;

    private const float epsilon = 0.0001f;

    private bool flagNoUndoDeselect = false;

    private static LevelEditorMovingPieceManager lemvpManager;

    [Header("Custom Level")]
    public CustomLevelLoader loader;
    public PanelExport panelExport;

    [Header("Escape Menu")]
    public EscapeMenu escapeMenu;

    [HideInInspector]
    public bool canEdit = true;
    [HideInInspector]
    public static bool isModified = false;


    // Handle box selection
    private bool isBoxSelection = false;
    private bool isBoxSelectionActive = false;
    private Vector3 mousePositionStart = Vector3.zero;
    // Used to draw the rectangle
    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    // Use this for initialization
    void Start()
    {
        SetupHoles();
        prefabs = new Dictionary<string, GameObject>();
        // Grab all prefabs previews
        //prefabs = Resources.LoadAll<GameObject>("Prefabs/Terrain");

        foreach (GameObject pref in Resources.LoadAll<GameObject>("Prefabs/Terrain"))
        {
            prefabs.Add(pref.name, pref);
            // /!\ CHANGES THE PREFAB ITSELF /!\
            foreach (Renderer r in pref.GetComponentsInChildren<Renderer>())
            {
                if (!(r is UnityEngine.ParticleSystemRenderer) && r.gameObject.GetComponent<MaterialSwaperoo>() == null)
                    r.gameObject.AddComponent<MaterialSwaperoo>();
            }

            listPrefabPanel.AddPiece(pref);

            /*//Debug.Log(pref.name);
            //string preview = Application.dataPath + "/Resources/Previews/" + pref.name + "Preview.png";
            GameObject previewImage = new GameObject(pref.name);

            previewImage.AddComponent<RectTransform>();
            previewImage.AddComponent<LayoutElement>();

            Image pi_im = previewImage.AddComponent<Image>();
            pi_im.sprite = Resources.Load<Sprite>("Previews/" + pref.name + "Preview");

            UIPieceHandler uiph = previewImage.AddComponent<UIPieceHandler>();
            uiph.editorMan = this;

            previewImage.transform.SetParent(scrollviewContent.transform);*/
        }

        // GameObjects
        gridGO = grid;
        planeGO = plane;

        // Setup our layerMask
        layerPlane = LayerMask.NameToLayer("LevelEditor");
        layerFloor = LayerMask.NameToLayer("Floor");
        layerWall = LayerMask.NameToLayer("Wall");
        layerDecor = LayerMask.NameToLayer("Decor");
        layerOverlay = LayerMask.NameToLayer("Overlay");
        layerMaskPieceSelection = (1 << layerFloor | 1 << layerWall | 1 << layerOverlay | 1 << layerDecor | 1 << LayerMask.NameToLayer("Gizmo"));


        // Setup the variables for the info panel
        _infoPanel = infoPanel;
        _pieceNameText = pieceNameText;
        _positionToggle = positionToggle;
        _inputPosX = inputPosX;
        _inputPosY = inputPosY;
        _inputPosZ = inputPosZ;
        _rotationToggle = rotationToggle;
        _inputRotX = inputRotX;
        _inputRotY = inputRotY;
        _inputRotZ = inputRotZ;
        _spinningPieceToggle = spinningPieceToggle;
        _inputSpinTime = inputSpinTime;
        _inputSpinPauseTime = inputSpinPauseTime;
        _inputSpinNbRota = inputSpinNbRota;
        _movingPieceToggle = movingPieceToggle;
        _inputMoveDestX = inputMoveDestX;
        _inputMoveDestY = inputMoveDestY;
        _inputMoveDestZ = inputMoveDestZ;
        _inputTravelTime = inputTravelTime;
        _inputMovePauseTime = inputMovePauseTime;
        _gizmoRotate = gizmoRotate;
        _gizmoScale = gizmoScale;
        _gizmoTranslate = gizmoTranslate;

        // Grab the lemvpManager instance
        lemvpManager = LevelEditorMovingPieceManager._instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!testMode.isInTest() && canEdit)
        {
            //Handle Escape for menus
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (!escapeMenu.gameObject.activeSelf)
                {
                    escapeMenu.gameObject.SetActive(true);
                }
                else
                {
                    if (escapeMenu.isDone)
                        escapeMenu.gameObject.SetActive(false);
                }
            }

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
                float x, y, z = 0f;

                // If the selected is a prefab or a simple floor
                if (currentPiece.layer != layerOverlay && currentPiece.layer != layerWall && currentPiece.layer != layerDecor)
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
                                }

                            }
                        }
                    }
                }
                else if (currentPiece.layer == layerWall)
                {

                }
                else if (currentPiece.layer == layerDecor)
                {
                    // We use a raycast to find the plane (layerPlane)
                    RaycastHit rayHitPlane;
                    Ray rayPlane = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(rayPlane, out rayHitPlane, Mathf.Infinity, ~(1 << 16)))
                    {
                        /*if (rayHitPlane.transform.gameObject.layer == layerPlane)
                        {*/
                        //Debug.Log("Hit the plane");

                        // Only move the piece if the cursor isn't on any UI element
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            //Debug.Log("hitPoint : " + rayHitPlane.point);

                            Vector3 pos = new Vector3();

                            // Round to the lowest even integer in order to move correctly on the grid
                            if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                            {
                                x = Mathf.Floor(rayHitPlane.point.x + 1 - offsetGridX); // + 1 because of grid offset (It is placed on (-1;0;-1))
                                x = (x % 2 == 0) ? x : x - 1;
                                x += offsetGridX;
                                y = rayHitPlane.point.y;
                                z = Mathf.Floor(rayHitPlane.point.z + 1 - offsetGridZ);
                                z = (z % 2 == 0) ? z : z - 1;
                                z += offsetGridZ;
                                pos = new Vector3(x, y, z);
                            }
                            else
                            {
                                pos = rayHitPlane.point;
                            }

                            currentPiece.transform.eulerAngles = new Vector3(0f, currentPiece.transform.eulerAngles.y, 0f);
                            currentPiece.transform.position = pos;

                            // If you left-click place the piece
                            if (Input.GetMouseButtonDown(0))
                            {
                                currParams = undoRedoStack.Do(new AddPieceCommand(currentPiece, pos, currentPiece.transform.rotation), currParams);
                            }

                        }
                        //}
                    }
                }
                else if (currentPiece.layer == layerOverlay)
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
                                Vector3 pos = new Vector3(x, rayHit.point.y + 0.001f, z);
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
                                    currParams.result.transform.parent = rayHit.transform.parent;
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
                    Debug.LogError("Unknown layer : " + currentPiece.layer + ", for piece : " + currentPiece.name);
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

                if (Input.GetKeyDown(KeyCode.D))
                {
                    gizmoScale.gameObject.SetActive(false);
                    gizmoTranslate.gameObject.SetActive(false);
                    gizmoRotate.gameObject.SetActive(false);
                    currParams = undoRedoStack.Do(new DeletePiecesCommand(), currParams);
                }


                //Use T to move the selected piece
                if (Input.GetKeyDown(KeyCode.T))
                {
                    //If we are not on the translate gizmo, we activate id
                    if (!gizmoTranslate.gameObject.activeSelf)
                    {
                        //Disable other gizmos
                        if (gizmoScale.gameObject.activeSelf)
                            gizmoScale.gameObject.SetActive(false);

                        if (gizmoRotate.gameObject.activeSelf)
                            gizmoRotate.gameObject.SetActive(false);

                        gizmoTranslate.translateTarget = selectedPiecesInPlace;
                        gizmoTranslate.gameObject.SetActive(true);
                    }
                    //Else, we disable it
                    else
                    {
                        gizmoTranslate.gameObject.SetActive(false);
                    }
                }

                // Use R to rotate the selected pieces 90 degrees clockwise
                if (Input.GetKeyDown(KeyCode.R))
                {
                    //If rotation gizmo is disabled, we activate it
                    if (!gizmoRotate.gameObject.activeSelf)
                    {
                        if (gizmoScale.gameObject.activeSelf)
                            gizmoScale.gameObject.SetActive(false);

                        if (gizmoTranslate.gameObject.activeSelf)
                            gizmoTranslate.gameObject.SetActive(false);

                        if (selectedPiecesInPlace.Count == 1)
                        {
                            gizmoRotate.transform.localEulerAngles = selectedPiecesInPlace[0].transform.localEulerAngles;
                            gizmoRotate.rotateTarget = selectedPiecesInPlace[0];
                            gizmoRotate.gameObject.SetActive(true);
                        }
                    }
                    //Else, we hide it
                    else
                    {
                        gizmoRotate.transform.localEulerAngles = Vector3.zero;
                        gizmoRotate.gameObject.SetActive(false);
                    }
                    //Rotate multiple pieces
                    /*else
                    {
                        currParams = undoRedoStack.Do(new RotateSelectedPiecesCommand(), currParams);
                    }*/
                }

                //Use S to scale the selected piece
                if (Input.GetKeyDown(KeyCode.S) && (!Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftShift)))
                {
                    //If we are not on the scaling gizmo, we activate id
                    if (!gizmoScale.gameObject.activeSelf)
                    {
                        if (gizmoTranslate.gameObject.activeSelf)
                            gizmoTranslate.gameObject.SetActive(false);

                        if (gizmoRotate.gameObject.activeSelf)
                            gizmoRotate.gameObject.SetActive(false);

                        if (selectedPiecesInPlace.Count == 1)
                        {
                            gizmoScale.transform.localEulerAngles = selectedPiecesInPlace[0].transform.localEulerAngles;
                            gizmoScale.scaleTarget = selectedPiecesInPlace[0];
                            gizmoScale.gameObject.SetActive(true);
                        }
                    }
                    //Else, we disable it
                    else
                    {
                        gizmoScale.transform.localEulerAngles = Vector3.zero;
                        gizmoScale.gameObject.SetActive(false);
                    }
                }


                // Use C to get a copy of the selected piece(s) in place as the piece(s) in hand
                if (Input.GetKeyDown(KeyCode.C))
                {
                    if (selectedPiecesInPlace.Count == 1)
                    {
                        // Name of the piece has (Clone) so we remove it to get the prefab name
                        // Get a copy of the selected piece in hand
                        clickOnPiece(selectedPiecesInPlace[0].name.Split('(')[0]);
                    }
                    else
                    {
                        // We allow the copy of multiple pieces at once
                        GameObject referencePiece = selectedPiecesInPlace[0];

                        // We spawn the copy of the first piece
                        GameObject prefabReferencePiece = prefabs[referencePiece.name.Split('(')[0]];
                        if (prefabReferencePiece != null)
                        {
                            Destroy(currentPiece);
                            currentPiece = Instantiate(prefabReferencePiece, prefabReferencePiece.transform.position, referencePiece.transform.rotation);
                            // Disable all colliders so that Raycasts can go through the currentPiece
                            SetAllCollidersStatus(false, currentPiece);
                        }
                        else
                            Debug.LogError("Piece not found in prefabs : " + referencePiece.name.Split('(')[0]);

                        // Then we need to spawn copies of the other pieces, that will use the reference piece as parent so that they move along and rotate around the piece
                        for (int i = 1; i < selectedPiecesInPlace.Count; i++)
                        {
                            GameObject piece = selectedPiecesInPlace[i];
                            GameObject prefabPiece = prefabs[piece.name.Split('(')[0]];
                            if (prefabPiece != null)
                            {
                                // We spawn the prefab using the relative position of the piece to the reference piece
                                // The new reference piece (which is the piece in hand = currentPiece) is used as parent
                                GameObject newCurrentPiece = Instantiate(prefabPiece, piece.transform.position - referencePiece.transform.position, piece.transform.rotation, currentPiece.transform);
                                // Disable all colliders so that Raycasts can go through the currentPiece
                                SetAllCollidersStatus(false, newCurrentPiece);
                            }
                            else
                                Debug.LogError("Piece not found in prefabs : " + piece.name.Split('(')[0]);
                        }
                    }
                }


                if (selectedPiecesInPlace.Count == 1)
                {
                    // Use O to define the piece as the origin of the grid (offset the grid to align the pieces correctly)
                    if (Input.GetKeyDown(KeyCode.O))
                    {
                        currParams = undoRedoStack.Do(new UsePieceAsOriginCommand(), currParams);
                    }
                    // Use shift + S to define the piece as the spawning point of the level
                    else if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
                    {
                        currParams = undoRedoStack.Do(new SetSpawnPointCommand(), currParams);
                    }
                }


                // No left-click interaction with mouse over UI
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // Click alone selects a single piece while deselecting other pieces
                    // Use shift + click to select additional pieces
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray rayPiece = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit[] hits = Physics.RaycastAll(rayPiece, Mathf.Infinity, layerMaskPieceSelection);

                        bool isGizmo = false;

                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Gizmo"))
                                isGizmo = true;
                        }

                        //If there is no gizmo on the click
                        if (!isGizmo)
                        {
                            RaycastHit rayHitPiece;
                            if (Physics.Raycast(rayPiece, out rayHitPiece, Mathf.Infinity, layerMaskPieceSelection))
                            {

                                GameObject piece = rayHitPiece.transform.gameObject;

                                string pName = piece.name.Split(' ')[0];
                                while (piece.transform.parent != null && pName != "Hole")
                                {
                                    pName = piece.transform.parent.gameObject.name.Split(' ')[0];
                                    if (pName != "Hole")
                                        piece = piece.transform.parent.gameObject;
                                }

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
                                //If gizmo rotation is activated, we disable it
                                if (gizmoRotate.gameObject.activeSelf)
                                {
                                    gizmoRotate.transform.localEulerAngles = Vector3.zero;
                                    gizmoRotate.gameObject.SetActive(false);
                                }

                                //If gizmo scaling is activated, we disable it
                                if (gizmoScale.gameObject.activeSelf)
                                {
                                    gizmoScale.transform.localEulerAngles = Vector3.zero;
                                    gizmoScale.gameObject.SetActive(false);
                                }

                                if (gizmoTranslate.gameObject.activeSelf)
                                {
                                    gizmoTranslate.gameObject.SetActive(false);
                                }

                                // Click on an empty space => deselect all pieces
                                // If the player is shifting, we tolerate clicks on empty spaces (to avoid ruining multi-selection)
                                if (!(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
                                {
                                    gizmoRotate.gameObject.SetActive(false);
                                    gizmoScale.gameObject.SetActive(false);
                                    gizmoTranslate.gameObject.SetActive(false);
                                    currParams = undoRedoStack.Do(new DeselectAllPiecesCommand(), currParams);
                                }
                            }
                        }

                        /*
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
                            string pName = piece.name.Split(' ')[0];
                            while (piece.transform.parent != null && pName != "Hole")
                            {
                                pName = piece.transform.parent.gameObject.name.Split(' ')[0];
                                if (pName != "Hole")
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
                        //If we click on something that is not a gizmo
                        else if(!Physics.Raycast(gizmoCamera.ScreenPointToRay(Input.mousePosition), out rayHitPiece, Mathf.Infinity, LayerMask.GetMask("Gizmo")))
                        {
                            //If gizmo rotation is activated, we disable it
                            if (gizmoRotate.gameObject.activeSelf)
                            {
                                gizmoRotate.transform.localEulerAngles = Vector3.zero;
                                gizmoRotate.gameObject.SetActive(false);
                            }

                            //If gizmo scaling is activated, we disable it
                            if(gizmoScale.gameObject.activeSelf)
                            {
                                gizmoScale.transform.localEulerAngles = Vector3.zero;
                                gizmoScale.gameObject.SetActive(false);
                            }

                            if(gizmoTranslate.gameObject.activeSelf)
                            {
                                gizmoTranslate.transform.localEulerAngles = Vector3.zero;
                                gizmoTranslate.gameObject.SetActive(false);
                            }

                            // Click on an empty space => deselect all pieces
                            // If the player is shifting, we tolerate clicks on empty spaces (to avoid ruining multi-selection)
                            if (!(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
                            {
                                gizmoRotate.gameObject.SetActive(false);
                                gizmoScale.gameObject.SetActive(false);
                                gizmoTranslate.gameObject.SetActive(false);
                                currParams = undoRedoStack.Do(new DeselectAllPiecesCommand(), currParams);
                            }
                        }*/
                    }
                }

            }
            else
            {
                // No piece selected, the player can click pieces in place to modify them or a piece in the menu to start placing them

                // Use O to define the piece as the origin of the grid (offset the grid to align the pieces correctly)
                if (Input.GetKeyDown(KeyCode.O) && originPiece != null)
                {
                    currParams = undoRedoStack.Do(new ResetOriginCommand(), currParams);
                }

                
                // The user left-clicked so we have a potential start of a box selection, we check if the mouse moved since he started holding the click
                if(!isBoxSelectionActive && isBoxSelection)
                {
                    if (Input.mousePosition != mousePositionStart)
                        isBoxSelectionActive = true;
                }

                // No left-click interaction with mouse over UI
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // Use left-click to select, simple click selects the piece at the cursor location
                    // You can make a box selection by maintaining the click and moving the cursor
                    if (Input.GetMouseButtonDown(0))
                    {
                        isBoxSelection = true;
                        mousePositionStart = Input.mousePosition;
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (isBoxSelectionActive)
                        {
                            Vector3 mousePositionEnd = Input.mousePosition;
                            Bounds bounds = new Bounds();
                            Vector3 viewportPositionStart = Camera.main.ScreenToViewportPoint(mousePositionStart);
                            Vector3 viewportPositionEnd = Camera.main.ScreenToViewportPoint(mousePositionEnd);
                            Vector3 minPos = Vector3.Min(viewportPositionStart, viewportPositionEnd);
                            Vector3 maxPos = Vector3.Max(viewportPositionStart, viewportPositionEnd);
                            minPos.z = Camera.main.nearClipPlane;
                            maxPos.z = Camera.main.farClipPlane;
                            bounds.SetMinMax(minPos, maxPos);

                            List<GameObject> boxSelectedPieces = new List<GameObject>();

                            foreach(GameObject piece in piecesInPlace[currentHole])
                            {
                                // If the piece is contained in the selection rectangle, we add it to the selection
                                if(bounds.Contains(Camera.main.WorldToViewportPoint(piece.transform.position)))
                                {
                                    boxSelectedPieces.Add(piece);
                                }
                            }

                            if (boxSelectedPieces.Count > 0)
                            {
                                currParams = undoRedoStack.Do(new SelectMultiplePiecesCommand(boxSelectedPieces), currParams);
                            }
                        }
                        else
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
                                string pName = piece.name.Split(' ')[0];
                                while (piece.transform.parent != null && pName != "Hole")
                                {
                                    pName = piece.transform.parent.gameObject.name.Split(' ')[0];
                                    if (pName != "Hole")
                                        piece = piece.transform.parent.gameObject;
                                }
                                //}

                                // Debug.Log(piece.name);

                                // We highlight the selected piece
                                currParams = undoRedoStack.Do(new SelectSinglePieceCommand(piece), currParams);
                            }
                        }
                        isBoxSelection = false;
                        isBoxSelectionActive = false;
                        mousePositionStart = Vector3.zero;
                    }
                }

                // Use C to get a copy of a piece in place as the piece in hand
                if (Input.GetKeyDown(KeyCode.C))
                {
                    // We use a raycast to find the pieces
                    RaycastHit rayHitPiece;
                    Ray rayPiece = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(rayPiece, out rayHitPiece, Mathf.Infinity, layerMaskPieceSelection))
                    {
                        //Debug.Log("Hit a piece : " + rayHitPiece.transform.gameObject.name);
                        GameObject piece = rayHitPiece.transform.gameObject;

                        string pName = piece.name.Split(' ')[0];
                        while (piece.transform.parent != null && pName != "Hole")
                        {
                            pName = piece.transform.parent.gameObject.name.Split(' ')[0];
                            if (pName != "Hole")
                                piece = piece.transform.parent.gameObject;
                        }

                        // Name of the piece has (Clone) so we remove it to get the prefab name
                        pName = piece.name.Split('(')[0];
                        //Debug.Log(pName);

                        // Get a copy of the clicked piece in hand
                        clickOnPiece(pName);
                    }
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
        if (piece.layer != layerOverlay && piece.layer != layerWall)
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

            GameObject piece = prefabs[pieceName];

            /*
            foreach (GameObject pref in prefabs)
            {
                if (pref.name == pieceName)
                    piece = pref;
            }*/

            if (piece != null)
            {
                Destroy(currentPiece);
                currentPiece = Instantiate(piece, piece.transform.position, piece.transform.rotation);
                // Disable all colliders so that Raycasts can go through the currentPiece
                SetAllCollidersStatus(false, currentPiece);

                // Deselect all pieces when we pick something in the UI
                //currParams = undoRedoStack.Do(new DeselectAllPiecesCommand(), currParams);
            }
            else
                Debug.LogError("Piece not found : " + pieceName);
        }
        else
            Debug.LogError("Empty string call");
    }


    // This event triggers when the user changes the value of the dropdown menu (holeSelection)
    void DropdownValueChanged(Dropdown change)
    {
        int holeSelected = change.value;

        // Debug.Log("Hole selected : " + (holeSelected + 1));

        // Just a safety check to avoid trying to change from current hole to current hole
        if (holeSelected != currentHole)
        {
            foreach (MaterialSwaperoo ms in currentHoleObject.GetComponentsInChildren<MaterialSwaperoo>())
            {
                ms.SwapToGrey(true);
            }

            currentHole = holeSelected;
            currentHoleObject = GameObject.Find("Hole " + (currentHole + 1));

            foreach (MaterialSwaperoo ms in currentHoleObject.GetComponentsInChildren<MaterialSwaperoo>())
            {
                ms.SwapToGrey(false);
            }
        }
    }


    // Handle loss/gain of focus
    private void OnApplicationFocus(bool focus)
    {
        if (!testMode.isInTest())
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    // Note that this function is only meant to be called from OnGUI() functions.
    public static void GUIDrawRect(Rect position, Color color)
    {
        if (_staticRectTexture == null)
        {
            _staticRectTexture = new Texture2D(1, 1);
        }

        if (_staticRectStyle == null)
        {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(position, GUIContent.none, _staticRectStyle);
    }

    // Used to draw the rectangle for box selection
    void OnGUI()
    {
        if (isBoxSelectionActive)
        {
            Vector3 _mousePositionStart = mousePositionStart;
            Vector3 _mousePositionEnd = Input.mousePosition;
            // Use a top left origin rather than bottom left (required for Rect)
            _mousePositionStart.y = Screen.height - _mousePositionStart.y;
            _mousePositionEnd.y = Screen.height - _mousePositionEnd.y;
            // Find the top-left and bottom-right corners
            Vector3 topLeft = Vector3.Min(_mousePositionStart, _mousePositionEnd);
            Vector3 bottomRight = Vector3.Max(_mousePositionStart, _mousePositionEnd);
            // Create the corresponding Rect on screen
            Rect rect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
            GUIDrawRect(rect, new Color(0.5f, 0.75f, 0.95f, 0.35f));
        }
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
                    outl.color = (active) ? 1 : 0;
                }
                else
                {
                    outl.eraseRenderer = !active;
                    outl.color = outlColor;
                    outl.enabled = active;
                }
            }
        }
    }


    // Fixes a weird bug with object that uses png textures and outline effect (fucking flag)
    public void setSelection(bool b)
    {
        if (b)
        {
            if (selectedPiecesInPlace.Count >= 1)
                currParams = undoRedoStack.Do(new DeselectAllPiecesCommand(), currParams);
            else
                flagNoUndoDeselect = true;
        }
        else
        {
            if (flagNoUndoDeselect)
                flagNoUndoDeselect = false;
            else
                currParams = undoRedoStack.Undo(currParams);
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


    // Enables/Disables the piece information panel
    private static void SetPieceInfoPanelVisibility()
    {
        //Debug.Log("SetPieceInfoPanelVisibility");
        if (selectedPiecesInPlace.Count >= 1)
            _infoPanel.SetActive(true);
        else
            _infoPanel.SetActive(false);
    }

    // Sets the name of the piece on top of the piece info panel
    // If multiple pieces are selected => "Multiple pieces selected"
    private static void SetPieceInfoPanelName()
    {
        //Debug.Log("SetPieceInfoPanelName");
        if (selectedPiecesInPlace.Count == 1)
        {
            _pieceNameText.text = selectedPiecesInPlace[0].name.Split('(')[0];
        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            _pieceNameText.text = "Multiple pieces selected";
        }
        else
            Debug.LogError("No piece are selected and we try to set the name for the piece information display");
    }

    // Sets the piece info such as position, rotation, booster pad, spinning piece parameters
    private static void SetPieceInfoData()
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            // Position and rotation are displayed by default
            _positionToggle.isOn = true;
            _rotationToggle.isOn = true;

            GameObject piece = selectedPiecesInPlace[0];

            // Check if the piece has the RotatePiece component to display or not the related values
            RotatePiece rtp = piece.GetComponent<RotatePiece>();
            if (rtp != null && rtp.enabled) // If the component is disabled it means it was added THEN removed from the piece (we only disable the script instead of removing it)
            {
                _spinningPieceToggle.isOn = true;
                _inputSpinTime.text = rtp.spinTime.ToString("F");
                _inputSpinPauseTime.text = rtp.pauseTime.ToString("F");
                _inputSpinNbRota.text = rtp.nbRotations.ToString("D");

                rtp.SetFlagStopSpin(true);
            }
            else
            {
                _spinningPieceToggle.isOn = false;
            }

            // Check if the piece has the MovingPiece component to display or not the related values
            MovingPiece mvp = piece.GetComponent<MovingPiece>();
            if (mvp != null && mvp.enabled) // If the component is disabled it means it was added THEN removed from the piece (we only disable the script instead of removing it)
            {
                _movingPieceToggle.isOn = true;
                _inputMoveDestX.text = (mvp.destX - mvp.initX).ToString("F");
                _inputMoveDestY.text = (mvp.destY - mvp.initY).ToString("F");
                _inputMoveDestZ.text = (mvp.destZ - mvp.initZ).ToString("F");
                _inputTravelTime.text = mvp.travelTime.ToString("F");
                _inputMovePauseTime.text = mvp.pauseTime.ToString("F");

                mvp.SetFlagStopMove(true);
            }
            else
            {
                _movingPieceToggle.isOn = false;
            }

            // Fill the inputs based on the piece values
            _inputPosX.text = piece.transform.position.x.ToString("F");
            _inputPosY.text = piece.transform.position.y.ToString("F");
            _inputPosZ.text = piece.transform.position.z.ToString("F");
            _inputRotX.text = piece.transform.eulerAngles.x.ToString("F");
            _inputRotY.text = piece.transform.eulerAngles.y.ToString("F");
            _inputRotZ.text = piece.transform.eulerAngles.z.ToString("F");

            // All position and rotation values can be edited in single-selection mode
            _inputPosX.interactable = true;
            _inputPosY.interactable = true;
            _inputPosZ.interactable = true;
            _inputRotX.interactable = true;
            _inputRotY.interactable = true;
            _inputRotZ.interactable = true;


        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            _positionToggle.isOn = true;
            _rotationToggle.isOn = true;

            RotatePiece rtp = selectedPiecesInPlace[0].GetComponent<RotatePiece>();
            if (rtp != null && rtp.enabled) // If the component is disabled it means it was added THEN removed from the piece (we only disable the script instead of removing it)
            {
                // Just prevent the pieces from spinning while selected
                rtp.SetFlagStopSpin(true); // Also resets rotation to the initial value of the piece
            }
            MovingPiece mvp = selectedPiecesInPlace[0].GetComponent<MovingPiece>();
            if (mvp != null && mvp.enabled) // If the component is disabled it means it was added THEN removed from the piece (we only disable the script instead of removing it)
            {
                // Just prevent the pieces from moving while selected
                mvp.SetFlagStopMove(true); // Also resets position to the initial value of the piece
            }

            // We display the values only they are equal across all selected pieces (ex : all pieces have pos.x=150)
            float xPos = selectedPiecesInPlace[0].transform.position.x;
            float yPos = selectedPiecesInPlace[0].transform.position.y;
            float zPos = selectedPiecesInPlace[0].transform.position.z;
            float xRot = selectedPiecesInPlace[0].transform.eulerAngles.x;
            float yRot = selectedPiecesInPlace[0].transform.eulerAngles.y;
            float zRot = selectedPiecesInPlace[0].transform.eulerAngles.z;
            bool[] sameVals = new bool[6];
            for (int i = 0; i < 6; i++) sameVals[i] = true;

            foreach (GameObject piece in selectedPiecesInPlace)
            {
                rtp = piece.GetComponent<RotatePiece>();
                if (rtp != null && rtp.enabled) // If the component is disabled it means it was added THEN removed from the piece (we only disable the script instead of removing it)
                {
                    // Just prevent the pieces from spinning while selected
                    rtp.SetFlagStopSpin(true); // Also resets rotation to the initial value of the piece
                }
                mvp = piece.GetComponent<MovingPiece>();
                if (mvp != null && mvp.enabled) // If the component is disabled it means it was added THEN removed from the piece (we only disable the script instead of removing it)
                {
                    // Just prevent the pieces from moving while selected
                    mvp.SetFlagStopMove(true); // Also resets position to the initial value of the piece
                }

                sameVals[0] &= ApproximatelyEquals(xPos, piece.transform.position.x);
                sameVals[1] &= ApproximatelyEquals(yPos, piece.transform.position.y);
                sameVals[2] &= ApproximatelyEquals(zPos, piece.transform.position.z);
                sameVals[3] &= ApproximatelyEquals(xRot, piece.transform.eulerAngles.x);
                sameVals[4] &= ApproximatelyEquals(yRot, piece.transform.eulerAngles.y);
                sameVals[5] &= ApproximatelyEquals(zRot, piece.transform.eulerAngles.z);
            }

            _inputPosX.text = (sameVals[0]) ? xPos.ToString("F") : "";
            _inputPosY.text = (sameVals[1]) ? yPos.ToString("F") : "";
            _inputPosZ.text = (sameVals[2]) ? zPos.ToString("F") : "";
            _inputRotX.text = (sameVals[3]) ? xRot.ToString("F") : "";
            _inputRotY.text = (sameVals[4]) ? yRot.ToString("F") : "";
            _inputRotZ.text = (sameVals[5]) ? zRot.ToString("F") : "";

            _inputPosX.interactable = sameVals[0];
            _inputPosY.interactable = sameVals[1];
            _inputPosZ.interactable = sameVals[2];
            _inputRotX.interactable = sameVals[3];
            _inputRotY.interactable = sameVals[4];
            _inputRotZ.interactable = sameVals[5];
        }
        else
            Debug.LogError("No piece are selected and we try to set the data for the piece information display");
    }

    // We have almost-zero value differences (like some pieces will have y=0 and others y=4e-17)
    private static bool ApproximatelyEquals(float a, float b)
    {
        return Mathf.Abs(a - b) < epsilon;
    }


    #region PieceInformationEdit

    /*****************************
     *  Piece position
     *****************************/

    public void updatePosX(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float x = 0f;
            float.TryParse(val, out x);
            piece.transform.position = new Vector3(x, piece.transform.position.y, piece.transform.position.z);
        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            float xPos = selectedPiecesInPlace[0].transform.position.x;
            bool sameVal = true;
            foreach (GameObject piece in selectedPiecesInPlace)
            {
                sameVal &= ApproximatelyEquals(xPos, piece.transform.position.x);
            }
            if (sameVal)
            {
                float x = 0f;
                float.TryParse(val, out x);
                foreach (GameObject piece in selectedPiecesInPlace)
                {
                    piece.transform.position = new Vector3(x, piece.transform.position.y, piece.transform.position.z);
                }
            }
        }
        else
            Debug.LogError("No piece are selected and we try to set the position X");
    }


    public void updatePosY(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float y = 0f;
            float.TryParse(val, out y);
            piece.transform.position = new Vector3(piece.transform.position.x, y, piece.transform.position.z);
        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            float yPos = selectedPiecesInPlace[0].transform.position.y;
            bool sameVal = true;
            foreach (GameObject piece in selectedPiecesInPlace)
            {
                sameVal &= ApproximatelyEquals(yPos, piece.transform.position.y);
            }
            if (sameVal)
            {
                float y = 0f;
                float.TryParse(val, out y);
                foreach (GameObject piece in selectedPiecesInPlace)
                {
                    piece.transform.position = new Vector3(piece.transform.position.x, y, piece.transform.position.z);
                }
            }
        }
        else
            Debug.LogError("No piece are selected and we try to set the position Y");
    }


    public void updatePosZ(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float z = 0f;
            float.TryParse(val, out z);
            piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, z);
        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            float zPos = selectedPiecesInPlace[0].transform.position.z;
            bool sameVal = true;
            foreach (GameObject piece in selectedPiecesInPlace)
            {
                sameVal &= ApproximatelyEquals(zPos, piece.transform.position.z);
            }
            if (sameVal)
            {
                float z = 0f;
                float.TryParse(val, out z);
                foreach (GameObject piece in selectedPiecesInPlace)
                {
                    piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, z);
                }
            }
        }
        else
            Debug.LogError("No piece are selected and we try to set the position Z");
    }


    /*****************************
     *  Piece rotation
     *****************************/

    public void updateRotX(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float x = 0f;
            float.TryParse(val, out x);
            piece.transform.eulerAngles = new Vector3(x, piece.transform.eulerAngles.y, piece.transform.eulerAngles.z);
        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            float xRot = selectedPiecesInPlace[0].transform.eulerAngles.x;
            bool sameVal = true;
            foreach (GameObject piece in selectedPiecesInPlace)
            {
                sameVal &= ApproximatelyEquals(xRot, piece.transform.eulerAngles.x);
            }
            if (sameVal)
            {
                float x = 0f;
                float.TryParse(val, out x);
                foreach (GameObject piece in selectedPiecesInPlace)
                {
                    piece.transform.eulerAngles = new Vector3(x, piece.transform.eulerAngles.y, piece.transform.eulerAngles.z);
                }
            }
        }
        else
            Debug.LogError("No piece are selected and we try to set the rotation X");
    }


    public void updateRotY(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float y = 0f;
            float.TryParse(val, out y);
            piece.transform.eulerAngles = new Vector3(piece.transform.eulerAngles.x, y, piece.transform.eulerAngles.z);
        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            float yRot = selectedPiecesInPlace[0].transform.eulerAngles.y;
            bool sameVal = true;
            foreach (GameObject piece in selectedPiecesInPlace)
            {
                sameVal &= ApproximatelyEquals(yRot, piece.transform.eulerAngles.y);
            }
            if (sameVal)
            {
                float y = 0f;
                float.TryParse(val, out y);
                foreach (GameObject piece in selectedPiecesInPlace)
                {
                    piece.transform.eulerAngles = new Vector3(piece.transform.eulerAngles.x, y, piece.transform.eulerAngles.z);
                }
            }
        }
        else
            Debug.LogError("No piece are selected and we try to set the rotation Y");
    }


    public void updateRotZ(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float z = 0f;
            float.TryParse(val, out z);
            piece.transform.eulerAngles = new Vector3(piece.transform.eulerAngles.x, piece.transform.eulerAngles.y, z);
        }
        else if (selectedPiecesInPlace.Count > 1)
        {
            float zRot = selectedPiecesInPlace[0].transform.eulerAngles.z;
            bool sameVal = true;
            foreach (GameObject piece in selectedPiecesInPlace)
            {
                sameVal &= ApproximatelyEquals(zRot, piece.transform.eulerAngles.z);
            }
            if (sameVal)
            {
                float z = 0f;
                float.TryParse(val, out z);
                foreach (GameObject piece in selectedPiecesInPlace)
                {
                    piece.transform.eulerAngles = new Vector3(piece.transform.eulerAngles.x, piece.transform.eulerAngles.y, z);
                }
            }
        }
        else
            Debug.LogError("No piece are selected and we try to set the rotation Z");
    }

    /*****************************
     *  SpinningPiece
     *****************************/

    public void spinningPieceToggleValueChanged(bool tog)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            RotatePiece rtp = piece.GetComponent<RotatePiece>();
            if (tog)
            {
                if (rtp == null)
                {
                    Vector3 initRota = piece.transform.eulerAngles;
                    rtp = piece.AddComponent<RotatePiece>();
                    rtp.SetFlagStopSpin(true);
                    piece.transform.eulerAngles = initRota;
                    rtp.UpdateInitialRotation();
                    lemvpManager.AddRotatePiece(rtp);

                    _inputSpinTime.text = rtp.spinTime.ToString("F");
                    _inputSpinPauseTime.text = rtp.pauseTime.ToString("F");
                    _inputSpinNbRota.text = rtp.nbRotations.ToString("D");
                }
                else
                {
                    rtp.enabled = true;
                }
            }
            else
            {
                if (rtp != null)
                    rtp.enabled = false;
            }
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to activate/deactivate the spinning");
    }


    public void updateSpinTime(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            RotatePiece rtp = piece.GetComponent<RotatePiece>();
            if (rtp != null)
            {
                rtp.spinTime = float.Parse(val);
            }
            else
                Debug.LogError("We try to change the spinning time but there's no RotatePiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the spin time");
    }


    public void updatePauseTime(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            RotatePiece rtp = piece.GetComponent<RotatePiece>();
            if (rtp != null)
            {
                rtp.pauseTime = float.Parse(val);
            }
            else
                Debug.LogError("We try to change the pause time but there's no RotatePiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the spin time");
    }


    public void updateNbRotations(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            RotatePiece rtp = piece.GetComponent<RotatePiece>();
            if (rtp != null)
            {
                rtp.nbRotations = int.Parse(val);
                rtp.UpdateRotations();
            }
            else
                Debug.LogError("We try to change the nbRotations but there's no RotatePiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the spin time");
    }

    /*****************************
     *  MovingPiece
     *****************************/

    public void movingPieceToggleValueChanged(bool tog)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            MovingPiece mvp = piece.GetComponent<MovingPiece>();
            if (tog)
            {
                if (mvp == null)
                {
                    mvp = piece.AddComponent<MovingPiece>();
                    mvp.UpdateInitialPosition();
                    mvp.UpdateDestination(mvp.initPos);
                    mvp.SetFlagStopMove(true);
                    lemvpManager.AddMovingPiece(mvp);

                    _inputMoveDestX.text = (mvp.destX - mvp.initX).ToString("F");
                    _inputMoveDestY.text = (mvp.destY - mvp.initY).ToString("F");
                    _inputMoveDestZ.text = (mvp.destZ - mvp.initZ).ToString("F");
                    _inputTravelTime.text = mvp.travelTime.ToString("F");
                    _inputMovePauseTime.text = mvp.pauseTime.ToString("F");
                }
                else
                {
                    mvp.enabled = true;
                }
            }
            else
            {
                if (mvp != null)
                    mvp.enabled = false;
            }
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to activate/deactivate the spinning");
    }

    public void updateMoveDestX(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float x = 0f;
            float.TryParse(val, out x);

            MovingPiece mvp = piece.GetComponent<MovingPiece>();
            if (mvp != null)
            {
                mvp.UpdateDestination(new Vector3(mvp.initX + x, mvp.destY, mvp.destZ));
            }
            else
                Debug.LogError("We try to change the X coordinate of the destination but there's no MovingPiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the X coordinate of the destination");
    }

    public void updateMoveDestY(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float y = 0f;
            float.TryParse(val, out y);

            MovingPiece mvp = piece.GetComponent<MovingPiece>();
            if (mvp != null)
            {
                mvp.UpdateDestination(new Vector3(mvp.destX, mvp.initY + y, mvp.destZ));
            }
            else
                Debug.LogError("We try to change the Y coordinate of the destination but there's no MovingPiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the Y coordinate of the destination");
    }

    public void updateMoveDestZ(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            float z = 0f;
            float.TryParse(val, out z);

            MovingPiece mvp = piece.GetComponent<MovingPiece>();
            if (mvp != null)
            {
                mvp.UpdateDestination(new Vector3(mvp.destX, mvp.destY, mvp.initZ + z));
            }
            else
                Debug.LogError("We try to change the Z coordinate of the destination but there's no MovingPiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the Z coordinate of the destination");
    }


    public void updateMoveTravelTime(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            MovingPiece mvp = piece.GetComponent<MovingPiece>();
            if (mvp != null)
            {
                mvp.travelTime = float.Parse(val);
            }
            else
                Debug.LogError("We try to change the travel time but there's no MovingPiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the travel time");
    }


    public void updateMovePauseTime(string val)
    {
        if (selectedPiecesInPlace.Count == 1)
        {
            GameObject piece = selectedPiecesInPlace[0];
            MovingPiece mvp = piece.GetComponent<MovingPiece>();
            if (mvp != null)
            {
                mvp.pauseTime = float.Parse(val);
            }
            else
                Debug.LogError("We try to change the pause time but there's no MovingPiece script on the object");
        }
        else if (selectedPiecesInPlace.Count > 1)
        {

        }
        else
            Debug.LogError("No piece are selected and we try to set the spin time");
    }

    #endregion

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

        // AddPieceCommand (stores the child pieces in case of multiple pieces copy)
        // DeletePieceCommand (stores the pieces to restore if undo / delete if out of the redo stack)
        // RotateSelectedPiecesCommand (parameter)
        // SelectSinglePieceCommand (stores the pieces that were deselected)
        // DeselectAllPiecesCommand (stores the pieces that were deselected)
        // SelectMultiplePiecesCommand (parameter)
        public List<GameObject> selectedPieces = new List<GameObject>();

        // SetSpawnPoint (stores wether the spawnPoint must be displayed or not)
        public bool b;
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

            // Handle the multiple piece copy
            foreach (Transform t in pr.transform)
            {
                GameObject child = t.gameObject;
                if (child.GetComponent<TerrainPiece>() != null)
                {
                    _CP.b = true;
                    _CP.selectedPieces.Add(child);
                }
            }

            _CP.prefab = prefabs[prName];
            _CP.position = pos;
            _CP.rotation = rot;
            _CP.result = null;
        }

        public CommandParams Do(CommandParams input = null)
        {
            if (_CP.result == null)
            {
                _CP.result = Instantiate(_CP.prefab, _CP.position, _CP.rotation);
                // Enable all colliders so that Raycasts do hit the piece
                _CP.result.transform.parent = currentHoleObject.transform;
                SetAllCollidersStatus(true, _CP.result);
                piecesInPlace[currentHole].Add(_CP.result);

                // If we have multiple pieces to instantiate
                if (_CP.b)
                {
                    // Instantiate copies of pieces
                    List<GameObject> newPieces = new List<GameObject>();
                    foreach (GameObject piece in _CP.selectedPieces)
                    {
                        GameObject newGO = Instantiate(prefabs[piece.name.Split('(')[0]], piece.transform.position, piece.transform.rotation);
                        newPieces.Add(newGO);
                        SetAllCollidersStatus(true, newGO);
                        piecesInPlace[currentHole].Add(newGO);
                        newGO.transform.parent = currentHoleObject.transform;
                    }

                    // Store the copy of the pieces so we can disable/enbale them on undo/redo
                    _CP.selectedPieces = new List<GameObject>(newPieces);
                }
            }
            else
            {
                _CP.result.SetActive(true);
                piecesInPlace[currentHole].Add(_CP.result);
                // If we created multiple pieces at once, we enable them all
                if (_CP.b)
                {
                    foreach (GameObject piece in _CP.selectedPieces)
                    {
                        piece.SetActive(true);
                        piecesInPlace[currentHole].Add(piece);
                    }
                }
            }

            isModified = true;

            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            _CP.result.SetActive(false);
            if (!piecesInPlace[currentHole].Remove(_CP.result))
                Debug.LogError("Failed to remove object on addPieceCommand undo");

            // If we created multiple pieces at once, we enable them all
            if (_CP.b)
            {
                foreach (GameObject piece in _CP.selectedPieces)
                {
                    piece.SetActive(false);
                    if (!piecesInPlace[currentHole].Remove(piece))
                        Debug.LogError("Failed to remove object on addPieceCommand undo");
                }
            }
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

            // Hide the piece info
            SetPieceInfoPanelVisibility();

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

            // Display the piece info
            SetPieceInfoPanelVisibility();
            SetPieceInfoPanelName();
            SetPieceInfoData();

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

            // Display the piece info
            SetPieceInfoPanelVisibility();
            SetPieceInfoPanelName();
            SetPieceInfoData();

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

                // Allow the piece to rotate again
                RotatePiece rtp = go.GetComponent<RotatePiece>();
                if (rtp != null && rtp.enabled)
                {
                    rtp.SetFlagStopSpin(false);
                }
                // Allow the piece to move again
                MovingPiece mvp = go.GetComponent<MovingPiece>();
                if (mvp != null && mvp.enabled)
                {
                    mvp.SetFlagStopMove(false);
                }
            }

            // Add the new piece to the selection
            SetHighlight(true, _CP.result);
            selectedPiecesInPlace.Add(_CP.result);

            //Activate translation gizmo
            _gizmoTranslate.translateTarget = selectedPiecesInPlace;
            _gizmoTranslate.gameObject.SetActive(true);

            //Disable other just in case
            if (_gizmoScale.gameObject.activeSelf)
                _gizmoScale.gameObject.SetActive(false);

            if (_gizmoRotate.gameObject.activeSelf)
                _gizmoRotate.gameObject.SetActive(false);

            // Display the piece info
            SetPieceInfoPanelVisibility();
            SetPieceInfoPanelName();
            SetPieceInfoData();

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

                // MAYBE UNNECESSARY (TOO LAZY TO CHECK => MATTHIEU 18/04/2018)
                RotatePiece rtp = go.GetComponent<RotatePiece>();
                if (rtp != null && rtp.enabled)
                {
                    rtp.SetFlagStopSpin(true);
                }
                MovingPiece mvp = go.GetComponent<MovingPiece>();
                if (mvp != null && mvp.enabled)
                {
                    mvp.SetFlagStopMove(true);
                }
            }

            _gizmoTranslate.gameObject.SetActive(false);

            // Hide the piece info in no pieces were selected before otherwise display the info of the previous selection
            SetPieceInfoPanelVisibility();
            if (_CP.selectedPieces.Count > 0)
            {
                _gizmoTranslate.translateTarget = selectedPiecesInPlace;
                _gizmoTranslate.gameObject.SetActive(true);
                SetPieceInfoPanelName();
                SetPieceInfoData();
            }


            return _CP;
        }
    }

    // Select a batch of pieces at once
    public class SelectMultiplePiecesCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public SelectMultiplePiecesCommand(List<GameObject> sel)
        {
            _CP.selectedPieces = sel;
        }

        public CommandParams Do(CommandParams input = null)
        {
            foreach (GameObject piece in _CP.selectedPieces)
            {
                SetHighlight(true, piece);
                selectedPiecesInPlace.Add(piece);
            }

            // Display the piece info
            SetPieceInfoPanelVisibility();
            SetPieceInfoPanelName();
            SetPieceInfoData();

            //Activate translation gizmo
            _gizmoTranslate.translateTarget = selectedPiecesInPlace;
            _gizmoTranslate.gameObject.SetActive(true);

            //Disable other just in case
            if (_gizmoScale.gameObject.activeSelf)
                _gizmoScale.gameObject.SetActive(false);

            if (_gizmoRotate.gameObject.activeSelf)
                _gizmoRotate.gameObject.SetActive(false);

            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            foreach (GameObject piece in _CP.selectedPieces)
            {
                selectedPiecesInPlace.Remove(piece);
                SetHighlight(false, piece);
            }

            // Disable the gizmo
            _gizmoTranslate.gameObject.SetActive(false);

            // Hide the piece info in no pieces were selected before otherwise display the info of the previous selection
            SetPieceInfoPanelVisibility();

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

            RotatePiece rtp = _CP.result.GetComponent<RotatePiece>();
            if (rtp != null && rtp.enabled)
            {
                rtp.SetFlagStopSpin(false);
            }
            MovingPiece mvp = _CP.result.GetComponent<MovingPiece>();
            if (mvp != null && mvp.enabled)
            {
                mvp.SetFlagStopMove(false);
            }

            // Display the piece info
            SetPieceInfoPanelVisibility();
            SetPieceInfoPanelName();
            SetPieceInfoData();


            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            // Add the piece to the selection
            SetHighlight(true, _CP.result);
            selectedPiecesInPlace.Add(_CP.result);

            // Display the piece info
            SetPieceInfoPanelVisibility();
            SetPieceInfoPanelName();
            SetPieceInfoData();

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
            //Debug.Log("DESELECT_ALL");
            // Remove all pieces from the selection
            selectedPiecesInPlace.Clear();
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(false, go);

                RotatePiece rtp = go.GetComponent<RotatePiece>();
                if (rtp != null && rtp.enabled)
                {
                    rtp.SetFlagStopSpin(false);
                }
                MovingPiece mvp = go.GetComponent<MovingPiece>();
                if (mvp != null && mvp.enabled)
                {
                    mvp.SetFlagStopMove(false);
                }
            }

            // Hide the piece info
            SetPieceInfoPanelVisibility();

            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            // Put the pieces back in the selection
            foreach (GameObject go in _CP.selectedPieces)
            {
                SetHighlight(true, go);
                selectedPiecesInPlace.Add(go);

                RotatePiece rtp = go.GetComponent<RotatePiece>();
                if (rtp != null && rtp.enabled)
                {
                    rtp.SetFlagStopSpin(true);
                }
                MovingPiece mvp = go.GetComponent<MovingPiece>();
                if (mvp != null && mvp.enabled)
                {
                    mvp.SetFlagStopMove(true);
                }
            }

            // Display the piece info
            SetPieceInfoPanelVisibility();
            SetPieceInfoPanelName();
            SetPieceInfoData();

            _gizmoTranslate.translateTarget = selectedPiecesInPlace;
            _gizmoTranslate.gameObject.SetActive(true);

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
                RotatePiece rtp = go.GetComponent<RotatePiece>();
                if (rtp != null && rtp.enabled)
                {
                    rtp.UpdateInitialRotation();
                }
            }

            // Update the piece information window
            SetPieceInfoData();

            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            foreach (GameObject go in _CP.selectedPieces)
            {
                go.transform.Rotate(new Vector3(0f, -90f, 0f));
                RotatePiece rtp = go.GetComponent<RotatePiece>();
                if (rtp != null && rtp.enabled)
                {
                    rtp.UpdateInitialRotation();
                }
            }

            // Update the piece information window
            SetPieceInfoData();

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
            if (_CP.result != null)
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

    // Set the spawn point
    public class SetSpawnPointCommand : ICommand<CommandParams>
    {
        private CommandParams _CP = new CommandParams();

        public SetSpawnPointCommand()
        {
            GameObject piece = selectedPiecesInPlace[0];
            GameObject spawnP = spawnPoints[currentHole];

            _CP.result = piece;
            _CP.prefab = spawnP;
            _CP.position = spawnP.transform.position;

            _CP.b = _CP.prefab.transform.GetChild(0).gameObject.activeSelf;
        }

        public CommandParams Do(CommandParams input = null)
        {
            _CP.prefab.transform.position = _CP.result.transform.position;
            _CP.prefab.transform.position = new Vector3(_CP.prefab.transform.position.x, _CP.prefab.transform.position.y + 0.2f, _CP.prefab.transform.position.z);

            _CP.prefab.transform.GetChild(0).gameObject.SetActive(true);
            return _CP;
        }

        public CommandParams Undo(CommandParams input = null)
        {
            _CP.prefab.transform.position = _CP.position;

            _CP.prefab.transform.GetChild(0).gameObject.SetActive(_CP.b);
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

    public Vector3 getSpawnPosition()
    {
        return spawnPoints[currentHole].transform.position;
    }


    // holeNb goes from 0 to 17
    public bool isHoleValid(int holeNb)
    {
        bool test = spawnPoints[holeNb].transform.GetChild(0).gameObject.activeSelf; // The spawnpoint must be placed
        if (test)
        {
            bool foundHole = false;
            List<GameObject> currentHolePieces = piecesInPlace[holeNb];
            foreach (GameObject piece in currentHolePieces)
            {
                if (piece.activeSelf)
                {
                    TerrainPiece tp = piece.GetComponent<TerrainPiece>();
                    if (tp.id.Substring(0, 4) == "Hole")    // /!\ ALL HOLE-TYPE PREFABS WILL NEED AN ID THAT STARTS WITH HOLE /!\
                    {
                        foundHole = true;
                        break;
                    }
                }
            }

            test &= foundHole;
        }

        return test;
    }

    public bool canStartTestMode()
    {
        return isHoleValid(currentHole);
    }

    public GameObject getCurrentHoleLevelProp()
    {
        return levelsProperties[currentHole];
    }

    private float ClampAngle(float angle, float min = 0f, float max = 360f)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    public void LoadLevel(string path)
    {
        CleanHoles();

        loader.LoadLevel(path);

        for (int i = 0; i < maxHoles; i++)
        {
            GameObject hole = holesObject.transform.Find("Hole " + (i + 1).ToString()).gameObject;
            GameObject spawnPoint = hole.transform.Find("Spawn Point").gameObject;
            spawnPoint.transform.GetChild(0).gameObject.SetActive(true);
            spawnPoints[i] = spawnPoint;
            levelsProperties[i] = hole.transform.Find("Level Properties").gameObject;

            foreach (Transform p in hole.transform)
            {
                if (!p.name.Equals("Spawn Point") && !p.name.Equals("Level Properties"))
                {
                    piecesInPlace[i].Add(p.gameObject);
                    RotatePiece rtp = p.gameObject.GetComponent<RotatePiece>();
                    if (rtp != null)
                        lemvpManager.AddRotatePiece(rtp);
                    MovingPiece mvp = p.gameObject.GetComponent<MovingPiece>();
                    if (mvp != null)
                        lemvpManager.AddMovingPiece(mvp);
                }
            }
        }

        panelExport.steamid = loader.steamid;
        panelExport.mapid = loader.mapid;
        panelExport.mapName = System.IO.Path.GetFileNameWithoutExtension(path);
    }

    private void CleanHoles()
    {
        List<GameObject> gos = new List<GameObject>();
        for (int i = 0; i < maxHoles; i++)
        {
            foreach (Transform t in holesObject.transform.Find("Hole " + (i + 1)))
            {
                gos.Add(t.gameObject);
            }
        }

        foreach (GameObject g in gos)
        {
            DestroyImmediate(g);
        }

        for (int i = 0; i < maxHoles; i++)
        {
            piecesInPlace[i].Clear();
            GameObject go = holesObject.transform.Find("Hole " + (i + 1)).gameObject;

            GameObject lvlProp = Instantiate(prefabLevelProperties);
            lvlProp.transform.parent = go.transform;
            lvlProp.name = prefabLevelProperties.name;
            levelsProperties[i] = lvlProp;

            GameObject spwn = Instantiate((i == 0) ? prefabSpawnPoint : prefabSpawnPointNoNetworkStart);
            spwn.transform.parent = go.transform;
            spwn.name = prefabSpawnPoint.name;
            spawnPoints[i] = spwn;
        }

        selectedPiecesInPlace.Clear();
    }

    private void SetupHoles()
    {
        List<string> dropOptions = new List<string>();

        piecesInPlace = new List<GameObject>[maxHoles];
        for (int i = 0; i < maxHoles; i++)
        {
            piecesInPlace[i] = new List<GameObject>();
            GameObject go = new GameObject("Hole " + (i + 1));
            go.transform.parent = holesObject.transform;

            // Add levelProperties and spawnPoint to holes
            GameObject lvlProp = Instantiate(prefabLevelProperties);
            lvlProp.transform.parent = go.transform;
            lvlProp.name = prefabLevelProperties.name;
            levelsProperties[i] = lvlProp;
            GameObject spwn = Instantiate((i == 0) ? prefabSpawnPoint : prefabSpawnPointNoNetworkStart);
            spwn.transform.parent = go.transform;
            spwn.name = prefabSpawnPoint.name;
            spawnPoints[i] = spwn;

            // Add "Hole i+1" to the hole selection dropdown
            dropOptions.Add("Hole " + (i + 1));
        }

        // Setup the dropdown
        holeSelection.AddOptions(dropOptions);
        holeSelection.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(holeSelection);
        });

        selectedPiecesInPlace = new List<GameObject>();

        currentHoleObject = GameObject.Find("Hole " + (currentHole + 1));
    }

    public void ReturnToMainMenu()
    {
        if (isModified)
        {
            saveBeforeExit.gameObject.SetActive(true);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void ShowEscapeMenu()
    {
        escapeMenu.gameObject.SetActive(true);
    }

    public void ShowHoleProperties()
    {
        if (panelHoleProperties.gameObject.activeSelf)
            panelHoleProperties.gameObject.SetActive(false);
        else
        {
            panelHoleProperties.Load(spawnPoints[currentHole], levelsProperties[currentHole]);
            panelHoleProperties.gameObject.SetActive(true);
        }
    }

    public void UpdateSpawnPoint(Vector3 newPos)
    {
        spawnPoints[currentHole].transform.position = newPos;
    }

    public void UpdateLevelProperties(int par, int maxshot, int time)
    {
        GameObject currentLevelProperties = levelsProperties[currentHole];

        LevelProperties levelProp = currentLevelProperties.GetComponent<LevelProperties>();

        levelProp.par = par;
        levelProp.maxShot = maxshot;
        levelProp.maxTime = time;

        levelsProperties[currentHole] = currentLevelProperties;
    }
}
