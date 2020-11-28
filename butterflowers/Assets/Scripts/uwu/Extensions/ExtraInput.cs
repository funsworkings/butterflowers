using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraInput : Input
{
    public static bool GetScrollDown()
    {
        return mouseScrollDelta.y < 0f; 
    }

    public static bool GetScrollUp()
    {
        return mouseScrollDelta.y > 0f;
    }
}
