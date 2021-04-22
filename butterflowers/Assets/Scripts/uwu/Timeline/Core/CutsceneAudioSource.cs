using UnityEngine;
using UnityEngine.Playables;

namespace uwu.Timeline.Core
{
	[System.Serializable]
	public struct CutsceneAudioSource
	{
		public PlayableAsset cutscene;
		public AudioSource[] sources;
	}
}