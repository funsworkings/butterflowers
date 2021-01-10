using butterflowersOS.Interfaces;
using TMPro;
using UnityEngine;

namespace butterflowersOS.UI
{
    public class DayTracker : MonoBehaviour, IReactToSun
    {
        TMP_Text text;

        public void ReactToDay(int days)
        {
            text.text = days + "";
        }

        public void ReactToTimeOfDay(float timeOfDay)
        {
            return;
        }

        void Awake()
        {
            text = GetComponent<TMP_Text>();
        }


    }
}
