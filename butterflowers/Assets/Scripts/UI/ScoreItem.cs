using System;
using TMPro;
using UnityEngine;

namespace UI
{
	public class ScoreItem : MonoBehaviour
	{
		
		#region Internal

		public enum Type
		{
			Integer,
			Percentage,
			AverageOverTime
		}
		
		#endregion
		
		// Events

		public System.Action onComplete;
		
		// Properties

		Animator animator;

		[SerializeField] TMP_Text labelField;
		[SerializeField] TMP_Text scoreField, progressField;
		
		// Attributes
		
		[SerializeField] Type type = Type.Integer;
		[SerializeField] string unitMeasurement = "";
		[HideInInspector] public string trajectorySign = "";
		[SerializeField] bool lerp = false;
		
		float scoreLerpSpeed = 1f;


		[SerializeField] float m_score = 0f, t_score = 0f, last_score = 0f;
		float progress = 0f;
		
		public bool completed = false;
		
		#region Accessors

		public bool Lerp => lerp;
		
		#endregion

		void OnEnable()
		{
			m_score = 0f;
		}

		void Update()
		{
			if (!completed) 
			{
				if (lerp) {
					float interval = Time.deltaTime * scoreLerpSpeed;
					float amount = ((t_score - m_score) * interval);

					amount = Mathf.Max(1f, Mathf.Abs(amount)) * Mathf.Sign(amount);
					m_score = Mathf.Min(m_score + amount, t_score);

					if (Mathf.Abs(t_score - m_score) < .1f) 
					{
						m_score = t_score; // Snap to target value
						
						completed = true;
						if (onComplete != null)
							onComplete();
					}
				}
			}

			var score = Mathf.FloorToInt(m_score);
			var lastScore = Mathf.FloorToInt(last_score);
			
			string text = score + " " + unitMeasurement;
			if (!string.IsNullOrEmpty(trajectorySign))
				text += (" " + trajectorySign);

			labelField.text = (labelField.text).ToUpper();
			scoreField.text = text;

			var _progress = (score - lastScore);
			if (_progress == 0) 
				progressField.text = string.Format("< {0} > +0", lastScore);
			else {
				var progressText = _progress.ToString();
				var colorText = "#FFFFFF";

				if (_progress > 0) {
					colorText = "#00FF00";
					progressText = progressText.Insert(0, "+");
				}
				else {
					colorText = "#FF0000";
				}

				progressField.text = string.Format("< {0} > <color={1}>{2}</color>", lastScore, colorText, progressText);
			}
		}

		#region Operations

		public void SetScore(int previousScore, int currentScore, float speed)
		{
			SetScore((float)previousScore, (float)currentScore, speed);
		}

		public void SetScore(float previousScore, float currentScore, float speed)
		{
			scoreLerpSpeed = speed;
			progress = (currentScore - previousScore);
			
			if (lerp) 
			{
				m_score = previousScore;
				t_score = currentScore;
				last_score = previousScore;
				
				if (type == Type.Percentage) {
					m_score *= 100f; // Clamp from 0-100
					t_score *= 100f; // Clamp from 0-100
					last_score *= 100f; // Clamp from 0-100
				}

				completed = false;
			}
			else 
			{
				m_score = t_score;
				last_score = previousScore;

				completed = true;
				if (onComplete != null)
					onComplete();
			}
		}
		
		#endregion
	}
}