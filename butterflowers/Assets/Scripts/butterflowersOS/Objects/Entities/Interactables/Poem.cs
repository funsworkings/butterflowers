using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using UnityEngine;

namespace butterflowersOS.Objects.Entities.Interactables
{

    public class Poem : Focusable, ITooltip
    {
        public string GetInfo()
        {
            return gameObject.name.AppendActionableInformation(this);
        }
    }

}
