using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.Core.Operations {

    public class Logic : Entry<bool>
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict)]  public bool a, b;

        public enum Gate { And, Or }
        [NodeEnum] public Gate gate;

        private bool passes = false;

        protected override bool ValueProvider()
		{
            // Cast both inputs to ensure float is processed
            bool a = GetInputValue<bool>("a", this.a);
            bool b = GetInputValue<bool>("b", this.b);

            passes = false;

            switch(gate){
                case Gate.And:
                    passes = (a && b);
                    break;
                case Gate.Or:
                    passes = (a || b);
                    break;
                default:
                    break;
            }

            return passes;
        }
    }

}
