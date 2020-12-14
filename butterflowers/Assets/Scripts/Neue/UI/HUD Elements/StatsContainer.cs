using UnityEngine;

namespace Neue.UI.HUD_Elements
{

    public class StatsContainer : MonoBehaviour
    {
        StatElement[] stats;

        // Start is called before the first frame update
        void Start()
        {
            stats = GetComponentsInChildren<StatElement>();
        }

        public void Visible()
        {
            foreach (StatElement s in stats) 
            {
                s.OnVisible();
            }
        }

        public void Hidden()
        {
            foreach (StatElement s in stats) 
            {
                s.OnHidden();
            }
        }
    }

}
