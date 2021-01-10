using System.Collections.Generic;
using butterflowersOS.Core;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Entities.Interactables.Empty;
using UnityEngine;

namespace butterflowersOS
{
    public enum GAMESTATE {
        INTRO = 0,
        GAME = 1,
        ABSORB = 2
    }

    public enum AGENT {
        NULL = 0,

        User,
        Wizard,
        Inhabitants,
        Beacon,
        Nest,
        World,
        Terrain,
        MagicStar,
        Tree,

        Unknown = 255
    }

    public enum EVENTCODE {
        NULL = -1,
        UNKNOWN = 255,

        DISCOVERY = 0,

        NESTKICK = 10, //
        NESTPOP = 11, //
        NESTCLEAR = 12, //
        NESTSPILL = 13,
        NESTGROW = 14,
        NESTSHRINK = 15,

        BEACONACTIVATE = 20, //
        BEACONADD = 21,
        BEACONDELETE = 22, //
        BEACONPLANT = 23, //
        BEACONFLOWER = 24, //

        GESTURE = 30,
        PHOTOGRAPH = 31,
        SLAUGHTER = 32,
        REFOCUS = 33, //

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
            { AGENT.User, "#FF0000" },
            { AGENT.Wizard, "#27FFF8" },
            { AGENT.Inhabitants, "#E1C2FF" },

            { AGENT.Unknown, "#BEFFD7" }
        };
    }


    public static class Copy
    {
        public static string FocusText = string.Format("Press {0} to FOCUS", Controls.Focus.ToString());
        public static string LoseFocusText = string.Format("Press {0} to LOSE FOCUS", Controls.LoseFocus.ToString());

        public static string AppendActionableInformation(this string info, Entity entity)
        {
            string actions = "";
        
            if (entity is Nest) actions += "\nClick to KICK";
            else if (entity is Beacon) actions += "\nDrag into objects";
            else if (entity is Flower) actions += "\nClick to DUPLICATE";

            if (entity is Focusable) 
            {
                if (!(entity as Focusable).isFocused) actions += ("\n" + FocusText);
                else actions += ("\n" + LoseFocusText);
            }

            if (!string.IsNullOrEmpty(actions)) 
            {
                actions = actions.Insert(0, "\n");
                return info += string.Format("<size=50%><i>{0}</i></size>", actions);
            }

            return info;
        }

        public static string AppendContextualInformation(this string info, Entity entity, Wand.DragContext context)
        {
            string contextual = "";

            if (entity is Beacon) 
            {
                if (context == Wand.DragContext.Addition) contextual = "ABSORB";
                else if (context == Wand.DragContext.Destroy) contextual = "DESTROY";
                else if (context == Wand.DragContext.Flower) contextual = "FLOWER";
                else if (context == Wand.DragContext.Plant) contextual = "PLANT";
                else if (context == Wand.DragContext.Release) contextual = "";
            }

            if (!string.IsNullOrEmpty(contextual)) 
            {
                contextual = contextual.Insert(0, "\n\n");
                info += string.Format("<size=50%><i>{0}</i></size>", contextual);
            }

            return info;
        }
    }

    public static class Controls
    {
        public static KeyCode Focus = KeyCode.LeftControl;
        public static KeyCode LoseFocus = KeyCode.Escape;
    }
}