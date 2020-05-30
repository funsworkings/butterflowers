using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Behaviors {

    public interface IActionListener {
        void Respond();
        void RespondWithData(object o);
    }

}