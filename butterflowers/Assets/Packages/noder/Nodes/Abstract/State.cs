using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    public abstract class State : Node
    {
        [Input(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Inherited)] public Node input;
    }

}
