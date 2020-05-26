using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder {

    [CreateAssetMenu(fileName = "New Noder Graph", menuName="Noder/Graph", order=53)]
    public class Graph : NodeGraph, IReset { 

        public Node activeNode;
        public Node firstNode {
            get{
                Node node = null;
                for(int i = 0; i < nodes.Count; i++){
                    node = (nodes[i] as Node);
                    if(node != null) return node;
                }

                return null;
            }
        }
        public Node rootNode {
            get{
                Nodes.States.Root[] roots = GetNodes<Nodes.States.Root>().ToArray();
                for(int i = 0; i < roots.Length; i++){
                    if(roots[i] != null) return roots[i];
                }

                return null;
            }
        }

        public List<T> GetNodes<T>() where T : Node {
            List<T> nodes = new List<T>();
            foreach (Node node in this.nodes) {
                if (node as T) 
                    nodes.Add((T) node);
            }
            return nodes;
        }

        public List<ITick> GetTimers() {
            List<ITick> timers = new List<ITick>();
            foreach(Node node in this.nodes){
                if (node is ITick)
                    timers.Add((node as ITick));
            }

            return timers;
        }

        public void Start(){
            activeNode = rootNode;
            if(activeNode != null)
                activeNode.Enter();
        }

        public void Step(){
            if(activeNode != null)
                activeNode.Next();
            else
                Start();
        }

        public void Restart(){
            if(activeNode != rootNode)
                activeNode.Exit(); // Exit current node
            
            Start();
        } 

        public void Reset(){
            foreach(Node node in this.nodes){
                if(node is IReset){
                    ((IReset)node).Reset();
                }       
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
        }

    }

}