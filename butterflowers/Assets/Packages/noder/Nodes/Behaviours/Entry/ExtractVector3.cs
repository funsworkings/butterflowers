using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Entries {

    public class ExtractVector3: Node {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public Vector3 value;
        [Output(connectionType: ConnectionType.Multiple)] public float x, y, z;

        public override void Next() { }

        protected override void OnEnter() { }
        protected override void OnExit() { }

        public override object GetValue(NodePort port)
        {
            Vector3 vector = GetInputValue<Vector3>("value", this.value);

            if (port.fieldName == "x")
                return vector.x;
            else if (port.fieldName == "y")
                return vector.y;
            else if (port.fieldName == "z")
                return vector.z;

            throw new System.Exception("Unable to find vector param during extract!");
        }

    }

}
