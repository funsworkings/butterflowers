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
using uwu;
using uwu.Camera.Instances;
using uwu.Timeline.Core;
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

		[SerializeField] Cutscenes cutscenes;
		[SerializeField] VineManager vines;
		[SerializeField] Cage cage;
		[SerializeField] PlayableDirector director;

		[Header("General")] 
			[SerializeField] PlayableAsset introCutscene;
			[SerializeField] PlayableAsset outroCutscene;

		[Header("Vines")] 
			[SerializeField] Transform vineCornerCameraPivot;
			[SerializeField] PlayableAsset vineCornerCutscene;
			[SerializeField] PlayableAsset vineCageCutscene;

		[Header("Sequences")]
			[SerializeField] Scene currentScene;
			[SerializeField] float sequenceMeshScaleDuration = 1f;
			[SerializeField] AnimationCurve sequenceMeshScaleCurve;
			[SerializeField] PlayableAsset sequenceCutscene;
			[SerializeField] Nest Nest;

		[Header("Export")] 
			[SerializeField] ParticleSystem exportPS;
			[SerializeField] Material exportMaterial;

		[Header("Status")] 
			public bool intro = false;
			public bool outro = false;
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
				if (currentScene != null) 
				{
					currentScene.Show(true); // Fully visible scene
				}
				
				currentScene = null; // Wipe current scene
				sequenceCutscene = null; // Wipe current cutscene
			}

			if (flag_save) 
			{
				_Save.data.cutscenes = (bool[]) Save();
				_Save.SaveGameData(); // Save all cutscene data immediately when completed!
			}
		}

		#region Intro/outro

		public void TriggerIntro()
		{
			cutscenes.Play(introCutscene);
		}

		public void TriggerOutro(int rows, int columns, Texture2D texture)
		{
			var textureSheetAnimation = exportPS.textureSheetAnimation;
				textureSheetAnimation.numTilesX = columns;
				textureSheetAnimation.numTilesY = rows;

			exportMaterial.mainTexture = texture;
		
			cutscenes.Play(outroCutscene);
		}
		
		#endregion
		
		#region Sequences

		public bool TriggerSequence(Scene seq)
		{
			if (seq == null) return false;
			
			var _cutscene = seq.cutscene;
			if (_cutscene != null) 
			{
				currentScene = seq;
				sequenceCutscene = _cutscene;
				
				cutscenes.Play(_cutscene);
				return true;
			}
			else
			{
				currentScene = null;
				sequenceCutscene = null;	
				
				seq.Show(true);	// Immediate visibility
				return false;
			}
		}

		public void ScaleSequenceObject()
		{
			currentScene.Show(false);
			
			SceneMesh[] meshes = currentScene.meshes;
			if (meshes.Length > 0) 
			{
				StartCoroutine("ScalingSequenceMeshes", meshes);
			}
		}

		IEnumerator ScalingSequenceMeshes(SceneMesh[] meshes)
		{
			float t = 0f;

			List<float> timestamps = new List<float>();
			foreach (SceneMesh mesh in meshes) 
			{
				float __t = Random.Range(0f, sequenceMeshScaleDuration);
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

						var si = Mathf.Clamp01((t - _t) / _duration);
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
		
		public void FocusSequenceCamera() 
		{
			CinemachineVirtualCamera camera = currentScene.camera;
			if(camera != null) camera.gameObject.SetActive(true);
		}

		public void DisableFocusCamera()
		{
			CinemachineVirtualCamera camera = currentScene.camera;
			if(camera != null) camera.gameObject.SetActive(false);
		}

		public void TriggerKick()
		{
			Nest.RandomKick(); // Re-activate nest after sequence pause
		}
		
		#endregion

		#region Vine cutscenes

		void DidCompleteCorner(Cage.Vertex vertex)
		{
			if (cutscenes.playing) return;

			if (cage.Completed) 
			{
				DidCompleteCage();
			}
			else 
			{
				vineCornerCameraPivot.position = vertex.top;
				vineCornerCameraPivot.transform.eulerAngles = Vector3.zero;
				cutscenes.Play(vineCornerCutscene);
			}
		}

		void DidCompleteCage()
		{
			if (cutscenes.playing) return;
			
			cutscenes.Play(vineCageCutscene);
		}
		
		#endregion
		
		#region Save/load

		public object Save()
		{
			return new bool[] { intro, outro };
		}

		public void Load(object data)
		{
			bool[] cutscenes = (bool[]) data;
				intro = cutscenes[0];
				outro = cutscenes[1];

			Sun = Sun.Instance;
			World = World.Instance;
			_Save = GameDataSaveSystem.Instance;
		}
		
		#endregion
	}
}