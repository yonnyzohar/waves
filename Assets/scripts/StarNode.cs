using System;
public class StarNode
{
    public float g;
    public float h;
    public float f;
    public StarNode parent;
    public int row;
    public int col;
    public bool walkable;

    public StarNode()
    {
    }
}
