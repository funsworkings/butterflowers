using System;
using UnityEngine;

namespace butterflowersOS.Visuals
{
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMesh : MonoBehaviour
	{
		// Properties

		[SerializeField] ParticleSystem ps;
		[SerializeField] Transform root;
		[SerializeField] MeshFilter basicMesh;
		[SerializeField] SkinnedMeshRenderer skinnedMesh;

		Mesh mesh;
		ParticleSystem.Particle[] particles;

		float timeDelta = 0f;
		float lastSpawnTime = 0f;

		int particleSetIndex = 0;
		
		// Attributes
		
		[SerializeField] int maxParticles = 100, particlesPerSpawn = 25;
		[SerializeField] float spawnInterval = 1f;
		[SerializeField] float minLifetime = 1f, maxLifetime = 3f;
		[SerializeField] float lifetimeDelta = .1f;
		[SerializeField] int spawnCount = 5;

		void Awake()
		{
			if (basicMesh != null) mesh = basicMesh.mesh;
			if(skinnedMesh != null) mesh = new Mesh();
			
			ps = GetComponent<ParticleSystem>();

			var ps_main = ps.main;
				ps_main.maxParticles = maxParticles;
				
			particles = new ParticleSystem.Particle[maxParticles];
		}

		void LateUpdate()
		{
			if (mesh == null) return;

			float timeDiff = (Time.time - lastSpawnTime);
			if (timeDiff <= spawnInterval) return;
			
			timeDelta = Mathf.Clamp(timeDiff * spawnCount, minLifetime, maxLifetime);
            lastSpawnTime = Time.time;

            ParticleSystem.EmitParams @params = new ParticleSystem.EmitParams();
			ps.Emit(@params, maxParticles);

			var particleCount = ps.GetParticles(particles);

			if (skinnedMesh != null) skinnedMesh.BakeMesh(mesh);

			var vertices = mesh.vertices;
			var vertexStep = (vertices.Length / particlesPerSpawn);
				vertexStep = (vertexStep == 0) ? 1 : vertexStep;

			var baseIndex = particleSetIndex * particlesPerSpawn;
			var maxIndex = baseIndex + particlesPerSpawn;

			int j = 0;
			float lifetimeStep = 0f;
			for (int i = baseIndex; i < maxIndex; i++) 
			{
				particles[i].remainingLifetime = timeDelta * lifetimeStep;
				particles[i].position = root.TransformPoint(vertices[j]);

				j += vertexStep;
				if (j >= vertices.Length) 
				{
					j = 1;
				}

				lifetimeStep = (lifetimeStep + lifetimeDelta) % 1f;
			}
			
			ps.SetParticles(particles, maxParticles);
			particleSetIndex = (++particleSetIndex % spawnCount);
		}
		
	}
}