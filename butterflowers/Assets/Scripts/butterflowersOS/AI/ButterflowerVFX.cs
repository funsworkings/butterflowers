using System;
using UnityEngine;
using uwu.Extensions;

namespace butterflowersOS.AI
{
	public class ButterflowerVFX : MonoBehaviour
	{
		// Properties

		ParticleSystem ps;
		int previousParticleCount = 0;

		float minLifetime, maxLifetime;
		float lifetimeOffset;
		
		// Attributes

		[SerializeField] AnimationCurve lifetimeEffectorCurve;

		void Awake()
		{
			ps = GetComponent<ParticleSystem>();
		}

		void Start()
		{
			var ps_main = ps.main;

			// Fetch bounds of lifetime
			minLifetime = ps_main.startLifetime.constantMin;
			maxLifetime = ps_main.startLifetime.constantMax;
				lifetimeOffset = maxLifetime - minLifetime;
		}

		void LateUpdate()
		{
			ParticleSystem.ShapeModule shape = ps.shape;
			Vector3 bounds = shape.scale;
			float boundsHeight = bounds.z / 2f;
			float dt = Time.deltaTime;
			
			int numberOfParticles = ps.particleCount;

			if (numberOfParticles > 0) 
			{ 
				ParticleSystem.Particle[] particles = new ParticleSystem.Particle[numberOfParticles];
				ps.GetParticles(particles, numberOfParticles); // Fetch the latest particles emitted

				int particleIndex = 0; int particlesChanged = 0;
				for(; particleIndex < numberOfParticles; particleIndex++) 
				{
					var particle = particles[particleIndex];
					var sLifetime = particle.startLifetime;
					var lifetime = particle.remainingLifetime;

					if (lifetime >= sLifetime - dt) 
					{
						Vector3 localPos = transform.InverseTransformPoint(particle.position); // Starting local position

						float height = localPos.z;
						float reduction =
							lifetimeEffectorCurve.Evaluate(height.RemapNRB(-boundsHeight, boundsHeight, 0, 1f));

						++particlesChanged;
						particles[particleIndex].remainingLifetime -= reduction * lifetime;
					}
				}

				ps.SetParticles(particles, numberOfParticles);
			}
			
			previousParticleCount = numberOfParticles;
		}
	}
}