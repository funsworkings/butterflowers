using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    public abstract class SimpleAction : Action
    {   
        public List<Data> GetData() {
            if (GetInputPort("data") != null) {
                List<Data> datas = GetInputValues<Data>("data").ToList();
                return datas;
            }
            return null;
        }

        public override object GetValue(NodePort port) {
            if (port.fieldName == "reference")
                return this;
            return null;
        }
    }

}
