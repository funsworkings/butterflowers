using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Wizard {

    [CreateAssetMenu(fileName = "New Wizard Memory BANK", menuName = "Internal/Wizard/Bank", order = 52)]
    public class MemoryBank: ScriptableDatabase<Memory> {
 
        public Memory GetMemoryByName(string name)
        {
            int len = items.Length;

            if (len > 0) {
                for (int i = 0; i < len; i++) {
                    if (items[i].name == name) return items[i];
                }
            }
            return null;
        }

        public Memory GetMemoryByThumbnail(string thumbnail)
        {
            int len = items.Length;

            if (len > 0) {
                for (int i = 0; i < len; i++) {
                    if (items[i].thumbnail == thumbnail) return items[i];
                }
            }
            return null;
        }
    }

}
