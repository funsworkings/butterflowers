using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Miscellaneous;
using butterflowersOS.Snippets;
using Cinemachine;
using Neue.Reference.Types;
using Neue.Reference.Types.Maps.Groups;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.Video;
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
			[SerializeField] GameObject greenscreenPanel;
			[SerializeField] VideoPlayer greenscreenVideo;
			[SerializeField, Range(0f, 1f)] float greenscreenDisposeTrigger;
			[SerializeField] TMP_Text sequenceFrameText;
			[SerializeField] FrameVector2Group frameTextSizingGroup;
			[SerializeField] FitViaScale frameScaler;
			[SerializeField] ToggleOpacity frameOpacity;
			[SerializeField] float frameTextDelay = 1f;

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
				// Do nothing, maybe?
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

				StartCoroutine(Sequencing(seq, _cutscene));
				
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

		IEnumerator Sequencing(Sequence sq, PlayableAsset cutscene)
		{
			greenscreenPanel.SetActive(true);
			greenscreenVideo.Play();

			while ((float) (greenscreenVideo.time / greenscreenVideo.length) < greenscreenDisposeTrigger) yield return null; // Wait for dispose trigger
			greenscreenVideo.Pause();
			
			TriggerFrameText(sq.frame);
			while (frameOpacity.Hidden) yield return null; // Wait for frame opacity to complete
			yield return new WaitForSecondsRealtime(frameTextDelay);
			HideFrameText();
			
			greenscreenVideo.Play();
			
			mainCamera.cullingMask = sq.culling;
			mainCamera.clearFlags = CameraClearFlags.Color;
			
			ToggleSubtitle(true);
			cutscenes.Play(cutscene);
			
			List<SceneMesh> meshes = new List<SceneMesh>();
			var _meshes = FindObjectsOfType<SceneMesh>();
			foreach (SceneMesh m in _meshes) 
			{
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
			
			greenscreenPanel.SetActive(false); // Disable 
			
			foreach (SceneMesh m in meshes) {
				m.visible = true;
			}
			
			currentScene = null; // Wipe current scene
			sequenceCutscene = null; // Wipe current cutscene
		}

		void TriggerFrameText(Frame frame)
		{
			sequenceFrameText.text = System.Enum.GetName(typeof(Frame), frame).ToUpper(); // Trigger name for framing
				
			Vector2 scale = frameTextSizingGroup.GetValue(frame);
			frameScaler.XScale = scale.x;
			frameScaler.YScale = scale.y;
			
			frameOpacity.Show();
		}

		void HideFrameText()
		{
			frameOpacity.Hide();
		}
		
		void FocusSequenceCamera() 
		{
			if(currentScene == null) return;
			
			CinemachineVirtualCamera camera = currentScene.camera;
			if(camera != null) camera.gameObject.SetActive(true);

			CinemachineBrain.SoloCamera = camera;
		}

		void DisableFocusCamera()
		{
			if(currentScene == null) return;
			
			CinemachineVirtualCamera camera = currentScene.camera;
			if(camera != null) camera.gameObject.SetActive(false);

			CinemachineBrain.SoloCamera = null;
		}

		void TriggerFadeInBGM()
		{
			sequenceBGM.Play();
			sequenceBGMFader.FadeIn();
		}
		
		void TriggerFadeOutBGM()
		{
			sequenceBGMFader.FadeOut();
		}

		void ScaleSequenceObject()
		{
			if(currentScene == null) return;
			currentScene.Show(true); // Trigger visibility immediately
		}

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