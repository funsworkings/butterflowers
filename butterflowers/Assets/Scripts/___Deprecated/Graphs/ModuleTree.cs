using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS;
using Neue.Agent.Actions.Types;
using Neue.Reference.Types.Maps;
using Neue.Reference.Types.Maps.Groups;
using Neue.Types;
using Objects.Managers;
using UnityEngine;
using XNode;

namespace Noder.Graphs {

    [Obsolete("Obsolete API!", true)]
    [CreateAssetMenu(fileName = "New Noder Module Tree", menuName = "Noder/Graphs/Module Tree", order = 53)]
    public class ModuleTree: Graph {
        
        public static System.Action<ModuleTree, EVENTCODE, object, FrameInt> onReceiveEvent;
        public static System.Action<ModuleTree, FailureCode> onFailEvent;
        public static System.Action<ModuleTree, FrameInt> onReceiveRewards;
        
        public static System.Action<ModuleTree, string, float> onReceiveDialogue;

        ModuleTreeHelper m_brain = null;
        public ModuleTreeHelper Brain {
            get
            {
                return m_brain;
            }
            set
            {
                m_brain = value;
            }
        }

        public FrameIntGroup rewards;
    }

}
