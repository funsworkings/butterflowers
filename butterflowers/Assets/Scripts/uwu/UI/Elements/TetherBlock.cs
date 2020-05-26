using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIExt.Elements
{

    public class TetherBlock<E> : ActionBlock, ITether<E>
    {
        public static System.Action<E> OnSelectTether;

        E m_tether;
        public E tether
        {
            get
            {
                return m_tether;
            }
            set
            {
                m_tether = value;
                OnUpdateTether(tether);
            }
        }


        protected virtual void OnUpdateTether(E tether)
        {

        }

        protected override void InvokeAction()
        {
            if (OnSelectTether != null)
                OnSelectTether(tether);
        }
    }

}
