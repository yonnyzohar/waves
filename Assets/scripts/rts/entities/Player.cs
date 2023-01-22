
namespace RTS
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    public class Player : Entity
    {
        
        private float moveSpeed = 2.0f;
        
        private int currPosInPath;
        bool useKeyboard = false;
        List<RowCol> path;
        private Vector3 clickPoint;
        Vector3 startPos;

        private int prevRow;
        private int prevCol;


        public delegate void Del(int deltaRows, int deltaCols);

        Del handler;

        enum State { ROTATING, MOVING, IDLE, PROCESS_ORDERS, MOVE_END };

        State currState;
        

        // Use this for initialization
        public Player(RTSContext _context, int _startRow, int startCol, Del _handler, GameObject prefab): base(_context, _startRow, startCol, prefab)
        {
            currState = State.IDLE;
            handler = _handler;
            rotSpeed = 100.0f;
            AdjustPlayerHeightAndRotationAccordingToTerrain(true);
        }

        

        

        // Update is called once per frame
        public override void Update() 
        {
            
            if (!charPh)
            {
                return;
            }

            base.Update();

            RTSModel model = getModel() as RTSModel;

            if (!TerrainUtils.NodeExists(model.map, grid_row, grid_col))
            {
                return;
            }
            Transform t = charPh.transform;

            if (useKeyboard)
            {


                if (Input.GetKey(KeyCode.UpArrow))
                {
                    t.position += (t.forward * moveSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    t.RotateAround(t.position, t.up, -rotSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    t.position -= (t.forward * moveSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    t.RotateAround(t.position, t.up, rotSpeed * Time.deltaTime);
                }
            }
            else
            {
                if (currState == State.IDLE)
                {
                    AdjustPlayerHeightAndRotationAccordingToTerrain(true);
                    return;
                }

                


                if (currState == State.PROCESS_ORDERS)
                {
                    startPos = charPh.transform.position;

                    RowCol next = path[currPosInPath];
                    Node nextNode = model.map[next.row][next.col];

                    targetPosition = nextNode.t1Center;
                    prevRow = grid_row;
                    prevCol = grid_col;

                    //this is used to see if we are at the last tile
                    if (currPosInPath == path.Count - 1)
                    {
                        float dist1 = Vector3.Distance(clickPoint, nextNode.t1Center);
                        float dist2 = Vector3.Distance(clickPoint, nextNode.t2Center);

                        if (dist2 < dist1)
                        {
                            //targetPosition = next.t2Center;
                        }
                    }

                    //TerrainUtils.DrawDebugCircle(targetPosition);

                    Vector3 pos = targetPosition - t.position;

                    //this means x will point there
                    float targetYangle = fixAngle(Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg);
                    float currYAngle = fixAngle(t.rotation.eulerAngles.y);
                    float d = fixAngle(360.0f - (targetYangle - 90.0f));
                    //add 90 for z to point there

                    //for some reason i need to subtract 90 degrees to the target angle for it to work.
                    //maybe it thinks the z is the x??

                    //this works!!!!
                    currRotationSpeed = rotSpeed * FindShortestRotation(currYAngle, d);

                    currState = State.ROTATING; //State.ROTATING do this later.... need callback on rotation end

                }
                /**/
                if (currState == State.ROTATING)
                {
                    // Calculate a rotation a step closer to the target and applies rotation to this object
                    bool complete = RotateOnYAxis(targetPosition, false, currRotationSpeed);

                    //the angle needs to equal to minus target angle. i dont understand why
                    if (complete)
                    {
                        RotateOnYAxis(targetPosition, true);
                        currState = State.MOVING;
                    }
                }

                if (currState == State.MOVING)
                {
                    float totalDistance = Vector3.Distance(targetPosition, startPos);
                    float traveledDistance = Vector3.Distance(startPos, t.position);
                    float per = traveledDistance / totalDistance;
                    float xDiff = Math.Abs(t.position.x - targetPosition.x);
                    float zDiff = Math.Abs(t.position.z - targetPosition.z);

                    if (per >= 1.0f || (xDiff <= 0.1f && zDiff <= 0.1f))
                    {
                        currState = State.MOVE_END;
                        t.position = startPos;
                        
                    }
                    else
                    {

                        // Move towards the target
                        t.position = Vector3.MoveTowards(t.position, targetPosition, moveSpeed * Time.deltaTime);

                    }
                }
                if (currState == State.MOVE_END)
                {
                    currPosInPath++;

                    int deltaRows = (grid_row - prevRow);
                    int deltaCols = (grid_col - prevCol);

                    //this is the advance map funciton in terrain
                    handler(deltaRows, deltaCols);

                    if (currPosInPath >= path.Count)
                    {
                        Debug.Log("REACHED_DESTINATION ");
                        currState = State.IDLE;
                        dispatchEvent("MOVE_COMPLETE");
                        return;
                    }

                    for(int i = currPosInPath; i < path.Count; i++)
                    {
                        RowCol next = path[i];
                        next.row -= deltaRows;
                        next.col -= deltaCols;
                    }
                    

                    currState = State.PROCESS_ORDERS;
                }

                if (currState != State.ROTATING)
                {
                    AdjustPlayerHeightAndRotationAccordingToTerrain(true);
                }
                else
                {
                    AdjustPlayerHeightAndRotationAccordingToTerrain(false);
                }

            }

        }

        

        public void SetPath(List<RowCol> _path, Vector3 _clickPoint)
        {
            path = _path;
            clickPoint = _clickPoint;
            currPosInPath = 0;
            currState = State.PROCESS_ORDERS;

        }

        

        
    }

}
