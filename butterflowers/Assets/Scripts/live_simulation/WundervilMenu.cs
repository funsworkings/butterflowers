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

        private CanvasGroup _canvasGroup;
        
        [SerializeField] private TMP_Text _currentFrame, _currentEyeAction, _currentEyeState, _bpm, _actionStack;
        [SerializeField] private Button _restartButton;
        [SerializeField] private bool visible = false;

        public BridgeUtil _Util { get; set; } = null;
        public Action<float, float> OnBeat => null;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        IEnumerator Start()
        {
            BridgeUtil.onUpdateEyeActionStack += UpdateActionStack;
            _restartButton.onClick.AddListener(RestartSimulation);

            while (!_Util) yield return null;
            
            if(_Util.PRESET.showDebugMenu) Show();
            else Hide();
        }
        
        private void OnDestroy()
        {
            BridgeUtil.onUpdateEyeActionStack -= UpdateActionStack;
        }
        
        #region Show/hide

        void Show()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = _canvasGroup.blocksRaycasts = true;
            visible = true;
        }

        void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = _canvasGroup.blocksRaycasts = false;
            visible = false;
        }
        
        #endregion

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
            if (Input.GetKeyUp(KeyCode.D))
            {
                if (visible) Hide();
                else Show();
            }
            
            if (_Util == null) return;

            _currentFrame.text = $"frame: {_Util.CurrentFrame}";
            _currentEyeState.text = $"state <eye>: {_Util.CurrentEyeState}";
            _currentEyeAction.text = $"action: {((_Util.CurrentAction.HasValue) ? _Util.CurrentAction.Value.ToString() : "NONE")}";
            _bpm.text = $"BPM: {_Util.BPM}";
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