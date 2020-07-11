using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Events
{
    public static System.Action<EVENTCODE, AGENT, AGENT, string> onFireEvent;

    public static EVENTCODE LAST_EVENT;
    public static string LAST_EVENT_DETAIL = "";

    public static void ReceiveEvent(EVENTCODE @event, AGENT a, AGENT b, string details = null)
    {
        bool isbeacon = (b == AGENT.Beacon);
        bool isunknown = (@event == EVENTCODE.UNKNOWN);

        if ((isbeacon || isunknown) && string.IsNullOrEmpty(details)) return;

        LAST_EVENT = @event;
        LAST_EVENT_DETAIL = details;

        if (onFireEvent != null)
            onFireEvent(@event, a, b, details);
    }
}
