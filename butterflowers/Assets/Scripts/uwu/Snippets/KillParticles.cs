using UnityEngine;
using uwu.Gameplay;

namespace uwu.Snippets
{
	[RequireComponent(typeof(ParticleSystem))]
	public class KillParticles : MonoBehaviour
	{
		bool played;
		ParticleSystem ps;

		public bool pooled = false;

		void Awake()
		{
			ps = GetComponent<ParticleSystem>();
		}

		// Update is called once per frame
		void Update()
		{
			if (played)
				if (!ps.isPlaying)
					if (ps.particleCount <= 0)
						Kill();

			if (ps.isPlaying)
				played = true;
		}

		void Kill()
		{
			if (pooled) 
			{
				GetComponent<PoolObject>().Dispose();
				return;
			}
			
			Destroy(gameObject);
		}
	}
}