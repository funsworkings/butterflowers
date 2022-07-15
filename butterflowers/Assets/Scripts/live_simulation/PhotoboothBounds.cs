using System;

namespace live_simulation
{
    [Serializable]
    public struct PhotoboothBounds
    {
        public float blurMin, blurMax;
        public float distortMin, distortMax;
        public float brightnessMin, brightnessMax;
        public float starVisMin, starVisMax;
        public float starSpeedMin, starSpeedMax;
        public float starTileMin, starTileMax;
    }
}