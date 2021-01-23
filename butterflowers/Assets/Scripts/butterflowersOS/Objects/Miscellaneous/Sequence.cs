using System;
using System.Collections;
using Neue.Reference.Types;
using UnityEngine;
using UnityEngine.Playables;

namespace butterflowersOS.Objects.Miscellaneous
{
	public class Sequence : MonoBehaviour
	{
		// Properties
		
		[SerializeField] Frame _frame;
		
		Scene[] _scenes;

		
		#region Accessors

		public Frame frame => _frame;
		
		#endregion

		void Start()
		{
			_scenes = GetComponentsInChildren<Scene>();
		}

		#region Ops

		public Scene Trigger(int index, bool load = false)
		{
			var scene = _scenes[index];
			
			ActivateScene(scene); // Activate scene by index
			if(load) scene.Show(true);
			
			return scene;
		}

		public void Dispose()
		{
			foreach(Scene scene in _scenes) 
				DeactivateScene(scene); // Deactivate all scenes
		}
		
		#endregion
		
		#region Activation

		void ActivateScene(Scene scene)
		{
			scene.gameObject.SetActive(true);
		}

		void DeactivateScene(Scene scene)
		{
			scene.gameObject.SetActive(false);
		}
		
		#endregion
		
	}
}