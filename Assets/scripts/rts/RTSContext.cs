namespace RTS
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using Engine;

    public class RTSContext : Engine.Context
    {
        CamCtrl camScript;
        Player player;
        Terrain terrain;
        RTSConfigs commandConfigs;
        Astar aStar;
        private EnemiesManager enemiesManager;

        GameObject light;
        //GameObject camCtrl;
        private Dictionary<string, Engine.Pool> allPools = new Dictionary<string, Engine.Pool>();

        private void EntityPoolCreateFnc(object ent)
        {
            Entity e = ent as Entity;
            e.GetGameObject().SetActive(false);
        }

        private void EntityPoolRetreiveFnc(object ent)
        {
            Entity e = ent as Entity;
            e.GetGameObject().SetActive(true);
        }


        public RTSContext()
        {
            UnityEngine.Random.InitState(1);
            aStar = new Astar();
            model = new RTSModel();

            RTSModel myModel = (RTSModel)model;

            commandConfigs = new RTSConfigs();
            commandConfigs.init();
            factory = new Engine.Factory(commandConfigs, this);

            GameObject EnemyCls = (GameObject)Resources.Load("Prefabs/elephant");
            EnemyCls.SetActive(false);

            int numOfPoolInsances = myModel.numCols * myModel.numRows;
            allPools.Add("Enemy", new Engine.Pool(this, typeof(Enemy), EnemyCls, numOfPoolInsances, EntityPoolCreateFnc, EntityPoolRetreiveFnc));
            terrain = new Terrain(this);

            int numRows = myModel.numRows;
            int numCols = myModel.numCols;
            List<List<Node>> map = myModel.map;
            int tileSize = (int)myModel.tileSize;

            int halfW = (numCols) / 2;
            int halfH = (numRows) / 2;

            light = GameObject.Find("Light");
            player = new Player(this, halfH, halfW, OnTileMoved, onTileStartMove, (GameObject)Resources.Load("Prefabs/Ship"));
            player.Init(halfH, halfW);

            camScript = new CamCtrl(this);
            enemiesManager = new EnemiesManager(this, allPools["Enemy"]);
            camScript.setTarget(player.GetGameObject());
            enemiesManager.setPlayer(player.GetGameObject());

            camScript.addListener("CLICK_ON_TERRAIN", onClick);
            enemiesManager.place();



            //SequenceCommand myCmd = factory.getCommand("mSequence") as SequenceCommand;
            //myCmd.addListener(Command.COMPLETE_MSG, onMyCmdComplete);
            //myCmd.execute();
        }

        private void onMyCmdComplete(Actor dispatcher)
        {
            dispatcher.removeListener(Command.COMPLETE_MSG, onMyCmdComplete);
        }

        public void onTileStartMove(int deltaRows, int deltaCols)
        {
            Debug.Log("WOW");
            terrain.setRowsColsStartOfMove(deltaRows, deltaCols);
        }

        public void OnTileMoved(int deltaRows, int deltaCols)
        {
            RTSModel myModel = (RTSModel)model;
            myModel.rowOffset += deltaRows;
            myModel.colOffset += deltaCols;

            Debug.Log(
                "deltaRows " + deltaRows +
                " deltaCols " + deltaCols +
                " rowOffset " + myModel.rowOffset +
                " colOffset " + myModel.colOffset
             );

            //this is will make the camera jump to the new offset insted of tweaning to it
            camScript.FocusOnTarget(player.GetGameObject(), true);
            terrain.setNewRowsColsEndOfMove(deltaRows, deltaCols);
            enemiesManager.place();
        }

        private void onClick(Actor dispatcher)
        {
            Travel(camScript.ClickedPoint, camScript.ClickedGameObject);

        }

        private void Travel(Vector3 clickedPoint, GameObject clickedGameObject)
        {

            //string[] row_col = clickedGameObject.name.Split('_');
            RTSModel myModel = (RTSModel)model;
            float tileSize = (float)myModel.tileSize;
            //tiles have center registration so we need to add half w and h to get accurate tile
            int grid_row = (int)((clickedPoint.z + (tileSize / 2)) / tileSize);
            int grid_col = (int)((clickedPoint.x + (tileSize / 2)) / tileSize);

            int world_row = grid_row + myModel.rowOffset;
            int world_col = grid_col + myModel.colOffset;

            int player_world_row = player.world_row;
            int player_world_col = player.world_col;

            int player_grid_row = player.grid_row;
            int player_grid_col = player.grid_col;

            //int col = Int32.Parse(row_col[1]);
            //int row = Int32.Parse(row_col[0]);

            Debug.Log(" grid_row " + grid_row + " grid_col " + grid_col + " name " + clickedGameObject.name);
            Debug.Log(" player.row " + player.grid_row + " player.col " + player.grid_col);
            List<List<Node>> map = myModel.map;
            if (!TerrainUtils.NodeExists(map, grid_row, grid_col))
            {
                return;
            }

            

            List<RowCol> path = aStar.CreatePath(map, 1, map[player_grid_row][player_grid_col], map[grid_row][grid_col]);
            if (path != null && path.Count > 0)
            {
                //clickIndicator.ShowIndication(map[row][col]);
                camScript.setTarget(player.GetGameObject());
                player.SetPath(path, clickedPoint);
                terrain.HighlightClickedNode(map[grid_row][grid_col]);

                player.addListener("MOVE_COMPLETE", onMoveComplete);
            }
            else
            {
                Debug.Log("INVALID PATH");
            }
        }

        private void onMoveComplete(Actor disaptcher)
        {
            player.removeListener("MOVE_COMPLETE", onMoveComplete);

            camScript.setTarget(null);
            Debug.Log("Hurray!");
        }

        public void Update()
        {
            //Debug.Log(System.DateTime.Now.Millisecond);
            player.Update();
            terrain.Update();
            Engine.Timer.Update();

            Vector3 playerPos = player.GetGameObject().transform.position;
            playerPos.y = 1;
            camScript.Update();
            enemiesManager.Update();

            light.transform.position = playerPos;
        }
    }

}
