using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Snippets;
using uwu.UI.Behaviors.Visibility;

public class Sequences : MonoBehaviour
{
	#region Properties

	[Header("Objects")]
		[SerializeField] World world;
		[SerializeField] Wand wand;
		[SerializeField] Wizard.Controller wizard;

	[Header("Intro")]
		[SerializeField] float introDelay = 1f;
		[SerializeField] float introEndDelay = 1.5f;
		[SerializeField] Animation wizardIntroAnimation;
		[SerializeField] ToggleOpacity escapeFocusContainer;

	[Header("Absorption")]
		[SerializeField] float absorbIntroDelay = 1f;
		[SerializeField] float dampenCameraTime = 1f, slowCameraDelay = 1f, wrapCameraDelay = 1f;
		[SerializeField] float levelUpSustainDelay = 1f;
		[SerializeField] float camerSpeedAcceleration = 1f;
		[SerializeField] Animation levelAnimation;
		[SerializeField] AnimationClip level_in, level_out;
		[SerializeField] SimpleRotate revolveFeedCamera;
		[SerializeField] Cinemachine.CinemachineVirtualCamera revolveCamera;
		[SerializeField] ToggleOpacity wizardFeedContainer, levelUpContainer;

	[Header("Shared")]
		[SerializeField] CanvasGroup wizardOpacity;
		[SerializeField] CanvasGroup wizardCloudsOpacity;

	#endregion

	#region Attributes

	[SerializeField] bool m_inprogress = false;

	#endregion

	#region Accessors

	public bool inprogress => m_inprogress;

	#endregion


	#region Introduction

	public void Intro()
	{
		m_inprogress = true;
		StartCoroutine("Sequence_Intro");
	}

	IEnumerator Sequence_Intro()
	{
		wand.spells = false;
		escapeFocusContainer.Hide();
		wizardCloudsOpacity.alpha = 0f;
		wizardOpacity.alpha = 0f;

		yield return new WaitForSeconds(introDelay);
		wizardIntroAnimation.Play();
		wizard.OnFocus();

		yield return new WaitForSeconds(introEndDelay);
		m_inprogress = false;
	}

	#endregion

	#region Game

	public void Game()
	{
		m_inprogress = true;
		StartCoroutine("Sequence_Game");
	}

	IEnumerator Sequence_Game()
	{
		wizardCloudsOpacity.alpha = 1f;
		wizardOpacity.alpha = 1f;

		yield return null;
		m_inprogress = false;
	}

	#endregion

	#region Absorption

	public void Consume()
	{
		m_inprogress = true;
		StartCoroutine("Sequence_Consume");
	}

	void Level_In()
	{
		levelAnimation.Play(level_in.name);
	}

	void Level_Out()
	{
		levelAnimation.Play(level_out.name);
	}

	IEnumerator Sequence_Consume()
	{
		wizardFeedContainer.Show();
		wand.spells = false;

		revolveCamera.enabled = true;
		yield return new WaitForSeconds(absorbIntroDelay);

		float baseSpeed = revolveFeedCamera.Speed;
		float maxSpeed = 0f;

		// ACCEL CAMERA + ABSORB WIZARD
		wizard.Appearance.LevelUp();
		yield return new WaitForEndOfFrame();

		while (wizard.Appearance.state == Wizard.Appearance.State.Leveling) 
		{
			revolveFeedCamera.Speed += (Time.deltaTime * camerSpeedAcceleration);
			yield return null;
		}
		maxSpeed = revolveFeedCamera.Speed;
		yield return new WaitForSeconds(slowCameraDelay);


		// SLOW CAMERA
		float t = 0f;
		float percent = 0f;
		while (t < dampenCameraTime) 
		{
			t += Time.deltaTime;
			percent = Mathf.Clamp01(t / dampenCameraTime);

			revolveFeedCamera.Speed = Mathf.Lerp(maxSpeed, baseSpeed, Mathf.Pow(percent, 2f));
			yield return null;
		}
		yield return new WaitForSeconds(wrapCameraDelay);


		Level_In();
		yield return new WaitForEndOfFrame();
		levelUpContainer.Show();
		revolveCamera.enabled = false;

		yield return new WaitForSeconds(levelUpSustainDelay);
		levelUpContainer.Hide();
		Level_Out();
		yield return new WaitForEndOfFrame();

		wizardFeedContainer.Hide();
		wand.spells = true;

		yield return null;
		m_inprogress = false;
	}

	#endregion
}
