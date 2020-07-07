using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.States;

namespace Noder.Nodes.Gates {

    public class ArrayLengthGate: Gate {

        [Input(ShowBackingValue.Never, ConnectionType.Override)] public Object[] elements;

        protected override bool EvaluateGate()
        {
            return elements.Length > 0;   
        }

    }

}
