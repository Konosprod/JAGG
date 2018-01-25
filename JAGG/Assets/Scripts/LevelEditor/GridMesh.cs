using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GridMesh : MonoBehaviour
{
    public int GridSize;

    void Awake()
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        var vertices = new List<Vector3>();

        var indices = new List<int>();
        for (int i = 0; i < GridSize+1; i+=2)
        {
            vertices.Add(new Vector3(i, 0, 0));
            vertices.Add(new Vector3(i, 0, GridSize));

            indices.Add(2 * i + 0);
            indices.Add(2 * i + 1);

            vertices.Add(new Vector3(0, 0, i));
            vertices.Add(new Vector3(GridSize, 0, i));

            indices.Add(2 * i + 2);
            indices.Add(2 * i + 3);
        }

        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = Color.white;
    }
}