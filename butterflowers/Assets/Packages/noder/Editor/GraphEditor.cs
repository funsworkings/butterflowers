using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNodeEditor;
using UnityEditor;

namespace Noder {

    [CustomNodeGraphEditor(typeof(Graph))]
    public class GraphEditor : NodeGraphEditor
    {
        public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace.Contains("Noder")) {
				return base.GetNodeMenuName(type).Replace("Noder/", "");
			} else return null;
		}
    }

}
