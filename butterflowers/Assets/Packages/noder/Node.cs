using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder {

    public abstract class Node : XNode.Node {

        public Graph Graph { get{ return (graph as Graph); } }

        public abstract void Next();

        protected void SendSignalToOutputs(XNode.NodePort[] ports){
            if(ports.Length <= 0) return;

            XNode.NodePort port;
            for(int i = 0; i < ports.Length; i++){
                port = ports[i];
                if(port != null){
                    XNode.NodePort connection;
                    for(int j = 0; j < port.ConnectionCount; j++){
                        connection = port.GetConnection(j);
                        (connection.node as Node).Enter(); // Enter selected node (after condition satisfied)
                    }
                }
            }
        }

        public bool isActive { get{ return (Graph.activeNode == this); } }

        public virtual void Enter(){ OnEnter(); }
        public virtual void Exit(){ OnExit(); }

        protected abstract void OnEnter();
        protected abstract void OnExit();
    }

}
