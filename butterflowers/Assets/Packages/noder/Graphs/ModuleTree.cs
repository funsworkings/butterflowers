using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;
using XNode;

namespace Noder.Graphs {

    [CreateAssetMenu(fileName = "New Noder Module Tree", menuName = "Noder/Graphs/Module Tree", order = 53)]
    public class ModuleTree: Graph {
        
        public static System.Action<ModuleTree, EVENTCODE, object> onReceiveEvent;
        public static System.Action<ModuleTree, string, float> onReceiveDialogue;

        Brain m_brain = null;
        public Brain Brain {
            get
            {
                return m_brain;
            }
            set
            {
                m_brain = value;
            }
        }

    }

}
