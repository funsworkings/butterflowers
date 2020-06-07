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

        public bool Satisfies(float a, float b)
        {
            bool passes = false;
            switch (type) {
                case Type.GreaterThan:
                    passes = (a > b);
                    break;
                case Type.GreaterThanOrEqual:
                    passes = (a >= b);
                    break;
                case Type.LessThan:
                    passes = (a < b);
                    break;
                case Type.LessThanOrEqual:
                    passes = (a <= b);
                    break;
                case Type.Equal:
                    passes = (a == b);
                    break;
                case Type.NotEqual:
                    passes = (a != b);
                    break;
                default:
                    passes = false;
                    break;
            }

            return passes;
        }
    }

}
