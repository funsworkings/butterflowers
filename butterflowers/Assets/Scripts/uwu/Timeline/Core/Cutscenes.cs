using UnityEngine;
using UnityEngine.Playables;

namespace uwu.Timeline.Core
{
	[RequireComponent(typeof(PlayableDirector))]
	public class Cutscenes : MonoBehaviour
	{
		#region Internal

		public enum State
		{
			Playing,
			Paused,
			Stopped
		}

		#endregion

		#region Properties

		PlayableDirector playableDirector;

		#endregion

		#region Attributes

		State state = State.Stopped;

		[SerializeField] PlayableAsset m_cutscene;

		#endregion

		#region Accessors

		public bool playing => state == State.Playing;
		public bool paused => state == State.Paused;
		public bool stopped => state == State.Stopped;

		public PlayableAsset cutscene
		{
			get => m_cutscene;
			set
			{
				if (!stopped)
					Stop();

				m_cutscene = value;
				playableDirector.playableAsset = cutscene;
			}
		}

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			playableDirector = GetComponent<PlayableDirector>();
		}

		void OnEnable()
		{
			playableDirector.played += onPlay;
			playableDirector.stopped += onStop;

			m_cutscene = playableDirector.playableAsset;
		}

		void OnDisable()
		{
			playableDirector.played -= onPlay;
			playableDirector.stopped -= onStop;
		}

		void Update()
		{
			/*
		if (Input.GetKeyDown(KeyCode.LeftControl)) 
		{
			if (!playing)
				Play();
			else
				Pause();
		}
		*/
		}

		#endregion

		#region Playable director callbacks

		void onPlay(PlayableDirector director)
		{
			state = State.Playing;
		}

		void onStop(PlayableDirector director)
		{
			state = State.Stopped;
		}

		#endregion

		#region Operations

		public void Play()
		{
			if (!playing) {
				if (paused) {
					playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
					state = State.Playing;
				}
				else {
					playableDirector.Play();
				}
			}
		}

		public void Play(PlayableAsset cutscene)
		{
			this.cutscene = cutscene;
			Play();
		}

		public void Stop()
		{
			if (!stopped) playableDirector.Stop();
		}

		public void Pause()
		{
			if (playing) {
				playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
				state = State.Paused;
			}
		}

		#endregion
	}
}