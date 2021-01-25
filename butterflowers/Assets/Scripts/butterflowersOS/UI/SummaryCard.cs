using butterflowersOS.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace butterflowersOS.UI
{
	public abstract class SummaryCard : MonoBehaviour
	{
		#region Internal

		public enum Type
		{
			Integer,
			Percentage,
			AverageOverTime
		}

		public enum State
		{
			Normal,
			
			Queue,
			Focus
		}
		
		#endregion
		
		// Properties

		protected SummaryDeck deck;
		
		[HideInInspector] public RectTransform rect;
		protected SummaryCardTrigger trigger;
		
		[HideInInspector] Image deltaField;
		[HideInInspector] TMP_Text labelField, scoreField, averageField;

		
		
		// Attributes

		[SerializeField] string label;
		[SerializeField] Type type = Type.Integer;
		[SerializeField] string unitMeasurement = "";
		
		[HideInInspector] public Vector3 normalScale;
		public Vector3 focusScale = Vector2.one;

		[SerializeField] bool m_focus = false;

		[HideInInspector] public State state = State.Normal;

		#region Accessors
		
		Transform scoreItem => transform.GetChild(3);
		Transform averageItem => transform.GetChild(4);
		Transform descriptionItem => transform.GetChild(1);

		public bool focus
		{
			get { return m_focus; }
			set
			{
				m_focus = value;
				
				if(value) trigger.Grow();
				else trigger.Shrink();
			}
		}
		
		#endregion
		

		void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		protected virtual void Start()
		{
			deck = GetComponentInParent<SummaryDeck>();
			trigger = GetComponentInChildren<SummaryCardTrigger>();

			labelField = descriptionItem.GetComponent<TMP_Text>();
			scoreField = scoreItem.GetComponentInChildren<TMP_Text>();
			deltaField = scoreItem.GetComponentInChildren<Image>();
			averageField = averageItem.GetComponentInChildren<TMP_Text>();

			labelField.text = label;

			normalScale = rect.localScale;
			focusScale.Scale(normalScale);
		}

		#region Operations
		
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
			if(averageField != null) averageField.text = average + ",  avg";
			scoreField.text = current + "  " + unitMeasurement;
		}

		void UpdateDelta(int direction)
		{
			bool visible = (direction != 0);
			deltaField.enabled = visible;
			
			if (direction > 0) 
			{
				deltaField.transform.localScale = Vector3.one;
				//deltaField.color = Color.green;
			}
			else if (direction < 0) 
			{
				deltaField.transform.localScale = new Vector3(1f, -1f, 1f);
				//deltaField.color = Color.red;
			}
		}
		
		#endregion
		
		#region  Pointer events
		
		public void Enter()
		{
			deck.Queue(this);
		}

		public void Exit()
		{
			deck.Dequeue(this);
		}
		
		#endregion
	}
}