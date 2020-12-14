using UnityEngine;
using UnityEngine.UI;

namespace Neue.UI.HUD_Elements
{
	public class AttributeElement : HUDElement<Image>
	{
		public override void UpdateValue(object val)
		{
			Sprite img = (Sprite) val;
			value.sprite = img;
		}
	}
}