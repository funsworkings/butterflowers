using UnityEngine;

namespace butterflowersOS.Snippets
{
	public class Burster : MonoBehaviour
	{
		[SerializeField] GameObject pr_burst;
		[SerializeField] float cameraDistance = 1f;

		Camera camera;
    
		// Start is called before the first frame update
		void Start()
		{
			camera = Camera.main;
		}

		public void Burst(Vector3 position)
		{
			var worldPosition = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, cameraDistance));
			Instantiate(pr_burst, worldPosition, pr_burst.transform.rotation);
		}
	}
}