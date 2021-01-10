using UnityEngine;

namespace butterflowersOS.Miscellaneous
{
    [CreateAssetMenu(fileName = "New Texture Collection", menuName = "Extras/Texture Collection", order = 52)]
    public class TextureCollection : ScriptableObject
    {
        public Texture2D[] elements;
    }
}
