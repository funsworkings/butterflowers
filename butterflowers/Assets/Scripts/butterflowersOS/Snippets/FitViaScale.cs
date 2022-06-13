using UnityEngine;

namespace butterflowersOS.Snippets
{
    [ExecuteInEditMode, RequireComponent(typeof(RectTransform))]
    public class FitViaScale : MonoBehaviour
    {
        // Properties

        Canvas canvas;
        RectTransform rect;
        float width, height;
    
        // Attributes

        [SerializeField] float xScale = 1f, yScale = 1f;
        
        public float XScale
        {
            get => xScale;
            set => xScale = value;
        }

        public float YScale
        {
            get => yScale;
            set => yScale = value;
        }
    
        // Start is called before the first frame update
        void OnEnable()
        {
            canvas = GetComponentInParent<Canvas>();
            rect = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            width = rect.rect.width;
            height = rect.rect.height;
            
            var tw = Screen.width / canvas.scaleFactor * xScale;
            var th = Screen.height / canvas.scaleFactor * yScale;
        
            transform.localScale = new Vector3(tw / width, th / height, 1f);
        }
    }
}
