using System.Collections.Generic;
using UnityEngine;

namespace RTS
{
    public class Model
    {
        public static float tileSize = 1f;
        public static int numRows = 20;
        public static int numCols = 20;
        public static float seedVariation = 2f;
        //public static float peakHeight = 2.1f;
        public static List<List<Node>> map = new List<List<Node>>();

        public static Texture grass_texture = Resources.Load("grass_texture") as Texture;
        public static Texture ground_texture = Resources.Load("ground_texture") as Texture;


        public static int rowOffset = 0;
        public static int colOffset = 0;

        public Model()
        {

        }
    }
}


