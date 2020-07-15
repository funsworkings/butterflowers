using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using NativeCursor = UnityEngine.Cursor;

public class CustomCursor : MonoBehaviour
{

    public enum State 
    {
        Normal,
        Hover,
        Down
    }

    public State state = State.Normal;

    [System.Serializable]
    public struct Setting {
        public State state;
        public Texture2D icon;
    }

    [SerializeField] Setting[] settings;

    // Update is called once per frame
    void Update()
    {
        var settings = this.settings.Where(s => s.state == state);
        if (settings.Count() > 0) 
        {
            var setting = settings.ElementAt(0);
            var tex = setting.icon;

            NativeCursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
        }
    }
}
