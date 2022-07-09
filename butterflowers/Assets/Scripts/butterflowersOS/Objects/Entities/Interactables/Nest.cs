using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Presets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using uwu;
using uwu.Audio;
using uwu.Extensions;
using uwu.Snippets;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace butterflowersOS.Objects.Entities.Interactables
{
    public class Nest : Focusable, IReactToSunCycle, ISaveable, IFlammable, ITooltip, IYves
    {
        public static Nest Instance = null;

        // Events

        public UnityEvent onOpen, onClose;
        public UnityEvent onIngestBeacon, onReleaseBeacon, onKick;

        // External

        [SerializeField] Quilt Quilt = null;
        [SerializeField] BeaconManager Beacons = null;
    
        Focusing Focusing;
        Camera main_camera;

        // Properties

        ApplyGravityRelativeToCamera gravity_ext;
        Material mat;
        new Collider collider;
        new Rigidbody rigidbody;
        [SerializeField] AudioHandler _audioHandler = null;
        Damage damage;

        [SerializeField] ParticleSystem sparklesPS = null, cometPS = null, deathPS = null;
        [SerializeField] GameObject pr_impactPS = null;

        [SerializeField] TMPro.TMP_Text infoText;

        [SerializeField] GameObject yvesMesh;
        
        // Attributes

        [Header("General")]

        public bool open = false;
        public bool queue = false;

        [SerializeField] bool disposeOnClose = true;
        [SerializeField] WorldPreset worldPreset = null;

        [Header("Physics")]
        [SerializeField] float force = 10f, m_energy = 0f;
        [SerializeField] float energyDecaySpeed = 1f, energyDecayDelay = 0f, timeSinceEnergyBoost = 0f;

        [Header("Beacons")]
        [SerializeField] List<Beacon> m_beacons = new List<Beacon>();
        [SerializeField] int m_capacity = 12;
        [SerializeField] float dropRadius = 3f, dropDistance = 50f;
        [SerializeField] LayerMask dropMask;

        [Header("Audio")] 
        [SerializeField] float minPitch = 1f;
        [SerializeField] float maxPitch = 2f;

        [Header("Debug")] 
        [SerializeField] float safePointRadius = 1f;
        [SerializeField] float safePointInterval = 1f;
        [SerializeField] List<Vector3> safePoints = new List<Vector3>();
        [SerializeField] float overrideFill = -1f;

        #region Accessors

        public float fill {
            get
            {
                int cap = capacity;
                int amt = beacons.Length;

                return (1f * amt) / cap;
            }
        }

        public int capacity { get { return m_capacity; } }
        public Beacon[] beacons { get { return m_beacons.ToArray(); } }

        public float energy => m_energy;

        public Vector3 trajectory => rigidbody.velocity.normalized;

        public int LEVEL => Mathf.FloorToInt(capacity / 6f) - 1;

        #endregion

        #region Monobehaviour callbacks

        protected override void Awake()
        {
            base.Awake();

            Instance = this;

            gravity_ext = GetComponent<ApplyGravityRelativeToCamera>();
            collider = GetComponent<Collider>();
            rigidbody = GetComponent<Rigidbody>();
            damage = GetComponent<Damage>();

            mat = GetComponent<Renderer>().sharedMaterial;
        }

        protected override void OnStart()
        {
            base.OnStart();
        
            Focusing = FindObjectOfType<Focusing>();
            main_camera = Camera.main;

            m_capacity = worldPreset.nestCapacity;

            if (Quilt == null) Quilt = FindObjectOfType<Quilt>();
            if (Beacons == null) Beacons = FindObjectOfType<BeaconManager>();

            if(damage != null)damage.onHit.AddListener(SpillKick);

            Beacon.Deleted += onDestroyBeacon;

            StartCoroutine("MaintainOnScreen");
            //StartCoroutine("FetchSafePoints");
        }

        protected override void Update()
        {
            base.Update();

            if (energy > 0f)
            {
                timeSinceEnergyBoost += Time.deltaTime;

                float t = Mathf.Max(0f, timeSinceEnergyBoost - energyDecayDelay);
                m_energy = Mathf.Max(0f, m_energy - Time.deltaTime * energyDecaySpeed * Mathf.Pow(t, 2f));
            }

            if (disposeduringframe) {
                DisposeDuringFrame();
                disposeduringframe = false;
            }

            //transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
            //transform.eulerAngles = new Vector3(Mathf.Round(transform.eulerAngles.x/15)*15, Mathf.Round(transform.eulerAngles.y/15)*15, Mathf.Round(transform.eulerAngles.z/15)*15);

            UpdateColorFromStateAndCapacity();
            _audioHandler.pitch = fill.RemapNRB(0f, 1f, minPitch, maxPitch);
        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            DrawSafePoints();
#endif
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            if(damage != null) damage.onHit.RemoveListener(SpillKick);

            Beacon.Deleted -= onDestroyBeacon;

            StopCoroutine("MaintainOnScreen");
        }

        #endregion

        #region Interactable callbacks

        protected override void onHover(Vector3 origin, Vector3 normal)
        {
            queue = true;

            base.onHover(origin, normal);
        }

        protected override void onUnhover()
        {
            queue = false;

            base.onUnhover();
        }

        protected override void onGrab(Vector3 origin, Vector3 direction)
        {
            Vector3 dir = (-direction - 3f * gravity_ext.gravity).normalized;
            AddForceAndOpen(origin, dir, force);
        }

        void AddForceAndOpen(Vector3 point, Vector3 direction, float force, AGENT agent = AGENT.User, bool particles = true, bool events = true)
        {
            rigidbody.AddForceAtPosition(direction * force, point);
            onKick.Invoke();

            if (particles) 
            {
                var impact = Instantiate(pr_impactPS, point, pr_impactPS.transform.rotation);
                impact.transform.up = direction.normalized;
                impact.GetComponent<ParticleSystem>().Play();
            }

            Open();

            if(events) 
                Events.ReceiveEvent(EVENTCODE.NESTKICK, agent, AGENT.Nest);
        }

        #endregion

        #region Core operations

        public bool Open()
        {
            if (!open) {
                open = true;
                onOpen.Invoke();

                return true;
            }
            return false;
        }

        public bool Close()
        {
            if (open) {
                if (disposeOnClose) Dispose();

                open = false;
                onClose.Invoke();

                return true;
            }
            return false;
        }

        public bool Dispose(bool release = true)
        {
            bool success = this.m_beacons.Count > 0;

            var beacons = this.m_beacons.ToArray();
            for (int i = 0; i < beacons.Length; i++) 
            {
                if (release) RemoveBeacon(beacons[i]);
                else beacons[i].Delete();
            }

            this.m_beacons = new List<Beacon>();

            Quilt.Dispose(true);

            return success;
        }

        public void Cycle(bool refresh)
        {
            if (worldPreset.emptyNestOnCycle)
            {
                Pulse();
                if (IsOnFire) Extinguish();
                if (refresh)
                    Close();
            }
        }

        #endregion

        #region Kicking

        void SpillKick()
        {
            RandomKick(1f, AGENT.NULL, ps: false, events: false);
        }

        public void RandomKick(float force = 1f, AGENT agent = AGENT.Inhabitants, bool ps = true, bool events = true)
        {
            Vector3 sphere_pos = Random.insideUnitSphere;
            Vector3 dir = -sphere_pos;

            Kick(dir, force, agent:agent);
        }

        public void Kick(Vector3 direction, float force = 1f, AGENT agent = AGENT.Inhabitants, bool ps = true, bool events = true)
        {
            Vector3 ray_origin = -direction * 5f;
            Vector3 ray_dir = direction;

            var ray = new Ray(transform.position + ray_origin, ray_dir);
            var hit = new RaycastHit();

            if (collider.Raycast(ray, out hit, 10f)) {
                var normal = hit.normal;

                Vector3 origin = hit.point;
                Vector3 dir = (-normal - 3f * gravity_ext.gravity).normalized;

                AddForceAndOpen(origin, dir, this.force * force, agent, particles: ps, events: events);
            }
        }

        IEnumerator MaintainOnScreen()
        {
            while (true) 
            {
                yield return new WaitForSeconds(3f);

                if (!Focusing.active && open && !World.Pause)  // Ignore if focus is focused on somethings
                {
                    Vector2 screen_pos = Vector2.zero;

                    bool visible = Extensions.IsVisible(transform, main_camera, out screen_pos);
                    if (!visible) {
                        Vector3 target_pos = main_camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 10f));
                        Vector3 dir = (target_pos - transform.position).normalized;

                        Kick(dir, 1f, AGENT.World); // Kick nest towards screen pos
                    }
                }
            }
        }

        #endregion

        #region Appearance

        void UpdateColorFromStateAndCapacity()
        {
            if (open) 
            {
                float fill = (overrideFill >= 0f)? overrideFill:(float)beacons.Length / capacity;
                //t_color = new Color(1f, (1f - fill), 1f);
                Shader.SetGlobalFloat("Fill", fill);
            }
            //else
                //t_color = inactiveColor;

            //mat.color = Color.Lerp(mat.color, t_color, Time.unscaledDeltaTime * colorSmoothSpeed);
        }

        #endregion

        #region Info
    
        public string GetInfo()
        {
            if (queue) 
            {
                string message = "{0} ({1})";
                string capacity = string.Format("{0} / {1}", beacons.Length, this.capacity);
            
                message = string.Format(message, "nest", capacity);
                message = message.AppendActionableInformation(this);
            
                return message;
            }

            return null;
        }

        #endregion

        #region Beacon operations

        public bool AddBeacon(Beacon beacon)
        {
            if (m_beacons.Contains(beacon)) return false;
            m_beacons.Add(beacon);

            if (beacon.IsOnFire) Fire(); // Set fire to nest with flaming beacon
        
            sparklesPS.Play();

            var dispose = (m_beacons.Count > capacity);
            if (dispose) 
            {
                disposeduringframe = true;
                return false;
            }
            else {
                disposeduringframe = false;
            }

            Pulse();

            onIngestBeacon.Invoke();

            var file = beacon.File;
            Quilt.Push(file);

            return true;
        }

        public bool RemoveBeacon(Beacon beacon)
        {
            if (!m_beacons.Contains(beacon)) return false;
            m_beacons.Remove(beacon);

            bool extinguish = true;
            foreach (Beacon b in beacons) 
            {
                if (b.IsOnFire) {
                    extinguish = false;
                    break;
                }
            }
        
            if(extinguish) Extinguish();

            Debug.LogFormat("Nest REMOVE = {0}", beacon.File);

            Vector3 origin = Vector3.zero;
            bool resetBeaconOrigin = false;

            try {
                origin = FindSafePositionWithinRadius();
                resetBeaconOrigin = true;
            }
            catch (System.Exception e) {
                Debug.LogWarning(e.Message);
            }

            beacon.RemoveFromNest(origin, resetBeaconOrigin);

            cometPS.Play();

            onReleaseBeacon.Invoke();

            var file = beacon.File;
            Quilt.Pop(file);

            return true;
        }

        Vector3 FindSafePositionWithinRadius()
        {
            float height = Mathf.Min(transform.position.y + dropDistance / 4f, dropDistance / 2f);
            Vector3 origin = transform.position + Vector3.up * height;

            float angle = Random.Range(0f, 2f * Mathf.PI);
            Vector3 offset = dropRadius * new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            var ray = new Ray(origin+offset, Vector3.down);
            var hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit, dropDistance, dropMask.value))
                return hit.point;

            throw new SystemException("Nest unable to find safe point within radius!");
        }

        bool disposeduringframe = false;
        void DisposeDuringFrame()
        {
            Dispose(true);

            Events.ReceiveEvent(EVENTCODE.NESTSPILL, AGENT.User, AGENT.Nest);
        
            if(damage != null)
                damage.Hit();
        }

        public Beacon RemoveLastBeacon()
        {
            if (m_beacons == null || m_beacons.Count == 0) return null;

            var beacon = m_beacons[m_beacons.Count - 1];
            RemoveBeacon(beacon);

            return beacon;
        }

        public void Pulse()
        {
            timeSinceEnergyBoost = 0f;
            m_energy = 1f;
        }

        #endregion

        #region Beacon helpers

        public bool HasBeacon(Beacon beacon)
        {
            return m_beacons.Contains(beacon);
        }

        #endregion

        #region Beacon callbacks

        void onDestroyBeacon(Beacon beacon)
        {
            if (!m_beacons.Contains(beacon)) return;

            Quilt.Pop(beacon.File);

            m_beacons.Remove(beacon);
            cometPS.Play();

            onReleaseBeacon.Invoke();
        }

        #endregion
    
        #region Collisions

        void OnTriggerEnter(Collider other)
        {
            Entity entity = other.GetComponent<Entity>();
            if (entity == null) return;

            //Debug.LogError("Nest collided with " + entity.name);

            if (entity is IFlammable) 
            {
                var e_Flammable = (entity as IFlammable);
            
                if (IsOnFire && !e_Flammable.IsOnFire) e_Flammable.Fire();
                else if(!IsOnFire && e_Flammable.IsOnFire) e_Flammable.Extinguish();
            }
        

            if (entity is Vine) 
            {
                Pulse();
                
                /*var vine = (entity as Vine);
            
                var file = vine.File;
                if(!string.IsNullOrEmpty(file)) 
                {
                    vine.Flutter();
                    Quilt.PushOverrideTexture(file);
                
                    Pulse();
                }*/
            }
        }

        /*
        void OnTriggerExit(Collider other)
        {
            
            Entity entity = other.GetComponent<Entity>();
            if (entity == null) return;
        
            if (entity is Vine) 
            {
                var vine = (entity as Vine);
            
                var file = vine.File;
                if(!string.IsNullOrEmpty(file)) 
                {
                    vine.Unflutter();
                    Quilt.PopOverrideTexture(file);
                }
            }
        }
        */

        #endregion
    
        #region Flammability

        public bool IsOnFire => deathPS.isPlaying;
    
        public void Fire()
        {
            deathPS.Play();
        }

        public void Extinguish()
        {
            deathPS.Stop();
        }
    
        #endregion
    
        #region Save/load

        public Object Save()
        {
            return open;
        }

        public void Load(Object dat)
        {
            var lastOpen = (bool)dat;
            if (lastOpen) Open();
            else Close();
        }

        #endregion
    
        #region Debug

#if UNITY_EDITOR
    
        IEnumerator FetchSafePoints()
        {
            while (true) 
            {
                safePoints.Clear();

                for (int i = 0; i < 10; i++) 
                {
                    try {
                        Vector3 pt = FindSafePositionWithinRadius();
                        safePoints.Add(pt);
                    }
                    catch (System.Exception e) {
                        Debug.LogWarning(e.Message);
                    }
                }
            
                yield return new WaitForSecondsRealtime(safePointInterval);
            }
        }

        void DrawSafePoints()
        {
            Handles.color = Color.green;
            float radius = safePointRadius;
        
            Vector3[] points = GetSafePoints();
            if (points == null)
                return;
        
            foreach (Vector3 pt in points) 
            {
                Handles.DrawWireCube(pt, Vector3.one * radius);
            }
        }
    
        Vector3[] GetSafePoints()
        {
            if (safePoints.Count == 0) return null;
            else return safePoints.ToArray();
        }
    
#endif
    
        #endregion
        
        #region Yves

        public void EnableYves()
        {
            yvesMesh.SetActive(true);
        }

        public void DisableYves()
        {
            yvesMesh.SetActive(false);
        }
        
        #endregion
    }
}
