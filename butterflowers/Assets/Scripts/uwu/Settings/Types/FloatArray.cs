using UnityEngine;
using uwu.Settings.Instances;

namespace uwu.Settings.Types
{
	[CreateAssetMenu(fileName = "New Float Array", menuName = "Settings/Global/Floats", order = 52)]
	public class FloatArray : Global<FloatSetting, float>
	{
	}
}