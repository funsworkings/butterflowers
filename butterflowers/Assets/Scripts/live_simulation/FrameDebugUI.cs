using System;
using Neue.Reference.Types;
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
        [SerializeField] private Frame _frame;
        private string _frameText = null;
        
        private float _value = 0f;
        public float Value => _value;

        private void Start()
        {
            _frameText = (System.Enum.GetName(typeof(Frame), _frame).ToString()).Substring(0, 1);
            
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
            _valueField.text = $"{_frameText}: {Mathf.FloorToInt(_value * 100f)}%";

            if (!fromSlider)
            {
                _slider.value = _value;
            }
        }
    }
}