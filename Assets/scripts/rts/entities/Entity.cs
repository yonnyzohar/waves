namespace RTS
{
    using UnityEngine;
    using System;
    public class Entity : Engine.Actor
    {
        protected float rotSpeed = 200.0f;
        public int grid_row;
        public int grid_col;
        public int world_row;
        public int world_col;
        protected GameObject charPh;
        protected Vector3 targetPosition;
        Vector3 closestPolygon;
        private Vector3 prevClosestPolygon;
        public float currRotationSpeed;
        protected GameObject target;
        //

        public Entity(RTSContext _context, int _startRow, int startCol, GameObject prefab):base(_context)
        {
            grid_row = world_row = _startRow;
            grid_col = world_col = startCol;
            charPh = MonoBehaviour.Instantiate(prefab);
        }

        public void SetTarget(GameObject _target)
        {
            target = _target;
            targetPosition = target.transform.position;
        }

        public void Init(int _startRow, int startCol)
        {
            RTSModel model = getModel() as RTSModel;
            grid_row = world_row = _startRow;
            grid_col = world_col = startCol;
            Node s = model.map[grid_row][grid_col];

            charPh.transform.position = s.t1Center;
            SetRowCol();
            SetRotationByTerrain(s.t1Normal, targetPosition);
        }

        protected void SetRowCol()
        {
            RTSModel model = getModel() as RTSModel;
            Transform t = charPh.transform;
            float tileSize = model.tileSize;
            grid_row = (int)((t.position.z + (tileSize / 2)) / tileSize);
            grid_col = (int)((t.position.x + (tileSize / 2)) / tileSize);
            world_row = grid_row + model.rowOffset;
            world_col = grid_col + model.colOffset;

        }

        public GameObject GetPlayer()
        {
            return charPh;
        }

        protected void SetRotationByTerrain(Vector3 polygonNormal, Vector3 targetPosition)
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

        public virtual void Update()
        {
            SetRowCol();
        }

        protected int FindShortestRotation(float currentRotation, float targetRotation)
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

        protected float fixAngle(float val)
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

        protected bool RotateOnYAxis(Vector3 targetPosition, bool immediate, float speed = 0)
        {
            Transform t = charPh.transform;
            Vector3 pos = targetPosition - t.position;

            //this means x will point there
            float targetYangle = fixAngle(Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg);
            //add 90 for z to point there
            //we then need to point to the degree * -1
            //this works!!!!

            float currYAngle = fixAngle(t.rotation.eulerAngles.y);
            float absSpeed = Math.Abs(speed * Time.deltaTime);
            float diff = Math.Abs(currYAngle - targetYangle);
            //if there is no need to rotate
            if (diff <= absSpeed)
            {
                return true;
            }

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
            diff = Math.Abs(currYAngle - angleAfterRotation);

            //Debug.Log("diff " + diff + " speed " + absSpeed);

            if (diff <= absSpeed)
            {
                return true;
            }
            return false;
        }

        protected void AdjustPlayerHeightAndRotationAccordingToTerrain(bool adjustRotation)
        {
            //first we need to figure out which square we are on
            Transform t = charPh.transform;
            RTSModel model = getModel() as RTSModel;
            Node s = model.map[grid_row][grid_col];

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

            //Debug.DrawRay(s.t1Center, s.t1Normal, Color.red);
            //Debug.DrawRay(s.t2Center, s.t2Normal, Color.black);

        }
    }

    

}
