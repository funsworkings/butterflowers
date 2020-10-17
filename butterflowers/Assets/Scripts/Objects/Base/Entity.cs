using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{

    // External

    protected World World;
    protected Sun Sun;
    
    // Properties

    [SerializeField] AGENT m_agent = AGENT.NULL;

    // Accessors

    public AGENT Agent => m_agent;
    

    #region Monobehaviour callbacks

    // Start is called before the first frame update
    protected void Start()
    {
        World = World.Instance;
        Sun = Sun.Instance;

        OnStart();
    }

    protected virtual void Update()
    {
	    if(EvaluateUpdate())
			OnUpdate();
    }

    #endregion

    #region Monobehaviour override callbacks

    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }

	#endregion
	
	#region Verify update

	protected virtual bool EvaluateUpdate()
	{
		return Sun.active;
	}

	#endregion
}
