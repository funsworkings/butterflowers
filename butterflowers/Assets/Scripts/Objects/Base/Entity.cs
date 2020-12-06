﻿using System;
using System.Collections;
using System.Collections.Generic;
using Objects.Base;
using UnityEngine;

public abstract class Entity : Element
{
	// Properties

    [SerializeField] AGENT m_agent = AGENT.NULL;

    // Accessors

    public AGENT Agent => m_agent;

    #region Verify update

	protected override bool EvaluateActiveState()
	{
		return Sun.active;
	}

	#endregion
}
