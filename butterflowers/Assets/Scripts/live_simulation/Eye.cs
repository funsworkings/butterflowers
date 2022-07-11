using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS;
using butterflowersOS.Core;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Managers;
using Neue.Reference.Types;
using UnityEngine;
using uwu.Extensions;
using Random = UnityEngine.Random;
using Terrain = butterflowersOS.Objects.Entities.Terrain;
using Tree = butterflowersOS.Objects.Entities.Interactables.Empty.Tree;

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

        [Header("Scene")] 
        [SerializeField] private BeaconManager _beaconManager;
        [SerializeField] private Wand _wand;
        [SerializeField] private Nest _nest;

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

                bool _action = true; // Wait for action
                MakeAction(() =>
                {
                    _action = false;
                });
                while (_action) yield return null;
            }
        }
        
        #region Behaviours

        [SerializeField] private Frame _debugFrame;
        Frame ChooseBestFrame()
        {
            float aggregate = _Util.ORDER + _Util.QUIET + _Util.NURTURE + _Util.DESTRUCTION;
        // todo SOlve for best option between frame weights
            return _debugFrame;
        }
        
        #endregion

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

        void HandleActionLoop(Entity entity, List<EVENTCODE> @eventcodes, System.Action onComplete, System.Action onFailure, bool? success = null)
        {
            if (!success.HasValue)
            {
                if (@eventcodes.Count > 0)
                {
                    var @event = @eventcodes[0];
                    @eventcodes.RemoveAt(0);

                    if (@event == EVENTCODE.BEACONADD) // Spawn event!
                    {
                        _Util.RequestWebcamTexture((img, imgPath) =>
                        {
                            if (img != null && !string.IsNullOrEmpty(imgPath))
                            {
                                var _beacon = _beaconManager.CreateBeacon(imgPath, Beacon.Type.Desktop, Beacon.Locale.Terrain, new Hashtable(), fromSave:false, transition: BeaconManager.TransitionType.Spawn);
                                entity = _beacon; // Swap to beacon element   
                                
                                HandleActionLoop(entity, @eventcodes, onComplete, onFailure);
                            }
                            else
                            {
                                HandleActionLoop(null, null, onComplete, onFailure, false);
                            }
                        });
                    }
                    else
                    {
                        try
                        {
                            switch (@event)
                            {
                                // NEST
                                case EVENTCODE.NESTKICK:
                                    _nest.RandomKick();
                                    break;
                                case EVENTCODE.NESTPOP:
                                    _nest.RemoveBeacon(_nest.beacons.First());
                                    break;
                                case EVENTCODE.NESTCLEAR:
                                    _nest.Dispose();
                                    break;
                                case EVENTCODE.NESTFIRE:
                                    _nest.Fire();
                                    break;
                                case EVENTCODE.NESTEXTINGUISH:
                                    _nest.Extinguish();
                                    break;

                                // BEACON (requires beacon)
                                case EVENTCODE.BEACONACTIVATE:
                                    (entity as Beacon).AddToNest();
                                    break;
                                case EVENTCODE.BEACONFLOWER:
                                    (entity as Beacon).Flower(Vector3.zero);
                                    break;
                                case EVENTCODE.BEACONDELETE:
                                    (entity as Beacon).Delete();
                                    break;
                                case EVENTCODE.BEACONPLANT:
                                    (entity as Beacon).Plant(Vector3.zero);
                                    break;
                                case EVENTCODE.BEACONFIRE:
                                    (entity as Beacon).Fire();
                                    break;
                                case EVENTCODE.BEACONEXTINGUISH:
                                    (entity as Beacon).Extinguish();
                                    break;

                                default:
                                    throw new SystemException($"Event type {@event} not supported!");
                                    break;
                            }
                        }
                        catch (SystemException e)
                        {
                            HandleActionLoop(null, null, onComplete, onFailure, false);
                        }
                        
                        HandleActionLoop(entity, @eventcodes, onComplete, onFailure);
                    }
                }
                else
                {
                    HandleActionLoop(null, null, onComplete, onFailure, true);
                }
            }
            else
            {
                bool _success = success.Value;
                
                if(_success) onComplete?.Invoke(); // Success condition
                else onFailure?.Invoke(); // Fail condition
            }
        }

        [SerializeField] private FrameEventSet[] _frameActions = new FrameEventSet[] { };

        IEnumerator ActionLoop(System.Action onComplete)
        {
            SmartInteractionMarker _marker = null;

            Frame _frame = ChooseBestFrame();
            List<EVENTCODE> _frameEvents = null;
            for (int i = 0; i < _frameActions.Length; i++)
            {
                if (_frameActions[i]._frame == _frame)
                {
                    _frameEvents = _frameActions[i]._events; // Assign matching event filter
                    break;
                }
            }

            bool waitForQuery = true;
            QueryInteractions(_frameEvents, (marker) =>
            {
                waitForQuery = false;
                _marker = marker;
            });
            while (waitForQuery) yield return null;

            bool waitForAction = true;
            if (_marker != null) // Has valid marker with valid entity+action
            {
                Debug.LogWarning($"Success find marker: {_marker.name} entity: {_marker.HitEntity.gameObject.name} action: {_marker.HitEvents.print()}");
                HandleActionLoop(_marker.HitEntity, _marker.HitEvents, () =>
                {
                    Debug.LogWarning($"Success handle action loop for entity {_marker.HitEntity.gameObject.name} stack: {_marker.HitEvents.print()}");
                    waitForAction = false;
                }, () =>
                {
                    Debug.LogError($"Fail handle action loop for entity {_marker.HitEntity.gameObject.name} stack: {_marker.HitEvents.print()}");
                    waitForAction = false;
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
            
            onComplete?.Invoke();
        }

        EVENTCODE[] QueryEventsFromRaycastHit(RaycastHit _raycastHit, out Entity _entity)
        {
            List<EVENTCODE> _eventcodes = new List<EVENTCODE>();
            
            var entity = _raycastHit.collider.GetComponent<Entity>();
            if (entity is Terrain)
            {
                var _terrain = (entity as butterflowersOS.Objects.Entities.Terrain);
                
                _eventcodes.AddRange(new EVENTCODE[]
                {
                    EVENTCODE.BEACONADD,
                    EVENTCODE.BEACONPLANT
                });
            }
            else if (entity is Nest)
            {
                var _nest = (entity as Nest);
                
                _eventcodes.AddRange(new EVENTCODE[]
                {
                    EVENTCODE.NESTKICK,
                    EVENTCODE.BEACONACTIVATE
                });
                
                if(!_nest.IsOnFire) _eventcodes.Add(EVENTCODE.NESTFIRE);
                else _eventcodes.Add(EVENTCODE.NESTEXTINGUISH);

                if (_nest.fill > 0f)
                {
                    _eventcodes.AddRange(new EVENTCODE[]
                    {
                        EVENTCODE.NESTPOP,
                        EVENTCODE.NESTCLEAR
                    });
                }
            }
            else if (entity is Tree)
            {
                var _tree = (entity as butterflowersOS.Objects.Entities.Interactables.Empty.Tree);
                
                _eventcodes.AddRange(new EVENTCODE[]
                {
                    EVENTCODE.BEACONFLOWER
                });
            }
            else if (entity is Beacon)
            {
                var _beacon = (entity as Beacon);
                
                _eventcodes.AddRange(new EVENTCODE[]
                {
                    EVENTCODE.BEACONACTIVATE,
                    EVENTCODE.BEACONDELETE,
                    EVENTCODE.BEACONPLANT,
                    EVENTCODE.BEACONFLOWER
                });
                
                if(!_beacon.IsOnFire) _eventcodes.Add(EVENTCODE.BEACONFIRE);
                else _eventcodes.Add(EVENTCODE.BEACONEXTINGUISH);
            }

            _entity = entity;
            return _eventcodes.ToArray();
        }
        
        #endregion

        #region Smart interaction

        void QueryInteractions(List<EVENTCODE> _events, System.Action<SmartInteractionMarker> onComplete)
        {
            ClearInteractions();
            StartCoroutine(InteractionQueryLoop(_events, onComplete));
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

        IEnumerator InteractionQueryLoop(List<EVENTCODE> events, System.Action<SmartInteractionMarker> onComplete)
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
                    Entity entity = null;
                    List<EVENTCODE> __events = null;

                    bool _hit = Physics.Raycast(ray, out hit, _interactionMaxDistance, _interactionMask.value);
                    if (_hit) // Any hit
                    {
                        var _events = QueryEventsFromRaycastHit(hit, out entity);
                        if (_events != null && _events.Length > 0)
                        {
                            var subEvents = events.Intersect(_events);
                            if (subEvents != null && subEvents.Count() > 0)
                            {
                                var subEvent = subEvents.ElementAt(Random.Range(0, subEvents.Count()));
                                __events = new List<EVENTCODE>();
                                
                                var subEventInt = (int) subEvent;
                                if (subEventInt >= 20 && subEventInt < 30 && subEvent != EVENTCODE.BEACONADD && !(entity is Beacon)) // Beacon event!
                                {
                                    __events.Add(EVENTCODE.BEACONADD); // Prepend beacon add event to supply entity
                                }
                                
                                __events.Add(subEvent);
                            }
                            else
                            {
                                _hit = false;
                            }
                        }
                        else
                        {
                            _hit = false;
                        }
                    }

                    var marker = Instantiate(_interactionMarkerPrefab, _interactionMarkersContainer);
                    marker.transform.position = screenPt; // Set transform position
                    marker.transform.localScale *= _markerScaleMultiplier;
                    marker.Setup(_hit, hit, entity, __events);

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