using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace uwu.Animation
{
	[ExecuteInEditMode]
	public class Gestures : MonoBehaviour
	{
		public AnimationClip clip;
		public bool recording;

		#region Properties

		public new UnityEngine.Animation animation;

		#endregion

		AnimationCurve xcurve;
		AnimationCurve ycurve;
		AnimationCurve zcurve;

		// Update is called once per frame
		void Update()
		{
#if UNITY_EDITOR

			if (animation == null) animation = GetComponent<UnityEngine.Animation>();
			if (clip == null || !recording) return;

			if (timeSinceLastKeyframe >= keyframeThreshold) {
				var length = clip.length;
				if (length > 0f)
					length += keyframeThreshold;

				CreateKeyframe(length);
			}

			timeSinceLastKeyframe += Time.deltaTime;

#endif
		}

		public void PlayAnimation(string clip_name)
		{
			var clip = animation.GetClip(clip_name);
			if (clip == null) return;

			PlayAnimation(clip);
		}

		public void StopAnimation()
		{
#if UNITY_EDITOR

			if (!Application.isPlaying) {
				if (!previewing) return;
				timeSincePreview -= previewClip.length;
			}
			else {
				animation.Stop();
			}

#else
        animation.Stop();

#endif
		}

		public void PlayAnimation(AnimationClip clip)
		{
			if (clip == null) return;
			clip.legacy = true;

			var existing_clip = animation.GetClip(clip.name);
			if (existing_clip == null)
				animation.AddClip(clip, clip.name);

			animation.clip = clip;

#if UNITY_EDITOR

			if (!Application.isPlaying) {
				timeSincePreview = EditorApplication.timeSinceStartup;
				previewClip = clip;

				EditorApplication.update += onEditorUpdate;
			}
			else {
				animation.Play();
			}

#else
        animation.Play();

#endif
		}

		#region Internal

		float timeSinceLastKeyframe;
		Vector3 event_position = Vector3.zero;

		double timeSincePreview;
		AnimationClip previewClip;

		bool init, position = false;

		#endregion

		#region Accessors

#if UNITY_EDITOR

		public int totalFrames =>
			clip == null || clip.length == 0f ? 0 : Mathf.FloorToInt(clip.length / (1f / clip.frameRate));

		public float keyframeThreshold => clip == null ? 0f : 1f / clip.frameRate;

		public bool previewing =>
			previewClip != null && EditorApplication.timeSinceStartup - timeSincePreview < previewClip.length;

#endif

		#endregion

#if UNITY_EDITOR

		public void LoadAnimation()
		{
			var binding = new EditorCurveBinding();
			binding.type = typeof(Transform);
			binding.path = "";

			binding.propertyName = "m_LocalPosition.x";
			xcurve = AnimationUtility.GetEditorCurve(clip, binding);

			binding.propertyName = "m_LocalPosition.y";
			ycurve = AnimationUtility.GetEditorCurve(clip, binding);

			binding.propertyName = "m_LocalPosition.z";
			zcurve = AnimationUtility.GetEditorCurve(clip, binding);

			init = xcurve != null && ycurve != null && zcurve != null;
		}

		public void SnapToLastFrameOfAnimation()
		{
			if (clip == null) return;

			var x = xcurve.keys[xcurve.keys.Length - 1].value;
			var y = ycurve.keys[ycurve.keys.Length - 1].value;
			var z = zcurve.keys[zcurve.keys.Length - 1].value;

			transform.localPosition = new Vector3(x, y, z);
		}

		public void SetEventPosition()
		{
			event_position = transform.localPosition;
		}

		public void RevertToEventPosition()
		{
			transform.localPosition = event_position;
		}

		void onEditorUpdate()
		{
			if (animation == null) animation = GetComponent<UnityEngine.Animation>();

			var progress = EditorApplication.timeSinceStartup - timeSincePreview;
			if (previewClip != null && progress < previewClip.length) {
				animation.Play(previewClip.name, PlayMode.StopAll);
				animation[previewClip.name].time = (float) progress;
				animation.Sample();
				SceneView.RepaintAll();
			}
			else {
				previewClip = null;

				EditorApplication.update -= onEditorUpdate;
			}
		}

		void CreateKeyframe(float time)
		{
			var position = transform.localPosition;

			var x = new Keyframe(time, position.x);
			var y = new Keyframe(time, position.y);
			var z = new Keyframe(time, position.z);

			if (init) {
				xcurve.AddKey(x);
				ycurve.AddKey(y);
				zcurve.AddKey(z);
			}
			else {
				xcurve = new AnimationCurve(x);
				ycurve = new AnimationCurve(y);
				zcurve = new AnimationCurve(z);

				init = true;
			}

			var binding = new EditorCurveBinding();
			binding.type = typeof(Transform);
			binding.path = "";

			binding.propertyName = "m_LocalPosition.x";
			AnimationUtility.SetEditorCurve(clip, binding, xcurve);

			binding.propertyName = "m_LocalPosition.y";
			AnimationUtility.SetEditorCurve(clip, binding, ycurve);

			binding.propertyName = "m_LocalPosition.z";
			AnimationUtility.SetEditorCurve(clip, binding, zcurve);
		}

		public void Dispose()
		{
			init = false;
			recording = false;

			timeSinceLastKeyframe = keyframeThreshold;

			xcurve = null;
			ycurve = null;
			zcurve = null;

			StopAnimation();
		}

#endif
	}
}