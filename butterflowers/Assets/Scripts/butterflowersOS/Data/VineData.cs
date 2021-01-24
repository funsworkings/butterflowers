using System.Linq;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Miscellaneous;
using UnityEngine;

namespace butterflowersOS.Data
{
    [System.Serializable]
    public class VineData
    {
        public sbyte status;

        public byte index = 0;
        public byte interval = 0;

        public ushort file = 0;

        public short[] waypoints_x;
        public short[] waypoints_y;
        public short[] waypoints_z;

        public LeafBundleData leaves;

        public VineData(Vine.Status status, byte index, byte interval, Vector3[] waypoints, ushort file, Leaf[] leaves)
        {
            this.status = (sbyte)status;
            this.index = index;
            
            this.interval = interval;

            waypoints_x = waypoints.Select(w => (short)Mathf.RoundToInt(w.x / Constants.VineWaypointSnapDistance)).ToArray();
            waypoints_y = waypoints.Select(w => (short)Mathf.RoundToInt(w.y / Constants.VineWaypointSnapDistance)).ToArray();
            waypoints_z = waypoints.Select(w => (short)Mathf.RoundToInt(w.z / Constants.VineWaypointSnapDistance)).ToArray();

            this.file = file;

            this.leaves = new LeafBundleData();
            this.leaves.leaves = (leaves != null)? leaves.Select(l => new LeafData(l)).ToArray():null;
        }
    }
}
