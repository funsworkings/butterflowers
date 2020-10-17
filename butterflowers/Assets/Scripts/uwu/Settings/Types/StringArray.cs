using UnityEngine;
using uwu.Settings.Instances;

namespace uwu.Settings.Types
{
	[CreateAssetMenu(fileName = "New Float Array", menuName = "Settings/Global/Strings", order = 52)]
	public class StringArray : Global<StringSetting, string>
	{
	}
}