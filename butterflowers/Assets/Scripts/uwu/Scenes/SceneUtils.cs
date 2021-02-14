using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace uwu.Scenes
{
	public class SceneUtils : MonoBehaviour
	{
		[SerializeField] Scene loadOnAwake = null;

		readonly Dictionary<string, LoadOp> queue = new Dictionary<string, LoadOp>();

		Scene target;


		void Start()
		{
			if (!string.IsNullOrEmpty(loadOnAwake.id))
				GoToScene(loadOnAwake, false);
		}

		void Update()
		{
			if (queue.Count == 0)
				return;

			foreach (var item in queue) {
				var scene = item.Key;
				var operation = item.Value;

				var ready = operation.ready = operation.op.progress >= .9f; // Flag operation to ready if load progress is 
				//Debug.Log(scene + " = " + operation.op.progress);

				var flag = operation.flag;

				if (flag && ready) {
					target = operation.scene;

					//Debug.Log("Trigger load: " + target.id);
					operation.op.allowSceneActivation = true; // Move to scene if flagged for move
				}
			}
		}

		public void GoToScene(int index)
		{
			var scene = new Scene();
			scene.index = index;

			GoToScene(scene);
		}

		public void GoToScene(string name)
		{
			var scene = new Scene();
			scene.id = name;

			GoToScene(scene);
		}

		public void LoadScene(string name)
		{
			var scene = new Scene();
			scene.id = name;

			GoToScene(scene, false);
		}


		public void GoToScene(Scene scene, bool immediate = true)
		{
			LoadOp op = null;
			queue.TryGetValue(scene.id, out op);

			if (op == null) 
			{
				var operation = new LoadOp();

				try {
					var index = -1;
					int.TryParse(scene.id, out index); // Attempt to parse index of scene from id

					operation.op = SceneManager.LoadSceneAsync(index);
				}
				catch (Exception e) {
					Debug.LogWarning("Unable to parse scene index from parameter, using id... " + e.Message);

					operation.op = SceneManager.LoadSceneAsync(scene.id);
				}

				operation.scene = scene;
				operation.op.allowSceneActivation = false;


				op = operation;
				queue.Add(scene.id, operation);
			}

			op.immediate = immediate;
		}

		[Serializable]
		public class Scene
		{
			public string id;

			public int index
			{
				set => id = value + "";
			}
		}

		public class LoadOp
		{
			public bool flag;
			public AsyncOperation op;
			public bool ready;
			public Scene scene;

			public bool immediate
			{
				set
				{
					if (value)
						flag = true;
				}
			}
		}
	}
}