using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using butterflowersOS;
using butterflowersOS.Objects.Base;
using JetBrains.Annotations;
using Neue.Reference.Types;
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
        public static System.Action<EVENTCODE[]> onUpdateEyeActionStack;

        // Properties

        private Monitor _monitor;
        private Eye _eye;

        [SerializeField] private OSC _osc;
        private bool _useOSC = false;
        
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
        private float _interval = 0f;

        public float NURTURE => _nurture;
        public float ORDER => _order;
        public float DESTRUCTION => _destruction;
        public float QUIET => _quiet;

        private bool init = false;

        public Frame CurrentFrame => _eye._currentFrame;
        public EVENTCODE? CurrentAction => _eye._currentAction;
        public Eye.State CurrentEyeState => _eye._state;

        private IBridgeUtilListener[] _listeners;
        
        // UI

        [SerializeField] private FrameDebugUI _nurtureUI, _quietUI, _orderUI, _destructionUI;
        [SerializeField] private Image _bpmUI;

        private const string Osc_DestructionKey = "/D";
        private const string Osc_NurtureKey = "/N";
        private const string Osc_QuietKey = "/Q";
        private const string Osc_OrderKey = "/O";
        private const string Osc_BpmKey = "/Bpm";

        private bool successLoadScene = false;
        
        void Start()
        {
            _useOSC = _osc && _osc.enabled;
            // Bind to OSC
            if (_useOSC)
            {
                _nurture = _quiet = _order = _destruction = 0f;
                
                _osc.SetAddressHandler(Osc_DestructionKey, ReceiveOsc_Destruction);
                _osc.SetAddressHandler(Osc_NurtureKey, ReceiveOsc_Nurture);
                _osc.SetAddressHandler(Osc_OrderKey, ReceiveOsc_Order);
                _osc.SetAddressHandler(Osc_QuietKey, ReceiveOsc_Quiet);
                _osc.SetAddressHandler(Osc_BpmKey, ReceiveOscBpm);
            }
            
            _nurtureUI.UpdateValue(_nurture);
            _quietUI.UpdateValue(_quiet);
            _destructionUI.UpdateValue(_destruction);
            _orderUI.UpdateValue(_order);
            
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

               successLoadScene = true;

            }, () =>
            {
                Debug.LogWarning("Failed to load webcam scene!");
                successLoadScene = false;
            }));

            onLoad += () =>
            {
                init = true;
            };
        }
        
        #region OSC

        void ReceiveOscStats(OscMessage message)
        {
            Debug.LogWarning("Did receive message from OSC!");
            
            /*_destruction = Mathf.Clamp01(message.GetFloat(0));
            _order = Mathf.Clamp01(message.GetFloat(1));
            _nurture = Mathf.Clamp01(message.GetFloat(2));
            _quiet = Mathf.Clamp01(message.GetFloat(3));*/
            bpm = message.GetInt(0);
        }

        void ReceiveOsc_Destruction(OscMessage message)
        {
            _destruction = Mathf.Clamp01(message.GetFloat(0));
        }
        
        void ReceiveOsc_Nurture(OscMessage message)
        {
            _nurture = Mathf.Clamp01(message.GetFloat(0));
        }
        
        void ReceiveOsc_Quiet(OscMessage message)
        {
            _quiet = Mathf.Clamp01(message.GetFloat(0));
        }
        
        void ReceiveOsc_Order(OscMessage message)
        {
            _order = Mathf.Clamp01(message.GetFloat(0));
        }

        void ReceiveOscBpm(OscMessage message)
        {
            bpm = message.GetInt(0);
        }
        
        #endregion
        
        #region Scene load/unload

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

        IEnumerator SceneUnloadAsync(System.Action onComplete, System.Action onFailure)
        {
            if (successLoadScene)
            {
                var t_scene = SceneManager.GetSceneByBuildIndex(1);
                if (t_scene.IsValid())
                {
                    var asyncUnloadOp = SceneManager.UnloadSceneAsync(1);
                    asyncUnloadOp.allowSceneActivation = true;
                
                    while (!asyncUnloadOp.isDone) yield return null; // Wait for scene unload op to complete   
                    
                    onComplete?.Invoke();
                }
                else
                {
                    onFailure?.Invoke();
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }
        
        #endregion

        private void Update()
        {
            if (_useOSC)
            {
                _nurtureUI.UpdateValue(_nurture);
                _quietUI.UpdateValue(_quiet);
                _destructionUI.UpdateValue(_destruction);
                _orderUI.UpdateValue(_order);
            }
            else
            {
                _nurture = _nurtureUI.Value;
                _quiet = _quietUI.Value;
                _destruction = _destructionUI.Value;
                _order = _orderUI.Value;
            }

            if (!init) return;
            
            // BPM

            t += Time.unscaledDeltaTime;

            var diff = t - thresh;
            if (diff >= 0f)
            {
                Beat(diff);
            }

            _interval = Mathf.Clamp01(1f - ((timeB - (t+timeA)) / thresh));;
            _bpmUI.fillAmount = _interval;
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

        public Task WaitForBeats(int beats)
        {
            TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();
            Task<bool> _task = _taskCompletionSource.Task;

            Task.Factory.StartNew(() =>
            {
                int _b = 0;
                onBeat += DidCompleteBeatLocal;

                void DidCompleteBeatLocal(float a, float b)
                {
                    onBeat -= DidCompleteBeatLocal;
                    
                    Debug.LogWarning($"wait beat: {_b}");
                    if (++_b >= beats)
                    {
                        _taskCompletionSource.SetResult(true);
                    }
                }
            });

            return _task;
        }

        public Task WaitForNextBeatWithDelay(float delay)
        {
            TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();
            Task<bool> _task = _taskCompletionSource.Task;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep((int)(delay * 1000));
                
                onBeat += DidCompleteBeatLocal;
                void DidCompleteBeatLocal(float a, float b)
                {
                    onBeat -= DidCompleteBeatLocal;
                    _taskCompletionSource.SetResult(true);
                }
            });

            return _task;
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

        #region Utils

        public void RestartSim(System.Action onFailure)
        {
            StartCoroutine(SceneUnloadAsync(() =>
            {
                SceneManager.LoadScene(0); // Reload current scene!
            }, () =>
            {
                onFailure?.Invoke();
            }));
        }

        #endregion
    }
}