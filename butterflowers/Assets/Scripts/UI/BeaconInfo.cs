using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wizard;
using UnityEngine.UI;

public class BeaconInfo : MonoBehaviour
{
    Beacon m_beacon = null;
    public Beacon beacon {
        get 
        {
            return m_beacon;
        }

        set
        {
            m_beacon = value;

            string file = "";
            if (value != null)
                file = (beacon.fileEntry == null) ? beacon.file : beacon.fileEntry.ShortName;

            def_file = file;
        }
    }

    CanvasGroup canvasGroup;

    [SerializeField] TMPro.TMP_Text text;
    [SerializeField] Color unknownColor, knownColor;

    string def_file;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(AGENT agent)
    {
        if (agent == AGENT.User) 
        {
            text.text = def_file;
        }
        else if (agent == AGENT.Wizard) 
        {
            float knowledge = beacon.knowledge;

            float random = 1f - knowledge;

            Color color = Color.Lerp(unknownColor, knownColor, knowledge);
            string color_hex = Extensions.ParseColor(color);

            string body = def_file.Scramble(random);
            text.text = string.Format("<color={0}>", color_hex) + body;
        }

        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        if (beacon.overrideFocus && beacon.visible) 
        {
            Show(AGENT.Wizard);
            return;
        }

        canvasGroup.alpha = 0f;
    }
}
