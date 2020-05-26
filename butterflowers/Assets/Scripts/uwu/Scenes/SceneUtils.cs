using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneUtils : MonoBehaviour {

    [System.Serializable]
    public class Scene
    {
        public string id;
        public int index
        {
            set
            {
                id = value + "";
            }
        }
    }

    public class LoadOp
    {
        public Scene scene;
        public AsyncOperation op;

        public bool flag = false;
        public bool ready = false;

        public bool immediate
        {
            set
            {
                if (value)
                    flag = true;
            }
        }

        public LoadOp() { }
    }

    [SerializeField]
    Scene loadOnAwake = null;

    Scene target;

    Dictionary<string, LoadOp> queue = new Dictionary<string, LoadOp>();


    void Start()
    {
        if (!string.IsNullOrEmpty(loadOnAwake.id))
            GoToScene(loadOnAwake, false);
    }

    void Update()
    {
        if (queue.Count == 0)
            return;

        foreach(KeyValuePair<string, LoadOp> item in queue)
        {
            var scene = item.Key;
            var operation = item.Value;

            var ready = operation.ready = (operation.op.progress >= .9f); // Flag operation to ready if load progress is 
            //Debug.Log(scene + " = " + operation.op.progress);

            var flag = operation.flag;

            if (flag && ready)
            {
                target = operation.scene;

                //Debug.Log("Trigger load: " + target.id);
                operation.op.allowSceneActivation = true; // Move to scene if flagged for move
            }
        }
    }

    public void GoToScene(int index){
        Scene scene = new Scene();
        scene.index = index;

        GoToScene(scene);
    }

    public void GoToScene(string name){
        Scene scene = new Scene();
        scene.id = name;

        GoToScene(scene);
	}

    public void LoadScene(string name)
    {
        Scene scene = new Scene();
        scene.id = name;

        GoToScene(scene, false);
    }


    public void GoToScene(Scene scene, bool immediate = true)
    {
        LoadOp op = null;
            queue.TryGetValue(scene.id, out op);

        if(op == null)
        {
            LoadOp operation = new LoadOp();

            try
            {
                int index = -1;
                int.TryParse(scene.id, out index); // Attempt to parse index of scene from id

                operation.op = SceneManager.LoadSceneAsync(index);
            }
            catch(System.Exception e)
            {
                Debug.LogWarning("Unable to parse scene index from parameter, using id...");

                operation.op = SceneManager.LoadSceneAsync(scene.id);
            }

            operation.scene = scene;
            operation.op.allowSceneActivation = false;


            op = operation;
            queue.Add(scene.id, operation);
        }

        op.immediate = immediate;
    }

    void OnDestroy()
    {
        foreach (KeyValuePair<string, LoadOp> item in queue)
        {
            var scene = item.Key;
            //if(scene.id != target.id)
              //  SceneManager.UnloadSceneAsync( // Unload all scenes in queue from memory
        }
    }
}
