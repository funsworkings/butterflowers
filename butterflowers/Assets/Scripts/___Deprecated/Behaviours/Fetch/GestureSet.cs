using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;

namespace Noder.Nodes.Behaviours.Fetch {

    [Obsolete("Obsolete API!", true)]
    public class GestureSet: Entry<Gesture[]> {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public GestureCollection collection;

        protected override Gesture[] ValueProvider()
        {
            Gesture[] dat = GetInputValue<Gesture[]>("value", this.value);

            var col = GetInputValue<GestureCollection>("collection", this.collection);
            if (col != null) 
                dat = col.items;

            return dat;
        }

    }

}
