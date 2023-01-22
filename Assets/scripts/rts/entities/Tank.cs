namespace RTS
{
    using System;
    using UnityEngine;
    public class Tank : Entity
    {
        

        public Tank(RTSContext _context,int _startRow, int startCol, GameObject prefab) : base(_context, _startRow, startCol, prefab)
        {

        }

        

        public override void Update()
        {
            if (!charPh || !target)
            {
                return;
            }
            base.Update();

            Transform t = charPh.transform;

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

            bool complete = RotateOnYAxis(targetPosition, false, currRotationSpeed);

            //the angle needs to equal to minus target angle. i dont understand why
            if (complete)
            {
                RotateOnYAxis(targetPosition, true);
            }


            AdjustPlayerHeightAndRotationAccordingToTerrain(false);
        }
    }

}
