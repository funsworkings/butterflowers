using UnityEngine;
using uwu.Net.Core;

namespace uwu.Net.Misc
{
	public class WebRelay : MonoBehaviour
	{
		[SerializeField] string url = "URL_GOES_HERE";

		public void GoTo()
		{
			WebHandler.Instance.GoTo(url);
		}

		public void GoTo(string url)
		{
			WebHandler.Instance.GoTo(url);
		}
	}
}