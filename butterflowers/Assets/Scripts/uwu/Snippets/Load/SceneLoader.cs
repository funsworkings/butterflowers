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
		
		#region Internal

		public enum SwapReason
		{
			Undefined,
			
			Import
		}
		
		#endregion
		
		#region Load dependencies
		
		public float Progress { get { return (loading)? Mathf.Clamp01(LoadOp.progress / .9f):1f; } }
		public bool Completed { get { return Progress >= 1f; } }
		
		#endregion
		
		// Properties

		Loader Loader;
		AsyncOperation LoadOp;
		
		int t_scene = -1;
		bool loading = false;
		
		// Attributes
		
		[SerializeField] SceneLoadTarget[] _loadTargets = new SceneLoadTarget[]{};

		public SceneLoadTarget LoadTarget { get; private set; } = null;
		
		public Scene From { get; private set; }
		public Scene To { get; private set; }

		public SwapReason Reason { get; set; } = SwapReason.Undefined;
		
		void Awake()
		{
			if (Instance == null) 
			{
				Instance = this;
				
				SceneManager.sceneLoaded += OnSceneChanged;
				SceneManager.activeSceneChanged += OnActiveSceneChange;
				
				DontDestroyOnLoad(gameObject);
			}
			else {
				Destroy(gameObject);
			}
		}

		void Start()
		{
			Loader = Loader.Instance;
		}

		#region Ops

		public void GoToScene(int buildIndex, SwapReason reason = SwapReason.Undefined)
		{
			bool isValid = buildIndex < SceneManager.sceneCountInBuildSettings;
			if (!isValid) return;

			int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
			if (currentSceneIndex == buildIndex) return; // Ignore req to go to current scene

			FetchLoadTarget(currentSceneIndex, buildIndex, out SceneLoadTarget _target);
			LoadTarget = _target;

			Reason = reason;
			
			t_scene = buildIndex;
			loading = true;
			
			StartCoroutine(LoadScene(buildIndex, 0f, LoadTarget.duration));
		}

		IEnumerator LoadScene(int buildIndex, float min, float max)
		{
			LoadOp = SceneManager.LoadSceneAsync(buildIndex);
			LoadOp.allowSceneActivation = false;
			Loader.Load(min, max);

			while (!Completed || Loader.IsLoading) yield return null;
			LoadOp.allowSceneActivation = true;
		}

		void FetchLoadTarget(int start, int finish, out SceneLoadTarget target)
		{
			foreach (SceneLoadTarget _target in _loadTargets) {
				if (_target.@from == start && _target.to == finish) {
					target = _target;
					return;
				}
			}
			
			throw new SystemException("Unable to find scene load target");
		}
		
		#endregion
		
		#region Scene callbacks

		void OnActiveSceneChange(Scene previous, Scene current)
		{
			From = previous;
			To = current;
		}

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