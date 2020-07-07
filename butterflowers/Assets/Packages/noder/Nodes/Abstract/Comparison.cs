using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Nodes.Abstract {

    public abstract class Comparison<E>: Entry<bool> where E : IComparable {

        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public E a, b;
        public Condition condition;

        private bool passes = false;

        protected override bool ValueProvider()
        {
            // Cast both inputs to ensure float is processed
            E a = GetInputValue<E>("a", this.a);
            E b = GetInputValue<E>("b", this.b);

            passes = condition.Satisfies<E>(a, b);
            return passes;
        }

    }

}
