using TMPro;
using UnityEngine;
using uwu.UI.Behaviors.Visibility;

namespace Neue.UI.HUD_Elements
{
	public class StatOverlayElement : MonoBehaviour
	{
		[SerializeField] TMP_Text[] stats = new TMP_Text[]{};
		[SerializeField] ToggleOpacity opacity = null;
		
		public TMP_Text[] Stats => stats;
		public ToggleOpacity Opacity => opacity;
	}
}