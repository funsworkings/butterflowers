using UnityEngine;

namespace butterflowersOS.Snippets
{
    [RequireComponent(typeof(RectTransform))]
    public class FitViaScale : MonoBehaviour
    {
        // Properties

        Canvas canvas;
        RectTransform rect;
        float width, height;
    
        // Attributes

        [SerializeField] float xScale = 1f, yScale = 1f;
    
        // Start is called before the first frame update
        void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            rect = GetComponent<RectTransform>();
            width = rect.rect.width;
            height = rect.rect.height;
        }

        // Update is called once per frame
        void Update()
        {
            var tw = Screen.width / canvas.scaleFactor * xScale;
            var th = Screen.height / canvas.scaleFactor * yScale;
        
            transform.localScale = new Vector3(tw / width, th / height, 1f);
        }
    }
}
