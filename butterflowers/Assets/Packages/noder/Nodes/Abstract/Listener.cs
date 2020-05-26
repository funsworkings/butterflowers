using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract
{
    [NodeTint(.87f, .87f, .87f)]
    public abstract class Listener<E> : Node
    {
        public abstract void Receive(E dat);
    }
}

