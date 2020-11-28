using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AI.UI
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