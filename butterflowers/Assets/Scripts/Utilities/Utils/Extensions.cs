/// <summary>
/// Extensions for core data types
/// </summary>

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class Extensions {

    private static System.Random Random = null;

    #region Float

    // Remaps a value [0 - 1] to [min - max]
    public static float Remap(this float number, float min, float max){
        if(min == max)
            return min; // Return value immediately if min and max are the same

        if(min > max){
            float temp = max;
                max = min;
                min = temp;
        }

        return (number * (max - min)) + min; 
    }

    public static Vector3 ClampToDirection(this Vector3 value, float z){
        return (new Vector3(value.x, value.y, z));
    }

    // Remaps a value from [min0 - max0] to [min1 - max1]
    public static float Remap(this float number, float min0, float max0, float min1, float max1){
        if(min1 == max1)
            return min1; // Return value immediately if min and max are the same

        if(min0 > max0){
            float temp = max0;
                max0 = min0;
                min0 = temp;
        }

        if(min1 > max1){
            float temp = max1;
                max1 = min1;
                min1 = temp;
        }

        float interval = Mathf.Clamp01((number - min0) / (max0 - min0));
            return (interval * (max1 - min1)) + min1; 
    }

    public static Color NextHue(this Color color, float offset){
        float h, s, v;

        Color.RGBToHSV(color, out h, out s, out v);

        h += offset;
        h = Mathf.Repeat(h, 1f);

        return Color.HSVToRGB(h, s, v);
    }

    #endregion

    #region Arrays

    // Return num_items random values.
    public static List<T> PickRandomSubset<T>(this T[] values, int num_values)
    {
        // Create the Random object if it doesn't exist.
        if (Random == null) Random = new System.Random();

        // Don't exceed the array's length.
        if (num_values >= values.Length)
            num_values = values.Length - 1;

        // Make an array of indexes 0 through values.Length - 1.
        int[] indexes =
            Enumerable.Range(0, values.Length).ToArray();

        // Build the return list.
        List<T> results = new List<T>();

        // Randomize the first num_values indexes.
        for (int i = 0; i < num_values; i++)
        {
            // Pick a random entry between i and values.Length - 1.
            int j = Random.Next(i, values.Length);

            // Swap the values.
            int temp = indexes[i];
            indexes[i] = indexes[j];
            indexes[j] = temp;

            // Save the ith value.
            results.Add(values[indexes[i]]);
        }

        // Return the selected items.
        return results;
    }

    #endregion
}


