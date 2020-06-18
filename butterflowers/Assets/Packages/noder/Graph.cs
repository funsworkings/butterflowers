using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder {

    [CreateAssetMenu(fileName = "New Noder Graph", menuName="Noder/Graph", order=53)]
    public class Graph : NodeGraph, IReset {

        public System.Action<Node> onUpdateNode;

        [SerializeField] Node m_rootNode = null;
        public Node rootNode {
            get
            {
                return m_rootNode;
            }
            set
            {
                m_rootNode = value;
            }
        }

        Node m_activeNode = null;
        public Node activeNode {
            get
            {
                return m_activeNode;
            }
            set
            {
                bool flag = (activeNode != value);

                m_activeNode = value;
                if (flag) {
                    if (onUpdateNode != null)
                        onUpdateNode(value);
                }
            }
        }

        public Node GetNodeByInstanceId(int id)
        {
            int len = nodes.Count;
            if (len == 0) return null;

            for (int i = 0; i < len; i++) {
                var node = nodes[i];
                if (node != null && node.GetInstanceID() == id)
                    return (nodes[i] as Node);
            }
            return null;
        }
        
        public List<T> GetNodes<T>() where T : Node {
            List<T> nodes = new List<T>();
            foreach (Node node in this.nodes) {
                if (node != null && node as T) 
                    nodes.Add((T) node);
            }
            return nodes;
        }

        public List<ITick> GetTimers() {
            List<ITick> timers = new List<ITick>();
            foreach(Node node in this.nodes){
                if (node != null && node is ITick)
                    timers.Add((node as ITick));
            }

            return timers;
        }

        void Start(){
            activeNode = rootNode;
            if (activeNode != null)
                activeNode.Enter();
        }

        public void Step(){
            if (activeNode != null)
                activeNode.Next();
            else
                Restart();
        }

        public void Restart(){
            Start(); // Set active node to root node
        }

        public void Dispose()
        {
            Reset();

            activeNode = null;
        }

        public void Reset(){
            foreach(Node node in this.nodes){
                if(node is IReset){
                    ((IReset)node).Reset();
                }       
            }
        }

    }

}