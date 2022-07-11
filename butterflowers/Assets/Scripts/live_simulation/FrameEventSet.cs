using System.Collections.Generic;
using butterflowersOS;
using Neue.Reference.Types;

namespace live_simulation
{
    [System.Serializable]
    public struct FrameEventSet
    {
        public Frame _frame;
        public List<EVENTCODE> _events;
    }
}