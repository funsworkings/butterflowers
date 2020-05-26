using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract
{

    public abstract class Constant<E> : SimpleEntry<E>
    {
        [Input(ShowBackingValue.Always, ConnectionType.Override, TypeConstraint.Strict)] public E input;

        string description = "Constant";
        public string Description {
            get{
                return description;
            }
            set{
                description = value;
            }
        }

        protected override E ValueProvider()
		{
            E val = GetInputValue<E>("input", input);
                input = val;

            return input;
        }
    }
    
}


