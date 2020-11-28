using UnityEngine;
using TMPro;
using uwu.UI.Behaviors.Visibility;

namespace AI.UI
{
	public class StatOverlayElement : MonoBehaviour
	{
		[SerializeField] TMP_Text[] stats;
		[SerializeField] ToggleOpacity opacity;
		
		public TMP_Text[] Stats => stats;
		public ToggleOpacity Opacity => opacity;
	}
}