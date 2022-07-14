using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using butterflowersOS;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Entities.Interactables.Empty;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Presets;
using Neue.Reference.Types;
using UnityEngine;
using uwu.Extensions;
using Random = UnityEngine.Random;
using Terrain = butterflowersOS.Objects.Entities.Terrain;
using Tree = butterflowersOS.Objects.Entities.Interactables.Empty.Tree;

namespace live_simulation
{
    public class Eye : MonoBehaviour, IBridgeUtilListener, IReactToSunCycleReliable
    {
        // Properties

        public enum State
        {
            Idle,
            Query,
            Action
        }
        public State _state { get; private set; } = State.Idle;
        
        public Frame _currentFrame { get; private set; } = Frame.Quiet;
        public EVENTCODE? _currentAction { get; private set; }= null;

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
        [SerializeField] private float _markerInstantiateTime = .1f;
        [SerializeField] private float _stepBetweenFailureMarkers = 2f;
        [SerializeField] private float _stepBetweenValidMarker = 1.5f;
        [SerializeField] private float _stepAfterValidMarker = 2f;
        [SerializeField, Range(.1f, 1f)] float _screenInteractionFillWidth = 1f;
        [SerializeField, Range(.1f, 1f)] float _screenInteractionFillHeight = 1f;
        private List<SmartInteractionMarker> hits = new List<SmartInteractionMarker>();
        private List<SmartInteractionMarker> misses = new List<SmartInteractionMarker>();

        [Header("Scene")] 
        [SerializeField] private BeaconManager _beaconManager;
        [SerializeField] private float _beaconSpawnDistanceFromCamera = 1f;
        [SerializeField] private Wand _wand;
        [SerializeField] private Nest _nest;
        [SerializeField] private ButterflowerManager _butterflowers;
        [SerializeField] private WorldPreset _world;
        private Monitor _monitor = null;

        [Header("Debug")] 
        [SerializeField] private EVENTCODE _debugOverrideEventCode = EVENTCODE.NULL;
        
        private void Start()
        {
            BridgeUtil.onLoad += Initialize;
        }

        void Initialize()
        {
            _availableFocus.AddRange(_focus.Focuses);
            _usedFocus.Add(null); // Append current default focus
                
            StartCoroutine("CoreLoop");
        }

        private void OnDestroy()
        {
            BridgeUtil.onLoad -= Initialize;
            
            StopAllCoroutines();
        }
        
        private bool _waitBeat = true;
        [SerializeField] private float _timeToIdle = 6f;

