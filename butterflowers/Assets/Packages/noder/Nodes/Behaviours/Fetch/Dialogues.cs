using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;

namespace Noder.Nodes.Behaviours.Fetch {

    public class Dialogues: Entry<string[]> {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public DialogueCollection collection;

        protected override string[] ValueProvider()
        {
            string[] dat = GetInputValue<string[]>("value", this.value);

            var collection = GetInputValue<DialogueCollection>("collection", this.collection);
            if (collection != null)
                dat = collection.elements;

            return dat;
        }

    }

}
