using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Generic;


namespace Wizard {

    [Obsolete("Obsolete API!", true)]
    [CreateAssetMenu(fileName = "New Wizard Memory BANK", menuName = "Internal/Wizard/Bank", order = 52)]
    public class Memories: ScriptableDatabase<Memory> {
 
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

        public override bool Contains(Memory item)
        {
            if (item == null) return false;

            for (int i = 0; i < items.Length; i++) {
                if (item.name == items[i].name)
                    return true;
            }
            return false;
        }

        public Memory AddMemory(Texture2D image, string thumbnail = null)
        {
            if (string.IsNullOrEmpty(thumbnail))
                thumbnail = image.name;

            Memory mem = ScriptableObject.CreateInstance<Memory>();
            mem.image = image;
            mem.thumbnail = thumbnail;

            bool success = Add(mem); // Add to database
            if (success) return mem;
            else return null;
        }
    }

}
