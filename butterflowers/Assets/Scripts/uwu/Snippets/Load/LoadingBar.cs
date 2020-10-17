using UnityEngine;
using UnityEngine.UI;

namespace uwu.Snippets.Load
{
	[RequireComponent(typeof(Loading))]
	public class LoadingBar : MonoBehaviour
	{
		[SerializeField] Image fill;
		Loading loading;

		void Awake()
		{
			loading = GetComponent<Loading>();
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