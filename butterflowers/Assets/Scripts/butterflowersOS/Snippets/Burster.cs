using UnityEngine;
using uwu.Gameplay;

namespace butterflowersOS.Snippets
{
	public class Burster : MonoBehaviour
	{
		[SerializeField] ObjectPool burstPool;
		[SerializeField] float cameraDistance = 1f;

		new Camera camera;
    
		// Start is called before the first frame update
		void Start()
		{
			camera = Camera.main;
		}

		public void Burst(Vector3 position)
		{
			var worldPosition = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, cameraDistance));
			var burst = burstPool.Request();
				burst.transform.position = worldPosition;
				
			burst.GetComponent<ParticleSystem>().Play();
		}
	}
}