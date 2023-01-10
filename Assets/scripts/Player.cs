using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player
{
    GameObject charPh;
    private float moveSpeed = 1.0f;
    private float rotSpeed = 100.0f;
    private int currPosInPath;
    bool useKeyboard = false;
    List<Node> path;
    private Vector3 clickPoint;
    Vector3 startPos;
    Vector3 targetPosition;
    public int row;
    public int col;
    Vector3 closestPolygon;
    float currRotationSpeed;

    enum State { ROTATING, MOVING, IDLE, PROCESS_ORDERS, MOVE_END };

    State currState;
    private Vector3 prevClosestPolygon;

    // Use this for initialization
    public Player(int _startRow, int startCol )
    {
        row = _startRow;
        col = startCol;
        currState = State.IDLE;

        GameObject Tank = (GameObject)Resources.Load("Prefabs/Ship");

        charPh = MonoBehaviour.Instantiate(Tank);// new GameObject("charPH");
                                                 //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                                 //cube.transform.position = new Vector3(0, 0.3f, 0);
                                                 //cube.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
                                                 //cube.transform.parent = charPh.transform;
        Node s = Model.map[_startRow][startCol];
        GameObject tile = s.go;

        float x = (col * Model.tileSize) ;
        float z = (row * Model.tileSize) ;

        Vector3 pos = new Vector3(x, 0, z);
        charPh.transform.position = tile.transform.position;
        
        SetRotationByTerrain(s.t1Normal, targetPosition);
        SetRowCol();

        AdjustPlayerHeightAndRotationAccordingToTerrain(true);
    }

    private void SetRowCol()
    {
        Transform t = charPh.transform;
        float tileSize = Model.tileSize;
        row = (int)((t.position.z + (tileSize / 2)) / tileSize);
        col = (int)((t.position.x + (tileSize / 2)) / tileSize);
        return;
        //row = (int)t.position.z / tileSize;
        //col = (int)t.position.x / tileSize;

        float raycastDistance = 2.0f;

        Vector3 down = t.up;
        down.y *= -1;
        // Cast a ray from the position of this game object in the forward direction
        RaycastHit[] hits = Physics.RaycastAll(t.position + t.up, down, raycastDistance);

        // Debug.DrawRay(t.position + t.up, down, Color.red);
        bool found = false;
        GameObject hit;// = new GameObject();
        foreach (RaycastHit currHit in hits)
        {
            if (currHit.transform.gameObject.CompareTag("ground"))
            {
                hit = currHit.transform.gameObject;
                string[] row_col = hit.name.Split('_');
                col = Int32.Parse(row_col[1]);
                row = Int32.Parse(row_col[0]);
                Debug.Log("row " + row + " col " + col);
                found = true;
                break;
            }
        }
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
        if(!charPh)
        {
            return;
        }

        int tileSize = (int)Model.tileSize;
        SetRowCol();
        


        if (!TerrainUtils.NodeExists(row, col))
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
                t.RotateAround(t.position,t.up, -rotSpeed * Time.deltaTime);
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
            if(currState == State.IDLE)
            {
                AdjustPlayerHeightAndRotationAccordingToTerrain(true);
                return;
            }

            Node next = path[currPosInPath];
            

            if (currState == State.PROCESS_ORDERS)
            {
                startPos = charPh.transform.position;
                targetPosition = next.t1Center;

                //TerrainUtils.DrawDebugCircle(startPos);
                
                //this is used to see if we are at the last tile
                if (currPosInPath == path.Count - 1)
                {
                    float dist1 = Vector3.Distance(clickPoint, next.t1Center);
                    float dist2 = Vector3.Distance(clickPoint, next.t2Center);

                    if (dist2 < dist1)
                    {
                        targetPosition = next.t2Center;
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
                

                bool complete = RotateOnYAxis( targetPosition, false, currRotationSpeed);

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

                if (
                    per >= 1.0f ||
                    (
                        Math.Abs(t.position.x - targetPosition.x) <= 0.1f &&
                        Math.Abs(t.position.z - targetPosition.z) <= 0.1f
                    )

                 )
                {
                    //AdjustPlayerHeightAndRotationAccordingToTerrain(true);
                    currState = State.MOVE_END;
                }
                else
                {

                    // Move towards the target
                    t.position = Vector3.MoveTowards(t.position, targetPosition, moveSpeed * Time.deltaTime);
                    
                }
            }
            if(currState == State.MOVE_END)
            {
                currPosInPath++;
                if (currPosInPath >= path.Count)
                {
                    Debug.Log("REACHED_DESTINATION ");
                    currState = State.IDLE;
                    EventsManager.getInstance().dispatchEvent("MOVE_COMPLETE", this);
                    return;
                }
                currState = State.PROCESS_ORDERS;
            }

            AdjustPlayerHeightAndRotationAccordingToTerrain(true);
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


        //Debug.Log("targetYangle " + (targetYangle - 90.0f) + " currYAngle " + currYAngle);

        if (immediate)
        {
            t.RotateAround(t.position, t.up, -currYAngle) ;
            t.RotateAround(t.position, t.up, -targetYangle);
            t.RotateAround(t.position, t.up, 90.0f);
            return true;
        }


        t.RotateAround(t.position, t.up, speed * Time.deltaTime);

        float d = fixAngle(360.0f - (targetYangle - 90.0f));

        if (Math.Abs(currYAngle - d) < 5.0f)
        {
            return true;
        }
        return false;
    }

    public void SetPath(List<Node> _path, Vector3 _clickPoint)
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
         
        Node s = Model.map[row][col];

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
        
        if(adjustRotation)
        {
            if(prevClosestPolygon != closestPolygon)
            {
                SetRotationByTerrain(polygonNormal, targetPosition);
                
            }
        }

        prevClosestPolygon = closestPolygon;

        Vector3 inputPoint = new Vector3(charPos.x, 80, charPos.z);
        Vector3 ret = TerrainUtils.GetPointOnPolygon(inputPoint, Vector3.down, go.GetComponent<MeshCollider>());

        if(ret != inputPoint)
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
