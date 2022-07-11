using System;
using System.Collections;
using System.Linq;
using butterflowersOS.Objects.Base;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace live_simulation
{
    public class BridgeUtil : MonoBehaviour
    {
        // Events

        public static System.Action onLoad;
        public static System.Action onCameraChange;
        public static System.Action onCycleDay;
        public static System.Action<string> onCreateImage;
        
        // Properties

        private Monitor _monitor;
        private Eye _eye;

        [SerializeField, Range(0f, 1f)] private float _nurture = 0f;
        [SerializeField, Range(0f, 1f)] private float _quiet = 1f;
        [SerializeField, Range(0f, 1f)] private float _destruction = 0f;
        [SerializeField, Range(0f, 1f)] private float _order = 0f;

        public float NURTURE => _nurture;
        public float ORDER => _order;
        public float DESTRUCTION => _destruction;
        public float QUIET => _quiet;

        private IBridgeUtilListener[] _listeners;
        
        // UI

        [SerializeField] private TMP_Text _nurtureUI, _quietUI, _orderUI, _destructionUI;
        
        void Start()
        {
            StartCoroutine(SceneLoadAsync(() =>
            {
                // Scene load completed

               _listeners = FindObjectsOfType<MonoBehaviour>().OfType<IBridgeUtilListener>().ToArray();
               foreach (IBridgeUtilListener listener in _listeners)
               {
                   listener._Util = this;
                   
                   if(listener is Monitor) _monitor = listener as Monitor; // Assign monitor
                   else if (listener is Eye) _eye = listener as Eye; // Assign eye
               }

            }, () =>
            {
                Debug.LogWarning("Failed to load webcam scene!");
            }));
        }
        
        IEnumerator SceneLoadAsync(System.Action onComplete, System.Action onFailure)
        {
            var asyncLoadOp = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            asyncLoadOp.allowSceneActivation = true;

            while (!asyncLoadOp.isDone)
            {
                Debug.Log($"Scene load progress for webcam: {asyncLoadOp.progress}%");
                yield return null; // Wait for async scene load to finish
            }

            var sc = SceneManager.GetSceneByBuildIndex(1);
            if (sc.IsValid())
            {
                onComplete?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }

        private void Update()
        {
            _nurtureUI.text = Mathf.FloorToInt(_nurture * 100f).ToString();
            _quietUI.text = Mathf.FloorToInt(_quiet * 100f).ToString();
            _destructionUI.text = Mathf.FloorToInt(_destruction * 100f).ToString();
            _orderUI.text = Mathf.FloorToInt(_order * 100f).ToString();
        }

        private void OnDestroy()
        {
            SceneManager.UnloadScene(1);
        }
        
        #region Bridge ops

        public void RequestWebcamTexture(System.Action<Texture2D, string> onComplete)
        {
            if (_monitor == null)
            {
                onComplete?.Invoke(null, null);
                return;
            }
            
            _monitor.CaptureWebcamImage(onComplete);
        }

        public void SwitchWebcam(System.Action<WebCamTexture> onComplete)
        {
            if (_monitor == null)
            {
                onComplete?.Invoke(null);
                return;
            }
            _monitor.SwitchWebcamDevice(onComplete);
        }

        public void SwitchFocus(System.Action<Focusable> onComplete)
        {
            if (_eye == null)
            {
                onComplete?.Invoke(null);
                return;
            }
            
            _eye.SwitchFOV(onComplete);
        }
        
        #endregion
    }
}