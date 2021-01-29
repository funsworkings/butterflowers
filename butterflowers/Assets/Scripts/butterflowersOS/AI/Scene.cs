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

		[SerializeField] ParticleSystem butterflowers;
		[SerializeField] Material butterflowersMaterial;
						 Texture2D butterflowersTexture;

		[SerializeField] RemoteAgent agent;
						 
		void Start()
		{
			Save = GameDataSaveSystem.Instance;
			Loader = Loader.Instance;

			StartCoroutine("Initialize");
		}

		void OnDestroy()
		{
			if(butterflowersTexture != null) Destroy(butterflowersTexture);
		}

		IEnumerator Initialize()
		{
			while (!Save.load) yield return null;
			
			Loader.Load(.1f, 1f); // Trigger load
			
			byte[] images = Save.data.images;
			ushort image_height = Save.data.image_height;
			
			if(images.Length > 0 && image_height > 0) ApplyImagesToParticleSystem(images, Library._COLUMNS, image_height); // Apply ground texture
			agent.Initialize(Save.data.surveillanceData);

			while (Loader.IsLoading) yield return null;
			Loader.Dispose();
		}
		
		
		#region Ground

		void ApplyImagesToParticleSystem(byte[] images, ushort _width, ushort _height)
		{
			int width = Library._WIDTH * _width;
			int height = Library._HEIGHT * _height;

			butterflowersTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
				butterflowersTexture.LoadImage(images);
				butterflowersTexture.Apply();

			var textureModule = butterflowers.textureSheetAnimation;
				textureModule.numTilesX = _width;
				textureModule.numTilesY = _height;
				textureModule.startFrameMultiplier = (_width * _height);

			butterflowersMaterial.mainTexture = butterflowersTexture;
		}
		
		#endregion
	}
}