using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using uwu.Extensions;

namespace uwu.Snippets.Load
{
	public class SceneLoader : MonoBehaviour, ILoadDependent
	{
		public static SceneLoader Instance = null;
		
		#region Load dependencies
		
		public float Progress { get { return (loading)? Mathf.Clamp01(LoadOp.progress / .9f):1f; } }
		public bool Completed { get { return Progress >= 1f; } }
		
		#endregion
		
		// Properties

		Loader Loader;
		AsyncOperation LoadOp;
		
		int t_scene = -1;
		bool loading = false;
		
		void Awake()
		{
			if (Instance == null) 
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else {
				Destroy(gameObject);
			}
		}

		void OnEnable()
		{
			Loader = Loader.Instance;

			if (Instance == this) 
			{
				SceneManager.sceneLoaded += OnSceneChanged;
			}
		}

		void OnDisable()
		{
			if (Instance == this) 
			{
				SceneManager.sceneLoaded -= OnSceneChanged;
			}
		}

		#region Ops

		public void GoToScene(int buildIndex, float min = 0f, float max = 1f)
		{
			bool isValid = buildIndex < SceneManager.sceneCountInBuildSettings;
			if (!isValid) return;

			t_scene = buildIndex;
			loading = true;
			
			StartCoroutine(LoadScene(buildIndex, min, max));
		}

		IEnumerator LoadScene(int buildIndex, float min, float max)
		{
			LoadOp = SceneManager.LoadSceneAsync(buildIndex);
			LoadOp.allowSceneActivation = false;
			Loader.Load(min, max);

			while (!Completed || Loader.IsLoading) yield return null;
			LoadOp.allowSceneActivation = true;
		}
		
		#endregion
		
		#region Scene callbacks

		void OnSceneChanged(Scene scene, LoadSceneMode mode)
		{
			if (loading) 
			{
				if (scene.buildIndex == t_scene) 
				{
					t_scene = -1;
					loading = false;
					LoadOp = null;
				}
			}
		}	
		
		#endregion
	}
}