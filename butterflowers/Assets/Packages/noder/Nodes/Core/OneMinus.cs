using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.Core.Operations {

    public class OneMinus: Entry<float>
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]  public float input;


        protected override float ValueProvider()
		{
            float _input  = GetInputValue<float>("input", this.input);

            return (1f - value);
        }
    }

}
