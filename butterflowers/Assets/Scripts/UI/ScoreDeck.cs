using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Extensions;

namespace UI
{

    public class ScoreDeck : MonoBehaviour
    {
        
        // Collections

        [SerializeField] ScoreCard[] cards;
        Dictionary<ScoreCard, int> cardIndices = new Dictionary<ScoreCard, int>();

        // Properties

        [SerializeField] bool open = false;
        [SerializeField] float t = 0f;

        [SerializeField] RectTransform anchor;
        [SerializeField] ScoreCard cardInFocus = null;
        [SerializeField] bool inprogress = false;
        
        [SerializeField] AnimationCurve openCurve, closeCurve, elasticCurve;
        [SerializeField] AnimationCurve speedCurve;
        
        // Attributes

        [SerializeField] float width, height;
        [Range(1f, 180f)] public float range = 180f;
        [Range(0f, 180f)] public float offset = 90f;
        [SerializeField] float focusSpeed = 1f;
        [SerializeField] float duration = 1f;


        // Start is called before the first frame update
        void Start()
        {
            cards = GetComponentsInChildren<ScoreCard>();
            for (int i = 0; i < cards.Length; i++) 
            {
                cardIndices.Add(cards[i], i);
            }
        }

        // Update is called once per frame
        void Update()
        {
            inprogress = (t >= 0f && t <= duration);
            
            if (inprogress) 
            {
                float dt = Time.unscaledDeltaTime;
                t += (open) ? dt : -dt;

                float ti = Mathf.Clamp01(t / duration);
                for (int i = 0; i < cards.Length; i++) 
                {
                    PlaceCard(cards[i], i, elasticCurve.Evaluate(ti));
                }

                cardInFocus = null;
            }
            else 
            {
                if (cardInFocus != null) 
                {
                    cardInFocus.rect.anchoredPosition = Vector2.Lerp(cardInFocus.rect.anchoredPosition, anchor.anchoredPosition, Time.deltaTime * focusSpeed); 
                    cardInFocus.rect.rotation = Quaternion.Lerp(cardInFocus.rect.rotation, anchor.rotation, Time.deltaTime * focusSpeed);
                    
                    if(Input.GetMouseButtonDown(0)) Exit(cardInFocus);
                }    
            }

            if (Input.GetKeyDown(KeyCode.P)) 
            {
                if(open) Close();
                else Open();
            }
        }
        
        #region Ops

        public void Open()
        {
            t = 0f;
            open = true;
        }

        public void Close()
        {
            t = (open) ? duration : 0f;
            open = false;
            
            if(cardInFocus != null) Exit(cardInFocus);
        }
        
        float GetInterval(int index){ return (index * 1f / (cards.Length-1)); }
        
        #endregion

        #region Visuals

        void PlaceCard(ScoreCard card, float time)
        {
            int index = cardIndices[card];
            PlaceCard(card, index, time);
        }

        void PlaceCard(ScoreCard card, int index, float time)
        {
            float interval = GetInterval(index);

            float angle = Mathf.Clamp(180f - (interval * range * time + offset), 0f, 180f); // Target angle of card
            float angleRads = angle * Mathf.Deg2Rad;
            
            Vector2 circle = new Vector2(Mathf.Cos(angleRads) * width, Mathf.Sin(angleRads) * height);
            Vector3 rotation = new Vector3(0f, 0f, angle - 90f);
            
            card.rect.anchoredPosition = circle;
            card.rect.eulerAngles = rotation;
        }
        
        #endregion
        
        #region Focus

        public void Enter(ScoreCard card)
        {
            if (inprogress) return;
            if (cardInFocus != null) return;
            
            cardInFocus = card;
            cardInFocus.transform.SetSiblingIndex(cards.Length-1);
        }

        public void Exit(ScoreCard card)
        {
            if (inprogress) return;

            if (cardInFocus == card) 
            {
                PlaceCard(cardInFocus, Mathf.Clamp01(t / duration));
                cardInFocus.transform.SetSiblingIndex(cardIndices[cardInFocus]);
                
                cardInFocus = null;
            }
        }
        
        #endregion
    }

}