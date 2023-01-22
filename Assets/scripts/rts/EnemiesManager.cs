namespace RTS {
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class EnemiesManager : Engine.Actor
    {
        private Engine.Pool pool;
        GameObject player;


        public EnemiesManager(RTSContext context, Engine.Pool _tanksPool) :base(context)
        {
            pool = _tanksPool;
        }

        public void Update()
        {
            RTSModel model = getModel() as RTSModel;
            int numRows = model.numRows;
            int numCols = model.numCols;
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    iterateLandNode(row, col);
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
                        GameObject tree1 = t.GetGameObject();

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
                    else
                    {
                        Tank t = (Tank)s.tree1.o;
                        t.Update();
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

                        GameObject tree2 = t.GetGameObject();

                        tree2.name = "tree2";
                        tree2.transform.parent = s.go.transform;
                        tree2.transform.position = s.t2Center;
                        PlaceTrees(row, col);
                    }
                    else
                    {
                        Tank t = (Tank)s.tree2.o;
                        t.Update();
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


        public void setPlayer(GameObject _player)
        {
            player = _player;
        }
    }
}
    
