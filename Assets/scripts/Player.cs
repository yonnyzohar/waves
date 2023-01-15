
namespace RTS
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    public class Player
    {
        GameObject charPh;
        private float moveSpeed = 3.0f;
        private float rotSpeed = 300.0f;
        private int currPosInPath;
        bool useKeyboard = false;
        List<RowCol> path;
        private Vector3 clickPoint;
        Vector3 startPos;
        Vector3 targetPosition;
        public int grid_row;
        public int grid_col;
        public int world_row;
        public int world_col;



        Vector3 closestPolygon;
        float currRotationSpeed;

        private int prevRow;
        private int prevCol;


        public delegate void Del(int deltaRows, int deltaCols);

        Del handler;

        enum State { ROTATING, MOVING, IDLE, PROCESS_ORDERS, MOVE_END };

        State currState;
        private Vector3 prevClosestPolygon;

        // Use this for initialization
        public Player(int _startRow, int startCol, Del _handler)
        {
            grid_row = world_row = _startRow;
            grid_col = world_col = startCol;
            currState = State.IDLE;
            handler = _handler;

            GameObject Tank = (GameObject)Resources.Load("Prefabs/Ship");

            charPh = MonoBehaviour.Instantiate(Tank);// new GameObject("charPH");
                                                     //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                                     //cube.transform.position = new Vector3(0, 0.3f, 0);
                                                     //cube.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
                                                     //cube.transform.parent = charPh.transform;
            Node s = Model.map[_startRow][startCol];

            charPh.transform.position = s.t1Center;

            SetRotationByTerrain(s.t1Normal, targetPosition);
            SetRowCol();

            AdjustPlayerHeightAndRotationAccordingToTerrain(true);
        }

        private void SetRowCol()
        {
            Transform t = charPh.transform;
            float tileSize = Model.tileSize;
            grid_row = (int)((t.position.z + (tileSize / 2)) / tileSize);
            grid_col = (int)((t.position.x + (tileSize / 2)) / tileSize);
            world_row = grid_row + Model.rowOffset;
            world_col = grid_col + Model.colOffset;

        }

        public GameObject GetPlayer()
        {
            return charPh;
        }

        int FindShortestRotation(float currentRotation, float targetRotation)
        {
            float difference = targetRotation - currentRotation;
            if (difference < 0)
            {
                difference += 360;
            }
            if (difference > 180)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        // Update is called once per frame
        public void Update()
        {
            if (!charPh)
            {
                return;
            }

            SetRowCol();



            if (!TerrainUtils.NodeExists(grid_row, grid_col))
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
                    Node nextNode = Model.map[next.row][next.col];

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
                        //currState = State.IDLE;

                        //Timer.CreateTimer(duration, onRotationComplete);
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
                        EventsManager.getInstance().dispatchEvent("MOVE_COMPLETE", this);
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

        private float fixAngle(float val)
        {
            if (val < 0)
            {
                val += 360.0f;
            }
            if (val > 360.0f)
            {
                val -= 360.0f;
            }

            return val;
        }

        private bool RotateOnYAxis(Vector3 targetPosition, bool immediate, float speed = 0)
        {
            Transform t = charPh.transform;
            Vector3 pos = targetPosition - t.position;

            //this means x will point there
            float targetYangle = fixAngle(Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg);
            //add 90 for z to point there
            //we then need to point to the degree * -1
            //this works!!!!

            float currYAngle = fixAngle(t.rotation.eulerAngles.y);


            //

            if (immediate)
            {
                t.RotateAround(t.position, t.up, -currYAngle);
                t.RotateAround(t.position, t.up, -targetYangle);
                t.RotateAround(t.position, t.up, 90.0f);
                return true;
            }


            t.RotateAround(t.position, t.up, speed * Time.deltaTime);

            float angleAfterRotation = fixAngle(360.0f - (targetYangle - 90.0f));
            float diff = Math.Abs(currYAngle - angleAfterRotation);
            float absSpeed = Math.Abs(speed * Time.deltaTime);
            Debug.Log("diff " + diff + " speed " + absSpeed);

            if (diff <= absSpeed)
            {
                return true;
            }
            return false;
        }

        public void SetPath(List<RowCol> _path, Vector3 _clickPoint)
        {
            path = _path;
            clickPoint = _clickPoint;
            currPosInPath = 0;
            currState = State.PROCESS_ORDERS;

        }

        public void AdjustPlayerHeightAndRotationAccordingToTerrain(bool adjustRotation)
        {
            //first we need to figure out which square we are on
            Transform t = charPh.transform;

            Node s = Model.map[grid_row][grid_col];

            //then we need to figure out which polygon we are on
            float distance1 = Vector3.Distance(t.position, s.t1Center);
            float distance2 = Vector3.Distance(t.position, s.t2Center);
            closestPolygon = s.t2Center;
            Vector3 polygonNormal = s.t2Normal;

            if (distance1 < distance2)
            {
                closestPolygon = s.t1Center;
                polygonNormal = s.t1Normal;
            }

            Vector3 charPos = t.position;
            GameObject go = s.go;

            if (adjustRotation)
            {
                if (prevClosestPolygon != closestPolygon)
                {
                    SetRotationByTerrain(polygonNormal, targetPosition);

                }
            }

            prevClosestPolygon = closestPolygon;

            Vector3 inputPoint = new Vector3(charPos.x, 80, charPos.z);
            Vector3 ret = TerrainUtils.GetPointOnPolygon(inputPoint, Vector3.down, go.GetComponent<MeshCollider>());

            if (ret != inputPoint)
            {
                charPos.y = ret.y;
                t.position = charPos;
            }

            Debug.DrawRay(s.t1Center, s.t1Normal, Color.red);
            Debug.DrawRay(s.t2Center, s.t2Normal, Color.black);

        }

        private void SetRotationByTerrain(Vector3 polygonNormal, Vector3 targetPosition)
        {
            Transform t = charPh.transform;
            /**/
            Quaternion newRotation = Quaternion.LookRotation(polygonNormal.normalized, Vector3.up);
            //this is the rotation according to the surface
            Vector3 quatToVec = newRotation.eulerAngles;
            quatToVec.x += 90.0f;
            //quatToVec.z += 90.0f;
            t.rotation = Quaternion.Euler(quatToVec);
            //we need the x rotation of the current plane but the y rotation of the destination
            RotateOnYAxis(targetPosition, true);



        }
    }

}
