using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard {

    [System.Serializable]
    public class Knowledge {

        public string file;
        public float time;

        public void AddTime(float dt) { time += dt; }
        public void Forget() { time = 0f; }
    }

}
