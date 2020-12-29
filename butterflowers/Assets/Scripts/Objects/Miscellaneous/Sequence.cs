using System;
using Neue.Reference.Types;
using UnityEngine;
using uwu.Extensions;

namespace Objects.Entities
{
	public class Sequence : MonoBehaviour
	{
		#region Internal

		[Serializable]
		public struct Scene
		{
			public string message;
			public string Message => Extensions.RandomString(128);
			
			public AudioClip audio;
			public GameObject mesh;
		}
		
		#endregion
		
		
		// Properties
		
		public Frame frame;
		public Scene[] scenes;


		void Awake()
		{
			foreach(Scene scene in scenes) DeactivateScene(scene);
		}

		#region Ops

		public Scene Trigger(int index)
		{
			var scene = scenes[index];
			
			ActivateScene(scene); // Activate scene by index
			return scene;
		}
		
		#endregion
		
		#region Activation

		void ActivateScene(Scene scene)
		{
			if(scene.mesh != null) scene.mesh.SetActive(true);
		}

		void DeactivateScene(Scene scene)
		{
			if(scene.mesh != null) scene.mesh.SetActive(false);
		}
		
		#endregion
		
	}
}