using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.States
{
    [NodeTint(1f, 1f, 0f)]
    public class Root : Basic { 
        private void Reset() {
            description = "Root";
        }
    }
}
