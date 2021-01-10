using Neue.Reference.Types;
using UnityEngine;
using UnityEngine.UI;

namespace Neue.UI
{
	public class HUDElement<E> : MonoBehaviour
	{
		[SerializeField] protected Image icon;
		[SerializeField] protected E value;
		[SerializeField] protected Frame _frame;

		public Image Icon => icon;
		public E Value => value;
		public Frame frame => _frame;

		public virtual void UpdateValue(object val){}
	}
}