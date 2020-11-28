using UnityEngine;
using UnityEngine.UI;
using Behaviour = AI.Types.Behaviour;

namespace AI.UI
{
	public class HUDElement<E> : MonoBehaviour
	{
		[SerializeField] protected Image icon;
		[SerializeField] protected E value;
		[SerializeField] protected Behaviour behaviour;

		public Image Icon => icon;
		public E Value => value;
		public Behaviour Behaviour => behaviour;

		public virtual void UpdateValue(object val){}
	}
}