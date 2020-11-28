using System.Collections;
using System.Collections.Generic;
using AI.Agent;
using AI.Types;
using AI.Types.Mappings;
using Objects.Managers;
using UnityEngine;
using XNode;

namespace Noder.Graphs {

    [CreateAssetMenu(fileName = "New Noder Module Tree", menuName = "Noder/Graphs/Module Tree", order = 53)]
    public class ModuleTree: Graph {
        
        public static System.Action<ModuleTree, EVENTCODE, object, BehaviourInt> onReceiveEvent;
        public static System.Action<ModuleTree, FailureCode> onFailEvent;
        public static System.Action<ModuleTree, BehaviourInt> onReceiveRewards;
        
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

        public BehaviourIntGroup rewards;
    }

}
