namespace RTS
{
    using UnityEngine;
    public enum TileType { LAND = 0, SEA = 1 };

    public class TerrainDef
    {
        public static int[] t1 = { 0, 2, 3 };
        public static int[] t2 = { 0, 3, 1 };
        public static Vector3[] quadVertices = new Vector3[9];
        public static int[] indices = new int[24];

        public static void SetTerrainDef(RTSModel model)
        {
            float hTile = model.tileSize / 2;


            /*
            2---6---3
            |       |
            7   4   8
            |       |
            0---5---1
             */

            quadVertices[0] = new Vector3(-hTile, 0.0f, -hTile);
            quadVertices[1] = new Vector3(hTile, 0.0f, -hTile);
            quadVertices[2] = new Vector3(-hTile, 0.0f, hTile);
            quadVertices[3] = new Vector3(hTile, 0.0f, hTile);

            //center point
            quadVertices[4] = new Vector3(0.0f, 0.0f, 0.0f);
            //btm point
            quadVertices[5] = new Vector3(0.0f, 0.0f, -hTile);
            //top point
            quadVertices[6] = new Vector3(0.0f, 0.0f, hTile);

            //left
            quadVertices[7] = new Vector3(-hTile, 0.0f, 0.0f);

            //right
            quadVertices[8] = new Vector3(hTile, 0.0f, 0.0f);


            indices[0] = 0;
            indices[1] = 4;
            indices[2] = 5;

            indices[3] = 0;
            indices[4] = 7;
            indices[5] = 4;

            indices[6] = 7;
            indices[7] = 6;
            indices[8] = 4;

            indices[9] = 7;
            indices[10] = 2;
            indices[11] = 6;


            indices[12] = 5;
            indices[13] = 8;
            indices[14] = 1;

            indices[15] = 5;
            indices[16] = 4;
            indices[17] = 8;

            indices[18] = 4;
            indices[19] = 3;
            indices[20] = 8;

            indices[21] = 4;
            indices[22] = 6;
            indices[23] = 3;
        }
    }
}
