using System;
using System.Collections.Generic;
using UnityEngine;

public class Terrain
{
    public static int[] t1 = { 0, 2, 3 };
    public static int[] t2 = { 0, 3, 1 };
    GameObject TreeCls = (GameObject)Resources.Load("Prefabs/Tree");

    Vector3 centerPoint = new Vector3(0,0,0);
    float perlinSeed = 0.001f;
    

    public Terrain()
    {
        
        int startRow = 0;
        int startCol = 0;
        int numRows = Model.numRows;
        int numCols = Model.numCols;
        //do this once
        GenerateBasicMapPortion(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this in loop
        GeneratePerlinNoiseValueForTiles(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this once
        AddTerrainColorVariaton(startRow, startCol, startRow + numRows, startCol + numCols);

        //do this in a loop
        AddterrainHeightVariation(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this in a loop
        ConstructsTerrainExtraData(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this once
        AddTrees(startRow, startCol, startRow + numRows, startCol + numCols);
        //in a loop
        PlaceTrees(startRow, startCol, startRow + numRows, startCol + numCols);

        
    }

    

    private void GenerateBasicMapPortion(int startRow, int startCol, int endRow, int endCol)
    {
        Node s;

        
        List<List<Node>> map = Model.map;
        int tileSize = (int)Model.tileSize;

        for (int row = startRow; row < endRow ; row++)
        {
            map.Add(new List<Node>());
            for (int col = startCol; col < endCol; col++)
            {
                s = new Node();
                

                s.row = row;
                s.col = col;
                s.perlin1 = UnityEngine.Random.Range(0f, 1f);
                s.perlin2 = UnityEngine.Random.Range(0f, 1f);

                /////plane 1

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.tag = "ground";
                go.name = row.ToString() + "_" + col.ToString();
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();

                s.go = go;

                //Vector3 pos = go.transform.position;
                //pos.x += (float)tileSize / 2.0f;
                //pos.z += (float)tileSize / 2.0f;//move to bottom left corner
                //plane1.transform.position = pos;

                Color green = new Color(0, 1, 0, 1);
                renderer.material.color = green;

                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter.mesh;
                Vector3[] vertices1 = mesh.vertices;

                /*
                2------3
                |      |
                |      |
                0------1
                 */

                float hTile = Model.tileSize / 2;

                vertices1[0] = new Vector3(-hTile, 0.0f, -hTile);
                vertices1[1] = new Vector3(hTile, 0.0f, -hTile);
                vertices1[2] = new Vector3(-hTile, 0.0f, hTile);
                vertices1[3] = new Vector3(hTile, 0.0f, hTile);

                mesh.vertices = vertices1;
                mesh.MarkDynamic();

                Vector3 tilePos = go.transform.position;
                tilePos += centerPoint;

                bool circular = false;

                if(circular)
                {
                    /*
                    tilePos.y = Mathf.Sin(currYAngle) * Model.radius;
                    tilePos.x = Mathf.Cos(currYAngle) * Model.radius; //
                    float halfCircumference = Model.radius * (float)Math.PI;
                    tilePos.z = row * tileSize;
                    currYAngle -= (Model.tileSize / halfCircumference) * (float)Math.PI;
                    go.transform.position = tilePos;
                    t.RotateAround(t.position, t.forward, (currYAngle * Mathf.Rad2Deg) - 90.0f);
                    if (col % 2 == 0)
                    {
                        t.RotateAround(t.position, t.forward, Model.tileSize * 10);
                    }
                    */
                }
                else
                {
                    tilePos.z = row * tileSize;
                    tilePos.x = col * tileSize;
                    go.transform.position = tilePos;
                }

                map[row].Add(s);
            }
        }


    }

    private void GeneratePerlinNoiseValueForTiles(int startRow, int startCol, int endRow, int endCol)
    {
        Node s;
        List<List<Node>> map = Model.map;
        float peakHeight = Model.peakHeight;

        for (int row = startRow; row < endRow; row++)
        {
            for (int col = startCol; col < endCol; col++)
            {
                s = map[row][col];
                float f1 = (float)col;
                float f2 = (float)row;
                
                f1 += s.perlin1  + (perlinSeed);
                f2 += s.perlin2 + (perlinSeed );
                float noise = Mathf.PerlinNoise(f1, f2);
                float val = (noise * 2) - 1;
                //float randomValue = UnityEngine.Random.Range(0f, 1f);
                

                s.noise = noise;
                s.height = noise;// val * peakHeight;
            }
        }
        float dt = Time.deltaTime;
        perlinSeed += dt;
    }


    private void AddTerrainColorVariaton(int startRow, int startCol, int endRow, int endCol)
    {
        Node s;
        List<List<Node>> map = Model.map;
        float peakHeight = Model.peakHeight;

        for (int row = startRow; row < endRow; row++)
        {
            for (int col = startCol; col < endCol; col++)
            {
                s = map[row][col];

                //Debug.Log(noise);
                GameObject go = s.go;
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();

                Color color = renderer.material.color;
                color.g = s.noise;
                //renderer.material.color = color;

                //renderer.material.shader = Shader.Find("Unlit/Color");
                

                if (s.noise < 0.4f)
                {
                    s.terrainType = 0;

                    if(UnityEngine.Random.Range(0, 1.0f) > 0.5f)
                    {
                        renderer.material.SetColor("_Color", color);
                    }
                    else
                    {
                        renderer.material.SetColor("_Color", RandomBrownColor());
                    }
                    
                    //rend.material.mainTexture = Model.grass_texture;
                }
                else
                {
                     Color blue = Color.blue;
                    blue.b = s.noise;
                    //renderer.material.color = grey;
                    //rend.material.mainTexture = Model.ground_texture;
                    renderer.material.SetColor("_Color", blue);
                    
                }
                
                
            }
        }
    }

    private void AddterrainHeightVariation(int startRow, int startCol, int endRow, int endCol)
    {

        for (int row = startRow; row < endRow; row++)
        {
            for (int col = startCol; col < endCol; col++)
            {
                AlterVertices(row, col);
            }
        }
    }

    private void AddTrees(int startRow, int startCol, int endRow, int endCol)
    {
        Node s;
        List<List<Node>> map = Model.map;
       
        for (int row = startRow; row < endRow; row++)
        {
            for (int col = startCol; col < endCol; col++)
            {
                s = map[row][col];
                if(s.terrainType == 0)
                {
                    bool dotree1 = UnityEngine.Random.Range(0, 1.0f) > 0.5f;
                    bool dotree2 = UnityEngine.Random.Range(0, 1.0f) > 0.5f; ;
                    if(dotree1)
                    {
                        GameObject tree1 = MonoBehaviour.Instantiate(TreeCls);
                        tree1.name = "tree1";


                        tree1.transform.parent = s.go.transform;
                        tree1.transform.position = s.t1Center ;

                        Transform treeTopT = tree1.transform.Find("treeTop");
                        GameObject treeTop = treeTopT.gameObject;

                        MeshRenderer renderer = treeTop.GetComponent<MeshRenderer>();

                        Color color = renderer.material.color;
                        color.g = UnityEngine.Random.Range(0.4f, 0.8f);
                        //renderer.material.shader = Shader.Find("Unlit/Color");
                        renderer.material.SetColor("_Color", color);


                    }
                    
                    if(dotree2)
                    {
                        GameObject tree2 = MonoBehaviour.Instantiate(TreeCls);
                        tree2.name = "tree2";
                        tree2.transform.parent = s.go.transform;
                        tree2.transform.position = s.t2Center ;

                        Transform treeTopT = tree2.transform.Find("treeTop");
                        GameObject treeTop = treeTopT.gameObject;

                        MeshRenderer renderer = treeTop.GetComponent<MeshRenderer>();

                        Color color = renderer.material.color;
                        color.g = UnityEngine.Random.Range(0.4f, 0.8f);
                        //renderer.material.shader = Shader.Find("Unlit/Color");
                        renderer.material.SetColor("_Color", color);

                    }
                    
                }
            }
        }
    }

    Color RandomBrownColor()
    {
        float h = UnityEngine.Random.Range(0.05f, 0.15f);
        float s = UnityEngine.Random.Range(0.3f, 0.6f);
        float v = UnityEngine.Random.Range(0.3f, 0.8f);
        return Color.HSVToRGB(h, s, v);
    }

    private void PlaceTrees(int startRow, int startCol, int endRow, int endCol)
    {
        Node s;
        List<List<Node>> map = Model.map;

        for (int row = startRow; row < endRow; row++)
        {
            for (int col = startCol; col < endCol; col++)
            {
                s = map[row][col];
                if (s.terrainType == 0)
                {
                    Transform tree1 = s.go.transform.Find("tree1");
                    if(tree1)
                    {
                        Vector3 edge1 = s.t1Vertices[1] - s.t1Vertices[0];
                        Quaternion newRotation = Quaternion.LookRotation(edge1.normalized, s.t1Normal.normalized);
                        tree1.rotation = newRotation;
                    }

                    Transform tree2 = s.go.transform.Find("tree2");
                    if (tree2)
                    {
                        Vector3 edge2 = s.t2Vertices[1] - s.t2Vertices[0];
                        Quaternion newRotation = Quaternion.LookRotation(edge2.normalized, s.t2Normal.normalized);
                        tree2.rotation = newRotation;
                    }

                }
            }
        }
    }

    internal void addCol(int v)
    {
        Model.endCol++;
    }

    internal void addRow(int v)
    {
        return;
        Model.endRow++;
        int startRow = Model.endRow;
        int startCol = 0;
        int numRows = 1;
        int numCols = Model.numCols;
        GenerateBasicMapPortion(startRow, startCol, startRow + numRows, startCol + numCols);
        AddTerrainColorVariaton(startRow, startCol, startRow + numRows, startCol + numCols);
        AddterrainHeightVariation(startRow-1, startCol, (startRow-1) + numRows, startCol + numCols);
        ConstructsTerrainExtraData(startRow, startCol, startRow + numRows, startCol + numCols);
        //redo last row in order to fix collider
        ConstructsTerrainExtraData(startRow - 1, startCol, (startRow - 1) + numRows, startCol + numCols);
        AddTrees(startRow, startCol, startRow + numRows, startCol + numCols);
    }

    

    private void ConstructsTerrainExtraData(int startRow, int startCol, int endRow, int endCol)
    {
        Node s;

        List<List<Node>> map = Model.map;

        for (int row = startRow; row < endRow; row++)
        {
            for (int col = startCol; col < endCol; col++)
            {
                s = map[row][col];
                GameObject go = s.go;
                MeshFilter meshFilter = go.GetComponentInChildren<MeshFilter>();
                Mesh mesh = meshFilter.mesh;
                Vector3[] vertices = mesh.vertices;

                Vector3 tilePos = go.transform.position;

                Vector3 tileCenterPos = new Vector3(tilePos.x, tilePos.y, tilePos.z);


                ////caclulate the center
                s.t1Center = new Vector3(0, 0, 0);

                for (int j = 0; j < t1.Length; j++)
                {
                    int index = t1[j];
                    s.t1Center.x += vertices[index].x;
                    s.t1Center.z += vertices[index].z;
                    s.t1Center.y += vertices[index].y;
                }

                s.t1Center /= 3.0f;
                s.t1Center.x += tileCenterPos.x;
                s.t1Center.z += tileCenterPos.z;

                s.t2Center = new Vector3(0, 0, 0);
                for (int j = 0; j < t2.Length; j++)
                {
                    int index = t2[j];
                    s.t2Center.x += vertices[index].x;
                    s.t2Center.z += vertices[index].z;
                    s.t2Center.y += vertices[index].y;
                }

                s.t2Center /= 3.0f;
                s.t2Center.x += tileCenterPos.x;
                s.t2Center.z += tileCenterPos.z;
                ///


                for (int j = 0; j < t1.Length; j++)
                {
                    int index = t1[j];
                    s.t1Vertices[j] = vertices[index] + tileCenterPos;
                    //DrawDebugCircle(s.t1Vertices[j]);
                }

                Vector3 edge1 = s.t1Vertices[1] - s.t1Vertices[0];
                Vector3 edge2 = s.t1Vertices[2] - s.t1Vertices[1];

                // Calculate the cross product of the edges
                s.t1Normal = Vector3.Cross(edge1, edge2);


                for (int j = 0; j < t2.Length; j++)
                {
                    int index = t2[j];
                    s.t2Vertices[j] = vertices[index] + tileCenterPos;
                }

                edge1 = s.t2Vertices[1] - s.t2Vertices[0];
                edge2 = s.t2Vertices[2] - s.t2Vertices[1];

                // Calculate the cross product of the edges
                s.t2Normal = Vector3.Cross(edge1, edge2);

                //update the collider to alter like the terrain
                MeshCollider collider = go.GetComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                collider.sharedMesh.MarkDynamic();

            }
        }
    }

    

    internal void HighlightClickedNode(Node node)
    {
        GameObject go = node.go;
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        Color existingColor = renderer.material.color;
        Color color = Color.red;
        renderer.material.color = color;


        void MyAction()
        {
            renderer.material.color = existingColor;
        }

        Timer.CreateTimer(0.1f, MyAction);
    }

    
            

    

    

    public void Update()
    {
        int startRow = 0;
        int startCol = 0;
        int numRows = Model.numRows;
        int numCols = Model.numCols;
        //do this once
        //GenerateBasicMapPortion(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this in loop
        GeneratePerlinNoiseValueForTiles(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this once
        //AddTerrainColorVariaton(startRow, startCol, startRow + numRows, startCol + numCols);

        //do this in a loop
        AddterrainHeightVariation(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this in a loop
        ConstructsTerrainExtraData(startRow, startCol, startRow + numRows, startCol + numCols);
        //do this once
        //AddTrees(startRow, startCol, startRow + numRows, startCol + numCols);
        //in a loop
        PlaceTrees(startRow, startCol, startRow + numRows, startCol + numCols);
    }
    //alter the height of one of the vertices to make a hill or valley
    private void AlterVertex(int row, int col, int v, float h)
    {
        if (!TerrainUtils.NodeExists(row, col))
        {
            return;
        }

        List<List<Node>> map = Model.map;
        Node s = map[row][col];

        GameObject go = s.go;
        MeshFilter meshFilter = go.GetComponentInChildren<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        vertices[v].y = (h);
        mesh.vertices = vertices;

    }

    /*
    2------3
    |      |
    |      |
    0------1
     */

    bool AlterVertices(int row, int col)
    {
        if (!TerrainUtils.NodeExists(row, col))
        {
            return false;
        }

        List<List<Node>> map = Model.map;
        Node s = map[row][col];
        if (s.altered)
        {
            //return false;
        }
        float height = s.height;

        //this is me
        AlterVertex(row, col, 2, height);
        //this is left
        AlterVertex(row, col - 1, 3, height);
        //this is top
        AlterVertex(row + 1, col, 0, height);
        //this is top left
        AlterVertex(row + 1, col - 1, 1, height);

        s.altered = true;

        //GameObject go = s.go;
        MeshFilter meshFilter = s.go.GetComponentInChildren<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();


        return true;
    }


    
}
