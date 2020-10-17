using UnityEngine;
using UnityEngine.Events;

namespace uwu.Snippets
{
	public class Damage : MonoBehaviour
	{
		public UnityEvent onHit, onRecover;

		[SerializeField] Color damageColor = Color.white;
		[SerializeField] Material[] damageMaterials = { };
		[SerializeField] float damageScale = 1f;
		[SerializeField] float damageDuration = .167f;

		[SerializeField] new Renderer renderer;

		[SerializeField] bool debugHit;

		bool damaging;
		Color[] def_colors;

		Vector3 def_scale;
		Material[] materials;
		float t_damage;

		public float damage_amount => 1f - Mathf.Clamp01(t_damage / damageDuration);

		void Awake()
		{
			if (renderer == null)
				renderer = GetComponent<Renderer>();
		}

		void Start()
		{
			def_scale = transform.localScale;

			if (renderer != null) {
				materials = renderer.materials;
				def_colors = new Color[materials.Length];
				for (var i = 0; i < materials.Length; i++)
					def_colors[i] = materials[i].color;
			}
		}

		void Update()
		{
			if (debugHit) {
				debugHit = false;
				Hit();
			}
		}

		void LateUpdate()
		{
			if (damaging) {
				if (t_damage > damageDuration) {
					damaging = false;
					onRecover.Invoke();
				}
				else {
					t_damage += Time.deltaTime;
				}

				if (renderer != null)
					UpdateMaterials();
				UpdateScale();
			}
		}

		public void Hit()
		{
			onHit.Invoke();

			t_damage = 0f;
			damaging = true;
		}

		void UpdateMaterials()
		{
			var mats = renderer.materials = damaging && damageMaterials.Length > 0 ? damageMaterials : materials;

			for (var i = 0; i < mats.Length; i++) renderer.materials[i].color = damaging ? damageColor : def_colors[i];
		}

		void UpdateScale()
		{
			transform.localScale = def_scale * (damaging ? 1f + (damageScale - 1f) * damage_amount : 1f);
		}
	}
}