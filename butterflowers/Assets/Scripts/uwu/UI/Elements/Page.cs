using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UIExt.Behaviors;

namespace UIExt.Elements {

    public class Page : MonoBehaviour
    {
        public UnityEvent onOpen, onClose;

        Section section;

        ToggleVisibility visibility;

        [SerializeField] List<GameObject> objects = new List<GameObject>();

        bool m_active = false;
        public bool active {
            get{ return m_active; }
            set{ m_active = value; }
        }

        void Awake() {
            section = GetComponentInParent<Section>();
            visibility = GetComponent<ToggleVisibility>();    

            int children = transform.childCount;
            for(int i = 0; i < children; i++) {
                var obj = transform.GetChild(i).gameObject;
                objects.Add(obj);
            }

            //Debug.LogFormat("Page \"{0}\" has {1} children", gameObject.name, children);
        }

        void Start() {
            m_active = false;
        }

        public void Open(bool events = false){
            bool inactive = !active;

            active = true;
            onUpdateActiveState();

            if(inactive || events)
                onOpen.Invoke();
        }

        public void Close(bool events = false){
            bool inactive = !active;

            active = false;
            if (!inactive || events)
                onClose.Invoke();

            onUpdateActiveState();
        }

        void onUpdateActiveState(){
            foreach(GameObject o in objects)
            {
                o.SetActive(active);
            }
               
        }
    }

}
