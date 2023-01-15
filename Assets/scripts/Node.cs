
namespace RTS
{
    using UnityEngine;

    public class Node
    {
        public GameObject go;
        public bool altered = false;
        //public float height = 0;

        public Vector3 t1Center;
        public Vector3 t2Center;
        public Vector3[] t1Vertices = new Vector3[3];
        public Vector3[] t2Vertices = new Vector3[3];
        public Vector3 t1Normal;
        public Vector3 t2Normal;
        public TileType terrainType;
        public int grid_row;
        public int grid_col;
        public int world_row;
        public int world_col;
        public float noise;
        public float perlin1 = 0f;
        public float perlin2 = 0f;
        public Color seaColor;
        public Color landColor;
        public float noiseBase;

        public PoolItem tree1 = null;
        public PoolItem tree2 = null;

        public Node()
        {
            terrainType = TileType.SEA;
        }
    }
}

