using System;
using System.Collections.Generic;
using UnityEngine;


public class Model
{
    public static float tileSize = 1f;
    public static int numRows = 10;
    public static int numCols = 10;
    public static int endRow = numRows - 1;
    public static int endCol = numCols - 1;
    public static float peakHeight = 2.1f;
    public static List<List<Node>> map = new List<List<Node>>();

    public static Texture grass_texture = Resources.Load("grass_texture") as Texture;
    public static Texture ground_texture = Resources.Load("ground_texture") as Texture;


    public Model()
    {

    }
}
