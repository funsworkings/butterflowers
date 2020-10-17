using TMPro;
using UnityEngine;

namespace uwu.UI.Elements
{
	public abstract class InfoTextField : MonoBehaviour
	{
		[Header("Properties")] [SerializeField]
		protected TMP_Text tMP_Text;

		[SerializeField] protected UnityEngine.UI.Text text;

		[Header("Attributes")] public bool continuous = true;

		void Update()
		{
			if (!continuous)
				return;

			ParseInfo();
		}

		void OnEnable()
		{
			ParseInfo();
		}

		void ParseInfo()
		{
			onUpdateInfo(FetchInfo());
		}

		protected abstract string FetchInfo();

		void onUpdateInfo(string info)
		{
			if (info == null)
				info = ""; // Empty null info

			if (tMP_Text != null) {
				tMP_Text.text = info;
				return;
			}

			if (text != null) {
				text.text = info;
				return;
			}

			Debug.LogWarning("No text field has been set for " + GetType().FullName);
		}
	}
}