using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;

namespace Noder.Nodes.Entries {

    public class FetchArrayLength: Entry<int> {

        [Input(ShowBackingValue.Unconnected, ConnectionType.Override, typeConstraint:TypeConstraint.None)] public Object[] elements;

        protected override int ValueProvider()
        {
            var el = GetInputValue<object[]>("elements", this.elements);
            return el.Length;
        }
    }

}
