using butterflowersOS.Snippets;
using UnityEngine;

namespace butterflowersOS.UI
{
    public class MenuCursor : MonoBehaviour
    {
        [SerializeField] Burster burster = null;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Click!
            {
                burster.Burst(Input.mousePosition);
            }
        }
    }
}
