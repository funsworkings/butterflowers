using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Obsolete API!", true)]
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

	public void OnCastSpell()
	{ //wizard.DidCastSpell(); 
	}

	public void OnDidInspect()
		{ //wizard.DidInspect(); 
		}

		#endregion
}
