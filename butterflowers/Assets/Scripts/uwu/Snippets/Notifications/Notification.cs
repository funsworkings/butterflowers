using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Notifications
{
    [CreateAssetMenu(fileName = "New Notification", menuName = "Notification", order = 53)]
    public class Notification : ScriptableObject
    {
        public bool dismiss = false;
        public bool isStatic
        {
            get
            {
                return !dismiss;
            }
        }

        public string dialog;
        public Sprite icon;
    }
}
