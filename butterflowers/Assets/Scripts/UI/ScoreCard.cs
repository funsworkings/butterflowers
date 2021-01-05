using System;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public abstract class ScoreCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

		ScoreDeck deck;
		[HideInInspector] public RectTransform rect;
		
		[SerializeField] Image deltaField;
		[SerializeField] TMP_Text labelField, scoreField, averageField;
		
		// Attributes
		
		[SerializeField] Type type = Type.Integer;
		[SerializeField] string unitMeasurement = "";
		
		
		#region Accessors
		
		Transform scoreItem => transform.GetChild(1);
		Transform averageItem => transform.GetChild(2);
		Transform descriptionItem => transform.GetChild(3);
		
		#endregion
		

		void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		void Start()
		{
			deck = GetComponentInParent<ScoreDeck>();

			labelField = descriptionItem.GetComponent<TMP_Text>();
			scoreField = scoreItem.GetComponentInChildren<TMP_Text>();
			deltaField = scoreItem.GetComponentInChildren<Image>();
			averageField = averageItem.GetComponentInChildren<TMP_Text>();

			labelField.text = Label;
		}

		#region Operations

		protected abstract string Label { get; }
		public abstract void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score);

		protected void ShowScore(float average, float score)
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
		}

		public void HideScore()
		{
			
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
		
		#region  Pointer events
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			deck.Enter(this);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			//deck.Exit(this);
		}
		
		#endregion
	}
}