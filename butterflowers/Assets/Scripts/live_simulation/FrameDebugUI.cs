using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace live_simulation
{
    public class FrameDebugUI : MonoBehaviour
    {
        // Properties

        [SerializeField] private TMP_Text _valueField;
        [SerializeField] Slider _slider;

        private float _value = 0f;
        public float Value => _value;

        private void Start()
        {
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.value = 0f;

            _slider.onValueChanged.AddListener((e) =>
            {
                UpdateValue(e, fromSlider:true);
            });
        }

        public void UpdateValue(float _value, bool fromSlider = false)
        {
            this._value = _value;
            _valueField.text = Mathf.FloorToInt(_value * 100f).ToString();

            if (!fromSlider)
            {
                _slider.value = _value;
            }
        }
    }
}