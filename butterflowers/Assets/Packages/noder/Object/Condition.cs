using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder {

    [System.Serializable]
    public class Condition {
        public enum Type {
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual,
            Equal,
            NotEqual
        }
        [NodeEnum] public Type type = Type.Equal;

        public bool Satisfies<E>(E a, E b) where E:IComparable
        {

            int comparison = a.CompareTo(b);

            bool passes = false;
            switch (type) {
                case Type.GreaterThan:
                    passes = comparison > 0;
                    break;
                case Type.GreaterThanOrEqual:
                    passes = comparison >= 0;
                    break;
                case Type.LessThan:
                    passes = comparison < 0;
                    break;
                case Type.LessThanOrEqual:
                    passes = comparison <= 0;
                    break;
                case Type.Equal:
                    passes = comparison == 0;
                    break;
                case Type.NotEqual:
                    passes = !(comparison == 0);
                    break;
                default:
                    passes = false;
                    break;
            }

            return passes;
        }
    }

}
