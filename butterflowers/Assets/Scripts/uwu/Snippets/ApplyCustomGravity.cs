using UnityEngine;

namespace uwu.Snippets
{
	[RequireComponent(typeof(Rigidbody))]
	public class ApplyCustomGravity : MonoBehaviour
	{
		protected Vector3 directionOfGravity = Vector3.down;
		protected new Rigidbody rigidbody;

		void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();
		}

		void FixedUpdate()
		{
			rigidbody.useGravity = false;

			var magnitude = Physics.gravity.magnitude;
			ApplyGravity(magnitude);
		}

		protected virtual void ApplyGravity(float magnitude)
		{
			rigidbody.AddForce(directionOfGravity.normalized * magnitude);
		}
	}
}