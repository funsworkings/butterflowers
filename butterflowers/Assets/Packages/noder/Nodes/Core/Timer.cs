using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.Core {

    public class Timer : SimpleEntry<float>, ITick, IReset
    {
        public float time;

        public void Tick(float timeDelta){
            time += timeDelta;
        }

        public void Reset(){
            time = 0f;
        }

        protected override float ValueProvider()
		{
            return time;
        }
    }

}
