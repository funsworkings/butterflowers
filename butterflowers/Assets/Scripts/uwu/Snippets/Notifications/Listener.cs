using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

namespace Notifications
{

    public class Listener : MonoBehaviour
    {
        [SerializeField] Controller controller;

        [System.Serializable]
        public struct NotificationEvent
        {
            public Notification notification;
            public UnityEvent response;
        }


        [SerializeField] NotificationEvent[] events = new NotificationEvent[] { };


        void OnEnable()
        {
            if (controller == null)
                return;

            controller.onDismissNotification += ReceiveNotificationEvent;
        }

        void OnDisable()
        {
            if (controller == null)
                return;

            controller.onDismissNotification -= ReceiveNotificationEvent;
        }

        void ReceiveNotificationEvent(Notification notification)
        {
            if (notification == null)
                return;


            foreach(NotificationEvent e in events)
            {
                if (e.notification == notification)
                    e.response.Invoke();
            }
        }
    }

}
