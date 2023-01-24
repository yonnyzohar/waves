


namespace RTS
{
    using UnityEngine;
    //inherit from Actor!!
    public class CamCtrl : Engine.Actor
    {
        GameObject camHolder;
        Camera cam;
        float camSpeed = 10;
        private GameObject target;
        public float distance = 5.0f;
        public float height = 5.0f;
        private Vector3 clickedPoint;
        private GameObject clickedGameObject;

        public Vector3 ClickedPoint { get => clickedPoint; set => clickedPoint = value; }
        public GameObject ClickedGameObject { get => clickedGameObject; set => clickedGameObject = value; }

        // Start is called before the first frame update
        public CamCtrl(RTSContext _context):base(_context)
        {
            camHolder = GameObject.Find("camCtrl");
            camSpeed = 10;
            if (!cam)
            {
                cam = camHolder.GetComponentInChildren<Camera>();
            }
        }

        public void setTarget(GameObject _target)
        {
            target = _target;
        }

        // Update is called once per frame
        public void Update()
        {
            if (cam)
            {
                if (!target)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        camHolder.transform.position += (Vector3.forward * camSpeed * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        camHolder.transform.position -= (Vector3.right * camSpeed * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        camHolder.transform.position -= (Vector3.forward * camSpeed * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        camHolder.transform.position += (Vector3.right * camSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    FocusOnTarget(target);

                }

                if (Input.GetMouseButtonDown(0))
                {
                    // Create a ray from the camera through the clicked point
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                    // Declare a RaycastHit variable to store the hit information
                    RaycastHit hit;

                    // Intersect the ray with any colliders in the scene
                    if (Physics.Raycast(ray, out hit))
                    {
                        // Get the point where the ray intersected the collider
                        clickedPoint = hit.point;
                        clickedGameObject = hit.collider.gameObject;
                        target = null;
                        dispatchEvent("CLICK_ON_TERRAIN");

                        // Do something with the point...
                    }
                }
            }
        }

        public void FocusOnTarget(GameObject _target, bool immediate = false)
        {
            if (!_target)
            {
                return;
            }
            if (!cam)
            {
                cam = camHolder.GetComponentInChildren<Camera>();
            }


            Vector3 targetPosition = _target.transform.position;
            Vector3 back = (Vector3.forward * distance);
            targetPosition -= back;
            targetPosition += Vector3.up * height;

            if (immediate)
            {
                camHolder.transform.position = targetPosition;
            }
            else
            {
                camHolder.transform.position = Vector3.MoveTowards(camHolder.transform.position, targetPosition, 10f * Time.deltaTime);

                
            }
        }
    }

}
