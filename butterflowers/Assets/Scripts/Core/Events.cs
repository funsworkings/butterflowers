public static class Events
{
    public static System.Action<EVENTCODE, AGENT, AGENT, string> onFireEvent;
    public static System.Action<EVENTCODE> onGlobalEvent;

    public static EVENTCODE LAST_EVENT;
    public static string LAST_EVENT_DETAIL = "";

    public static void ReceiveEvent(EVENTCODE @event, AGENT agentA, AGENT agentB, string details = null)
    {
        bool isbeacon = (agentB == AGENT.Beacon);
        if (isbeacon && string.IsNullOrEmpty(details)) return;

        if (agentA != AGENT.User) return;

        LAST_EVENT = @event;
        LAST_EVENT_DETAIL = details;

        if (onFireEvent != null)
            onFireEvent(@event, agentA, agentB, details);

        if (@event.isGlobalEvent()) 
        {
            if (onGlobalEvent != null)
                onGlobalEvent(@event);
        }
    }


    public static bool isGlobalEvent(this EVENTCODE @event)
    {
        return (@event == EVENTCODE.NESTSPILL || @event == EVENTCODE.SLAUGHTER || @event == EVENTCODE.NESTGROW || @event == EVENTCODE.NESTSHRINK);
    }
}
