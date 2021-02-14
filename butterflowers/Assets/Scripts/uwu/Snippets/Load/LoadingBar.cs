using UnityEngine;
using UnityEngine.UI;

namespace uwu.Snippets.Load
{
	[RequireComponent(typeof(Loader))]
	public class LoadingBar : MonoBehaviour
	{
		[SerializeField] Image fill = null;
		Loader loading;

		void Awake()
		{
			loading = GetComponent<Loader>();
		}

		void OnEnable()
		{
			loading.onProgress += UpdateFill;
		}

		void OnDisable()
		{
			loading.onProgress -= UpdateFill;
		}

		void UpdateFill(float value)
		{
			fill.fillAmount = value;
		}
	}
}