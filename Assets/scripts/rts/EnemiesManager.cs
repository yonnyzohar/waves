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
            public Node s;
            public int globalRow = 0;
            public int globalCol = 0;

            public EnemyStruct(Enemy _enemy, EnemyState _state, Node _s)
            {
                enemy = _enemy;
                state = _state;
                s = _s;
            }

        }

        /*
         The code here is a bit tricky. we have a global endless map and then there are the
        local tiles we show on screen. we store the global ones with elephants in enemiesMap
        - but they are global. we need to map them to the ones currently on screen, and show the
        enemy in the correct rotation
         */


        public EnemiesManager(RTSContext context, Engine.Pool _tanksPool) :base(context)
        {
            enemiesMap = new Dictionary<string, EnemyStruct>();
            pool = _tanksPool;
        }

        //find out if the enemy on a global tile is currently on screen
        public void place()
        {
            RTSModel model = getModel() as RTSModel;
            int numRows = model.numRows;
            int numCols = model.numCols;

            foreach (var item in enemiesMap)
            {
                EnemyStruct enemyStruct = item.Value;
                if(enemyStruct.enemy == null)
                {
                    continue;
                }
                int globalRow = enemyStruct.globalRow;
                int globalCol = enemyStruct.globalCol;

                bool display = true;
                if (globalRow >= numRows + model.rowOffset)
                {
                    display = false;
                }
                else
                {
                    if (globalRow < model.rowOffset)
                    {
                        display = false;
                    }
                    else
                    {
                        if (globalCol >= numCols + model.colOffset)
                        {
                            display = false;
                        }
                        else
                        {
                            if (globalCol < model.colOffset)
                            {
                                display = false;
                            }
                        }
                    }
                }


                if (!display)
                {
                    GameObject ent = enemyStruct.enemy.GetGameObject();
                    pool.putBack(enemyStruct.enemy);
                    enemyStruct.enemy = null;
                }
            }

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
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            foreach (var item in enemiesMap)
            {
                EnemyStruct enemyStruct = item.Value;
                if(enemyStruct.enemy != null)
                {
                    Enemy enemy = enemyStruct.enemy;
                    enemy.Update();
                    GameObject ent = enemy.GetGameObject();
                    enemyStruct.lastRotation = ent.transform.rotation;

                    Node s2 = map[enemyStruct.globalRow - model.rowOffset][enemyStruct.globalCol - model.colOffset];

                    float a = s2.alpha;
                    Vector3 scale = new Vector3(a, a, a);
                    ent.transform.localScale = scale;

                }
            }
        }

        private void iterateLandNode(int row, int col)
        {
            RTSModel model = getModel() as RTSModel;
            List<List<Node>> map = model.map;
            Node s = map[row][col];
            string key = s.world_row + "_" + s.world_col;

            if (s.terrainType == TileType.LAND)
            {
                Debug.Log(key + " is land! row " + row + " col " + col);
                Enemy enemy = null; 
                if (!enemiesMap.ContainsKey(key))
                {
                    Debug.Log("adding tank to " + key + " for the first time");
                    EnemyStruct enemyStruct = new EnemyStruct((Enemy)pool.get(), EnemyState.ALIVE, s);
                    enemyStruct.globalCol = s.world_col;
                    enemyStruct.globalRow = s.world_row;

                    enemiesMap.Add(key, enemyStruct);

                    enemy = enemyStruct.enemy;


                    Node s2 = map[row][col];

                    enemy.Init(row, col);
                    GameObject ent = enemy.GetGameObject();

                    ent.name = "tree1";
                    //ent.transform.parent = s.go.transform;
                    ent.transform.position = s2.t1Center;


                    //PlaceTrees(row, col);

                }
                else
                {
                    //Debug.Log(key + " is a land node");
                    //entry already exists, tank does not
                    EnemyStruct enemyStruct = enemiesMap[key];
                    Debug.Log(key + " already exists");
                    enemy = enemyStruct.enemy;
                    GameObject ent;
                    if (enemy == null)
                    {
                        Debug.Log(key + " recreating tank");
                        enemy = (Enemy)pool.get();
                        enemyStruct.s = s;
                        enemyStruct.enemy = enemy;
                        enemy.Init(row, col);
                        ent = enemy.GetGameObject();
                        ent.name = "tree1";
                        //ent.transform.parent = s.go.transform;
                        
                        
                        //setGroundRotation(row, col);
                    }
  
                    Node s2 = map[row][col];
                    ent = enemy.GetGameObject();
                    ent.transform.position = s2.t1Center;
                    ent.transform.rotation = enemyStruct.lastRotation;
                }

                
                if (player)
                {
                    enemy.SetTarget(player);
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