using TMPro;
using UnityEngine;
using uwu.UI.Behaviors.Visibility;

namespace Neue.UI.HUD_Elements
{
	public class StatOverlayElement : MonoBehaviour
	{
		[SerializeField] TMP_Text[] stats;
		[SerializeField] ToggleOpacity opacity;
		
		public TMP_Text[] Stats => stats;
		public ToggleOpacity Opacity => opacity;
	}
}