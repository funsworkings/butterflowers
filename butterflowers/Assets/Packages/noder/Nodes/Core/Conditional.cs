using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;


namespace Noder.Nodes.Core.Operations {

    [System.Serializable]
    public class Condition {
        public enum Type {
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual,
            Equal,
            NotEqual
        }
        [NodeEnum] public Type type = Type.Equal;

        public bool Satisfies(float a, float b)
        {
            bool passes = false;
            switch (type) {
                case Type.GreaterThan:
                    passes = (a > b);
                    break;
                case Type.GreaterThanOrEqual:
                    passes = (a >= b);
                    break;
                case Type.LessThan:
                    passes = (a < b);
                    break;
                case Type.LessThanOrEqual:
                    passes = (a <= b);
                    break;
                case Type.Equal:
                    passes = (a == b);
                    break;
                case Type.NotEqual:
                    passes = (a != b);
                    break;
                default:
                    passes = false;
                    break;
            }

            return passes;
        }
    }

    public class Conditional : SimpleEntry<bool> 
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public float a, b;
        public Condition condition;

        private bool passes = false;

        protected override bool ValueProvider()
		{
            // Cast both inputs to ensure float is processed
            float a = GetInputValue<float>("a", this.a);
            float b = GetInputValue<float>("b", this.b);

            passes = condition.Satisfies(a, b);
            return passes;
        }
    }

}
