using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace uwu.Snippets.Notifications
{
	public class Instance : MonoBehaviour
	{
		public delegate void OnDismissed(Notification notification);

		[SerializeField] TMP_Text dialog = null;
		[SerializeField] Image icon = null;

		Notification m_notification;

		public Notification notification
		{
			set
			{
				m_notification = value;
				if (m_notification != null) {
					dialog.text = value.dialog;
					icon.sprite = value.icon;
				}
			}
		}

		public event OnDismissed onDismissed;

		public void Dismiss()
		{
			if (onDismissed != null)
				onDismissed(m_notification);
		}
	}
}