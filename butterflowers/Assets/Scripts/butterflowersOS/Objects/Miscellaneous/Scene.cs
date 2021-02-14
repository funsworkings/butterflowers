using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace butterflowersOS.Objects.Miscellaneous
{
	public class Scene : MonoBehaviour
	{
		// Properties
		
		[SerializeField] SceneMesh[] _meshes;
		[SerializeField] PlayableAsset _cutscene = null;
		[SerializeField] CinemachineVirtualCamera _camera = null;
		
		
		#region Accessors

		public SceneMesh[] meshes => _meshes;
		public PlayableAsset cutscene => _cutscene;
		public new CinemachineVirtualCamera camera => _camera;
		
		#endregion

		void Start()
		{
			_meshes = GetComponentsInChildren<SceneMesh>(true); // Retrieve all meshes
			Hide(false); // Hide immediately
		}


		#region Ops
		
		public void Hide(bool visible)
		{
			foreach(SceneMesh mesh in meshes)
			{
				mesh.Hide();
				mesh.visible = visible;
			}
		}

		public void Show(bool visible)
		{
			foreach(SceneMesh mesh in meshes) {
				if (visible) mesh.Show();
				else mesh.Hide();
				
				mesh.visible = true;
			}
		}

		#endregion
	}
}