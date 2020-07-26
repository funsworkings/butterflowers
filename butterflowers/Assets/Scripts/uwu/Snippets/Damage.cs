using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damage : MonoBehaviour
{
    public UnityEvent onHit;

    [SerializeField] Color damageColor = Color.white;
    [SerializeField] Material[] damageMaterials = new Material[] { };
    [SerializeField] float damageScale = 1f;
    [SerializeField] float damageDuration = .167f;

    Vector3 def_scale;
    Color[] def_colors;

    [SerializeField] new Renderer renderer;
    Material[] materials;

    bool damaging = false;
    float t_damage = 0f;

    public float damage_amount => 1f - Mathf.Clamp01( t_damage / damageDuration );

    [SerializeField] bool debugHit = false;

    void Awake()
    {
        if(renderer == null)
            renderer = GetComponent<Renderer>();
    }

    void Start()
    {
        def_scale = transform.localScale;

        if (renderer != null) {
            materials = renderer.materials;
            def_colors = new Color[materials.Length];
            for (int i = 0; i < materials.Length; i++)
                def_colors[i] = materials[i].color;
        }
    }

    void Update() {

        if (debugHit) 
        {
            debugHit = false;
            Hit();
        }

    }

    void LateUpdate()
    {
        if (damaging) 
        {
            if (t_damage > damageDuration)
                damaging = false;
            else
                t_damage += Time.deltaTime;

            if(renderer != null)
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
        var mats = renderer.materials = (damaging && damageMaterials.Length > 0) ? damageMaterials : materials;

        for (int i = 0; i < mats.Length; i++) 
        {
            renderer.materials[i].color = (damaging) ? damageColor : def_colors[i];
        }
    }

    void UpdateScale()
    {
        transform.localScale = def_scale * ((damaging) ? (1f + (damageScale - 1f) * damage_amount) : 1f);
    }
}
