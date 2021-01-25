using UnityEngine;
using Type = butterflowersOS.Objects.Entities.Interactables.Beacon.Type;
using Locale = butterflowersOS.Objects.Entities.Interactables.Beacon.Locale;

namespace butterflowersOS.Data
{
    [System.Serializable]
    public class BeaconData 
    {
        public sbyte type = 0;
        public sbyte state = -1;

        public ushort path = 0;

        public short x;
        public short y;
        public short z;

        public BeaconData(ushort path, Vector3 origin, Type type, Locale state)
        {
            this.path = path;
            this.type = (sbyte)type;
            this.state = (sbyte)state;

            this.x = (short)Mathf.RoundToInt(origin.x / Constants.BeaconSnapDistance);
            this.y = (short)Mathf.RoundToInt(origin.y / Constants.BeaconSnapDistance);
            this.z = (short)Mathf.RoundToInt(origin.z / Constants.BeaconSnapDistance);
        }
    }
}
