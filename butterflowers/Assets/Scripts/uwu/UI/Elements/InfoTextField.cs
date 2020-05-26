using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UIExt.Elements
{

    public abstract class InfoTextField : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] protected TMPro.TMP_Text tMP_Text;
        [SerializeField] protected UnityEngine.UI.Text text;

        [Header("Attributes")]
        public bool continuous = true;

        void OnEnable()
        {
            ParseInfo();
        }

        void Update()
        {
            if (!continuous)
                return;

            ParseInfo();
        }

        void ParseInfo()
        {
            onUpdateInfo(FetchInfo());
        }

        protected abstract string FetchInfo();

        void onUpdateInfo(string info)
        {
            if (info == null)
                info = ""; // Empty null info

            if (tMP_Text != null)
            {
                tMP_Text.text = info;
                return;
            }

            if (text != null)
            {
                text.text = info;
                return;
            }

            Debug.LogWarning("No text field has been set for " + GetType().FullName);
        }
    }

}
