using System;
using butterflowersOS.Objects.Entities;
using UnityEngine;
using UnityEngine.Playables;
using uwu.Timeline.Core;

namespace butterflowersOS.Objects.Managers
{
	public class CutsceneManager : MonoBehaviour
	{
		// Properties

		[SerializeField] Cutscenes cutscenes;
		[SerializeField] VineManager vines;
		[SerializeField] Cage cage;

		[Header("Vines")] 
			[SerializeField] Transform vineCornerCameraPivot;
			[SerializeField] PlayableAsset vineCornerCutscene;
			[SerializeField] PlayableAsset vineCageCutscene;
			

		void OnEnable()
		{
			vines.onCompleteCorner += DidCompleteCorner;
		}

		void OnDisable()
		{
			vines.onCompleteCorner -= DidCompleteCorner;
		}


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
	}
}