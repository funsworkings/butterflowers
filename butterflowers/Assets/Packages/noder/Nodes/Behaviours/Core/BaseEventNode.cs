using System.Collections;
using System.Collections.Generic;
using Neue.Reference.Types.Maps;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Core {

    public abstract class BaseEventNode<E> : State {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override, typeConstraint = TypeConstraint.Inherited)] public E data;
        [SerializeField] protected FrameInt rewards;
        
        private bool fire = false;

        protected override void OnEnter()
        {
            base.OnEnter();

            if (!fire) 
                fire = FireEvent();
        }

        protected override void OnExit()
        {
            base.OnExit();
            fire = false;
        }

        protected abstract bool FireEvent();
    }

}
