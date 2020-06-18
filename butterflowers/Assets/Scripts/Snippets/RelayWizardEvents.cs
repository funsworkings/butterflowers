using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelayWizardEvents : MonoBehaviour
{
    Wizard.Controller wizard;

	#region Monobehaviour callbacks

	void Awake()
	{
		wizard = GetComponentInParent<Wizard.Controller>();
	}

	#endregion

	#region Callbacks

	public void OnCastSpell() { wizard.DidCastSpell(); }

	#endregion
}
