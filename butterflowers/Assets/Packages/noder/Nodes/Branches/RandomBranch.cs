using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Nodes.Branches {

    public abstract class RandomBranch<E> : Branch<E> 
    {
        public sealed override void Next()
        {
            int index = Random.Range(0, routes.Length);
            Next(routes[index]);
        }
    }

}
