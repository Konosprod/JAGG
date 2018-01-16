using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HoleInfo
{
    public Vector3 spawnPoint;
    public int par;
    public int maxShot;
    public float maxTime;
}

[Serializable]
public class Hole
{
    public HoleInfo properties;
    public List<Piece> pieces;
}

[Serializable]
public class Piece
{
    public string id;
    public Vector3 rotation;
    public Vector3 position;
    public Vector3 scale;
}

[Serializable]
public class CustomLevel {
    public string name;
    public string author;
    public List<Hole> holes;
}
