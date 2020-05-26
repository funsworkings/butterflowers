using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "New Font Style", menuName = "Settings/Fonts/Style", order = 52)]
    public class FontStyle : Global<FontSetting, FontFormat> { 
        public Font font;
        public TMPro.TMP_FontAsset tmpfont;

        public enum Style
        {
            //H1, H2, H3, H4, Body, Small, H5,

            //new
            ItemHeader,
            Header1,
            Header2,
            Subheader1,
            Body1,
            Body2,
            Notification,
            Search
        };
    }

}


