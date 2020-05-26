using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Notifications
{

    using instance = Instance;

    public class Controller : MonoBehaviour
    {
        GameDataSaveSystem Save;

        public Dictionary<Notification, instance> instances = new Dictionary<Notification, instance>();

        public System.Action<Notification> onReceiveNotification;
        public System.Action<Notification> onDismissNotification;

        List<Notification> staticNotifications = new List<Notification>();
        List<Notification> nonstaticNotifications = new List<Notification>();

        [SerializeField] float clearTime = 1f, timeout = 5f;
        bool clearing = false;

        [SerializeField] Transform staticContainer;
        [SerializeField] GameObject staticPrefab;

        [SerializeField] Transform nonstaticContainer;
        [SerializeField] GameObject nonstaticPrefab;

        public void Receive(Notification notification)
        {
            instance instance = null;
            instances.TryGetValue(notification, out instance);

            if (instance == null)
            {
                if (notification.isStatic)
                {
                    staticNotifications.Add(notification);

                    if (!clearing)
                    {
                        StartCoroutine("Clear");
                        clearing = true;
                    }
                }
                else
                {
                    nonstaticNotifications.Add(notification);
                }

                AddToContainer(notification, notification.isStatic);

                if (onReceiveNotification != null)
                    onReceiveNotification(notification);

            }
        }

        void onDismissedNotification(Notification notification)
        {
            Dismiss(notification, true);
        }

        public void Dismiss(Notification notification)
        {
            Dismiss(notification, false);
        }

        public void Dismiss(Notification notification, bool events = false)
        {
            instance instance = null;
            instances.TryGetValue(notification, out instance);

            if (instance != null)
            {
                if (notification.isStatic)
                {
                    staticNotifications.Remove(notification);
                }
                else
                {
                    nonstaticNotifications.Remove(notification);
                    instance.onDismissed -= onDismissedNotification;
                }

                GameObject.Destroy(instance.gameObject);
                instances.Remove(notification);

                if (events)
                {
                    if (onDismissNotification != null)
                        onDismissNotification(notification);
                }
            }
        }

        public void ClearAll()
        {
            List<Notification> active = new List<Notification>();
            foreach(KeyValuePair<Notification, instance> notification in instances)
            {
                var notif = notification.Key;
                if (notif != null)
                    active.Add(notif);
            }

            Notification[] activeArr = active.ToArray();
            foreach (Notification notif in activeArr)
                Dismiss(notif);
        }

        public void AddToContainer(Notification notification, bool isStatic)
        {
            Transform container = (isStatic) ? staticContainer : nonstaticContainer;
            GameObject prefab = (isStatic) ? staticPrefab : nonstaticPrefab;

            GameObject obj = Instantiate(prefab, container);

            instance obj_i = obj.GetComponent<instance>();
            obj_i.notification = notification;

            if (!isStatic)
                obj_i.onDismissed += onDismissedNotification;

            instances.Add(notification, obj_i);
        }

        IEnumerator Clear()
        {
            float t = 0f;

            while(staticNotifications.Count > 0)
            {
                if (t >= clearTime)
                {
                    var count = staticNotifications.Count;
                    Dismiss(staticNotifications[count - 1]);

                    t = 0f;
                }
                else
                    t += Time.deltaTime;

                yield return null;
            }

            clearing = false;
        }
    }

}
