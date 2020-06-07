using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.Core.Operations {

    public class Not : Entry<bool>
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict)]  public bool input;

        protected override bool ValueProvider()
		{
            bool input = GetInputValue<bool>("input", this.input);
            return !input;
        }
    }

}
