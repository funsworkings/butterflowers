using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Snippets.Load
{
	public class Loading : MonoBehaviour
	{
		// Events
		
		public Action<float> onProgress;
		public UnityEvent onComplete;
		
		// Properties

		[SerializeField] bool loading = false;
		
		
		#region Accessors

		public bool IsLoading => loading;
		
		#endregion


		#region Ops

		public void Load()
		{
			ILoadDependent[] dependencies = FindObjectsOfType<MonoBehaviour>().OfType<ILoadDependent>().ToArray();
			
			StartCoroutine("Continue", dependencies);
			loading = true;
		}

		IEnumerator Continue(ILoadDependent[] dependencies)
		{
			float progress = 0f;
			int dependents = dependencies.Length;

			if (onProgress != null)
				onProgress(0f);
			
			do {
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
				else progress = 1f;

				if (onProgress != null)
					onProgress(progress);

				yield return null;
			
			} while (progress < 1f);
			
			onComplete.Invoke();
			loading = false;
		}
		
		#endregion
	}
}