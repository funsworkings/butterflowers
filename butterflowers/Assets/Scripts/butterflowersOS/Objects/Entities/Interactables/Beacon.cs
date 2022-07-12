﻿using System;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities.Interactables.Empty;
using butterflowersOS.Presets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace butterflowersOS.Objects.Entities.Interactables
{
    public class Beacon: Interactable, IFlammable, ITooltip, IFileContainer, IYves {

        #region Internal

        public enum Type 
        {
            Desktop,
            World
        }

        public enum Status 
        {
            NULL,

            UNKNOWN,
            COMFORTABLE,
            ACTIONABLE
        }

        public enum Locale 
        {
            Terrain,
            Nest,
            Planted,
            Destroyed,
            Drag,
            Flower
        }

        [System.Serializable]
        public class Transition
        {
            [System.Serializable] public class Event :UnityEvent<Beacon, Vector3> {}
            public Event onBegin, onEnd;
            public Event onInstanceBegin, onInstanceEnd;

            [HideInInspector] public Vector3 posA, posB;
            [HideInInspector] public Vector3 scaleA, scaleB;

            public float time = 0f;
            public float duration = 0f;
            public float height = 0f;
            public float delay = 0f; public float delay_time = 0f;
            
            public Transform _tracking = null; // Object to attract to
            
            public AnimationCurve heightCurve;
            public AnimationCurve scaleCurve;
            public AnimationCurve positionCurve;

            public Transition()
            {
                
            }
            
            public Transition(Transition copy)
            {
                this.onBegin = copy.onBegin;
                this.onEnd = copy.onEnd;

                this.posA = copy.posA;
                this.posB = copy.posB;
                this.scaleA = copy.scaleA;
                this.scaleB = copy.scaleB;

                this.time = copy.time;
                this.delay_time = copy.time;
                this.delay = copy.delay;
                this.duration = copy.duration;
                this.height = copy.height;

                this.heightCurve = copy.heightCurve;
                this.scaleCurve = copy.scaleCurve;
                this.positionCurve = copy.positionCurve;
            }

            public bool Continue(Beacon beacon, float dt)
            {
                if (delay_time < delay)
                {
                    delay_time += dt;
                    return false;
                }
                
                bool flagStart = (time <= 0f), flagEnd = (time+dt >= duration);

                time += dt;

                float interval = Mathf.Clamp01(time / duration);

                if (_tracking != null) posB = _tracking.position;

                Vector3 position = (posA != posB)? Vector3.Lerp(posA, posB, positionCurve.Evaluate(interval)) : posB;
                position += (Vector3.up * height * heightCurve.Evaluate(interval));

                float scaleLerp = scaleCurve.Evaluate(interval);
                Vector3 scale =  (scaleB - scaleA) * scaleLerp + scaleA;

                beacon.transform.position = position;
                beacon.transform.localScale = scale;
            
                if(flagStart) {onBegin?.Invoke(beacon, position);onInstanceBegin?.Invoke(beacon, position);}
                if(flagEnd) {onEnd?.Invoke(beacon, position); onInstanceEnd?.Invoke(beacon, position);}
            
                return time >= duration;
            }

            public void AddCallback(Action onComplete)
            {
                Debug.LogWarning("Did add new callback for beacon!");
                if (onInstanceEnd == null) onInstanceEnd = new Event();
                onInstanceEnd.AddListener(((beacon, pos) =>
                {
                    onComplete?.Invoke();
                }));
            }
        }

        #endregion
    
        // External
    
        World Room;
        Nest Nest;

        // Events

        public static System.Action<Beacon> OnRegister, OnUnregister;
        public static System.Action<Beacon> Activated, Deactivated, Destroyed, Deleted, Planted, Flowered;
        public static System.Action<Beacon, bool> onFire, onExtinguish;
        public static System.Action<Beacon> onUpdateState;

        public UnityEvent OnFlower, OnVine, OnSpawn;
        [FormerlySerializedAs("OnDestroy")] public UnityEvent OnDestruct;
        public UnityEvent OnFire, OnExtinguish, OnFlowerSpawn;

        // Properties

        [SerializeField] WorldPreset preset = null;
        [SerializeField] ParticleSystem deathPS = null;
        [SerializeField] TrailRenderer trails = null;
    
        new MeshRenderer renderer;
        new Collider collider;
        Material material;
        [SerializeField] Material yvesMat;
        
        
        [SerializeField] GameObject pr_impactPS = null;
        [SerializeField] GameObject pr_flower = null;
    
        public Type type;
        public Locale state = Locale.Terrain;
        public Flower flower = null;
    
        [SerializeField] string m_file = null;

        public Vector3 origin = Vector3.zero;
        public Vector3 size = Vector3.one;
    
        [SerializeField] Transition releaseTransition = null;

        // Attributes

        Transition transition = null;
        [SerializeField] bool transitioning = false;
    
        [SerializeField] bool m_discovered = false, m_destroyed = false;


        #region Accessors

        public string File {
            get
            {
                return m_file;
            }
            set
            {
                m_file = value;
            }
        }

        public Vector3 Origin => origin;

        public bool discovered {
            get
            {
                return m_discovered;
            }
            set
            {
                m_discovered = value;
            }
        }

        public bool destroyed
        {
            get { return m_destroyed; }
            private set { m_destroyed = value; }
        }

        public float knowledge 
        {
            get
            {
                return (Room == null)? 1f:Room.FetchBeaconKnowledgeMagnitude(this);
            }
        }
    
        public bool visible => state == Locale.Terrain;

        public TrailRenderer Trails => trails;

        #endregion

        #region Monobehaviour callbacks

        protected override void Update()
        {
            UpdateColor();
            base.Update();

            if (transitioning) 
            {
                transitioning = !transition.Continue(this, Time.deltaTime);
                if (!transitioning) transition = null;
                
                ToggleCapabilities(!transitioning);
            }
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            
            Debug.LogFormat("Beacon => {0} was destroyed!", gameObject.GetInstanceID());
        }

        #endregion

        #region Interactable callbacks

        protected override void onHover(Vector3 point, Vector3 normal)
        {
            if (!Active) return;
            base.onHover(point, normal);
        }

        protected override void onUnhover()
        {
            if (!Active) return;
            base.onUnhover();
        }

        #endregion

        #region Registration & Initialization

        public void Register(Type type, Locale state, Vector3 origin, Transition transition, bool load = false)
        {
            Room = World.Instance;
            Nest = Nest.Instance;

            collider = GetComponent<Collider>();
            renderer = GetComponentInChildren<MeshRenderer>();
            material = renderer.material;

            this.type = type;
            var targetState = state;
            this.origin = origin;
        
            this.size = preset.normalBeaconScale * Vector3.one;

            if(IsOnFire) Extinguish();

            if (load) 
            {
                switch (targetState) // Trigger initial state from target (normal mechanics)
                {
                    case Locale.Flower:
                        Flower(origin, events: false);
                        break;
                    case Locale.Nest:
                        AddToNest();
                        break;
                    case Locale.Planted:
                        Plant(origin, events: false);
                        break;
                    case Locale.Terrain:
                        ToggleCapabilities(true);
                        transform.localScale = size;
                        break;
                    default: break;
                }
            }
            else 
            {
                if(state == Locale.Terrain) OnSpawn.Invoke();    
            }
            
            this.state = state;
        
            StartTransition(transition);
            if (transitioning) 
            {
                transitioning = !this.transition.Continue(this, Time.deltaTime);
                ToggleCapabilities(!transitioning);
            }

            if (OnRegister != null)
                OnRegister(this);
        }

        void Unregister()
        {
            if (OnUnregister != null)
                OnUnregister(this);
            
            Dispose();
        }
        
        /// <summary>
        /// Remove all beacon attributes (usually on unregister)
        /// </summary>
        void Dispose()
        {
            this.type = default(Type);
            this.state = Locale.Terrain;
            this.flower = null;

            this.m_file = null;

            this.transition = null;

            this.transitioning = false;
            this.m_discovered = false;
            this.m_destroyed = false;
        }

        #endregion

        #region Operations

        public bool AddToNest(bool events = true) 
        {
            if (state == Locale.Nest) return false;
            state = Locale.Nest;

            var impact = Instantiate(pr_impactPS, transform.position, transform.rotation);
            impact.GetComponent<ParticleSystem>().Play(); // Trigger particle sys

            transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
        
            ToggleCapabilities(false);
            transform.localScale = Vector3.zero;
        
            if (Activated != null && events)
                Activated(this);

            return true;
        }
    
        public bool RemoveFromNest(Vector3 origin, bool resetOrigin = false, bool events = true)
        {
            Debug.LogFormat("Deactivate {0} when state is {1}", File, state);
            if (state != Locale.Nest) return false;
            state = Locale.Terrain;

            transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
            transform.localScale = Vector3.zero;

            if (resetOrigin) this.origin = origin;
            ReleaseTransition();

            if (Deactivated != null && events)
                Deactivated(this);

            return true;
        }

        public bool Plant(Vector3 point, bool events = true)
        {
            if (state == Locale.Planted) return false;
            state = Locale.Planted;
        
            origin = point;
        
            ToggleCapabilities(false);
            if(IsOnFire)Extinguish();
            transform.localScale = Vector3.zero;

            OnVine.Invoke();
            if (Planted != null && events)
                Planted(this);

            return true;
        }

        public bool Flower(Vector3 point, bool events = true)
        {
            if (state == Locale.Flower) return false;
            state = Locale.Flower;
        
            origin = point;
        
            ToggleCapabilities(false);
            if(IsOnFire)Extinguish();
            transform.localScale = Vector3.zero;

            if (flower == null) 
            {
                var flowerInstance = Instantiate(pr_flower, origin, Quaternion.identity);
            
                flower = flowerInstance.GetComponentInChildren<Flower>(); 
                flower.Grow(global::butterflowersOS.Objects.Entities.Interactables.Empty.Flower.Origin.Beacon, File, type);    
            }

            OnFlower.Invoke();
            if (Flowered != null && events)
                Flowered(this);
        
            return true;
        }

        public bool Delete(bool events = true)
        {
            Debug.LogFormat("Attempt to delete beacon => {0}", m_file);
        
            if (state == Locale.Destroyed) return false;
            state = Locale.Destroyed;

            if (Deleted != null && events) 
            {
                Deleted(this);
            }
        
            Unregister();
            return true;
        }

        public void Grab()
        {
            if (state != Locale.Terrain) return;
        
            state = Locale.Drag;
            ToggleCapabilities(false);
        }

        public void Release()
        {
            if (state != Locale.Drag) return;
            state = Locale.Terrain;
        
            ToggleCapabilities(true);
            ReleaseTransition();
        }

        public bool CustomDestroy(bool events = true)
        {
            if (state != Locale.Drag) return false;
            state = Locale.Terrain;
        
            ReleaseTransition();
            Fire();

            OnDestruct.Invoke();
            if (Destroyed != null && events) 
                Destroyed(this);

            return true;
        }

        #endregion

        #region Appearance

        void UpdateColor() 
        {
            if (material == null) return;

            float hue = 0f;
            float sat = 0f, t_sat = (((Element) this).Active) ? 1f : 0f;
            float val = 0f;

            Color.RGBToHSV(material.color, out hue, out sat, out val);
            Color actual = Color.HSVToRGB(hue, Mathf.Lerp(sat, t_sat, Time.deltaTime), val);
            actual.a = material.color.a;
        
            material.color = actual;
        }

        #endregion
    
        #region Capabilities

        void ToggleCapabilities(bool capable)
        {
            collider.enabled = capable;
        }
    
        #endregion
    
        #region Flammable

        public bool IsOnFire
        {
            get => deathPS.isPlaying;
        }

        public void Fire()
        {
            deathPS.Play();
            onFire?.Invoke(this, false);
            OnFire.Invoke();
        }

        public void Extinguish()
        {
            deathPS.Stop();
            onExtinguish?.Invoke(this, false);
            OnExtinguish.Invoke();
        }
    
        #endregion
    
        #region Element overrides

        protected override bool EvaluateActiveState()
        {
            return Sun.active && Nest.open;
        }

        #endregion

        #region Transitions

        public void StartTransition(Transition transition)
        {
            if (transition == null) 
            {
                this.transition = null;
                transitioning = false;
                
                return;
            }

            var _transition = transition;
                _transition.time = 0f;

            this.transition = _transition;
            transitioning = true;
        }

        void ReleaseTransition()
        {
            Transition _transition = releaseTransition;
            _transition.posA = transform.position;
            _transition.posB = origin;
            _transition.scaleA = transform.localScale;
            _transition.scaleB = size;

            StartTransition(_transition);
        }

        #endregion

        #region Info
    
        public string GetInfo()
        {
            return File.AppendActionableInformation(this);
        }
    
        #endregion

        public void EnableYves()
        {
            renderer.material = yvesMat;
        }

        public void DisableYves()
        {
            renderer.material = material;
        }
    }
}
