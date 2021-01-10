using Neue.Reference.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uwu.Snippets;

namespace Neue.UI.HUD_Elements
{
	public class StatElement : HUDElement<TMP_Text>
	{
		// Properties

		[SerializeField] StatOverlayElement overlay;
		[SerializeField] Image fillArea;
		[SerializeField] Animator animator;
		[SerializeField] Damage damage;

		bool visible = false;

		int current = 0;
		int delta = 0;
		int signDelta = 0;
		
		public override void UpdateValue(object val)
		{
			current = (int) val;
		}

		public void UpdateFill(float fill)
		{
			fill = Mathf.Clamp01(fill);
			fillArea.fillAmount = fill;
		}

		public void UpdateDelta(int delta)
		{
			if (visible) return;
			
			this.delta = delta;
			signDelta = (delta == 0)? 0:(int) Mathf.Sign(delta);

			value.text = (current - delta).ToString(); // Set previous value before OnChange
		}

		public void ShowOverlay()
		{
			var value = System.Enum.GetName(typeof(Frame), frame).ToUpper();
			
			foreach(TMP_Text t in overlay.Stats)
				t.text = value;
			
			overlay.Opacity.Show();
		}

		public void HideOverlay()
		{
			overlay.Opacity.Hide();
		}

		public void OnChange()
		{
			value.text = current.ToString();
		}

		public void OnVisible()
		{
			visible = true; 
			
			if(signDelta > 0)
				animator.SetTrigger("increase");
			else if(signDelta < 0)
				animator.SetTrigger("decrease");

			signDelta = 0;
		}

		public void OnHidden()
		{
			visible = false;
			delta = signDelta = 0;
		}
	}
}