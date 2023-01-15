


namespace RTS
{
    using UnityEngine;

    public class CamCtrl : MonoBehaviour
    {
        Camera cam;
        float camSpeed = 10;
        private GameObject target;
        public float distance = 20.0f;
        public float height = 10.0f;
        private Vector3 clickedPoint;
        private GameObject clickedGameObject;

        public Vector3 ClickedPoint { get => clickedPoint; set => clickedPoint = value; }
        public GameObject ClickedGameObject { get => clickedGameObject; set => clickedGameObject = value; }

        // Start is called before the first frame update
        void Start()
        {
            camSpeed = 10;
            if (!cam)
            {
                cam = GetComponentInChildren<Camera>();
            }
        }

        public void setTarget(GameObject _target)
        {
            target = _target;
        }

        // Update is called once per frame
        void Update()
        {
            if (cam)
            {
                if (!target)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        this.transform.position += (Vector3.forward * camSpeed * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        this.transform.position -= (Vector3.right * camSpeed * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        this.transform.position -= (Vector3.forward * camSpeed * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        this.transform.position += (Vector3.right * camSpeed * Time.deltaTime);
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
                        EventsManager.getInstance().dispatchEvent("CLICK_ON_TERRAIN", this);

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
                cam = GetComponentInChildren<Camera>();
            }


            Vector3 targetPosition = _target.transform.position;
            Vector3 back = (Vector3.forward * distance);
            targetPosition -= back;
            targetPosition += Vector3.up * height;

            if (immediate)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 10f * Time.deltaTime);

                
            }
            //cam.transform.LookAt(_target.transform);
            // Rotate the camera to look at the target
            //
        }
    }

}
