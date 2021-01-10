using UnityEngine;

namespace butterflowersOS.UI
{
    public class MenuCursor : MonoBehaviour
    {
        [SerializeField] GameObject pr_burst;
        [SerializeField] float cameraDistance = 1f;

        Camera camera;
    
        // Start is called before the first frame update
        void Start()
        {
            camera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Click!
            {
                Burst();
            }
        }

        void Burst()
        {
            var position = Input.mousePosition;
            var worldPosition = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, cameraDistance));

            Instantiate(pr_burst, worldPosition, pr_burst.transform.rotation);
        }
    }
}
