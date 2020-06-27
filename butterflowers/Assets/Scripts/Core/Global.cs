using System.Collections.Generic;
using UnityEngine;

public enum AGENT {
    NULL,

    Inhabitant0,
    Inhabitant1,
    Inhabitants,
    Beacon,
    Nest,
    World,
    Terrain,

    Unknown = 255
}

public enum EVENTCODE {
    UNKNOWN = 255,

    DISCOVERY = 0,

    NESTKICK = 10,
    NESTPOP = 11,
    NESTCLEAR = 12,
    NESTSPILL = 13,
    NESTGROW = 14,
    NESTSHRINK = 15,

    BEACONACTIVATE = 20,
    BEACONADD = 21,
    BEACONDELETE = 22
}

public static class COLOR_LOOKUP 
{
    public static Dictionary<AGENT, string> AGENTS = new Dictionary<AGENT, string> 
    {
        { AGENT.Nest, "#FF00FF" },
        { AGENT.Beacon, "#00FF00" },
        { AGENT.Terrain, "#FFB742" },
        { AGENT.Inhabitant0, "#FF0000" },
        { AGENT.Inhabitant1, "#27FFF8" },
        { AGENT.Inhabitants, "#E1C2FF" },

        { AGENT.Unknown, "#BEFFD7" }
    };

    public static Dictionary<EVENTCODE, string> EVENTS = new Dictionary<EVENTCODE, string> 
    {

    };
}