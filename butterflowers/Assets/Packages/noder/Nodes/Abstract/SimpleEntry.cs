﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    public abstract class SimpleEntry<E> : Entry
    {
        [Output(connectionType: ConnectionType.Override)] public E value;
        
        protected abstract E ValueProvider();
        
        public override object GetValue(NodePort port) {
            if (port.fieldName == "value") 
                return ValueProvider();
            return null;
        }
        
        protected T GetData<T>() where T : Object {
            List<Data> datas = GetInputValues<Data>("Data").ToList();
            datas.RemoveAll(data => data == null);
            datas = datas.Where(data => data.data is T).ToList();
            if (datas.Count > 1)
                throw new System.Exception("Multiple Data found for type " + typeof(T) + " in " + name + 
                                    ", you should consider using GetData with a dataTag as parameter");
            if (datas.Count > 0)
                return datas.First().data as T;
            return null;
        }

        protected T GetData<T>(string tag) where T : Object {
            List<Data> datas = GetInputValues<Data>("Data").ToList();
            datas.RemoveAll(data => data == null);
            datas = datas.Where(data => data.data is T && data.tag == tag).ToList();
            if (datas.Count > 1)
                throw new System.Exception("Multi Data found for type " + typeof(T) + " and tag " + tag + 
                                    " in " + name + ", don't use the same dataTag twice as input");
            if (datas.Count > 0)
                return datas.First().data as T;
            return null;
        }
    }

}
