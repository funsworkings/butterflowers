using System;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Entries {

    [Obsolete("Obsolete API!", true)]
    public class BindVector3: Entry<Vector3> {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public float x, y, z;

        protected override Vector3 ValueProvider()
        {
            float x = GetInputValue<float>("x", this.x);
            float y = GetInputValue<float>("y", this.y);
            float z = GetInputValue<float>("z", this.z);

            return new Vector3(x, y, z);
        }

    }

}
