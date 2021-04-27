using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Miscellaneous;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using uwu;
using uwu.Audio;
using uwu.Camera.Instances;
using uwu.Timeline.Core;
using uwu.UI.Behaviors.Visibility;
using Random = UnityEngine.Random;

namespace butterflowersOS.Objects.Managers
{
	public class CutsceneManager : MonoBehaviour, ISaveable
	{
		// External

		Sun Sun;
		World World;
		GameDataSaveSystem _Save;
		
		// Properties

		[SerializeField] Cutscenes cutscenes = null;
		[SerializeField] VineManager vines = null;
		[SerializeField] Cage cage;
		[SerializeField] PlayableDirector director;

		[Header("General")] 
			[SerializeField] PlayableAsset introCutscene = null;
			[SerializeField] PlayableAsset outroCutscene = null;

		[Header("Vines")] 
			[SerializeField] Transform vineCornerCameraPivot = null;
			[SerializeField] PlayableAsset vineCornerCutscene = null;
			[SerializeField] PlayableAsset vineCageCutscene;

			[Header("Sequences")] 
			[SerializeField] Camera mainCamera;
			CinemachineBrain mainCameraBrain;
			[SerializeField] int def_cameraCullingMask = 0;
			[SerializeField] CameraClearFlags def_cameraClearFlags;
			[SerializeField] Scene currentScene;
			[SerializeField] float sequenceMeshScaleDuration = 1f;
			[SerializeField] float sequenceMeshScaleTime = 1f;
			[SerializeField] AnimationCurve sequenceMeshScaleCurve = null;
			[SerializeField] PlayableAsset sequenceCutscene;
			[SerializeField] Nest Nest = null;
			[SerializeField] ToggleOpacity sequenceSubtitles = null;
			[SerializeField] Text sequenceSubtitleText = null;
			[SerializeField] AudioFader sequenceBGMFader = null;
			[SerializeField] AudioSource sequenceBGM = null;

		[Header("Export")] 
			[SerializeField] ParticleSystem exportPS = null;
			[SerializeField] Material exportMaterial = null;

		[Header("Status")] 
			public bool intro = false;
			public bool outro = false;
			public bool epilogue = false;
			public bool inprogress = false;

		void OnEnable()
		{
			cutscenes.Completed += DidCompleteCutscene;
			vines.onCompleteCorner += DidCompleteCorner; 
		}

		void OnDisable()
		{
			cutscenes.Completed -= DidCompleteCutscene;
			vines.onCompleteCorner -= DidCompleteCorner;
		}

		void Start()
		{
			mainCamera = Camera.main;
			mainCameraBrain = mainCamera.GetComponent<CinemachineBrain>();
			
			def_cameraCullingMask = mainCamera.cullingMask;
			def_cameraClearFlags = mainCamera.clearFlags;
		}

		void Update()
		{
			inprogress = cutscenes.playing;
		}

		void DidCompleteCutscene(PlayableAsset cutscene)
		{
			bool flag_save = false;
			
			if (cutscene == introCutscene) { intro = true; flag_save = true; }
			else if (cutscene == outroCutscene) 
			{ 
				outro = true;
				flag_save = true;
				
				World.ExportNeueAgent(); // Export neue agent at end of cutscene at ending!
			}
			else if (cutscene == sequenceCutscene) 
			{
				
			}
			
			ToggleSubtitle(false);

			if (flag_save) 
			{
				_Save.data.cutscenes = (bool[]) Save();
				_Save.SaveGameData(); // Save all cutscene data immediately when completed!
			}
		}
		
		#region Accessors

		public bool HasCompletedIntro => intro;
		
		#endregion

		#region Intro/outro

		public void TriggerIntro()
		{
			ToggleSubtitle(true);
			cutscenes.Play(introCutscene);
		}

		public void TriggerOutro(int rows, int columns, Texture2D texture)
		{
			var textureSheetAnimation = exportPS.textureSheetAnimation;
				textureSheetAnimation.numTilesX = columns;
				textureSheetAnimation.numTilesY = rows;

			exportMaterial.mainTexture = texture;
		
			ToggleSubtitle(true);
			cutscenes.Play(outroCutscene);
		}
		
		#endregion
		
		#region Sequences

		public bool TriggerSequence(Scene sc, Sequence seq)
		{
			if (sc == null) return false;
			
			var _cutscene = sc.cutscene;
			if (_cutscene != null) 
			{
				currentScene = sc;
				sequenceCutscene = _cutscene;

				ToggleSubtitle(true);
				cutscenes.Play(_cutscene);

				StartCoroutine("Sequencing", seq);
				
				return true;
			}
			else
			{
				currentScene = null;
				sequenceCutscene = null;	
				
				sc.Show(true);	// Immediate visibility
				return false;
			}
		}

