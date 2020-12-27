using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

using WMemory = Wizard.Memory;
using System.Linq;

namespace Noder.Nodes.External {

    [Obsolete("Obsolete API!", true)]
    public class Memory: BaseDialogueNode {

        public WMemory memory;
        public override string body => (memory == null) ? "" : string.Format(memory.body + ":m:{0}:m:", memory.name);

    }

}
