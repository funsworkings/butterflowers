using System;
using System.Collections;
using System.Collections.Generic;
using B83.Win32;
using butterflowersOS.Core;
using butterflowersOS.Menu;
using butterflowersOS.Presets;
using Neue.Agent.Brain.Data;
using UnityEngine;
using UnityEngine.Playables;
using uwu;
using uwu.Data;
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
		SceneLoader SceneLoader;

		[SerializeField] WorldPreset preset;
		[SerializeField] Material butterflowersMaterial = null;
		[SerializeField] Texture2D butterflowersTexture;
		[SerializeField] int bWidth = 0, bHeight = 0;

		[SerializeField] Driver agent = null;
		[SerializeField] Wrapper wrapper = null;
		[SerializeField] Cutscenes cutscenes = null;
		[SerializeField] PlayableAsset epilogue = null;
		[SerializeField] PauseMenu _pauseMenu = null;

		bool listen = false;
						 
		void Start()
		{
			Save = GameDataSaveSystem.Instance;
			Loader = Loader.Instance;
			SceneLoader = SceneLoader.Instance;
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

			Loader.CompleteLoad();
			
			ImportFromSaveFile();

			while (Loader.IsLoading) yield return null;
			Loader.Dispose();
			
			_pauseMenu.ToggleTeleport(!string.IsNullOrEmpty(Save.data.username));
			
			bool hasCompletedEpilogue = Save.data.cutscenes[2];
			if (SceneLoader.Reason == SceneLoader.SwapReason.Import && !hasCompletedEpilogue) 
			{
				cutscenes.Play(epilogue);
				while (!cutscenes.playing) yield return null;
				
				Save.data.cutscenes[2] = true;
				Save.SaveGameData();
			}

			listen = true;
		}

		#region Ground

		void UnpackTexture(byte[] images, ushort _width, ushort _height)
		{
			int width = Library._WIDTH * _width;
			int height = Library._HEIGHT * _height;

			bWidth = _width;
			bHeight = _height;

			butterflowersTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			butterflowersTexture.filterMode = FilterMode.Point;
			butterflowersTexture.wrapMode = TextureWrapMode.Repeat;
				butterflowersTexture.LoadImage(images);
				butterflowersTexture.Apply();

			butterflowersMaterial.SetTexture("_MainTex", butterflowersTexture);
			butterflowersMaterial.mainTextureScale = new Vector2(1f / _width, 1f / _height);
			
			wrapper.Setup(10);
		}
		
		#endregion
		
		#region Import

		protected override FileNavigator _Files
		{
			get => Files;
		}

		protected override void OnDebugImportFNS(string path, BrainData dat)
		{
			HandleBrainImport(path, dat, new POINT(0, 0)); // Trigger brain import
		}

		void ImportFromSaveFile()
		{
			string path = Save.data.import_agent_created_at;
			if (string.IsNullOrEmpty(path)) return;
			
			BrainData dat = DataHandler.Read<BrainData>(path);
			if (dat != null) 
			{
				Debug.LogWarning("Validate => " + dat.created_at);
				if (dat.IsProfileValid()) 
				{
					Import(dat); // Break out of loop, successfully found file!
				}
			}
		}

		protected override void HandleBrainImport(string path, BrainData brain, POINT point)
		{
			if (!listen) return; // Ignore brain import if loading!
			
			if (Save.IsExternalProfileValid()) { // Overwrite if assigned previously
				Save.data.import_agent_created_at = path;
				Save.SaveGameData();
			}

			Import(brain);
		}

		void Import(BrainData dat)
		{
			byte[] images = dat.images;
			
			ushort image_height = dat.image_height;
			ushort image_width = dat.image_width;

			if (images.Length > 0 && image_height > 0 && image_width > 0) 
			{
				UnpackTexture(images, image_width, image_height); // Apply ground texture
			}

			agent.Initialize(dat.profile, dat.surveillanceData, butterflowersTexture, bWidth, bHeight);
		}

		#endregion
	}
}