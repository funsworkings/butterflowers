using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace UIExt.Extras {

    [ExecuteInEditMode]
    public class Tooltip : MonoBehaviour 
    {
        Camera mainCamera;

        public Transform target = null;

        [SerializeField] Vector3 offset = Vector3.zero;
        [SerializeField] bool globalOffset = true;

        [SerializeField] bool smooth = false;
        [SerializeField] float smoothSpeed = 1f;

        bool set = false;

        void Awake() {
            mainCamera = Camera.main;  
        }

        void Update() {
            if (target == null) {
                set = false;
                return;
            }

            Vector3 pos = CalculatePosition();
            if (set && smooth) {
                transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * smoothSpeed);
                return;
            }

            transform.position = pos;
            set = true;
        }

        Vector3 CalculatePosition() {
            if (globalOffset)
                return mainCamera.WorldToScreenPoint(target.position) + offset;
            else
                return mainCamera.WorldToScreenPoint(target.position + target.TransformVector(offset));
        }
    }

}