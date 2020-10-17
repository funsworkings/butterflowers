using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Types {

    [System.Serializable]
    public class Knowledge {

        public int file;
        public float time;

        public float AddTime(float dt) { time += dt; return time; }
        public void Forget() { time = 0f; }
    }

}
