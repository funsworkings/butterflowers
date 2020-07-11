using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.Core.Miscellaneous {

    public class RandomNumber: Entry<float> {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public float min = 0f, max = 1f;

        protected override float ValueProvider()
        {
            float min = GetInputValue<float>("min", this.min);
            float max = GetInputValue<float>("max", this.max);

            return Random.Range(min, max);
        }
    }

}
