using System.Collections;
using System.Collections.Generic;
using Neue.Agent.Brain;
using UnityEngine;
using UnityEngine.Events;
using uwu.Extensions;

namespace UI
{

    public class SummaryDeck : MonoBehaviour
    {
        #region Internal

        public enum State
        {
            Disabled,

            Normal,
            Focus
        }

        #endregion


        // Events

        public UnityEvent onOpen, onClose;

        // Collections

        [SerializeField] SummaryCard[] cards;
        Dictionary<SummaryCard, int> cardIndices = new Dictionary<SummaryCard, int>();

        // Properties

        [SerializeField] State state = State.Disabled;
        [SerializeField] bool open = false;
        [SerializeField] float t = 0f;

        [SerializeField] RectTransform anchor;
        Animator animator;
        
        [SerializeField] SummaryCard cardInQueue, cardInFocus = null;
        [SerializeField] int startingCardIndex = -1;
        [SerializeField] AnimationCurve elasticCurve;

        // Attributes

        [SerializeField] float width, height;
        [Range(1f, 180f)] public float range = 180f;
        [Range(0f, 180f)] public float offset = 90f;
        [SerializeField] float focusSpeed = 1f;
        [SerializeField] float openDuration, closeDuration;

        [HideInInspector] public bool inprogress = false;

        #region Accessors

        public State _State => state;
        public SummaryCard[] Items => cards;

        float duration => (open) ? openDuration : closeDuration;

        #endregion

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        // Start is called before the first frame update
        void Start()
        {
            cards = GetComponentsInChildren<SummaryCard>();
            
            for (int i = 0; i < cards.Length; i++) 
            {
                var card = cards[i];
                if (startingCardIndex < 0)
                    startingCardIndex = card.transform.GetSiblingIndex();
                
                cardIndices.Add(card, i);
            }
            
            PlaceAnchor();
        }

        // Update is called once per frame
        void Update()
        {
            if (state == State.Disabled) 
            {
                inprogress = (t >= 0f && t <= duration);
                
                if (inprogress) 
                {
                    float dt = Time.unscaledDeltaTime;
                    t += (open) ? dt : -dt;

                    float ti = Mathf.Clamp01(t / duration);
                    for (int i = 0; i < cards.Length; i++) {
                        PlaceCard(cards[i], i, elasticCurve.Evaluate(ti));
                    }

                    if (t < 0f)
                        onClose.Invoke();
                    else if (t > duration) 
                    {
                        onOpen.Invoke();
                        state = State.Normal;
                    }
                }
            }
            else if (state == State.Normal) 
            {
                if (cardInQueue != null) {
                    if (Input.GetMouseButtonUp(0))
                        SelectCardInQueue();
                }
            }
            else 
            {
                cardInFocus.rect.anchoredPosition = Vector2.Lerp(cardInFocus.rect.anchoredPosition,
                    anchor.anchoredPosition, Time.unscaledDeltaTime * focusSpeed);
                cardInFocus.rect.rotation = Quaternion.Lerp(cardInFocus.rect.rotation, anchor.rotation,
                    Time.unscaledDeltaTime * focusSpeed);
                cardInFocus.rect.localScale = Vector3.Lerp(cardInFocus.rect.localScale, cardInFocus.focusScale,
                    Time.unscaledDeltaTime * focusSpeed);

                if (cardInFocus == cardInQueue) 
                {
                    if (Input.GetMouseButtonUp(0))
                        DeselectCardInFocus();
                }
            }
        }

        void Dispose()
        {
            if (cardInQueue != null) Dequeue(cardInQueue);
            if (cardInFocus != null) DeselectCardInFocus();
        }

        #region Ops

        public void Open()
        {
            t = 0f;
            open = true;

            Dispose();
            state = State.Disabled;
        }

        public void Close(bool immediate = false)
        {
            t = (open && !immediate) ? closeDuration : 0f;
            open = false;

            Dispose();
            state = State.Disabled;
        }

        public void Drop()
        {
            animator.SetTrigger("drop");
        }

        float GetInterval(int index)
        {
            return (index * 1f / (cards.Length - 1));
        }

        #endregion

        #region Placement

        void PlaceAnchor()
        {
            Vector2 position = new Vector2(width * 2f, height);
            anchor.anchoredPosition = position;
        }

        void PlaceCard(SummaryCard card, float time)
        {
            int index = cardIndices[card];
            PlaceCard(card, index, time);
        }

        void PlaceCard(SummaryCard card, int index, float time)
        {
            float interval = GetInterval(index);

            float angle = Mathf.Clamp(180f - (interval * range * time + offset), 0f, 180f); // Target angle of card
            float angleRads = angle * Mathf.Deg2Rad;
            
            Vector2 circle = new Vector2(Mathf.Cos(angleRads) * width, Mathf.Sin(angleRads) * height);
            Vector3 rotation = new Vector3(0f, 0f, angle - 90f);
            
            card.rect.anchoredPosition = circle + new Vector2(width*2f, 0f);
            card.rect.eulerAngles = rotation;
            card.rect.localScale = card.normalScale;

            ResetToIndex(card);
        }
        
        #endregion
        
        #region Order
        
        void BringToFront(SummaryCard card){ card.transform.SetSiblingIndex(startingCardIndex + cards.Length-1); }
        void ResetToIndex(SummaryCard card){ card.transform.SetSiblingIndex(startingCardIndex + cardIndices[card]); }
        
        #endregion
        
        #region Focus

        public void Queue(SummaryCard card)
        {
            if (state == State.Disabled) return;
            if (cardInQueue != null) return;
            
            cardInQueue = card;
            if(state == State.Normal) BringToFront(cardInQueue);
        }

        public void Dequeue(SummaryCard card)
        {
            if (state == State.Disabled) return;
            if (cardInQueue != card) return;
            
            if(state == State.Normal) PlaceCard(cardInQueue, 1f); // Reset position of card (only for unqueue card)
            cardInQueue = null;
        }

        void SelectCardInQueue()
        {
            if (state != State.Normal) return;
            if (cardInQueue == null) return;
            
            cardInFocus = cardInQueue;
            cardInFocus.focus = true;
            
            state = State.Focus;
        }

        void DeselectCardInFocus()
        {
            if (state != State.Focus) return;

            PlaceCard(cardInFocus, 1f);
            cardInFocus.focus = false;
            cardInFocus = null;

            state = State.Normal;
        }

        #endregion
    }

}