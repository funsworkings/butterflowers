using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ScoreCard : MonoBehaviour
	{
		
		#region Internal

		public enum Type
		{
			Integer,
			Percentage,
			AverageOverTime
		}
		
		#endregion
		
		// Properties

		Animator animator;
		
		[SerializeField] Image deltaField;
		[SerializeField] TMP_Text labelField, scoreField, averageField;
		
		// Attributes
		
		[SerializeField] Type type = Type.Integer;
		[SerializeField] string unitMeasurement = "";


		void Awake()
		{
			animator = GetComponent<Animator>();
		}

		void Start()
		{
			labelField.text = gameObject.name;
		}

		#region Operations

		public void ShowScore(float average, float score)
		{
			if (type == Type.Percentage) 
			{
				score *= 100f;
				average *= 100f;
			}

			int _score = Mathf.FloorToInt(score);
			int _average = Mathf.FloorToInt(average);
			UpdateScores(_average, _score);

			int direction = 0;
			float diff = (_score - _average);

			if (diff > 0f) direction = 1;
			else if (diff < 0f) direction = -1;
			UpdateDelta(direction); // Update delta arrow UI 
			
			animator.SetTrigger("drop");
			animator.SetBool("visible", true);
		}

		public void HideScore()
		{
			animator.SetBool("visible", false);
		}
		
		#endregion
		
		#region UI

		void UpdateScores(int average, int current)
		{
			averageField.text = average + "  [ avg ]";
			scoreField.text = current + "  " + unitMeasurement;
		}

		void UpdateDelta(int direction)
		{
			bool visible = (direction != 0);
			deltaField.enabled = visible;
			
			if (direction > 0) 
			{
				deltaField.transform.localScale = Vector3.one;
				deltaField.color = Color.green;
			}
			else if (direction < 0) 
			{
				deltaField.transform.localScale = new Vector3(1f, -1f, 1f);
				deltaField.color = Color.red;
			}
		}
		
		#endregion
	}
}