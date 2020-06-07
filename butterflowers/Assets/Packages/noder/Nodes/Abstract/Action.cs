using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using System.Linq;

namespace Noder.Nodes.Abstract {

    [NodeTint(1f, .33f, .33f)]
    public abstract class Action : Node
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.Strict)] public Data data;
        [Output(connectionType: ConnectionType.Multiple)] public Action reference; // Reference to this action, called by connected node

        // Overrides all abstract methods, never called
        public override void Next() {}
        protected override void OnEnter() {}
        protected override void OnExit() {}

        public void Enact(){ Execute(); }
        protected abstract void Execute();

        public List<Data> GetData()
        {
            if (GetInputPort("data") != null) {
                List<Data> datas = GetInputValues<Data>("data").ToList();
                return datas;
            }
            return null;
        }

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "reference")
                return this;
            return null;
        }
    }

}
