using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using Settings;

using Setting = Settings.FontStyle.Style;
using FontStyle = UnityEngine.FontStyle;

namespace UIExt.Utilities {

    [ExecuteInEditMode]
    public class FontDriver : MonoBehaviour
    {
        public enum Type { Empty, Native, TMP }
        Type type = Type.Empty;

        public Setting setting = Setting.Body2;

        Local settings;
        
        [SerializeField] 
        Settings.FontStyle styling;

        Text text;
        TMP_Text tMP_Text;

        void Awake()
        {
            tMP_Text = GetComponent<TMP_Text>();
            if (tMP_Text == null)
            {
                text = GetComponent<Text>();
                if (text == null)
                    type = Type.Empty; // Disable component if no component available
                else
                    type = Type.Native;
            }
            else
                type = Type.TMP;
        }

        // Update is called once per frame
        void Update()
        {
            if (type == Type.Empty || styling == null) return;

            string key = System.Enum.GetName(typeof(Setting), setting);
            FontFormat format = styling.GetValueFromKey(key);

            if (format == null)
            {
                format = new FontFormat();
                format.weight = FontFormat.Weight.Bold;
                format.size = 64f;
                return;
            }

            float size = format.size;
            FontFormat.Weight weight = format.weight;


            if (type == Type.Native)
            {
                if (!Application.isPlaying)
                {
                    if (text == null)
                        text = GetComponent<Text>();
                }

                if (text == null)
                    return;

                text.font = styling.font;

                text.fontSize = (int)(size);
                text.fontStyle = GetNativeFontWeight(weight);
            }
            else if (type == Type.TMP)
            {
                if (!Application.isPlaying)
                {
                    if (tMP_Text == null)
                        tMP_Text = GetComponent<TMP_Text>();
                }

                if (tMP_Text == null)
                    return;

                tMP_Text.font = styling.tmpfont;

                tMP_Text.fontSize = size;
                tMP_Text.fontStyle = GetTMPFontWeight(weight);
            }
        }

        FontStyles GetTMPFontWeight(FontFormat.Weight weight)
        {
            if (weight == FontFormat.Weight.Bold)
                return FontStyles.Bold;

            return FontStyles.Normal;
        }

        FontStyle GetNativeFontWeight(FontFormat.Weight weight)
        {
            if (weight == FontFormat.Weight.Bold)
                return FontStyle.Bold;

            return FontStyle.Normal;
        }
    }

}
