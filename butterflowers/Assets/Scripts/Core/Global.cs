﻿using System.Collections.Generic;
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
    NULL = -1,
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
    BEACONDELETE = 22,

    GESTURE = 30,
    PHOTOGRAPH = 31,
    SLAUGHTER = 32,

    DAY = 41,
    NIGHT = 42,
    CYCLE = 43
}

public enum SUGGESTION {
    NULL,

    ENDOW_KINDNESS,
    VISIT_TREE,
    ADD_BEACON
}

public enum INTENT {
    UNKNOWN,

    PLAY,
    FOIL,
    OBSERVE
}


public static class EVENT_WEIGHT_LOOKUP 
{
    public static Dictionary<EVENTCODE, float> WEIGHTS = new Dictionary<EVENTCODE, float>
   {
        { EVENTCODE.BEACONACTIVATE, .65F },
        { EVENTCODE.BEACONADD, 1F },
        { EVENTCODE.BEACONDELETE, -1F },

        { EVENTCODE.NESTCLEAR, -.96F },
        { EVENTCODE.NESTGROW, .93F },
        { EVENTCODE.NESTKICK, .7F },
        { EVENTCODE.NESTPOP, -.65F },
        { EVENTCODE.NESTSHRINK, -.9F },
        { EVENTCODE.NESTSPILL, -.93F },

        { EVENTCODE.DISCOVERY, 1F },
        { EVENTCODE.GESTURE, 0F },
        { EVENTCODE.PHOTOGRAPH, 1F },

        { EVENTCODE.NULL, 0F },
        { EVENTCODE.UNKNOWN, 0F }
    };
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