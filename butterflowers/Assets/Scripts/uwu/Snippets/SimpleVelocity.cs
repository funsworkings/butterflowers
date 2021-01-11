using UnityEngine;

namespace uwu.Snippets
{
	/// <summary>
	/// Calculates the velocity via frame to frame changes in fixed update
	/// </summary>
	public class SimpleVelocity : MonoBehaviour
	{
		Vector3 a, b;
		Vector3 m_velocity = Vector3.zero;

		public Vector3 velocity => m_velocity;

		void Start()
		{
			a = b = transform.position;
		}
		
		void Update()
		{
			float dt = Time.deltaTime;

			b = transform.position;
			if (dt > 0f)
				m_velocity = (b - a) / dt;
			else
				m_velocity = Vector3.zero;
			a = b;
		}
	}
}