using UnityEngine;
using uwu.Settings.Instances;

namespace uwu.Settings.Types
{
	[CreateAssetMenu(fileName = "New Float Array", menuName = "Settings/Global/Bools", order = 52)]
	public class BoolArray : Global<BoolSetting, bool>
	{
	}
}