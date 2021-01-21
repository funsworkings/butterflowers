using System;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Entities;
using UnityEngine;
using UnityEngine.Playables;
using uwu;
using uwu.Timeline.Core;

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

		[Header("General")] 
			[SerializeField] PlayableAsset introCutscene;
			[SerializeField] PlayableAsset outroCutscene;

		[Header("Vines")] 
			[SerializeField] Transform vineCornerCameraPivot;
			[SerializeField] PlayableAsset vineCornerCutscene;
			[SerializeField] PlayableAsset vineCageCutscene;

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
			if (cutscene == introCutscene) { intro = true; }
			else if (cutscene == outroCutscene) 
			{ 
				outro = true;
				World.ExportNeueAgent(); // Export neue agent at end of cutscene at ending!
			}

			_Save.data.cutscenes = (bool[])Save();
			_Save.SaveGameData(); // Save all cutscene data immediately when completed!
		}

		#region Intro/outro

		public void TriggerIntro()
		{
			cutscenes.Play(introCutscene);
		}

		public void TriggerOutro()
		{
			cutscenes.Play(outroCutscene);
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