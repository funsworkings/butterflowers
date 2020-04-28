using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public static class Stringing
{

    public static string ToUppercase(this string value, bool caps)
    {
        if (value.Length > 0)
        {
            if (caps) return value.ToUpper();
            else return (value[0] + "").ToUpper() + value.Substring(1);
        }
        return value;
    }

    public static bool isNumeric(this string value)
    {
        if (value == null)
        {
            return false;
        }

        try
        {
            int num = int.Parse(value);
        }
        catch (System.Exception e)
        {
            return false;
        }
        return true;
    }

    public static string print(this IList array)
    {
        string combined = "[";

        if (array != null && array.Count > 0)
        {
            for (int i = 0; i < array.Count; i++)
            {
                var el = array[i];
                combined += el.ToString();

                if (i < array.Count - 1)
                    combined += ", ";
            }
        }
        combined += "]";

        return combined;
    }
}