using Neue.Reference.Nodes.Behaviours;
using UnityEngine;

namespace Neue.Reference.Debug
{
	public class TestNode : MonoBehaviour, IReadFrame
	{
		public string Name => Object.name;
		public GameObject Object => gameObject;
		public Collider Collider => Object.GetComponent<Collider>();
		
		[SerializeField] Vector4 frame = Vector4.zero;

		void Start()
		{
			frame.x = Random.Range(0f, 1f);
			frame.y = Random.Range(0f, 1f);
			frame.z = Random.Range(0f, 1f);
			frame.w = Random.Range(0f, 1f);
		}
		
		public Vector4 GetFrame()
		{
			return frame;
		}
	}
}