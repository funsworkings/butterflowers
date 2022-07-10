using System;
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

        [SerializeField] private float originLerpSpeed, scaleLerpSpeed;

        [SerializeField]private Vector2 _scale, _origin;
        [SerializeField]private Vector2 t_scale, t_origin;

        private void Start()
        {
            _scaleUIParent = _scaleUI.transform.parent as RectTransform;
            _selectionCanvas = _scaleUI.GetComponentInParent<Canvas>();

            _scale = t_scale = _scaleUI.sizeDelta;
            _origin = t_origin = Vector2.zero;
            
            Debug.LogWarning($"Selection parent: w{_scaleUIParent.rect.width*_selectionCanvas.scaleFactor} h{_scaleUIParent.rect.height*_selectionCanvas.scaleFactor}");
        }

        void Update()
        {
            if (Pause) return;
            
            _scale = Vector2.Lerp(_scale, t_scale, Time.unscaledDeltaTime * scaleLerpSpeed);
            _origin = Vector2.Lerp(_origin, t_origin, Time.unscaledDeltaTime * originLerpSpeed);
            
            ClampScale(t_scale, out t_scale);
            ClampOrigin(_origin, out _origin);
            
            UpdateUITransform(_origin, _scale);
        }

        public void UpdateTransform(Vector2 pos, Vector2 scale)
        {
            t_origin = pos;
            t_scale = scale;
        }

        #region Safety

        void ClampOrigin(Vector2 origin, out Vector2 origin_clamped)
        {
            var cw = _scaleUIParent.rect.width;
            var ch = _scaleUIParent.rect.height;

            var w = _scale.x;
            var h = _scale.y;
            
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
        
        #region UI

        void UpdateUITransform(Vector2 o, Vector2 sc)
        {
            _scaleUI.localPosition = new Vector2(o.x, -o.y) + new Vector2(-_scaleUIParent.rect.width/2f, -_scaleUIParent.rect.height/2f);
            _scaleUI.sizeDelta = new Vector2(sc.x, sc.y);
        }
        
        #endregion
    }
}