using System;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Snippets.Load
{
	public class Loading : MonoBehaviour
	{
		[SerializeField] float m_progress;

		[SerializeField] bool smooth;
		[SerializeField] float smoothSpeed = 1f, minSpeed = -1f, maxSpeed = -1f;
		float t_progress;

		public float progress
		{
			get => m_progress;
			set
			{
				var val = Mathf.Clamp01(value);

				if (smooth) {
					t_progress = val;
				}
				else {
					m_progress = val;
					onUpdateProgress();
				}
			}
		}

		// Start is called before the first frame update
		void Start()
		{
			m_progress = t_progress = 0f;
		}

		// Update is called once per frame
		void Update()
		{
			if (!smooth) return;

			m_progress = Mathf.Clamp01(m_progress);
			t_progress = Mathf.Clamp01(t_progress);

			if (m_progress >= t_progress) m_progress = t_progress; // Immediately move back if progress reverts

			var speed = (t_progress - m_progress) * smoothSpeed;
			if (minSpeed > 0f) speed = Mathf.Max(speed, minSpeed);
			if (maxSpeed > 0f) speed = Mathf.Min(speed, maxSpeed);

			m_progress = Mathf.Clamp(m_progress + speed * Time.deltaTime, 0f, t_progress);
			onUpdateProgress();
		}

		void onUpdateProgress()
		{
			if (m_progress >= 1f) {
				onComplete.Invoke();
			}
			else {
				if (onProgress != null)
					onProgress(progress);
			}
		}

		#region Events

		public UnityEvent onComplete;

		public Action<float> onProgress;

		#endregion
	}
}