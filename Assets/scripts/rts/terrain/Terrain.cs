﻿namespace RTS
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Terrain : Engine.Actor
    {

        GameObject player;
        Vector3 centerPoint = new Vector3(0, 0, 0);
        GameObject center;


        float perlinSeed = 0.001f;
        private Engine.Pool pool;

        public Color BROWN_COLOR { get; private set; }

        private float LAND_THRESHOLD = 0.4f;



        public Terrain(RTSContext _context, Engine.Pool _tanksPool):base(_context)
        {
            RTSModel model = getModel() as RTSModel;
            pool = _tanksPool;
            BROWN_COLOR = RandomBrownColor();
            int startRow = 0;
            int startCol = 0;
            int numRows = model.numRows;
            int numCols = model.numCols;

            TerrainDef.SetTerrainDef(model);

            centerPoint.x = (numRows * model.tileSize) / 2;
            centerPoint.z = (numRows * model.tileSize) / 2;
            centerPoint.y = -(numRows / 2);
            center = new GameObject();
            center.transform.position = centerPoint;



            //CreateBtmQuad();
            //do this once
            GenerateBasicMapPortion(startRow, startCol, startRow + numRows, startCol + numCols);

            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    //do this in loop
                    GeneratePerlinNoiseValueForTiles(row, col);
                    AddTerrainColorVariaton(row, col);
                    AlterVertices(row, col);
                    ConstructsTerrainExtraData(row, col);
                    iterateLandNode(row, col);
                    
                }
            }
        }

        public void setPlayer(GameObject _player)
        {
            player = _player;
        }

        //-1 deltaRows means down, 1 is up
        public void setNewRowsCols(int deltaRows, int deltaCols)
        {
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            int numRows = model.numRows;
            int numCols = model.numCols;

            if (deltaRows == -1)
            {
                for (int col = 0; col < numCols; col++)
                {
                    map[0][col].alpha = 0f;
                }
            }
            if (deltaRows == 1)
            {
                for (int col = 0; col < numCols; col++)
                {
                    map[numRows - 1][col].alpha = 0f;
                }
            }
            if (deltaCols == -1)
            {
                for (int row = 0; row < numRows; row++)
                {
                    map[row][0].alpha = 0f;
                }
            }
            if (deltaCols == 1)
            {
                for (int row = 0; row < numRows; row++)
                {
                    map[row][numCols - 1].alpha = 0f;
                }
            }
        }

        public void Update()
        {
            RTSModel model = getModel() as RTSModel;
            int numRows = model.numRows;
            int numCols = model.numCols;
            //do this once
            //do this in loop - this is SUPER important, it ill regenerate the map
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    //do this in loop
                    GeneratePerlinNoiseValueForTiles(row, col);
                    AddTerrainColorVariaton(row, col);
                    AlterVertices(row, col);
                    ConstructsTerrainExtraData(row, col);
                    iterateLandNode(row, col);
                    ProcessAlpha(row, col);
                }
            }


            //this is what creates the waves!!
            perlinSeed += (Time.deltaTime / model.seedVariation);
        }

        private void ProcessAlpha(int row, int col)
        {
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            Node s = model.map[row][col];
            if (s.alpha < 1f)
            {
                s.alpha += 0.01f;
            }

        }

        private void GenerateBasicMapPortion(int startRow, int startCol, int endRow, int endCol)
        {
            Node s;
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            int tileSize = (int)model.tileSize;
            int numRows = model.numRows;
            int numCols = model.numCols;

            for (int row = 0; row < numRows; row++)
            {
                map.Add(new List<Node>());
                for (int col = 0; col < numCols; col++)
                {
                    s = new Node((RTSContext)getContext());
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.tag = "ground";
                    s.go = go;
                    MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                    //renderer.material.shader = Shader.Find("Unlit/Color");
                    //renderer.material.shader = Shader.Find("Nature/SpeedTree");
                    MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                    Mesh mesh = meshFilter.mesh;
                    s.perlin1 = Mathf.PerlinNoise((float)row + 0.01f, (float)col + 0.01f);
                    s.perlin2 = Mathf.PerlinNoise((float)row + 0.02f, (float)col + 0.02f);

                    s.landColor = RandomBrownColor();



                    /*
                    2------3
                    |      |
                    |      |
                    0------1
                     */

                    mesh.vertices = TerrainDef.quadVertices;
                    mesh.triangles = TerrainDef.indices;

                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    mesh.MarkDynamic();
                    Vector3 tilePos = go.transform.position;
                    //tilePos += centerPoint;
                    tilePos.z = row * tileSize;
                    tilePos.x = col * tileSize;
                    go.transform.position = tilePos;
                    Color green = new Color(0, 1, 0, 1);
                    renderer.material.color = green;
                    map[row].Add(s);
                    s.grid_row = row;
                    s.grid_col = col;
                    go.name = row.ToString() + "_" + col.ToString();


                }
            }


        }

        private void GeneratePerlinNoiseValueForTiles(int row, int col)
        {
            Node s;
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;

            s = map[row][col];

            //this is the magic that makes the map endless
            int newCol = col + model.colOffset;
            int newRow = row + model.rowOffset;


            float f1 = (float)newCol + 0.1f;
            float f2 = (float)newRow + 0.1f;

            //this is used to find if we are working with land or sea, nver changes
            s.noiseBase = Mathf.PerlinNoise(f1, f2);

            f1 += s.perlin1 + perlinSeed;
            f2 += s.perlin2 + perlinSeed;

            s.noise = Mathf.PerlinNoise(f1, f2);

            Color brown = BROWN_COLOR;
            brown.g = s.noiseBase;

            s.landColor = brown;

            Color blue = Color.blue;
            blue.g = s.noise + (s.noiseBase);
            s.seaColor = blue;


            s.world_row = newRow;
            s.world_col = newCol;
            s.go.name = newRow.ToString() + "_" + newCol.ToString();



        }


        private void AddTerrainColorVariaton(int row, int col)
        {
            Node s;
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;


            s = map[row][col];

            //Debug.Log(noise);
            GameObject go = s.go;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            if (s.noiseBase < LAND_THRESHOLD)
            {
                s.terrainType = TileType.LAND;

                Color c = new Color(s.landColor.r * s.alpha, s.landColor.g * s.alpha, s.landColor.b * s.alpha);
                renderer.material.SetColor("_Color", c);


                //rend.material.mainTexture = Model.grass_texture;
            }
            else
            {
                s.terrainType = TileType.SEA;

                if(!s.highlighted)
                {
                    Color c = new Color(s.seaColor.r * s.alpha, s.seaColor.g * s.alpha, s.seaColor.b * s.alpha);
                    //renderer.material.color = grey;
                    //rend.material.mainTexture = Model.ground_texture;
                    renderer.material.SetColor("_Color", c);
                }
            }
        }



        private void iterateLandNode(int row, int col)
        {
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            Node s = map[row][col];
            if (s.terrainType == TileType.LAND)
            {

                bool dotree1 = s.noiseBase > 0.3f;
                bool dotree2 = false;// Mathf.PerlinNoise(s.t2Center.x, s.t2Center.z) > 0.5f;
                if (dotree1)
                {
                    if (s.tree1 == null)
                    {
                        s.tree1 = pool.get();
                        Tank t = (Tank)s.tree1.o;
                        t.Init(row, col);
                        GameObject tree1 = t.GetPlayer();

                        tree1.name = "tree1";

                        tree1.transform.parent = s.go.transform;
                        tree1.transform.position = s.t1Center;
                        PlaceTrees(row, col);
                        /*

                        Transform treeTopT = tree1.transform.Find("treeTop");
                        GameObject treeTop = treeTopT.gameObject;

                        MeshRenderer renderer = treeTop.GetComponent<MeshRenderer>();

                                Color color = renderer.material.color;
                                color.g = UnityEngine.Random.Range(0.4f, 0.8f);
                                //renderer.material.shader = Shader.Find("Unlit/Color");
                                renderer.material.SetColor("_Color", color);
                       */
                    }
                    if (player)
                    {
                        Tank t = (Tank)s.tree1.o;
                        t.SetTarget(player);
                    }
                }

                if (dotree2)
                {
                    if (s.tree2 == null)
                    {
                        s.tree2 = pool.get();
                        Tank t = (Tank)s.tree2.o;
                        t.Init(row, col);
                        if (player)
                        {
                            t.SetTarget(player);
                        }
                        
                        GameObject tree2 = t.GetPlayer();

                        tree2.name = "tree2";
                        tree2.transform.parent = s.go.transform;
                        tree2.transform.position = s.t2Center;
                        PlaceTrees(row, col);
                    }
                }

            }
            else
            {
                if (s.tree1 != null)
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



        private void PlaceTrees(int row, int col)
        {
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            Node s = map[row][col];
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

        Color RandomBrownColor()
        {
            float h = (0.05f + 0.15f) / 2;// UnityEngine.Random.Range(0.05f, 0.15f);
            float s = (0.3f + 0.6f) / 2; //UnityEngine.Random.Range(0.3f, 0.6f);
            float v = ((0.3f + 0.8f)) / 2; // UnityEngine.Random.Range(0.3f, 0.8f);
            return Color.HSVToRGB(h, s, v);
        }


        private void ConstructsTerrainExtraData(int row, int col)
        {
            Node s;
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            s = map[row][col];
            GameObject go = s.go;
            MeshFilter meshFilter = go.GetComponentInChildren<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;

            Vector3 tilePos = go.transform.position;

            Vector3 tileCenterPos = new Vector3(tilePos.x, tilePos.y, tilePos.z);


            ////caclulate the center
            s.t1Center = new Vector3(0, 0, 0);

            for (int j = 0; j < TerrainDef.t1.Length; j++)
            {
                int index = TerrainDef.t1[j];
                s.t1Center.x += vertices[index].x;
                s.t1Center.z += vertices[index].z;
                s.t1Center.y += vertices[index].y;
            }

            s.t1Center /= 3.0f;
            s.t1Center.x += tileCenterPos.x;
            s.t1Center.z += tileCenterPos.z;

            s.t2Center = new Vector3(0, 0, 0);
            for (int j = 0; j < TerrainDef.t2.Length; j++)
            {
                int index = TerrainDef.t2[j];
                s.t2Center.x += vertices[index].x;
                s.t2Center.z += vertices[index].z;
                s.t2Center.y += vertices[index].y;
            }

            s.t2Center /= 3.0f;
            s.t2Center.x += tileCenterPos.x;
            s.t2Center.z += tileCenterPos.z;
            ///


            for (int j = 0; j < TerrainDef.t1.Length; j++)
            {
                int index = TerrainDef.t1[j];
                s.t1Vertices[j] = vertices[index] + tileCenterPos;
                //DrawDebugCircle(s.t1Vertices[j]);
            }

            Vector3 edge1 = s.t1Vertices[1] - s.t1Vertices[0];
            Vector3 edge2 = s.t1Vertices[2] - s.t1Vertices[1];

            // Calculate the cross product of the edges
            s.t1Normal = Vector3.Cross(edge1, edge2);


            for (int j = 0; j < TerrainDef.t2.Length; j++)
            {
                int index = TerrainDef.t2[j];
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

        internal void HighlightClickedNode(Node node)
        {
            GameObject go = node.go;
            node.highlighted = true;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            Color existingColor = renderer.material.color;
            Color color = Color.red;
            renderer.material.color = color;


            void MyAction()
            {
                node.highlighted = false;
                renderer.material.color = existingColor;
            }

            Engine.Timer.CreateTimer(0.1f, MyAction);
        }



        //alter the height of one of the vertices to make a hill or valley
        private void AlterVertex(int row, int col)
        {
            RTSModel model = getModel() as RTSModel;

            List<List<Node>> map = model.map;

            if (!TerrainUtils.NodeExists(map, row, col))
            {
                return;
            }

            
            Node s = map[row][col];

            

            GameObject go = s.go;
            MeshFilter meshFilter = go.GetComponentInChildren<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;
            Transform t = go.transform;
            if (s.terrainType == TileType.LAND)
            {
                Vector3 pos = new Vector3(t.position.x, 0.20f, t.position.z);
                go.transform.position = pos;
            }
            else
            {
                Vector3 pos = new Vector3(t.position.x, 0.0f, t.position.z);
                go.transform.position = pos;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                float c = (float)col + model.colOffset;
                float r = (float)row + model.rowOffset;
                float f1 = c + vertices[i].x;
                float f2 = r + vertices[i].z;
                float noise = 0;
                if (s.terrainType == TileType.LAND)
                {
                    noise = Mathf.PerlinNoise(s.noiseBase + f1, f2 + s.noiseBase);
                }
                else
                {
                    noise = Mathf.PerlinNoise(f1 + perlinSeed, f2 + perlinSeed);
                }

                 
                vertices[i].y = noise;
            }

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
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;

            if (!TerrainUtils.NodeExists(map,row, col))
            {
                return false;
            }
            
            Node s = map[row][col];
            if (s.altered)
            {
                //return false;
            }
            float height = s.noise;


            //this is me - i am bottom RIGHT
            AlterVertex(row, col);
            //this is left
            AlterVertex(row, col - 1);
            //this is top
            AlterVertex(row + 1, col);
            //this is top left
            AlterVertex(row + 1, col - 1);

            s.altered = true;


            return true;
        } 
    }
}
