using System;
using System.Collections;
using System.Linq;
using butterflowersOS.Objects.Base;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace live_simulation
{
    public class BridgeUtil : MonoBehaviour
    {
        // Events

        public static System.Action onLoad;
        public static System.Action onCameraChange;
        public static System.Action onCycleDay;
        public static System.Action<string> onCreateImage;
        public static System.Action<float, float> onBeat;
        
        // Properties

        private Monitor _monitor;
        private Eye _eye;

        [SerializeField] private OSC _osc;

        [SerializeField, Range(0f, 1f)] private float _nurture = 0f;
        [SerializeField, Range(0f, 1f)] private float _quiet = 1f;
        [SerializeField, Range(0f, 1f)] private float _destruction = 0f;
        [SerializeField, Range(0f, 1f)] private float _order = 0f;

        [SerializeField, Min(1)] int bpm = 60;
        public int BPM => bpm;
        
        private float t = 0f;
        private float thresh = 0f;
        int w_bpm = 0;
        private float timeA, timeB;

        public float NURTURE => _nurture;
        public float ORDER => _order;
        public float DESTRUCTION => _destruction;
        public float QUIET => _quiet;

        private bool init = false;

        private IBridgeUtilListener[] _listeners;
        
        // UI

        [SerializeField] private TMP_Text _nurtureUI, _quietUI, _orderUI, _destructionUI;
        [SerializeField] private Image _bpmUI;

        private const string Osc_StatKey = "/Stats";
        
        void Start()
        {
            // Bind to OSC
            if (_osc && _osc.enabled)
            {
                _osc.SetAddressHandler(Osc_StatKey, ReceiveOscStats);
            }
            
            StartCoroutine(SceneLoadAsync(() =>
            {
                // Scene load completed

                int beatIntervalMaxValue = 1;

               _listeners = FindObjectsOfType<MonoBehaviour>().OfType<IBridgeUtilListener>().ToArray();
               foreach (IBridgeUtilListener listener in _listeners)
               {
                   listener._Util = this;
                   onBeat += listener.OnBeat; // Attach callback
                   
                   if(listener is Monitor) _monitor = listener as Monitor; // Assign monitor
                   else if (listener is Eye) _eye = listener as Eye; // Assign eye
               }

            }, () =>
            {
                Debug.LogWarning("Failed to load webcam scene!");
            }));

            onLoad += () =>
            {
                init = true;
            };
        }
        
        #region OSC

        void ReceiveOscStats(OscMessage message)
        {
            _destruction = Mathf.Clamp01(message.GetFloat(0));
            _order = Mathf.Clamp01(message.GetFloat(1));
            _nurture = Mathf.Clamp01(message.GetFloat(2));
            _quiet = Mathf.Clamp01(message.GetFloat(3));
        }
        
        #endregion
        
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

            if (!init) return;
            
            // BPM

            t += Time.unscaledDeltaTime;

            var diff = t - thresh;
            if (diff >= 0f)
            {
                Beat(diff);
            }

            _bpmUI.fillAmount = Mathf.Clamp01(1f - ((timeB - (t+timeA)) / thresh));
        }

        void Beat(float diff)
        {
            if (w_bpm != bpm)
            {
                w_bpm = bpm;
                thresh = 60f / bpm;
            }
            
            timeA = Time.time;
            timeB = timeA + (thresh - diff); // Apply offset from tDiff
            t = 0f;
            
            onBeat?.Invoke(timeA, timeB);
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