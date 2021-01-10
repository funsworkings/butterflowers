using butterflowersOS.Objects.Managers;
using TMPro;
using UnityEngine;

namespace butterflowersOS.UI
{
	public class EventLog : MonoBehaviour
	{
		EventManager manager;

		TMPro.TMP_Text textElement;

		float lifetime = 0f;

		public string text;
		
		public void Initialize(string message, float lifetime, EventManager manager)
		{
			textElement.text = text = message;

			this.manager = manager;
			this.lifetime = lifetime;
		}
		
		#region Monobehaviour callbacks

		void Awake()
		{
			textElement = GetComponentInChildren<TMP_Text>();
		}

		void Update()
		{
			/*lifetime -= Time.deltaTime;
			if (lifetime < 0f) 
				manager.Remove(0);*/
		}
		
		#endregion
	}
}