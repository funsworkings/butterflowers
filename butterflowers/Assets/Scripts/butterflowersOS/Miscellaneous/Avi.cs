using System;
using System.Collections;
using butterflowersOS.Interfaces;
using UnityEngine;

namespace butterflowersOS.Miscellaneous
{
	public class Avi : MonoBehaviour, IPausable
	{
		
		// Properties

		Animator _animator;
		[SerializeField] GameObject hat, glasses;

		void Awake()
		{
			_animator = GetComponent<Animator>();
		}

		void Start()
		{
			Hide();
		}

		public void Show(int layer)
		{
			hat.SetActive(true);
			glasses.SetActive(true);

			hat.layer = glasses.layer = layer;
		}

		public void Hide()
		{
			hat.SetActive(false);
			glasses.SetActive(false);
		}

		public void Pause()
		{
			_animator.speed = 0f;
		}

		public void Resume()
		{
			_animator.speed = 1f;
		}
	}
}