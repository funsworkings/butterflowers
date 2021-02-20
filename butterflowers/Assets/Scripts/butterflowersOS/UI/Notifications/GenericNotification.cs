using TMPro;
using UnityEngine;

namespace butterflowersOS.UI.Notifications
{
	public abstract class GenericNotification : MonoBehaviour
	{
		// Properties

		protected RectTransform rect;
		[SerializeField] protected TMP_Text textField;
		
		protected float time = 0f;
		protected float lifetime = 0f;
		
		[SerializeField] bool visible = false;
		
		#region Accessors

		bool persistent => (lifetime == Mathf.Infinity);

		public RectTransform Rect
		{
			get
			{
				if (rect == null)
					rect = GetComponent<RectTransform>();

				return rect;
			}
		}
		
		#endregion
		
		
		protected virtual void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		protected void Update()
		{
			if (!visible) return;
			if (persistent) return;

			time += Time.unscaledDeltaTime;
			if (time > lifetime) time = lifetime;
			
			if(time >= lifetime) 
				Clear();
			else
				OnUpdate();
		}
		
		#region Abstract methods

		protected abstract void OnInitialize();
		protected abstract void OnUpdate();
		
		#endregion
		
		#region Ops

		public void Initialize(string message, float lifetime = Mathf.Infinity)
		{
			if (!string.IsNullOrEmpty(message)) 
			{
				this.time = 0f;
				this.lifetime = lifetime;

				textField.text = message;

				visible = true;
				
				OnInitialize();
			}
			else 
			{
				Clear();
			}
		}

		protected virtual void Clear()
		{
			visible = false;
			Destroy(gameObject);
		}
		
		#endregion
	}
}