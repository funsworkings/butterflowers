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

		[SerializeField] ToggleOpacity opacity;
		[SerializeField] Image fill;
		
		[SerializeField] bool loading = false;
		
		
		private const float minimumLoadTime = 1.3f;

		#region Accessors

		public bool IsLoading => loading;
		
		#endregion

		void Awake()
		{
			if (Instance == null) {
				Instance = this;
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

		public void Dispose()
		{
			StopAllCoroutines();
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
				
				Debug.Log("progress= " + progress);

				loadTime += Time.unscaledDeltaTime;
				progress = Mathf.Min(loadTime / minimumLoadTime, progress);

				UpdateFill(Mathf.Pow(progress, 4f).RemapNRB(0f, 1f, min, max));
				
				if (onProgress != null)
					onProgress(progress);

				yield return null;
			
			} while (progress < 1f);
			
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