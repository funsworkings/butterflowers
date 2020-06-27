using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Events
{
    public static System.Action<EVENTCODE, AGENT, AGENT, string> onFireEvent;

    public static void ReceiveEvent(EVENTCODE @event, AGENT a, AGENT b, string details = null)
    {
        bool isbeacon = (b == AGENT.Beacon);
        bool isunknown = (@event == EVENTCODE.UNKNOWN);

        if ((isbeacon || isunknown) && string.IsNullOrEmpty(details)) return;

        if (onFireEvent != null)
            onFireEvent(@event, a, b, details);
    }
}
