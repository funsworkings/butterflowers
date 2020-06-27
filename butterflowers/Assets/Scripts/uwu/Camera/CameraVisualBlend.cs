using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UIExt.Behaviors.Visibility;
using Cinemachine;

public class CameraVisualBlend: MonoBehaviour {

	#region External

	CameraManager CameraManager;

	#endregion

	#region Properties

	[SerializeField] Camera transitionCamera;
	[SerializeField] CinemachineBrain cinemachineBrain;

	public CameraVisualBlendDefinition blendDefinition;
		   CameraVisualBlendDefinition defaultBlendDefinition;

	[SerializeField] [Range(0f, 1f)] float cinemachineBlendMultiplier = 1f;

	[SerializeField] Snapshot transitioner;
	[SerializeField] RawImage transitionPlane;

	[SerializeField] CanvasGroup planeOpacity;
	[SerializeField] Animation planeAnimation;

	#endregion

	#region Attributes

	Texture2D transitionImage;
	bool blending = false;

	#endregion

	#region Monobehaviour callbacks

	void OnEnable()
	{
		transitioner.onSuccess += onReceiveSnapshot;
	}

	void OnDisable()
	{
		transitioner.onSuccess -= onReceiveSnapshot;
	}

	void Start()
	{
		CameraManager = FindObjectOfType<CameraManager>();

		defaultBlendDefinition = ScriptableObject.CreateInstance<CameraVisualBlendDefinition>();
		defaultBlendDefinition.type = CameraVisualBlendDefinition.Type.Opacity;
	}

	#endregion

	#region Operations

	public void BlendTo(GameCamera to, GameCamera from = null, GameCamera blendCamera = null, float duration = 1f)
	{
		var a = (from == null) ? CameraManager.ActiveCamera : from;
		var b = to;

		if (a == null || b == null || a == b) return;

		if (blending) StopCoroutine("Blend");

		if (transitionImage != null){
			Texture2D.Destroy(transitionImage);
			transitionImage = null;
		}
		
		blending = true;
		StartCoroutine(Blend(a, b, duration, blendCamera));
	}

	IEnumerator Blend(GameCamera current, GameCamera target, float duration, GameCamera blendCamera = null)
	{
		transitionCamera.enabled = true;

		if (blendCamera == null) blendCamera = current;
		CaptureLastFrame(blendCamera);

		while(transitionImage == null)
			yield return null;

		transitionCamera.enabled = false;

		transitionPlane.texture = transitionImage;

		planeOpacity.alpha = 1f;
		ParseBlendDefinition(0f);

		CameraManager.SetCamera(target);
		while (CameraManager.ActiveCamera != target) 
			yield return null;

		if (cinemachineBrain != null) 
		{
			yield return new WaitForEndOfFrame();

			var blend = cinemachineBrain.ActiveBlend;
			if (blend == null) {
				var bl = cinemachineBrain.m_DefaultBlend;
				var dur = (bl.m_Style == CinemachineBlendDefinition.Style.Cut) ? duration : bl.m_Time;

				duration = dur;
			}
			else
				duration = blend.Duration;

			Debug.Log("duration = " + duration);

			duration *= cinemachineBlendMultiplier;
		}

		float t = 0f;
		while (t < duration) 
		{
			t += Time.deltaTime;

			var interval = Mathf.Clamp01(t / duration);
			ParseBlendDefinition(interval);

			yield return null;
		}

		blending = false;
		planeOpacity.alpha = 0f;

		Texture2D.Destroy(transitionImage);
		transitionImage = null;
	}

	void ParseBlendDefinition(float interval)
	{
		var def = blendDefinition;
		if (def == null) def = defaultBlendDefinition;

		if (def.type == CameraVisualBlendDefinition.Type.Opacity) {

			planeAnimation.transform.ResetTransformValues();
			planeOpacity.alpha = (1f - interval);
		}
		else if (def.type == CameraVisualBlendDefinition.Type.Animation) {
			var anim = def.animation;
			var clip_name = anim.name;

			if (interval == 0f) {
				if (planeAnimation.GetClip(clip_name) == null)
					planeAnimation.AddClip(def.animation, clip_name);

				planeAnimation.Play(clip_name, PlayMode.StopAll);
			}
			else {
				var duration = anim.length;

				planeAnimation[clip_name].time = (interval * duration);
				planeAnimation.Sample();
			}
		}
	}

	#endregion

	#region Snapshot callbacks

	void onReceiveSnapshot(Texture2D tex) 
	{
		if(blending)
			transitionImage = tex;
	}

	#endregion

	#region Internal

	void CaptureLastFrame(GameCamera camera)
	{
		transitionCamera.transform.CopyTransformValuesFrom(camera.transform); // Match transform values

		transitionCamera.fieldOfView = camera.fov;
		transitionCamera.nearClipPlane = camera.nearClipPlane;
		transitionCamera.farClipPlane = camera.farClipPlane;

		transitioner.Capture();
	}

	#endregion
}
