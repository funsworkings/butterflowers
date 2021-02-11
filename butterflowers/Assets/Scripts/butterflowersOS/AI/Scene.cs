using System;
using System.Collections;
using System.Collections.Generic;
using B83.Win32;
using butterflowersOS.Core;
using Neue.Agent.Brain.Data;
using UnityEngine;
using UnityEngine.Playables;
using uwu;
using uwu.IO;
using uwu.Snippets.Load;
using uwu.Timeline.Core;

namespace butterflowersOS.AI
{
	public class Scene : Importer
	{
		// External

		GameDataSaveSystem Save;
		FileNavigator Files;

		// Properties

		Loader Loader;

		[SerializeField] ParticleSystem butterflowers;
		[SerializeField] Material butterflowersMaterial;
						 Texture2D butterflowersTexture;

		[SerializeField] RemoteAgent agent;
		[SerializeField] Cutscenes cutscenes;
		[SerializeField] PlayableAsset epilogue;

		bool listen = false;
						 
		void Start()
		{
			Save = GameDataSaveSystem.Instance;
			Loader = Loader.Instance;
			Files = FileNavigator.Instance;

			StartCoroutine("Initialize");
		}

		void OnDestroy()
		{
			if(butterflowersTexture != null) Destroy(butterflowersTexture);
		}

		IEnumerator Initialize()
		{
			if (!Save.load) 
			{
				Save.LoadGameData<GameData>(createIfEmpty: true);
				while (!Save.load) yield return null;
			}

			Loader.Load(.1f, 1f); // Trigger load
			Import();

			while (Loader.IsLoading) yield return null;
			Loader.Dispose();

			bool hasCompletedEpilogue = Save.data.cutscenes[2];
			if (!hasCompletedEpilogue) 
			{
				cutscenes.Play(epilogue);
				while (!cutscenes.playing) yield return null;
				
				Save.data.cutscenes[2] = true;
				Save.SaveGameData();
			}

			listen = true;
		}

		#region Ground

		void ApplyImagesToParticleSystem(byte[] images, ushort _width, ushort _height)
		{
			int width = Library._WIDTH * _width;
			int height = Library._HEIGHT * _height;

			butterflowersTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
			butterflowersTexture.filterMode = FilterMode.Point;
				butterflowersTexture.LoadImage(images);
				butterflowersTexture.Apply();

			var textureModule = butterflowers.textureSheetAnimation;
				textureModule.numTilesX = _width;
				textureModule.numTilesY = _height;
				textureModule.startFrameMultiplier = (_width * _height);

			butterflowersMaterial.mainTexture = butterflowersTexture;
		}
		
		#endregion
		
		#region Import

		protected override FileNavigator _Files
		{
			get => Files;
		}

		protected override void HandleBrainImport(BrainData brain, POINT point)
		{
			if (!listen) return; // Ignore brain import if loading!
			
			Save.data.surveillanceData = brain.surveillanceData;
			
			Save.data.agent_created_at = brain.created_at;
			Save.data.username = brain.username;
			Save.data.profile = brain.profile;
			Save.data.images = brain.images;
			Save.data.image_height = brain.image_height;

			Save.data.agent_event_stack = brain.surveillanceData.Length; // Total stack of events to parse from
			
			Save.SaveGameData();
			Import();
		}

		void Import()
		{
			byte[] images = Save.data.images;
			ushort image_height = Save.data.image_height;
			
			if(images.Length > 0 && image_height > 0) ApplyImagesToParticleSystem(images, Library._COLUMNS, image_height); // Apply ground texture
			agent.Initialize(Save.data.surveillanceData);
		}

		#endregion
	}
}