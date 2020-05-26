using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.Core.Operations {

    public class Arithmetic: SimpleEntry<float>
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]  public float a, b;

        public enum Operation { Add, Subtract, Divide, Multiply };
        public Operation operation = Operation.Add;


        protected override float ValueProvider()
		{
            float a  = GetInputValue<float>("a", this.a);
            float b  = GetInputValue<float>("b", this.b);

            switch(operation){
                case Operation.Add:
                    value = (a + b);
                    break;
                case Operation.Subtract:
                    value = (a - b);
                    break;
                case Operation.Divide:
                    value = (b == 0f)? 0f: (a / b);
                    break;
                case Operation.Multiply:
                    value = (a * b);
                    break;
                default:
                    value = 0f;
                    Debug.Log("Invalid operation set for arithmetic node!");
                    break;
            }

            return value;
        }
    }

}
