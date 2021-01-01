using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Extensions;

[Obsolete("Obsolete API!", true)]
public class CollisionPlane : MonoBehaviour
{

    #region Internal

    public enum Type {
        Bounce,
        Death
    }

    #endregion

    // Properties

    new Renderer renderer;
    Material material;

    // Attributes

    public Type type = Type.Death;

    [SerializeField] float pulseDecayTime = 1f;
    [SerializeField] [Range(0f, 1f)] float maxPulseOpacity = .87f;

    #region Monobehaviour callbacks

    void Start()
    {
        renderer = GetComponent<Renderer>();
        material = renderer.material;

        SetOpacityFromPulseStrength(0f);
    }

	#endregion

	#region Collision callbacks

	void OnTriggerEnter(Collider collider)
    {
        var butterflower = collider.GetComponent<Butterfly>();
        if (butterflower != null) 
        {
            if (type == Type.Death) 
            {
//                butterflower.Kill();
            }
        }

        Pulse();
    }

    #endregion

    #region Pulse

    void Pulse()
    {
        SetOpacityFromPulseStrength(1f);

        StopCoroutine("Pulsing");
        StartCoroutine("Pulsing");
    }

    IEnumerator Pulsing()
    {
        float t = 0f;
        while (t < pulseDecayTime) 
        {
            t += Time.deltaTime;

            var i = 1f - Mathf.Clamp01(t / pulseDecayTime);
            SetOpacityFromPulseStrength(i);

            yield return null;
        }
    }

    void SetOpacityFromPulseStrength(float strength)
    {
        float opacity = strength.RemapNRB(0f, 1f, 0f, maxPulseOpacity);
        material.color = new Color(1f, 0f, 1f, opacity);
    }

	#endregion
}
