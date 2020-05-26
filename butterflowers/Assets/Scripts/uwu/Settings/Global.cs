using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    public class Global<U, T> : ScriptableObject where U:Setting<T>
    {
        public U[] settings;

        public T GetValueFromKey(string key){
            foreach(U s in settings){
                if(s.key == key)
                    return s.value;
            }
            return default(T);
        }
    }

}
