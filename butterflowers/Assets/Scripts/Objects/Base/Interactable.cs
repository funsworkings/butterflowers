using UnityEngine;
using NativeInteractable = uwu.Gameplay.Interactable;

namespace Objects.Base
{
    [RequireComponent(typeof(NativeInteractable))]
    public class Interactable : Entity
    {

        // Properties

        NativeInteractable m_interactable;

        // Attributes

        [Header("Interaction")]
        [SerializeField] bool m_interactive = true;
        bool hovering = false;
    
        
        #region Accessors

        public bool interactive 
        {
            get
            {
                return m_interactive && Sun.active;
            }
            set
            {
                m_interactive = value;
                m_interactable.enabled = value;
            }
        }

        public NativeInteractable interactable
        {
            get { return m_interactable; }
            set
            {
                if (m_interactable != value) 
                {
                    UnsubscribeFromInteractableEvents();
                    m_interactable = value;
                    SubscribeToInteractableEvents();
                }
            }
        }

        #endregion

        #region Monobehaviour callbacks

        protected virtual void Awake()
        {
            m_interactable = GetComponent<NativeInteractable>();
            if (m_interactable == null)
                m_interactable = gameObject.AddComponent<NativeInteractable>();
        }

        // Start is called before the first frame update
        protected override void OnStart()
        {
            base.OnStart();

            SubscribeToInteractableEvents();

            interactive = m_interactive; // Set interactive behaviours
        }

        protected override void Update()
        {
            m_interactable.enabled = interactive;
            base.Update();
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
        
            UnsubscribeFromInteractableEvents();
        }

        #endregion
    
        #region Interactable event subscriptions

        void SubscribeToInteractableEvents()
        {
            interactable.onHover += onHover;
            interactable.onUnhover += onUnhover;

            interactable.onGrab += onGrab;
            interactable.onContinue += onContinue;
            interactable.onRelease += onRelease;
        }

        void UnsubscribeFromInteractableEvents()
        {
            interactable.onHover -= onHover;
            interactable.onUnhover -= onUnhover;

            interactable.onGrab -= onGrab;
            interactable.onContinue -= onContinue;
            interactable.onRelease -= onRelease;
        }
    
        #endregion

        #region Interactable callbacks

        protected virtual void onHover(Vector3 point, Vector3 normal)
        {
            hovering = true;
        }

        protected virtual void onUnhover(Vector3 point, Vector3 normal)
        {
            hovering = false;
        }

        protected virtual void onGrab(Vector3 point, Vector3 normal)
        {

        }

        protected virtual void onContinue(Vector3 point, Vector3 normal)
        {

        }

        protected virtual void onRelease(Vector3 point, Vector3 normal)
        {

        }

        #endregion
    }
}
