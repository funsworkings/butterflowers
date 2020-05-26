using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    [NodeTint(1f, .33f, .33f)]
    public abstract class Action : Node
    {
        [Output(connectionType: ConnectionType.Multiple)] public Action reference; // Reference to this action, called by connected node

        // Overrides all abstract methods, never called
        public override void Next() {}
        protected override void OnEnter() {}
        protected override void OnExit() {}

        public void Enact(){ Execute(); }
        protected abstract void Execute();
    }

}
