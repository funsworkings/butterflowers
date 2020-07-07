using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;


namespace Noder.Nodes.Core.Operations {

    public class Conditional : Entry<bool> 
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public float a, b;
        public Condition condition;

        private bool passes = false;

        protected override bool ValueProvider()
		{
            // Cast both inputs to ensure float is processed
            float a = GetInputValue<float>("a", this.a);
            float b = GetInputValue<float>("b", this.b);

            passes = condition.Satisfies<float>(a, b);
            return passes;
        }
    }

}
