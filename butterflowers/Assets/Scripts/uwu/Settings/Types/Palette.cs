using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "New Palette", menuName = "Settings/Colors/Palette", order = 52)]
    public class Palette: Global<ColorSetting, Color> {

        public enum Style
        {
            None,
            Default
        }

        public Color FetchColorFromSetting(Style style, float opacity = 1f)
        {
            Color c = Color.black;

            string key = System.Enum.GetName(typeof(Style), style);
            c = GetValueFromKey(key);
            c.a = Mathf.Clamp01(opacity);

            return c;
        }

    }

}


