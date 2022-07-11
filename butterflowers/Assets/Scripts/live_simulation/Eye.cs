using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Objects.Base;
using UnityEngine;
using uwu.Extensions;
using Random = UnityEngine.Random;

namespace live_simulation
{
    public class Eye : MonoBehaviour, IBridgeUtilListener
    {
        // Properties

        [Header("General")]
        [SerializeField] private float _fovUpdateInterval = .5f;
        
        [Header("Focus")]
        [SerializeField] private Focusing _focus;
            private List<Focusable> _availableFocus = new List<Focusable>();
            private List<Focusable> _usedFocus = new List<Focusable>();

        [Header("Smart Interaction")] 
        [SerializeField] private Camera _interactionCamera;
        [SerializeField] private LayerMask _interactionMask;
        [SerializeField] private float _interactionMaxDistance = 999f;
        [SerializeField] private int _queryInteractionsPerWidth = 10;
        [SerializeField] private int _queryInteractionsPerHeight = 10;
        [SerializeField] private SmartInteractionMarker _interactionMarkerPrefab;
        [SerializeField] private RectTransform _interactionMarkersContainer;
        [SerializeField] private float _markerScaleMultiplier = 1f;
        [SerializeField] private float _stepBetweenMarkers = .1f;
        [SerializeField] private float _stepBetweenFailureMarkers = 2f;
        [SerializeField] private float _stepBetweenValidMarker = 1.5f;
        [SerializeField] private float _stepAfterValidMarker = 2f;
        [SerializeField, Range(.1f, 1f)] float _screenInteractionFillWidth = 1f;
        [SerializeField, Range(.1f, 1f)] float _screenInteractionFillHeight = 1f;
        private List<SmartInteractionMarker> hits = new List<SmartInteractionMarker>();
        private List<SmartInteractionMarker> misses = new List<SmartInteractionMarker>();

        private void Start()
        {
            BridgeUtil.onLoad += () =>
            {
                _availableFocus.AddRange(_focus.Focuses);
                _usedFocus.Add(null); // Append current default focus
                
                StartCoroutine("CoreLoop");
            };
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        IEnumerator CoreLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(_fovUpdateInterval);

                SmartInteractionMarker _marker = null;

                bool waitForQuery = true;
                QueryInteractions((marker) =>
                {
                    waitForQuery = false;
                    _marker = marker;
                });
                while (waitForQuery) yield return null;

                bool waitForAction = true;
                if (_marker != null) // Has valid marker
                {
                    Debug.LogWarning($"Success find marker: {_marker.name}");
                    _Util.RequestWebcamTexture((img) =>
                    {
                        SwitchFOV((focus) =>
                        {
                            waitForAction = false;
                        });
                    });
                }
                else // No valid marker
                {
                    Debug.LogError("Found no marker for AI behaviour!");
                    SwitchFOV((focus) =>
                    {
                        waitForAction = false;
                    });
                }
                while(waitForAction) yield return null;
            }
        }

        #region Actions

        public void SwitchFOV(System.Action<Focusable> onComplete)
        {
            Focusable _nextFocus = FindNextFocus();
            //return;
            if(_nextFocus != null) {_focus.SetFocus(_nextFocus); onComplete?.Invoke(_nextFocus);}
            else {_focus.LoseFocus(); onComplete?.Invoke(null);}
        }

        Focusable FindNextFocus()
        {
            if (_availableFocus.Count == 0)
            {
                _availableFocus = new List<Focusable>(_usedFocus);
                _usedFocus = new List<Focusable>();
            }
            
            var i = Random.Range(0, _availableFocus.Count);
            var f = _availableFocus[i];
            _availableFocus.RemoveAt(i);
            _usedFocus.Add(f);

            return f;
        }


        #endregion
        
        #region Smart action

        private Coroutine _actionLoop = null;
        
        void MakeAction(System.Action onComplete)
        {
            ClearAction();
            _actionLoop = StartCoroutine(ActionLoop(onComplete));
        }

