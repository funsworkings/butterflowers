using UnityEngine;

namespace uwu.Snippets
{
	public class Cursor : MonoBehaviour
	{
		[SerializeField] Vector3 m_position;

		[SerializeField] Vector3 m_velocity = Vector3.zero;

		public float speed;

		Vector3 a, b;

		public Vector3 position => m_position;

		public Vector3 velocity => m_velocity;


		void Start()
		{
			a = b = position;
			m_velocity = Vector3.zero;
		}

		protected virtual void Update()
		{
			m_position = Position();

			var dt = Time.unscaledDeltaTime;

			b = position;
			m_velocity = (b - a) / dt;
			a = b;

			speed = velocity.magnitude;
		}

		protected virtual Vector3 Position()
		{
			return Input.mousePosition;
		}
	}
}