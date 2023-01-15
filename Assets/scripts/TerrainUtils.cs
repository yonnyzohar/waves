namespace RTS
{
    using System.Collections.Generic;
    using UnityEngine;

    public class TerrainUtils
    {
        public TerrainUtils()
        {
        }

        public static bool NodeExists(int row, int col)
        {
            List<List<Node>> map = Model.map;

            if (row >= 0 && row < map.Count && col >= 0 && col < map[row].Count)
            {
                return true;
            }
            return false;
        }

        public static void DrawDebugCircle(Vector3 startPos)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = startPos;
            float scale = 0.1f;
            sphere.transform.localScale = new Vector3(scale, scale, scale);

        }



        public static Vector3 GetPointOnPolygon(Vector3 point, Vector3 direction, MeshCollider polygon)
        {
            float maxDistance = 100f;
            // Cast a ray from the point upward
            Ray ray = new Ray(point, direction);

            Debug.DrawRay(point, direction, Color.blue);

            // Create a RaycastHit object to store the intersection information
            RaycastHit hit;

            // Perform the raycast and store the result in the hit object
            if (polygon.Raycast(ray, out hit, maxDistance))
            {
                // If the raycast hit the polygon, return the intersection point
                return hit.point;
            }
            else
            {
                // If the raycast didn't hit the polygon, return the original point
                return point;
            }
        }
    }

}
