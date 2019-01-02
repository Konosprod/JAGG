using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListPrefabPanel : MonoBehaviour {

    public EditorManager editorManager;

    [Header("Navigation")]
    public Button StraightButton;
    public Button CornerButton;
    public Button OtherButton;
    public Button HoleButton;
    public Button SpecialsButton;
    public Button ImportButton;

    [Header("Contents")]
    public GameObject listStraight;
    public GameObject listCorner;
    public GameObject listOther;
    public GameObject listHole;
    public GameObject listSpecials;
    public GameObject listImport;

    [Header("Scroll Views")]
    public GameObject scrollViewStraight;
    public GameObject scrollViewCorner;
    public GameObject scrollViewOther;
    public GameObject scrollViewHole;
    public GameObject scrollViewSpecials;
    public GameObject scrollViewImport;

	// Use this for initialization
	void Start () {
        StraightButton.onClick.RemoveAllListeners();
        CornerButton.onClick.RemoveAllListeners();
        OtherButton.onClick.RemoveAllListeners();
        HoleButton.onClick.RemoveAllListeners();
        SpecialsButton.onClick.RemoveAllListeners();
        ImportButton.onClick.RemoveAllListeners();

        StraightButton.onClick.AddListener(ShowStraight);
        CornerButton.onClick.AddListener(ShowCorner);
        OtherButton.onClick.AddListener(ShowOther);
        HoleButton.onClick.AddListener(ShowHole);
        SpecialsButton.onClick.AddListener(ShowSpecials);
        ImportButton.onClick.AddListener(ShowImport);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void ShowStraight()
    {
        scrollViewStraight.SetActive(true);
        scrollViewCorner.SetActive(false);
        scrollViewOther.SetActive(false);
        scrollViewHole.SetActive(false);
        scrollViewImport.SetActive(false);
        scrollViewSpecials.SetActive(false);
    }

    private void ShowCorner()
    {
        scrollViewStraight.SetActive(false);
        scrollViewCorner.SetActive(true);
        scrollViewOther.SetActive(false);
        scrollViewHole.SetActive(false);
        scrollViewImport.SetActive(false);
        scrollViewSpecials.SetActive(false);
    }

    private void ShowOther()
    {
        scrollViewStraight.SetActive(false);
        scrollViewCorner.SetActive(false);
        scrollViewOther.SetActive(true);
        scrollViewHole.SetActive(false);
        scrollViewImport.SetActive(false);
        scrollViewSpecials.SetActive(false);
    }

    private void ShowHole()
    {
        scrollViewStraight.SetActive(false);
        scrollViewCorner.SetActive(false);
        scrollViewOther.SetActive(false);
        scrollViewHole.SetActive(true);
        scrollViewImport.SetActive(false);
        scrollViewSpecials.SetActive(false);
    }

    private void ShowSpecials()
    {
        scrollViewStraight.SetActive(false);
        scrollViewCorner.SetActive(false);
        scrollViewOther.SetActive(false);
        scrollViewHole.SetActive(false);
        scrollViewImport.SetActive(false);
        scrollViewSpecials.SetActive(true);
    }

    private void ShowImport()
    {
        scrollViewStraight.SetActive(false);
        scrollViewCorner.SetActive(false);
        scrollViewOther.SetActive(false);
        scrollViewHole.SetActive(false);
        scrollViewSpecials.SetActive(false);
        scrollViewImport.SetActive(true);
    }
    
    public void AddImportedPiece(GameObject piece)
    {
        GameObject previewImage = new GameObject(piece.name);

        previewImage.AddComponent<RectTransform>();
        previewImage.AddComponent<LayoutElement>();

        Image pi_im = previewImage.AddComponent<Image>();
        Texture2D texture = RuntimePreviewGenerator.GenerateModelPreview(piece.transform, 128, 128, true);

        pi_im.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        UIPieceHandler uiph = previewImage.AddComponent<UIPieceHandler>();
        uiph.editorMan = editorManager;

        previewImage.transform.SetParent(listImport.transform);
    }

    public void AddPiece(GameObject pref)
    {
        string id = pref.GetComponent<TerrainPiece>().id;
        string type = "";

        if (id.IndexOf("/") > 0)
            type = id.Substring(0, id.IndexOf("/")).ToLower();

        if (type != "")
        {
            GameObject previewImage = new GameObject(pref.name);

            previewImage.AddComponent<RectTransform>();
            previewImage.AddComponent<LayoutElement>();

            Image pi_im = previewImage.AddComponent<Image>();
            pi_im.sprite = Resources.Load<Sprite>("Previews/" + pref.name + "Preview");

            UIPieceHandler uiph = previewImage.AddComponent<UIPieceHandler>();
            uiph.editorMan = editorManager;

            if (type == "straight")
                previewImage.transform.SetParent(listStraight.transform);
            else if(type == "corner")
                previewImage.transform.SetParent(listCorner.transform);
            else if (type == "other")
                previewImage.transform.SetParent(listOther.transform);
            else if (type == "hole")
                previewImage.transform.SetParent(listHole.transform);
            else if (type == "specials")
                previewImage.transform.SetParent(listSpecials.transform);
        }
    }
}
