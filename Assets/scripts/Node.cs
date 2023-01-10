using System;
using UnityEngine;

public class Node
{
    public GameObject go;
    public bool altered = false;
    public float height = 0;

    public Vector3 t1Center;
    public Vector3 t2Center;
    public Vector3[] t1Vertices = new Vector3[3];
    public Vector3[] t2Vertices = new Vector3[3];
    public Vector3 t1Normal;
    public Vector3 t2Normal;
    public int terrainType;
    public int row;
    public int col;
    public float noise;
    public float perlin1 = 0f;
    public float perlin2 = 0f;

    public Node()
    {
        terrainType = 1;
    }
}
