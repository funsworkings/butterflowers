using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace live_simulation
{
    public class SelectionBox : MonoBehaviour
    {
        // Properties

        [SerializeField] private RectTransform _scaleUI;
        
        private RectTransform _scaleUIParent;
        public RectTransform Container => _scaleUIParent;
        
        private Canvas _selectionCanvas;
        
        private float ScaleFactor => _selectionCanvas.scaleFactor;
        private float ScreenAspect => 1f * Screen.width / Screen.height;

        public float X => (_origin.x + _scaleUIParent.rect.width/2f - _scale.x/2f) * ScaleFactor;
        public float Y => (_origin.y + _scaleUIParent.rect.height/2f + _scale.y/2f) * ScaleFactor;
        public float W => _scale.x * ScaleFactor;
        public float H => _scale.y * ScaleFactor;

        public bool Pause { get; set; } = false;
        
        [SerializeField] private AnimationCurve _originCurve, _scaleCurve;
        [SerializeField]private Vector2 _scale, _origin;

        private void Start()
        {
            _scaleUIParent = _scaleUI.transform.parent as RectTransform;
            _selectionCanvas = _scaleUI.GetComponentInParent<Canvas>();

            _scale = _scaleUI.sizeDelta;
            _origin = Vector2.zero;
            
            Debug.LogWarning($"Selection parent: w{_scaleUIParent.rect.width*_selectionCanvas.scaleFactor} h{_scaleUIParent.rect.height*_selectionCanvas.scaleFactor}");
        }

        void Update()
        {
            if (Pause) return;
            
            UpdateUITransform(_origin, _scale);
        }

        #region Safety

        void ClampOrigin(Vector2 origin, Vector2 scale, out Vector2 origin_clamped)
        {
            var cw = _scaleUIParent.rect.width;
            var ch = _scaleUIParent.rect.height;

            var w = scale.x;
            var h = scale.y;
            
            origin_clamped = new Vector2(Mathf.Clamp(origin.x, -cw/2f + w/2f, cw/2f - w/2f), Mathf.Clamp(origin.y, -ch/2f + h/2f, ch/2f - h/2f));
        }

        void ClampScale(Vector2 scale, out Vector2 scale_clamped)
        {
            var cw = _scaleUIParent.rect.width;
            var ch = _scaleUIParent.rect.height;
            
            scale_clamped = new Vector2(Mathf.Clamp(scale.x, 0, cw),
                Mathf.Clamp(scale.y, 0, ch));
        }
        
        #endregion
        
        #region Movement
        
        public void UpdateTransform(Vector2 pos, Vector2 scale, float duration, float wait, System.Action onComplete, BridgeUtil util = null)
        {
            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
                _transitionRoutine = null;
            }
            StartCoroutine(TransitionRoutine(pos, scale, duration, wait, onComplete, util));
        }

        private Coroutine _transitionRoutine = null;
        IEnumerator TransitionRoutine(Vector2 to, Vector2 ts, float duration, float wait, System.Action onComplete, BridgeUtil util)
        {
            float t = 0f;

            Vector2 o = _origin;
            Vector2 s = _scale;
            
            ClampScale(ts, out ts);
            ClampOrigin(to, ts, out to);

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;

                float i = Mathf.Clamp01(t / duration);
                    _origin = Vector2.Lerp(o, to, _originCurve.Evaluate(i));
                    _scale = Vector2.Lerp(s, ts, _scaleCurve.Evaluate(i));

                yield return null;
            }
            
            if(util == null) yield return new WaitForSecondsRealtime(wait);
            else
            {
                var task = util.WaitForNextBeatWithDelay(wait);
                while (!task.IsCompleted) yield return null;
            }
            
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region UI

        void UpdateUITransform(Vector2 o, Vector2 sc)
        {
            _scaleUI.localPosition = new Vector2(o.x, -o.y) + new Vector2(-_scaleUIParent.rect.width/2f, -_scaleUIParent.rect.height/2f);
            _scaleUI.sizeDelta = new Vector2(sc.x, sc.y);
        }
        
        #endregion
    }
}