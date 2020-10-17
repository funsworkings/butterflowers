using System.Collections;
using UnityEngine;

namespace uwu.Snippets
{
	public class SimpleOscillate : MonoBehaviour
	{
		[SerializeField] Vector3 axis = Vector3.up;
		[SerializeField] float delay;

		public float speed = 1f, magnitude = 1f;
		public bool local;

		[SerializeField] bool randomSpeed, randomMagnitude, randomDelay;

		bool oscillating;
		Vector3 root = Vector3.zero;
		float t;

		// Start is called before the first frame update
		IEnumerator Start()
		{
			if (randomDelay) delay = Random.Range(0f, delay);
			yield return new WaitForSeconds(delay);

			if (randomSpeed)
				speed = Random.Range(0f, speed);
			if (randomMagnitude)
				magnitude = Random.Range(0f, magnitude);

			//axis = transform.InverseTransformDirection(axis.normalized

			root = transform.position;
			axis = axis.normalized;

			if (local)
				axis = transform.TransformDirection(axis);

			oscillating = true;
		}

		// Update is called once per frame
		void Update()
		{
			if (!oscillating) return;

			transform.position = root + magnitude * Mathf.Sin(t) * axis;
			t += Time.deltaTime * speed;
		}

		void OnEnable()
		{
			if (oscillating) root = transform.localPosition;
		}
	}
}