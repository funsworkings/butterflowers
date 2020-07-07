using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;

namespace Noder.Nodes.Entries {

    public abstract class RandomArrayElement<E>: Flexible<E> {

        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public E[] elements;

        protected override E ValueProvider()
        {
            var el = GetInputValue<E[]>("elements", this.elements);
            if (el.Length > 0)
                return el[Random.Range(0, el.Length)];
            else
                throw new System.Exception("Node error = Unable to pull random element from empty array");
        }
    }

}
