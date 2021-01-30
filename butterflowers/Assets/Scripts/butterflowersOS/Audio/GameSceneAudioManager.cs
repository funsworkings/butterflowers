using UnityEngine;
using UnityEngine.Audio;
using uwu.Timeline.Core;

namespace butterflowersOS.Audio
{
	public class GameSceneAudioManager : SceneAudioManager
	{
		[SerializeField] AudioMixer cutsceneMixer, gameMixer;

		[Header("Scene")] 
			[SerializeField] Cutscenes _cutscenes;

		[Header("Parameters")] 
			[SerializeField] string cutsceneVolumeParam;
			[SerializeField] string gameVolumeParam;
	}
}