		IEnumerator Sequencing(Sequence sq)
		{
			mainCamera.cullingMask = sq.culling;
			mainCamera.clearFlags = CameraClearFlags.Color;
			
			List<SceneMesh> meshes = new List<SceneMesh>();
			var _meshes = FindObjectsOfType<SceneMesh>();
			foreach (SceneMesh m in _meshes) {
				if (m.visible) {
					m.visible = false;
					meshes.Add(m);
				}
			}
			
			FocusSequenceCamera();
			TriggerFadeInBGM();
			ScaleSequenceObject();
			
			while (cutscenes.playing) yield return null; // Wait for subtitles + VO to finish

			mainCamera.cullingMask = def_cameraCullingMask; // Switch back to def culling mask
			mainCamera.clearFlags = def_cameraClearFlags;

			currentScene.Show(true); // Fully visible scene
			
			DisableFocusCamera(); // Disable focus camera
			TriggerFadeOutBGM();
			
			foreach (SceneMesh m in meshes) {
				m.visible = true;
			}
			
			currentScene = null; // Wipe current scene
			sequenceCutscene = null; // Wipe current cutscene
		}
		
		public void FocusSequenceCamera() 
		{
			if(currentScene == null) return;
			
			CinemachineVirtualCamera camera = currentScene.camera;
			if(camera != null) camera.gameObject.SetActive(true);

			CinemachineBrain.SoloCamera = camera;
		}

		public void DisableFocusCamera()
		{
			if(currentScene == null) return;
			
			CinemachineVirtualCamera camera = currentScene.camera;
			if(camera != null) camera.gameObject.SetActive(false);

			CinemachineBrain.SoloCamera = null;
		}

		public void TriggerFadeInBGM()
		{
			sequenceBGM.Play();
			sequenceBGMFader.FadeIn();
		}
		
		public void TriggerFadeOutBGM()
		{
			sequenceBGMFader.FadeOut();
		}

		public void ScaleSequenceObject()
		{
			if(currentScene == null) return;
			currentScene.Show(true); // Trigger visibility immediately
			
			/*
			SceneMesh[] meshes = currentScene.meshes;
			if (meshes.Length > 0) 
			{
				StartCoroutine("ScalingSequenceMeshes", meshes);
			}*/
		}

		/*
		IEnumerator ScalingSequenceMeshes(SceneMesh[] meshes)
		{
			float t = 0f;

			List<float> timestamps = new List<float>();
			foreach (SceneMesh mesh in meshes) 
			{
				float __t = Random.Range(0f, sequenceMeshScaleDuration - sequenceMeshScaleTime);
				timestamps.Add(__t);
			}
			
			float _t = 0f;
			float _duration = 0f;

			while (t < sequenceMeshScaleDuration) 
			{
				t += Time.deltaTime;

				for (int i = 0; i < meshes.Length; i++) 
				{
					var mesh = meshes[i];

					_t = timestamps[i];

					if (t > _t) 
					{
						_duration = (sequenceMeshScaleDuration - _t);

						var si = Mathf.Clamp01((t - _t) / sequenceMeshScaleTime);
						var sc = sequenceMeshScaleCurve.Evaluate(si);

						Vector3 a = mesh.hidden;
						Vector3 b = mesh.shown;
							b.y = Mathf.Lerp(a.y, b.y, sc);

						mesh.transform.localScale = b;
					}
				}

				yield return null;
			}
		}
		
		public void TriggerKick()
		{
			Nest.RandomKick(); // Re-activate nest after sequence pause
		}
		
		*/

		#endregion
		
		#region Subtitles

		void ToggleSubtitle(bool visible)
		{
			sequenceSubtitleText.text = "";
			
			if(visible) sequenceSubtitles.Show();
			else sequenceSubtitles.Hide();
		}
		
		#endregion

		#region Vine cutscenes

		void DidCompleteCorner(Cage.Vertex vertex)
		{
			if (cutscenes.playing) return;

			vineCornerCameraPivot.position = vertex.top;
			vineCornerCameraPivot.transform.eulerAngles = Vector3.zero;
			cutscenes.Play(vineCornerCutscene);
		}

		#endregion
		
		#region Save/load

		public object Save()
		{
			return new bool[] { intro, outro, epilogue };
		}

		public void Load(object data)
		{
			bool[] cutscenes = (bool[]) data;
				intro = cutscenes[0];
				outro = cutscenes[1];
				epilogue = cutscenes[2];

			Sun = Sun.Instance;
			World = World.Instance;
			_Save = GameDataSaveSystem.Instance;
		}
		
		#endregion
	}
}