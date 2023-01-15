namespace RTS
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Terrain
    {
        private int[] t1 = { 0, 2, 3 };
        private int[] t2 = { 0, 3, 1 };


        GameObject TreeCls = (GameObject)Resources.Load("Prefabs/Tree");

        Vector3 centerPoint = new Vector3(0, 0, 0);
        float perlinSeed = 0.001f;
        private Vector3[] quadVertices = new Vector3[4];

        private Pool pool;

        public Color BROWN_COLOR { get; private set; }

        public Terrain()
        {
            pool = new Pool(TreeCls, Model.numCols * Model.numRows * 2);
            BROWN_COLOR = RandomBrownColor();
            int startRow = 0;
            int startCol = 0;
            int numRows = Model.numRows;
            int numCols = Model.numCols;

            float hTile = Model.tileSize / 2;

            quadVertices[0] = new Vector3(-hTile, 0.0f, -hTile);
            quadVertices[1] = new Vector3(hTile, 0.0f, -hTile);
            quadVertices[2] = new Vector3(-hTile, 0.0f, hTile);
            quadVertices[3] = new Vector3(hTile, 0.0f, hTile);

            CreateBtmQuad();
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

        public void Update()
        {
            //return;
            int startRow = 0;
            int startCol = 0;
            int numRows = Model.numRows;
            int numCols = Model.numCols;
            //do this once
            //GenerateBasicMapPortion(startRow, startCol, startRow + numRows, startCol + numCols);
            //do this in loop - this is SUPER important, it ill regenerate the map
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

        

        private void CreateBtmQuad()
        {
            float numRows = (float)Model.numRows;
            float numCols = (float)Model.numCols;
            float x = ((numCols / 2) * Model.tileSize);
            float z = ((numRows / 2) * Model.tileSize);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            mesh.vertices = quadVertices;

            go.transform.position = new Vector3(x - (Model.tileSize / 2), -.5f, z - (Model.tileSize / 2));
            go.transform.localScale = new Vector3(Model.tileSize * numCols * 2, 1, Model.tileSize * numRows * 2);
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            Color blue = Color.blue;
            renderer.material.shader = Shader.Find("Nature/SpeedTree");
            renderer.material.SetColor("_Color", blue);

            mesh.MarkDynamic();
        }




        private void GenerateBasicMapPortion(int startRow, int startCol, int endRow, int endCol)
        {
            Node s;


            List<List<Node>> map = Model.map;
            int tileSize = (int)Model.tileSize;
            int numRows = Model.numRows;
            int numCols = Model.numCols;

            for (int row = 0; row < numRows; row++)
            {
                map.Add(new List<Node>());
                for (int col = 0; col < numCols; col++)
                {
                    s = new Node();
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.tag = "ground";
                    s.go = go;
                    MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                    renderer.material.shader = Shader.Find("Nature/SpeedTree");
                    MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                    Mesh mesh = meshFilter.mesh;
                    s.perlin1 = 0.5f;// UnityEngine.Random.Range(0f, 1f);
                    s.perlin2 = 0.3f;// UnityEngine.Random.Range(0f, 1f);

                    s.landColor = RandomBrownColor();



                    /*
                    2------3
                    |      |
                    |      |
                    0------1
                     */

                    mesh.vertices = quadVertices;
                    mesh.MarkDynamic();
                    Vector3 tilePos = go.transform.position;
                    tilePos += centerPoint;
                    tilePos.z = row * tileSize;
                    tilePos.x = col * tileSize;
                    go.transform.position = tilePos;
                    Color green = new Color(0, 1, 0, 1);
                    renderer.material.color = green;
                    map[row].Add(s);



                    s.grid_row = row;
                    s.grid_col = col;
                    go.name = row.ToString() + "_" + col.ToString();



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
            }


        }

        private void GeneratePerlinNoiseValueForTiles(int startRow, int startCol, int endRow, int endCol)
        {
            Node s;
            List<List<Node>> map = Model.map;
            int numRows = Model.numRows;
            int numCols = Model.numCols;

            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    s = map[row][col];

                    int newCol = col + Model.colOffset;
                    int newRow = row + Model.rowOffset;

                    float f1 = (float)newCol;
                    float f2 = (float)newRow;


                    f1 += s.perlin1;
                    f2 += s.perlin2;

                    s.noiseBase = Mathf.PerlinNoise(f1, f2);

                    f1 += s.perlin1 + perlinSeed;
                    f2 += s.perlin2 + perlinSeed;

                    s.noise = Mathf.PerlinNoise(f1, f2);

                    Color brown = BROWN_COLOR;
                    brown.g = s.noiseBase;

                    s.landColor = brown;

                    Color blue = Color.blue;
                    blue.g = s.noiseBase;
                    s.seaColor = blue;


                    s.world_row = newRow;
                    s.world_col = newCol;
                    s.go.name = newRow.ToString() + "_" + newCol.ToString();
                }
            }
            //this is what creates the waves!!
            perlinSeed += (Time.deltaTime / Model.seedVariation);
        }


        private void AddTerrainColorVariaton(int startRow, int startCol, int endRow, int endCol)
        {
            Node s;
            List<List<Node>> map = Model.map;

            for (int row = startRow; row < endRow; row++)
            {
                for (int col = startCol; col < endCol; col++)
                {
                    s = map[row][col];

                    //Debug.Log(noise);
                    GameObject go = s.go;
                    MeshRenderer renderer = go.GetComponent<MeshRenderer>();


                    //renderer.material.color = color;

                    //renderer.material.shader = Shader.Find("Unlit/Color");


                    if (s.noiseBase < 0.4f)
                    {
                        s.terrainType = TileType.LAND;


                        renderer.material.SetColor("_Color", s.landColor);


                        //rend.material.mainTexture = Model.grass_texture;
                    }
                    else
                    {
                        s.terrainType = TileType.SEA;
                        //renderer.material.color = grey;
                        //rend.material.mainTexture = Model.ground_texture;
                        renderer.material.SetColor("_Color", s.seaColor);

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
                    if (s.terrainType == TileType.LAND)
                    {

                        bool dotree1 = true;// UnityEngine.Random.Range(0, 1.0f) > 0.5f;
                        bool dotree2 = true;// UnityEngine.Random.Range(0, 1.0f) > 0.5f;
                        if (dotree1)
                        {
                            if(s.tree1 == null)
                            {
                                s.tree1 = pool.get();
                                GameObject tree1 = s.tree1.o;

                                tree1.name = "tree1";

                                tree1.transform.parent = s.go.transform;
                                tree1.transform.position = s.t1Center;

                                Transform treeTopT = tree1.transform.Find("treeTop");
                                GameObject treeTop = treeTopT.gameObject;

                                MeshRenderer renderer = treeTop.GetComponent<MeshRenderer>();

                                Color color = renderer.material.color;
                                color.g = UnityEngine.Random.Range(0.4f, 0.8f);
                                //renderer.material.shader = Shader.Find("Unlit/Color");
                                renderer.material.SetColor("_Color", color);
                            }
                            
                        }

                        if (dotree2)
                        {
                            if (s.tree2 == null)
                            {
                                s.tree2 = pool.get();
                                GameObject tree2 = s.tree2.o;

                                tree2.name = "tree2";
                                tree2.transform.parent = s.go.transform;
                                tree2.transform.position = s.t2Center;

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
                    else
                    {
                        if(s.tree1 != null)
                        {
                            pool.putBack(s.tree1);
                            s.tree1 = null;
                        }
                        if (s.tree2 != null)
                        {
                            pool.putBack(s.tree2);
                            s.tree2 = null;
                        }
                    }
                }
            }
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
                    if (s.terrainType == TileType.LAND)
                    {
                        Transform tree1 = s.go.transform.Find("tree1");
                        if (tree1)
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

        Color RandomBrownColor()
        {
            float h = (0.05f + 0.15f) / 2;// UnityEngine.Random.Range(0.05f, 0.15f);
            float s = (0.3f + 0.6f) / 2; //UnityEngine.Random.Range(0.3f, 0.6f);
            float v = ((0.3f + 0.8f)) / 2; // UnityEngine.Random.Range(0.3f, 0.8f);
            return Color.HSVToRGB(h, s, v);
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

            vertices[v].y = h;
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
            float height = s.noise;

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

}
