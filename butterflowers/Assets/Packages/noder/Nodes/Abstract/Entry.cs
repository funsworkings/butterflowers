using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    [NodeTint(.33f, 1f, .33f)]
    public abstract class Entry : Node
    {        
        // Overrides all abstract methods, does not call them ever

        public override void Next(){}
        protected override void OnEnter(){}
        protected override void OnExit(){}
    }

}
