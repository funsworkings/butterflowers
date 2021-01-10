using UnityEngine;

namespace butterflowersOS.Snippets
{
    public class BindDistortionWithOpacity : MonoBehaviour
    {
        [SerializeField] CanvasGroup opacity;
        [SerializeField] Material material;

        [SerializeField] AnimationCurve curve;
        [SerializeField] bool invert = false;

        // Update is called once per frame
        void LateUpdate()
        {
            if (opacity == null || material == null) return;

            float val = opacity.alpha;
            if (invert)
                val = (1f - val);

            material.SetFloat("_Distort", curve.Evaluate(val));
        }
    }
}
