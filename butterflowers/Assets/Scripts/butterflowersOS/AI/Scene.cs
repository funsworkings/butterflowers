using System;
using System.Collections;
using butterflowersOS.Core;
using UnityEngine;
using uwu;
using uwu.Snippets.Load;

namespace butterflowersOS.AI
{
	public class Scene : MonoBehaviour
	{
		// External

		GameDataSaveSystem Save;


		// Properties

		Loader Loader;

		[SerializeField] MeshRenderer ground;
						 Texture2D groundTexture;
						 
		void Start()
		{
			Save = GameDataSaveSystem.Instance;
			Loader = Loader.Instance;

			StartCoroutine("Initialize");
		}

		void OnDestroy()
		{
			if(groundTexture != null) Destroy(groundTexture);
		}

		IEnumerator Initialize()
		{
			while (!Save.load) yield return null;
			
			Loader.Load(.1f, 1f); // Trigger load
			
			byte[] images = Save.data.images;
			ushort image_height = Save.data.image_height;
			
			if(images.Length > 0 && image_height > 0) ApplyImagesToGround(images, image_height); // Apply ground texture

			while (Loader.IsLoading) yield return null;
			Loader.Dispose();
		}
		
		
		#region Ground

		void ApplyImagesToGround(byte[] images, ushort _height)
		{
			int width = Library._WIDTH * Library._COLUMNS;
			int height = Library._HEIGHT * _height;

			groundTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
				groundTexture.LoadImage(images);
				groundTexture.Apply();

			ground.material.mainTexture = groundTexture;
		}
		
		#endregion
	}
}