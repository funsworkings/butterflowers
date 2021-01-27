using UnityEngine;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.UI
{
	public class TutorialPanel : MonoBehaviour
	{
		// Properties

		ToggleOpacity opacity;
		public ToggleOpacity Opacity => opacity;

		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}
	}
}