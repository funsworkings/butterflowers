using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace live_simulation
{
    public class CaptureThumbnail : MonoBehaviour
    {
        [SerializeField] private RawImage _texture;
        [SerializeField] private AnimationCurve _heightCurve, _scaleCurve;
        
        private RectTransform _rect;

        RectTransform rect
        {
            get
            {
                if (_rect == null)
                {
                    _rect = GetComponent<RectTransform>();
                }

                return _rect;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void Setup(Texture2D _img, Vector2 t_pos, float duration, System.Action onComplete, float height = 1f)
        {
            _texture.texture = _img; // Assign texture

            float aspectRatio = 1f * _img.height / _img.width;
            var dimensions = rect?.sizeDelta;
            if (dimensions.HasValue)
            {
                var _d = dimensions.Value;
                rect.sizeDelta = new Vector2(_d.x, _d.x * aspectRatio);
            }

            StartCoroutine(Run(t_pos, duration, height, onComplete));
        }

        IEnumerator Run(Vector2 t_pos, float duration, float height, System.Action onComplete)
        {
            float t = 0f;
            Vector2 o_pos = rect.position;
            Vector2 heightVector = Vector2.Perpendicular((t_pos - o_pos).normalized);

            Vector3 o_scale = transform.localScale;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float i = Mathf.Clamp01(t / duration);
                
                var pos = Vector2.Lerp(o_pos, t_pos, i);
                pos += (heightVector * _heightCurve.Evaluate(i) * height);

                rect.position = pos;
                rect.localScale = Vector3.Lerp(o_scale, Vector3.zero, _scaleCurve.Evaluate(i));
                
                yield return null;
            }
            
            onComplete?.Invoke();
        }
    }
}