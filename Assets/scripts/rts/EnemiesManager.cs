namespace RTS {
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class EnemiesManager : Engine.Actor
    {
        public enum EnemyState { ALIVE = 0, DEAD = 1 };

        private Engine.Pool pool;
        GameObject player;
        private Dictionary<string, EnemyStruct> enemiesMap;

        public class EnemyStruct
        {
            public Enemy enemy;
            public EnemyState state;
            public Quaternion lastRotation;

            public EnemyStruct(Enemy _enemy, EnemyState _state)
            {
                enemy = _enemy;
                state = _state;
            }

        }


        public EnemiesManager(RTSContext context, Engine.Pool _tanksPool) :base(context)
        {
            enemiesMap = new Dictionary<string, EnemyStruct>();
            pool = _tanksPool;
        }

        public void place()
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

        public void Update()
        {
            foreach (var item in enemiesMap)
            {
                EnemyStruct enemyStruct = item.Value;
                if(enemyStruct.enemy != null)
                {
                    enemyStruct.enemy.Update();
                }
            }
        }

        private void iterateLandNode(int row, int col)
        {
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            Node s = map[row][col];
            string key = (s.world_row - model.rowOffset ) + "_" + (s.world_col - model.colOffset);

            
;
            if (s.terrainType == TileType.LAND)
            {
                Debug.Log(key + " is land! row " + row + " col " + col);
                Enemy enemy = null; 
                if (!enemiesMap.ContainsKey(key))
                {
                    Debug.Log("adding tank to " + key + " for the first time");
                    enemiesMap.Add(key, new EnemyStruct((Enemy)pool.get(), EnemyState.ALIVE));

                    EnemyStruct enemyStruct = enemiesMap[key];
                    enemy = enemyStruct.enemy;
                    enemy.Init(row, col);
                    GameObject ent = enemy.GetGameObject();

                    ent.name = "tree1";
                    //ent.transform.parent = s.go.transform;
                    ent.transform.position = s.t1Center;


                    //PlaceTrees(row, col);

                }
                else
                {
                    //Debug.Log(key + " is a land node");
                    //entry already exists, tank does not
                    EnemyStruct enemyStruct = enemiesMap[key];
                    Debug.Log(key + " already exists");
                    enemy = enemyStruct.enemy;

                    if(enemy == null)
                    {
                        Debug.Log(key + " recreating tank");
                        enemy = (Enemy)pool.get();
                        enemyStruct.enemy = enemy;
                        enemy.Init(row, col);
                        GameObject ent = enemy.GetGameObject();


                        ent.name = "tree1";
                        //ent.transform.parent = s.go.transform;
                        ent.transform.position = s.t1Center;
                        ent.transform.rotation = enemyStruct.lastRotation;
                        //setGroundRotation(row, col);
                    }
                }

                
                if (player)
                {
                    enemy.SetTarget(player);
                }

            }
            else
            {
                Debug.Log(key + " is water");
                if (enemiesMap.ContainsKey(key))
                {
                    EnemyStruct enemyStruct = enemiesMap[key];
                    if (enemyStruct != null && enemyStruct.enemy != null)
                    {
                        Debug.Log(key + " removing tank!");

                        GameObject ent = enemyStruct.enemy.GetGameObject();
                        enemyStruct.lastRotation = ent.transform.rotation;
                        pool.putBack(enemyStruct.enemy);
                        enemyStruct.enemy = null;
                    }
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

/*

Transform treeTopT = tree1.transform.Find("treeTop");
GameObject treeTop = treeTopT.gameObject;

MeshRenderer renderer = treeTop.GetComponent<MeshRenderer>();

        Color color = renderer.material.color;
        color.g = UnityEngine.Random.Range(0.4f, 0.8f);
        //renderer.material.shader = Shader.Find("Unlit/Color");
        renderer.material.SetColor("_Color", color);
*/