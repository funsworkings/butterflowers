using Cinemachine;
using UnityEngine;

namespace uwu.Snippets
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	public class Screenshake : MonoBehaviour
	{
		[SerializeField] float radius = 1f;
		[SerializeField] float dampening = 1f;

		bool active = false;
		CinemachineBrain brain;
		new UnityEngine.Camera camera;

		Vector3 origin = Vector3.zero;
		float shake;

		public bool inprogress => shake > 0f;

		void Awake()
		{
			camera = GetComponent<UnityEngine.Camera>();
			brain = GetComponent<CinemachineBrain>();
		}

		void Update()
		{
			if (inprogress) {
				shake -= Time.deltaTime * dampening;
				if (shake < 0f) {
					shake = 0f;
					Dispose();
				}
				else {
					onShake();
				}
			}
		}

		public void Shake(float strength = 1f)
		{
			if (inprogress) return;
			shake = strength;

			if (camera == null) return;
			if (brain != null) brain.enabled = false;

			var transform = camera.transform;
			origin = transform.position;

			onShake();
		}

		void onShake()
		{
			var direction = Random.insideUnitSphere;

			var adjustment = 1f / direction.magnitude;
			direction *= adjustment * radius * shake;

			transform.position = origin + direction;
		}

		void Dispose()
		{
			if (camera == null) return;

			camera.transform.position = origin;
			if (brain != null) brain.enabled = true;
		}
	}
}