        IEnumerator CoreLoop()
        {
            while (true)
            {
                float t = 0f;
                float cache_beat_t = Beat_T;
                
                while (t < _timeToIdle) // Wait for idle time to pass
                {
                    float tDiff = (Beat_T - Time.time);

                    t += tDiff;
                    yield return new WaitForSecondsRealtime(tDiff);
                    bool _waitFov = true;
                    SwitchFOV((_focus) =>
                    {
                        _waitFov = false;
                    });
                    while (_waitFov)
                    {
                        t += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }

                bool _action = true; // Wait for action
                yield return new WaitForSecondsRealtime((Beat_T - Time.time)); // Wait for seconds to pass on-beat
                MakeAction(() =>
                {
                    _action = false;
                });
                while (_action) yield return null;
                _state = State.Idle; // Return to idle state

                yield return null;
            }
        }
        
        #region Behaviours

        Frame ChooseBestFrame()
        {
            float aggregate = 0f;

            List<float> _frameWeight = new List<float>();
            List<Frame> _frameSelection = new List<Frame>();

            var _frames = System.Enum.GetValues(typeof(Frame));
            foreach (Frame frame in _frames)
            {
                var frameVal = GetValueFromFrame(frame);
                if (frameVal > 0f)
                {
                    aggregate += frameVal;
                    _frameWeight.Add(frameVal);
                    _frameSelection.Add(frame);
                }
            }

            int frameCount = _frameWeight.Count;
            if (frameCount > 0)
            {
                if (frameCount == 1)
                {
                    return _frameSelection[0];
                }
                else
                {
                    float random = UnityEngine.Random.Range(0f, aggregate);
                    for (int i = 0; i < frameCount; i++)
                    {
                        float w = _frameWeight[i];
                        if (random <= w)
                        {
                            return _frameSelection[i];
                        }
                    }   
                }
            }

            return Frame.Quiet;
        }

        float GetValueFromFrame(Frame frame)
        {
            if (frame == Frame.Destruction) return _Util.DESTRUCTION;
            if (frame == Frame.Nurture) return _Util.NURTURE;
            if (frame == Frame.Order) return _Util.ORDER;

            return _Util.QUIET;
        }
        
        #endregion
        
        #region Callbacks
        
        public void Cycle(bool refresh)
        {
            var flammables = FindObjectsOfType<MonoBehaviour>().OfType<IFlammable>().ToArray();
            if (flammables != null && flammables.Length > 0)
            {
                foreach (IFlammable flammable in flammables)
                {
                    if(flammable.IsOnFire) flammable.Vanish();
                }
            }
        }
        
        #endregion

        #region Actions

        public void SwitchFOV(System.Action<Focusable> onComplete)
        {
            Focusable _nextFocus = FindNextFocus();
            //return;
            if(_nextFocus != null) {_focus.SetFocus(_nextFocus);}
            else {_focus.LoseFocus();}

            if (_monitor == null) _monitor = FindObjectOfType<Monitor>(); // Runtime assign monitor
            if (_monitor != null)
            {
                _monitor.SwitchWebcamDevice((webcam) =>
                {
                    onComplete?.Invoke(_nextFocus);
                });
            }
            else
            {
                onComplete?.Invoke(_nextFocus);
            }
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
        private SmartInteractionMarker _currentActionMarker = null;
        
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

            _currentActionMarker = null; // Wipe action!
            _currentFrame = Frame.Quiet;
            _currentAction = null;
            _state = State.Idle;
            frameEvents = null;
        }

        void HandleActionLoop(Entity entity, List<EVENTCODE> @eventcodes, System.Action onComplete, System.Action onFailure, bool? success = null)
        {
            _currentAction = null; // Clear current action
            
            if (!success.HasValue)
            {
                if (entity == null)
                {
                    HandleActionLoop(null, null, onComplete, onFailure, false);
                }
                else
                {
                    if (@eventcodes.Count > 0)
                    {
                        var @event = _currentAction = @eventcodes[0];
                        @eventcodes.RemoveAt(0);

                        EVENTCODE? nextEvent = null;
                        if (@eventcodes.Count > 0) nextEvent = @eventcodes[0];

                        Debug.LogWarning($"Handle event: {@event}");
                        if (@event == EVENTCODE.BEACONADD) // Spawn event!
                        {
                            if ((entity is Flower)) // Spawning duplicate from flower
                            {
                                (entity as Flower).SpawnBeacon();
                                HandleActionLoop(entity, @eventcodes, onComplete, onFailure); // Wait for transition to complete then next action
                            }
                            else // Spawning entire new beacon
                            {
                                _Util.RequestWebcamTexture((img, imgPath) =>
                                {
                                    if (img != null && !string.IsNullOrEmpty(imgPath))
                                    {
                                        var @params = new Hashtable()
                                        {
                                            { "position" , _interactionCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, _beaconSpawnDistanceFromCamera)) }
                                        };
                                        
                                        var _transition = new Beacon.Transition()
                                        {
                                            scaleA = _world.normalBeaconScale * Vector3.one,
                                            scaleB = _world.normalBeaconScale * Vector3.one,
                                            delay = 1.5f,
                                            _transitionTexture = img
                                        };

                                        if (nextEvent.HasValue && nextEvent.Value == EVENTCODE.BEACONACTIVATE) // Trigger attract transition for beacon
                                        {
                                            _transition._tracking = _nest.transform;
                                        }

                                        Vector3? origin = null;
                                        if ((entity is Tree || entity is Terrain))
                                        {
                                            origin = _currentActionMarker.HitInfo.point + Vector3.up *
                                                ((nextEvent.HasValue && nextEvent.Value == EVENTCODE.BEACONFLOWER)
                                                    ? 0f
                                                    : .67f);
                                        }

                                        if (origin.HasValue)
                                        {
                                            @params.Add("origin", origin);
                                        }

                                        Debug.LogWarning("Create beacon for action loop : )"); 
                                        var _beacon = _beaconManager.CreateBeacon(imgPath, Beacon.Type.Desktop, Beacon.Locale.Terrain, @params, fromSave:false, transition: BeaconManager.TransitionType.Flower, _overrideTransition:_transition, onCompleteTransition:
                                        () =>
                                        {
                                            HandleActionLoop(entity, @eventcodes, onComplete, onFailure); // Wait for transition to complete then next action
                                        });
                                        entity = _beacon; // Swap to beacon element   
                                    }
                                    else
                                    {
                                        HandleActionLoop(null, null, onComplete, onFailure, false);
                                    }
                                });
                            }

                            
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
                                        (entity as Beacon).Flower((entity as Beacon).Origin); // Plant where it lands
                                        break;
                                    case EVENTCODE.BEACONDELETE:
                                        (entity as Beacon).Delete();
                                        break;
                                    case EVENTCODE.BEACONPLANT:
                                        (entity as Beacon).Plant((entity as Beacon).Origin);
                                        break;
                                    case EVENTCODE.BEACONFIRE:
                                        (entity as Beacon).Fire();
                                        break;
                                    case EVENTCODE.BEACONEXTINGUISH:
                                        (entity as Beacon).Extinguish();
                                        break;
                                    
                                    case EVENTCODE.FLOWERFIRE:
                                        (entity as Flower).Fire();
                                        break;
                                    case EVENTCODE.FLOWEREXTINGUISH:
                                        (entity as Flower).Extinguish();
                                        break;
                                        
                                    case EVENTCODE.SLAUGHTER:
                                        _butterflowers.KillButterflies();
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
            }
            else
            {
                bool _success = success.Value;
                
                if(_success) onComplete?.Invoke(); // Success condition
                else onFailure?.Invoke(); // Fail condition
            }
        }
        

        [SerializeField] private FrameEventSet[] _frameActions = new FrameEventSet[] { };
        [SerializeField] private EVENTCODE[] _frameEvents = new EVENTCODE[] { };
        private EVENTCODE[] frameEvents
        {
            get => _frameEvents;
            set
            {
                _frameEvents = value;
                BridgeUtil.onUpdateEyeActionStack?.Invoke(_frameEvents);
            }
        }

        IEnumerator ActionLoop(System.Action onComplete)
        {
            Frame _frame = _currentFrame = ChooseBestFrame();
            List<EVENTCODE> _frameEvents = null;
            for (int i = 0; i < _frameActions.Length; i++)
            {
                if (_frameActions[i]._frame == _frame)
                {
                    _frameEvents = _frameActions[i]._events; // Assign matching event filter
                    break;
                }
            }

            if (_debugOverrideEventCode != EVENTCODE.NULL)
            {
                _frameEvents = new List<EVENTCODE>(new EVENTCODE[]{ _debugOverrideEventCode });
            }

            bool waitForQuery = true; _state = State.Query;
            QueryInteractions(_frameEvents, (marker) =>
            {
                waitForQuery = false;
                _currentActionMarker = marker;

                this.frameEvents = (_currentActionMarker != null) ? _currentActionMarker.HitEvents.ToArray() : null;
            });
            while (waitForQuery) yield return null;

            bool waitForAction = true; _state = State.Action;
            if (_currentActionMarker != null) // Has valid marker with valid entity+action
            {
                Debug.LogWarning($"Success find marker: {_currentActionMarker.name} entity: {_currentActionMarker.HitEntity.gameObject.name} action: {_currentActionMarker.HitEvents.print()}");
                HandleActionLoop(_currentActionMarker.HitEntity, _currentActionMarker.HitEvents, () =>
                {
                    Debug.LogWarning($"Success handle action loop for entity {_currentActionMarker.HitEntity.gameObject.name} stack: {_currentActionMarker.HitEvents.print()}");
                    waitForAction = false;
                }, () =>
                {
                    Debug.LogError($"Fail handle action loop for entity {_currentActionMarker.HitEntity.gameObject.name} stack: {_currentActionMarker.HitEvents.print()}");
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
            else if (entity is Flower)
            {
                var _flower = (entity as Flower);
                
                _eventcodes.AddRange(new EVENTCODE[]
                {
                    EVENTCODE.BEACONADD
                });
                
                if(!_flower.IsOnFire) _eventcodes.Add(EVENTCODE.FLOWERFIRE);
                else _eventcodes.Add(EVENTCODE.FLOWEREXTINGUISH);
            }
            else if (entity is Star)
            {
                var _star = (entity as Star);
                
                _eventcodes.AddRange(new EVENTCODE[]
                {
                    EVENTCODE.SLAUGHTER
                });
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
            yield return new WaitForSecondsRealtime(Beat_T - Time.time);
            
            var w = _interactionMarkersContainer.rect.width;
            var h = _interactionMarkersContainer.rect.height;

            var xOffset = (1f - _screenInteractionFillWidth) / 2f;
            var yOffset = (1f - _screenInteractionFillHeight) / 2f;

            int markerI = 0;
            int totalMarkers = _queryInteractionsPerHeight * _queryInteractionsPerWidth;
            float _markerDelay = _markerInstantiateTime / totalMarkers;

            Task _waitTask = null;
            
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
                                
                                var subEventInt = (int) subEvent; Debug.LogWarning($"Subevent index: {subEventInt}");
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

                    if (++markerI >= (totalMarkers - 1)) //Last
                    {
                        _waitTask = _Util.WaitForNextBeatWithDelay(_markerDelay);
                        while (!_waitTask.IsCompleted) yield return null;
                    }
                    else
                    {
                        yield return new WaitForSecondsRealtime(_markerDelay);
                    }
                }
            }
            
            _waitTask = _Util.WaitForNextBeatWithDelay(_stepBetweenFailureMarkers);
            while (!_waitTask.IsCompleted) yield return null;

            var _misses = this.misses.ToArray();
            ClearInteractions(_misses);
            misses = new List<SmartInteractionMarker>();

            if (hits.Count > 0)
            {
                SmartInteractionMarker _successHit = hits[0];
                if (hits.Count > 1)
                {
                    _waitTask = _Util.WaitForNextBeatWithDelay(_stepBetweenValidMarker);
                    while (!_waitTask.IsCompleted) yield return null;
                    
                    _successHit = hits[Random.Range(0, hits.Count)];
                    
                    var _removeHits = hits.Except(new SmartInteractionMarker[] {_successHit}).ToArray();
                    ClearInteractions(_removeHits);
                }
                _waitTask = _Util.WaitForNextBeatWithDelay(_stepAfterValidMarker);
                while (!_waitTask.IsCompleted) yield return null;
                
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
        
        public void Beat(float a, float b)
        {
            Beat_T = b;
            _waitBeat = false;
        }

        public float Beat_T { get; private set; } = 0f;

        public Action<float, float> OnBeat => Beat;
    }
}