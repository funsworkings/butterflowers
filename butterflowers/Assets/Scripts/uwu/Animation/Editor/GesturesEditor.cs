using UnityEditor;
using UnityEngine;

namespace uwu.Animation.Editor
{
	[CustomEditor(typeof(Gestures))]
	public class GesturesEditor : UnityEditor.Editor
	{
		AnimationClip clip;
		string clip_name = "";
		Gestures Gestures;

		// Update is called once per frame
		public override void OnInspectorGUI()
		{
			Gestures = serializedObject.targetObject as Gestures;

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			Gestures.clip = (AnimationClip) EditorGUILayout.ObjectField(Gestures.clip, typeof(AnimationClip), false);
			if (EditorGUI.EndChangeCheck()) {
				Gestures.Dispose();
				if (Gestures.clip != null)
					Gestures.LoadAnimation();
			}

			EditorGUILayout.BeginVertical();
			if (Gestures.clip != null) {
				EditorGUILayout.Space();
				if (!Gestures.previewing) {
					EditorGUI.BeginChangeCheck();
					Gestures.recording = EditorGUILayout.Toggle("Recording?", Gestures.recording);
					if (EditorGUI.EndChangeCheck()) {
						if (Gestures.recording) {
							Gestures.SetEventPosition();
							Gestures.SnapToLastFrameOfAnimation();
						}
						else {
							Gestures.RevertToEventPosition();
						}
					}
				}

				EditorGUILayout.IntField(Gestures.totalFrames);

				var bindings = AnimationUtility.GetCurveBindings(Gestures.clip);
				foreach (var binding in bindings)
					EditorGUILayout.LabelField(binding.propertyName);

				EditorGUILayout.Space();

				if (!Gestures.recording) {
					if (Gestures.previewing) {
						if (GUILayout.Button("Stop")) {
							Gestures.RevertToEventPosition();
							Gestures.StopAnimation();
						}
					}
					else {
						if (GUILayout.Button("Play")) {
							Gestures.SetEventPosition();
							Gestures.PlayAnimation(Gestures.clip);
						}
					}
				}
			}
			else {
				clip_name = EditorGUILayout.TextField(clip_name);
				if (GUILayout.Button("Create")) CreateAnimation(clip_name);
			}

			EditorGUILayout.EndVertical();


			serializedObject.ApplyModifiedProperties();
		}

		void CreateAnimation(string name = null)
		{
			var clip = new AnimationClip();

			var pos = Gestures.transform.localPosition;

			/*
		EditorCurveBinding binding = new EditorCurveBinding();
		binding.type = typeof(Transform);
		binding.path = "";

		var x = new Keyframe(0f, pos.x);
		var y = new Keyframe(0f, pos.y);
		var z = new Keyframe(0f, pos.z);

		var xcurve = new AnimationCurve(new Keyframe[] { x });
		var ycurve = new AnimationCurve(new Keyframe[] { y });
		var zcurve = new AnimationCurve(new Keyframe[] { z });

		binding.propertyName = "m_LocalPosition.x";
		AnimationUtility.SetEditorCurve(clip, binding, xcurve);

		binding.propertyName = "m_LocalPosition.y";
		AnimationUtility.SetEditorCurve(clip, binding, ycurve);

		binding.propertyName = "m_LocalPosition.z";
		AnimationUtility.SetEditorCurve(clip, binding, zcurve);

	*/

			var id = string.IsNullOrEmpty(name) ? Extensions.Extensions.RandomString(12) + ".anim" : name + ".anim";
			AssetDatabase.CreateAsset(clip, string.Format("Assets/Animations/Clips/Gestures/{0}", id));

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Gestures.clip = clip;
		}
	}
}