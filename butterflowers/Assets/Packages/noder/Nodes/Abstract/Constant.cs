using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract
{

    public abstract class Constant<E> : Entry<E>
    {
        public E input;

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
            return input;
        }
    }
    
}


