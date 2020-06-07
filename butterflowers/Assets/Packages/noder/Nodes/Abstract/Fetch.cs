using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract
{

    public class Fetch : Entry<Data>
    {
        public Data dat;

        protected override Data ValueProvider()
        {
            return dat;
        }
    }
    
}


