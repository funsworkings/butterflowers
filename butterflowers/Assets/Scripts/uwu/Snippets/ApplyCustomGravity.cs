using UnityEngine;

namespace uwu.Snippets
{
	[RequireComponent(typeof(Rigidbody))]
	public class ApplyCustomGravity : MonoBehaviour
	{
		public Vector3 directionOfGravity { get; set; } = Vector3.down;
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