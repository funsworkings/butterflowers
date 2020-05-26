using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Notifications
{

    using UnityEngine.UI;

    public class Instance : MonoBehaviour
    {
        public delegate void OnDismissed(Notification notification);
        public event OnDismissed onDismissed;

        Notification m_notification = null;
        public Notification notification
        {
            set
            {
                m_notification = value;
                if(m_notification != null)
                {
                    dialog.text = value.dialog;
                    icon.sprite = value.icon;
                }
            }
        }

        [SerializeField] TMPro.TMP_Text dialog;
        [SerializeField] Image icon;

        public void Dismiss()
        {
            if (onDismissed != null)
                onDismissed(m_notification);
        }
    }

}
