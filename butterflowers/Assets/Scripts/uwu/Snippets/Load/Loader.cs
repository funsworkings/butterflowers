using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using uwu.Extensions;
using uwu.UI.Behaviors.Visibility;

namespace uwu.Snippets.Load
{
	public class Loader : MonoBehaviour
	{
		public static Loader Instance = null;
		
		// Events
		
		public Action<float> onProgress;
		public UnityEvent onBegin, onComplete;
		
		// Properties

		[SerializeField] ToggleOpacity opacity = null;
		[SerializeField] Image fill = null;
		
		[SerializeField] bool loading = false;
		[SerializeField] float currentLoadTime = 0f;
		
		private const float minimumLoadTime = 1.3f;

		#region Accessors

		public bool IsLoading => loading;
		
		#endregion

		void Awake()
		{
			if (Instance == null) 
			{
				Instance = this;
				currentLoadTime = 0f; // Clear current load time
				
				DontDestroyOnLoad(gameObject);
			}
			else {
				Destroy(gameObject);
			}
		}

		#region Ops

		public void Load(float min = 0f, float max = 1f)
		{
			ILoadDependent[] dependencies = FindObjectsOfType<MonoBehaviour>().OfType<ILoadDependent>().ToArray();
			
			opacity.Show();
			UpdateFill(min);
			onBegin.Invoke();
			
			StartCoroutine(Continue(dependencies, min, max));
			loading = true;
		}

		public void CompleteLoad()
		{
			Load(min: currentLoadTime, max: 1f); // Resume load from current state
		}

		public void Dispose()
		{
			StopAllCoroutines();
			StartCoroutine("Disposing");
		}

		IEnumerator Disposing()
		{
			if (!loading) // In the middle of loading
			{
				if (currentLoadTime > 0f && currentLoadTime < 1f) 
				{
					CompleteLoad();
					while (loading)
						yield return null;
				}
			}

			loading = false;
			opacity.Hide();
		}

		IEnumerator Continue(ILoadDependent[] dependencies, float min, float max)
		{
			while (!opacity.Visible) yield return null;
			
			float progress = 0f;
			float loadTime = 0f;
			
			int dependents = dependencies.Length;

			if (onProgress != null)
				onProgress(0f);
			
			do 
			{
				progress = 0f;
				if (dependents > 0) 
				{
					for (int i = 0; i < dependents; i++) 
					{
						var _dependency = dependencies[i];
						
						float _progress = (_dependency.Completed)? 1f:_dependency.Progress;
						progress += _progress / dependents;
					}
				}
				else 
					progress = 1f;
				
				Debug.Log("progress= " + progress + "  loadtime = " + loadTime);

				loadTime += Time.unscaledDeltaTime;
				progress = Mathf.Min(loadTime / minimumLoadTime, progress);

				currentLoadTime = Mathf.Pow(progress, 4f).RemapNRB(0f, 1f, min, max);
				UpdateFill(currentLoadTime);
				
				if (onProgress != null)
					onProgress(progress);

				yield return null;
			
			} while (progress < 1f);

			if (currentLoadTime >= 1f) currentLoadTime = 0f; // Wipe current load time if completed full load
			
			onComplete.Invoke();
			loading = false;
		}
		
		#endregion
		
		#region UI

		void UpdateFill(float progress)
		{
			fill.color = Extensions.Extensions.SetOpacity(progress, fill.color);
		}
		
		#endregion
	}
}