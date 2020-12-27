using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Entries;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.Behaviours.Entries {
    
    
    [Obsolete("Obsolete API!", true)]
    public class RandomDialogue: Flexible<string> {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public DialogueCollection collection;

        protected override string ValueProvider()
        {
            DialogueCollection collection = GetInputValue<DialogueCollection>("collection", this.collection);

            if (collection != null)
            {
                string el = collection.FetchRandomItem();
                if (el != null)
                    return el;
            }

            throw new System.Exception("No dialogue collection assigned!");
        }
    }

}
