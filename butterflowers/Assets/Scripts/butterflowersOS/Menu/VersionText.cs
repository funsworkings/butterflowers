using TMPro;
using UnityEngine;

namespace butterflowersOS.Menu
{
    public class VersionText : MonoBehaviour
    {
        TMP_Text text;

        void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        // Start is called before the first frame update
        void Start()
        {
            text.text = string.Format("v {0}", Application.version);
        }
    }
}
