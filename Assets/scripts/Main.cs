


namespace RTS
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Main : MonoBehaviour
    {
        // Start is called before the first frame update



        Player player;
        Terrain terrain;
        //ClickIndicator clickIndicator;


        Astar aStar = new Astar();

        GameObject light;
        GameObject camCtrl;

        void Start()
        {
            UnityEngine.Random.InitState(1);
            terrain = new Terrain();

            int numRows = Model.numRows;
            int numCols = Model.numCols;
            List<List<Node>> map = Model.map;
            int tileSize = (int)Model.tileSize;

            int halfW = (numCols) / 2;
            int halfH = (numRows) / 2;

            light = GameObject.Find("Light");
            camCtrl = GameObject.Find("camCtrl");
            Vector3 currCamPos = camCtrl.transform.position;

            player = new Player(halfH, halfW, OnTileMoved);

            CamCtrl script = camCtrl.GetComponent<CamCtrl>();
            script.setTarget(player.GetPlayer());

            EventsManager.getInstance().addListener("CLICK_ON_TERRAIN", new EventObject(onClick, script));



            //clickIndicator = new ClickIndicator();



            //these are the vertices of a quad

            /*
             float minValue = 0.5f;
    float maxValue = 1f;
    float randomValue = Random.Range(0f, 1f);
    green.g = Mathf.Lerp(minValue, maxValue, randomValue);

    plane.transform.position = new Vector3(0, 0, 0);
    plane.transform.rotation = Quaternion.Euler(0, 0, 0);
    plane.transform.localScale = new Vector3(1, 1, 1);


            MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
    Material material = new Material(Shader.Find("Standard"));
    Texture2D texture = Resources.Load<Texture2D>("Textures/MyTexture");
    material.mainTexture = texture;
    renderer.material = material;


             */

        }

        public void OnTileMoved(int deltaRows, int deltaCols)
        {

            Model.rowOffset += deltaRows;
            Model.colOffset += deltaCols;

            Debug.Log(
                "deltaRows " + deltaRows +
                " deltaCols " + deltaCols +
                " rowOffset " + Model.rowOffset +
                " colOffset " + Model.colOffset
             );
            GameObject camCtrl = GameObject.Find("camCtrl");
            CamCtrl script = camCtrl.GetComponent<CamCtrl>();
            script.FocusOnTarget(player.GetPlayer(), true);
        }

        private void onClick()
        {
            GameObject camCtrl = GameObject.Find("camCtrl");
            CamCtrl script = camCtrl.GetComponent<CamCtrl>();

            Travel(script.ClickedPoint, script.ClickedGameObject);

        }

        private void Travel(Vector3 clickedPoint, GameObject clickedGameObject)
        {

            //string[] row_col = clickedGameObject.name.Split('_');

            float tileSize = (float)Model.tileSize;
            //tiles have center registration so we need to add half w and h to get accurate tile
            int grid_row = (int)((clickedPoint.z + (tileSize / 2)) / tileSize);
            int grid_col = (int)((clickedPoint.x + (tileSize / 2)) / tileSize);

            int world_row = grid_row + Model.rowOffset;
            int world_col = grid_col + Model.colOffset;

            int player_world_row = player.world_row;
            int player_world_col = player.world_col;

            int player_grid_row = player.grid_row;
            int player_grid_col = player.grid_col;

            //int col = Int32.Parse(row_col[1]);
            //int row = Int32.Parse(row_col[0]);

            Debug.Log(" grid_row " + grid_row + " grid_col " + grid_col + " name " + clickedGameObject.name);
            Debug.Log(" player.row " + player.grid_row + " player.col " + player.grid_col);

            if (!TerrainUtils.NodeExists(grid_row, grid_col))
            {
                return;
            }

            List<List<Node>> map = Model.map;

            List<RowCol> path = aStar.CreatePath(map, 1, map[player_grid_row][player_grid_col], map[grid_row][grid_col]);
            if (path != null && path.Count > 0)
            {
                //clickIndicator.ShowIndication(map[row][col]);

                GameObject camCtrl = GameObject.Find("camCtrl");
                CamCtrl script = camCtrl.GetComponent<CamCtrl>();
                script.setTarget(player.GetPlayer());
                player.SetPath(path, clickedPoint);
                terrain.HighlightClickedNode(map[grid_row][grid_col]);

                EventsManager.getInstance().addListener("MOVE_COMPLETE", new EventObject(onMoveComplete, player));
            }
            else
            {
                Debug.Log("INVALID PATH");
            }




        }

        private void onMoveComplete()
        {
            EventsManager.getInstance().removeListener("MOVE_COMPLETE", new EventObject(onMoveComplete, player));
            GameObject camCtrl = GameObject.Find("camCtrl");
            CamCtrl script = camCtrl.GetComponent<CamCtrl>();
            
            script.setTarget(null);
            Debug.Log("Hurray!");
        }


        // Update is called once per frame
        void Update()
        {

            //Debug.Log(System.DateTime.Now.Millisecond);
            player.Update();
            terrain.Update();
            Timer.Update();

            Vector3 playerPos = player.GetPlayer().transform.position;
            playerPos.y = 1;

            light.transform.position = playerPos;

        }
    }

}
