using System.Collections.Generic;
using UnityEngine;

namespace RTS
{
    public class RTSModel : Engine.Model
    {
        public float tileSize = 1f;
        public int numRows = 6;
        public int numCols = 6;
        public float seedVariation = 2f;
        //public static float peakHeight = 2.1f;
        public List<List<Node>> map = new List<List<Node>>();

        public Texture grass_texture = Resources.Load("grass_texture") as Texture;
        public Texture ground_texture = Resources.Load("ground_texture") as Texture;

        public int rowOffset = 0;
        public int colOffset = 0;

        public RTSModel()
        {

        }
    }
}