        void ClearAction()
        {
            if (_actionLoop != null)
            {
                StopCoroutine(_actionLoop);
                _actionLoop = null;
            }
        }

        IEnumerator ActionLoop(System.Action onComplete)
        {
            bool query = true;
            QueryInteractions((marker) =>
            {
                if (marker != null) // Success
                {
                    
                }
                else // Failure
                {
                    
                }
            });
            while(query) yield return null;
        }
        
        #endregion

        #region Smart interaction

        void QueryInteractions(System.Action<SmartInteractionMarker> onComplete)
        {
            ClearInteractions();
            StartCoroutine(InteractionQueryLoop(onComplete));
        }

        void ClearInteractions(SmartInteractionMarker[] markers = null)
        {
            bool flagAll = markers == null;
            
            if(flagAll) markers = _interactionMarkersContainer.GetComponentsInChildren<SmartInteractionMarker>(); // Auto-load markers!
            if (markers != null && markers.Length > 0)
            {
                foreach (SmartInteractionMarker m in markers)
                {
                    DestroyImmediate(m.gameObject);
                }

                if (flagAll)
                {
                    hits = new List<SmartInteractionMarker>();
                    misses = new List<SmartInteractionMarker>();
                }
            }
        }

        IEnumerator InteractionQueryLoop(System.Action<SmartInteractionMarker> onComplete)
        {
            var w = _interactionMarkersContainer.rect.width;
            var h = _interactionMarkersContainer.rect.height;

            var xOffset = (1f - _screenInteractionFillWidth) / 2f;
            var yOffset = (1f - _screenInteractionFillHeight) / 2f;
            
            for (int y = 0; y < _queryInteractionsPerHeight; y++)
            {
                for (int x = 0; x < _queryInteractionsPerWidth; x++)
                {
                    var ix = (1f * x / (_queryInteractionsPerWidth-1)).RemapNRB(0f, 1f, xOffset, 1f - xOffset);
                    var iy = (1f * y / (_queryInteractionsPerHeight-1)).RemapNRB(0f, 1f, yOffset, 1f - yOffset);

                    Vector3 viewPt = new Vector3(ix * 1f, iy * 1f, 0f);
                    Vector3 screenPt = _interactionCamera.ViewportToScreenPoint(viewPt);
                    
                    Ray ray = _interactionCamera.ViewportPointToRay(viewPt);
                    RaycastHit hit = new RaycastHit();

                    bool _hit = Physics.Raycast(ray, out hit, _interactionMaxDistance, _interactionMask.value);
                    
                    var marker = Instantiate(_interactionMarkerPrefab, _interactionMarkersContainer);
                    marker.transform.position = screenPt; // Set transform position
                    marker.transform.localScale *= _markerScaleMultiplier;
                    marker.Setup(_hit, hit);

                    if (_hit) hits.Add(marker);
                    else misses.Add(marker);

                    yield return new WaitForSecondsRealtime(_stepBetweenMarkers);
                }
            }
            
            yield return null;
            yield return new WaitForSecondsRealtime(_stepBetweenFailureMarkers);
            
            var _misses = this.misses.ToArray();
            ClearInteractions(_misses);
            misses = new List<SmartInteractionMarker>();

            if (hits.Count > 0)
            {
                SmartInteractionMarker _successHit = hits[0];
                if (hits.Count > 1)
                {
                    yield return new WaitForSecondsRealtime(_stepBetweenValidMarker);
                    _successHit = hits[Random.Range(0, hits.Count)];
                    
                    var _removeHits = hits.Except(new SmartInteractionMarker[] {_successHit}).ToArray();
                    ClearInteractions(_removeHits);
                }
                yield return new WaitForSecondsRealtime(_stepAfterValidMarker);
                
                hits = new List<SmartInteractionMarker>(new SmartInteractionMarker[] {_successHit}); // Assign only valid hit!
                onComplete?.Invoke(_successHit);
            }
            else
            {
                hits = new List<SmartInteractionMarker>();
                onComplete?.Invoke(null); // No interaction found!
            }
        }

        #endregion

        public BridgeUtil _Util { get; set; } = null;
    }
}