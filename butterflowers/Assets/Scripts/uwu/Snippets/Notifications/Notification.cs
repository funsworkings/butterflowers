using UnityEngine;

namespace uwu.Snippets.Notifications
{
	[CreateAssetMenu(fileName = "New Notification", menuName = "Notification", order = 53)]
	public class Notification : ScriptableObject
	{
		public bool dismiss;

		public string dialog;
		public Sprite icon;

		public bool isStatic => !dismiss;
	}
}