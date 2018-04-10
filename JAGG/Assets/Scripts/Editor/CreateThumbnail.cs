using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class CreateThumbnail : Editor {

    [MenuItem("Tools/Create preview for prefabs")]
    private static void CreatePreviews()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Terrain");
        foreach (GameObject pref in prefabs)
        {
            Debug.Log("Generating : " + pref.name); 
            string path = "Resources/Previews/" + pref.name + "Preview.png";
            Texture2D prev = AssetPreview.GetAssetPreview(pref);
            Texture2D prev_copy = new Texture2D(prev.width, prev.height, prev.format, prev.mipmapCount > 1);
            prev_copy.LoadRawTextureData(prev.GetRawTextureData());
            byte[] b_prev = prev_copy.EncodeToPNG();

            File.WriteAllBytes(Application.dataPath+"/"+ path, b_prev);
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset("Assets/" + path);

            TextureImporter importer = AssetImporter.GetAtPath("Assets/"+path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();

        }
    }
}
