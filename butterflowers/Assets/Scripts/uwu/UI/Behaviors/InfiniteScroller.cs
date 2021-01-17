using UnityEngine;

namespace uwu.UI.Behaviors
{
    [RequireComponent(typeof(RectTransform))]
    public class InfiniteScroller : MonoBehaviour
    {

        // Internal

        public enum Direction { Horizontal, Vertical, Both }

        // Properties

        Canvas canvas;

        RectTransform rect;
        RectTransform container;

        float w = -1f, h = -1f;
        float cw = -1f, ch = -1f;

        // Attributes

        [SerializeField] Direction scrollDirection = Direction.Horizontal;
        [SerializeField] float m_scrollSpeed = 1f;

        [SerializeField, Tooltip("If scrolling element is smaller than container, ignore scrolling behaviours.")] 
        bool ignoreContainedRect = false;
        
        
        #region Accessors

        float scrollSpeed => canvas.scaleFactor * m_scrollSpeed;
        
        #endregion
        

        #region Monobehaviour callbacks

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        // Start is called before the first frame update
        void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            container = transform.parent.GetComponent<RectTransform>();

            CalculateBounds();
        }

        // Update is called once per frame
        void Update()
        {
            Scroll();
        }

        #endregion

        #region Bounds

        void CalculateBounds()
        {
            w = rect.sizeDelta.x;
            h = rect.sizeDelta.y;

            cw = container.sizeDelta.x;
            ch = container.sizeDelta.y;
        }

        #endregion
        
        #region Scrolling

        void Scroll()
        {
            var dt = Time.unscaledDeltaTime;

            var px = rect.localPosition.x;
            var py = rect.localPosition.y;

            var mx = (scrollDirection == Direction.Horizontal || scrollDirection == Direction.Both);
            var my = (scrollDirection == Direction.Vertical || scrollDirection == Direction.Both);

            if (w <= cw && ignoreContainedRect) mx = false;
            if (h <= ch && ignoreContainedRect) my = false;

            if (mx)
            {
                px += (dt * scrollSpeed);
            }

            if (my)
            {
                py += (dt * scrollSpeed);
            }
            

            var position = new Vector2(px, py);
            rect.localPosition = position;
        }
        
        #endregion
    }
}
