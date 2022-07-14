using System;
using System.Collections;
using butterflowersOS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace live_simulation
{
    public class WundervilMenu : MonoBehaviour, IBridgeUtilListener
    {
        // Properties
        
        [SerializeField] private TMP_Text _currentFrame, _currentEyeAction, _currentEyeState, _bpm, _actionStack;
        [SerializeField] private Button _restartButton;

        public BridgeUtil _Util { get; set; } = null;
        public Action<float, float> OnBeat => null;

        void Start()
        {
            BridgeUtil.onUpdateEyeActionStack += UpdateActionStack;
            
            _restartButton.onClick.AddListener(RestartSimulation);
        }

        void UpdateActionStack(EVENTCODE[] actions)
        {
            string _eventStackMessage = "";
            if (actions != null && actions.Length > 0)
            {
                var i = 0;
                foreach (EVENTCODE @eventcode in actions)
                {
                    if (i++ > 0)
                    {
                        _eventStackMessage += "\n"; 
                    }
                    _eventStackMessage += @eventcode.ToString();
                }
            }

            if (string.IsNullOrEmpty(_eventStackMessage)) _eventStackMessage = "[NONE]";
            _actionStack.text = _eventStackMessage;
        }

        void Update()
        {
            if (_Util == null) return;

            _currentFrame.text = $"frame: {_Util.CurrentFrame}";
            _currentEyeState.text = $"state <eye>: {_Util.CurrentEyeState}";
            _currentEyeAction.text = $"action: {((_Util.CurrentAction.HasValue) ? _Util.CurrentAction.Value.ToString() : "NONE")}";
            _bpm.text = $"BPM: {_Util.BPM}";
        }

        private void OnDestroy()
        {
            BridgeUtil.onUpdateEyeActionStack -= UpdateActionStack;
        }

        #region Utils

        private bool restartInProgress = false;
        public void RestartSimulation()
        {
            if (restartInProgress) return;
            
            restartInProgress = true;
            _restartButton.interactable = false;
            
            _Util.RestartSim(() =>
            {
                restartInProgress = false;
                _restartButton.interactable = true;
            });
        }
        
        #endregion
    }
}