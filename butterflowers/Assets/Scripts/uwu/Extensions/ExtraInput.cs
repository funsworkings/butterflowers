using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Extensions;

public class ExtraInput : Singleton<ExtraInput>
{
    // Accessors

    Vector2 scrollDelta => Input.mouseScrollDelta;
    
    // Collections

    public int scrollDirection = 0;
    public float scrollStreak = 0f;

    // Constants

    static int detectScrollThreshold = 2;

    void Update()
    {
        UpdateScroll();
    }
    
    #region Scroll

    void UpdateScroll()
    {
        var amt = (int)scrollDelta.y;

        var direction = (amt != 0) ? (int) Mathf.Sign(amt) : 0;
        if (direction != 0) 
        {
            if (direction != scrollDirection)
                scrollStreak = 0;
            else
                scrollStreak += Mathf.Abs(amt);
        }
        else 
        {
            if(scrollDirection == 0)
                scrollStreak -= Time.deltaTime * 1f;
        }

        scrollDirection = direction;
    }

    public static bool GetScrollDown()
    {
        var instance = ExtraInput.Instance;
        return instance.scrollDirection == -1 && (instance.scrollStreak > detectScrollThreshold);
    }

    public static bool GetScrollUp()
    {
        var instance = ExtraInput.Instance;
        return instance.scrollDirection == 1 && (instance.scrollStreak > detectScrollThreshold);
    }
    
    #endregion
}
