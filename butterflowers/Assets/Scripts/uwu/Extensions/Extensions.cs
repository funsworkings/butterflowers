/// <summary>
/// Extensions for core data types
/// </summary>

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using Rand = System.Random;
using Random = UnityEngine.Random;

using Object = UnityEngine.Object;
using Obj = System.Object;
using System.Security.Cryptography;

public static class Extensions
{
    private static Rand gen = new Rand();

    #region Date/time

    public static DateTime UTCStart()
    {
        return new System.DateTime(1970, 1, 1);
    }

    public static DateTime RandomDay()
    {
        DateTime start = UTCStart();
        int range = (DateTime.UtcNow - start).Days;
        return start.AddDays(gen.Next(range));
    }

    #endregion

    #region Arrays

    // Return num_items random values.
    public static List<T> PickRandomSubset<T>(this T[] values, int num_values)
    {
        // Don't exceed the array's length.
        if (num_values > values.Length)
            num_values = values.Length;

        // Make an array of indexes 0 through values.Length - 1.
        int[] indexes =
            Enumerable.Range(0, values.Length).ToArray();

        // Build the return list.
        List<T> results = new List<T>();

        // Randomize the first num_values indexes.
        for (int i = 0; i < num_values; i++)
        {
            // Pick a random entry between i and values.Length - 1.
            int j = gen.Next(i, values.Length);

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

    #region Transforms

    public static void ChangeLayersRecursively(Transform trans, string name, bool self)
    {
        if(self) trans.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in trans)
        {
            child.gameObject.layer = LayerMask.NameToLayer(name);
            ChangeLayersRecursively(child, name, false);
        }
    }

    public static void CopyTransformValues(GameObject from, ref GameObject to, bool local){
        if(local){
            to.transform.localPosition = from.transform.localPosition;
            to.transform.localRotation = from.transform.localRotation;
            to.transform.localScale = from.transform.localScale;
        }
        else{
            to.transform.position = from.transform.position;
            to.transform.rotation = from.transform.rotation;
            to.transform.localScale = from.transform.localScale;
        }
        
    }

    #endregion

    #region Textures

    public static Texture2D GetRandomSection(Texture texture, int width, int height){
        int w = texture.width;
        int h = texture.height;

        int x = Random.Range(0, w - width);
        int y = Random.Range(0, h - height);

        return GetSection(texture, x, y, width, height);
    }

    public static Texture2D GetSection(Texture texture, int x, int y, int width, int height){
        Texture2D aTex = ToTexture2D(texture);
        Texture2D bTex = new Texture2D(width, height, aTex.format, false);

        Color[] cols = aTex.GetPixels(x, y, width, height);
        bTex.SetPixels(cols);
        bTex.Apply();

        return bTex;
    }

    public static Texture2D ToTexture2D(Texture texture){
        return (Texture2D)texture;
    }

    public static Texture2D EncodeBytesToTexture2D(byte[] bytes){
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false, false);

        tex.LoadImage(bytes);
        tex.Apply();

        return tex;
    }

    #endregion

    #region Float

    // Remaps a value [0 - 1] to [min - max]
    public static float Remap(this float number, float min, float max)
    {
        if (min == max)
            return min; // Return value immediately if min and max are the same

        if (min > max)
        {
            float temp = max;
            max = min;
            min = temp;
        }

        return (number * (max - min)) + min;
    }

    // Remaps a value from [min0 - max0] to [min1 - max1]
    public static float Remap(this float number, float min0, float max0, float min1, float max1)
    {
        if (min1 == max1)
            return min1; // Return value immediately if min and max are the same

        if (min0 > max0)
        {
            float temp = max0;
            max0 = min0;
            min0 = temp;
        }

        if (min1 > max1)
        {
            float temp = max1;
            max1 = min1;
            min1 = temp;
        }

        float interval = Mathf.Clamp01((number - min0) / (max0 - min0));
        return (interval * (max1 - min1)) + min1;
    }
    // A non-bullshit (non-rick) version of remap
    //(remapNoRickBullshit)
    public static float RemapNRB(this float oldValue, float oldMin, float oldMax, float newMin, float newMax, bool clamped = true)
    {
        if (clamped)
        {
            float realOldMax = Mathf.Max(oldMin, oldMax);
            float realOldMin = Mathf.Min(oldMin, oldMax);
            oldValue = Mathf.Clamp(oldValue, realOldMin, realOldMax);
        }

        float oldRange = (oldMax - oldMin);
        float newRange = (newMax - newMin);
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;
        return newValue;
    }

    public static float Absolute(this float number)
    {
        return Mathf.Abs(number);
    }

    public static Color NextHue(this Color color, float offset)
    {
        float h, s, v;

        Color.RGBToHSV(color, out h, out s, out v);

        h += offset;
        h = Mathf.Repeat(h, 1f);

        return Color.HSVToRGB(h, s, v);
    }

    // Integrate area under AnimationCurve between start and end time
    public static float IntegrateCurve(AnimationCurve curve, float startTime, float endTime, int steps)
    {
        return Integrate(curve.Evaluate, startTime, endTime, steps);
    }

    // Integrate function f(x) using the trapezoidal rule between x=x_low..x_high
    public static float Integrate(System.Func<float, float> f, float x_low, float x_high, int N_steps)
    {
        float h = (x_high - x_low) / N_steps;
        float res = (f(x_low) + f(x_high)) / 2;
        for (int i = 1; i < N_steps; i++)
        {
            res += f(x_low + i * h);
        }
        return h * res;
    }

    #endregion

    #region Screen conversions

    public static float pt = (1f / 163f); // 1 point per inch

	// Default scaling to WIDTH
	public static float NormalizeToScreen(float f){
		return (f / Screen.width);
	}

	public static Vector2 NormalizeToScreen(Vector2 v){
		return new Vector2 (v.x / Screen.width, v.y / Screen.height);
	}

	public static Vector3 NormalizeToScreen(Vector3 v){
		return new Vector3 (v.x / Screen.width, v.y / Screen.height, v.z);
	}

	public static Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - (size * 0.5f), size);
	}

	public static float PixelsToPoints(float pixels){ return PixelsToPoints((int)pixels); }
	public static float PixelsToPoints(int pixels){
		float dpi = Screen.dpi;
		return (163f * pixels) / dpi;
	}

	public static float PointsToPixels(float points){ return PointsToPixels((int)points); }
	public static float PointsToPixels(int points){
		float dpi = Screen.dpi;
		return (points * dpi) / 163f;
	}

    #endregion

    #region Vectors

    public static Vector3 Zero(Vector3 v, char param){
		switch (param) {
			case 'x':
				return new Vector3 (0f, v.y, v.z);
			case 'y':
				return new Vector3 (v.x, 0f, v.z);
			case 'z':
				return new Vector3 (v.x, v.y, 0f);
			default:
				Debug.Log ("Invalid parameter for vector conversion.");
				return Vector3.zero;
		}
	}

	public static Vector2 RemoveZ(Vector3 v){
		return new Vector2 (v.x, v.y);
	}

	public static Vector3 Absolute(Vector3 v){
		return new Vector3 (Mathf.Abs (v.x),
							Mathf.Abs (v.y),
							Mathf.Abs (v.z));
	}

	public static Vector3 CompositeMax(Vector3 v0, Vector3 v1){
		return new Vector3 (Mathf.Max (v0.x, v1.x),
							Mathf.Max (v0.y, v1.y),
							Mathf.Max (v0.z, v1.z));
	}

	public static Vector3 AbsoluteMax(Vector3 v0, Vector3 v1){
		if (v0.magnitude >= v1.magnitude)
			return v0;

		return v1;
	}

	public static Vector3 NormalizedDirection(Vector3 a, Vector3 b){
		return (b - a).normalized;
	}

    public static Vector3 RandomInsideBounds(Vector3 v){
        return RandomInsideBounds(v, false);
    }

	public static Vector3 RandomInsideBounds(Vector3 v, bool negative){
        float m_x = 0f, m_y = 0f, m_z = 0f;

		if(negative) {
			m_x = -v.x;
			m_y = -v.y;
			m_z = -v.z;
		}

		return new Vector3(Random.Range(m_x, v.x), Random.Range(m_y, v.y), Random.Range(m_z, v.z));
    }

	public static Vector2 Clamp(Vector2 v, float b){
		return Clamp(v, b, b);
	}

	public static Vector2 Clamp(Vector2 v, float x, float y){
		return new Vector2(Mathf.Clamp(v.x, -x, x), 
						   Mathf.Clamp(v.y, -y, y));
	}

	public static Vector3 Clamp(Vector3 v, float b){
		return Clamp(v, b, b, b);
	}

	public static Vector3 Clamp(Vector3 v, float x, float y, float z){
		return new Vector3(Mathf.Clamp(v.x, -x, x), 
						   Mathf.Clamp(v.y, -y, y),
						   Mathf.Clamp(v.z, -z, z));
	}

    #endregion

    #region Colors

    public static Color SetOpacity (float a, Color c){
		return new Color (c.r, c.g, c.b, a);
	}

	public static Color SetColor(Color src, Color dest){
		return new Color(dest.r, dest.g, dest.b, src.a);
	}

	public static Color Blend(Color a, Color b, Vector2 weights){
		// Ensure weights are clamped 0-1
		weights.x = Mathf.Clamp01(weights.x);
		weights.y = Mathf.Clamp01(weights.y);
	
		Vector4 ca = new Vector4(a.r, a.g, a.b, a.a) * weights.x;
		Vector4 cb = new Vector4(b.r, b.g, b.b, b.a) * weights.y;
		
		return new Color  (Mathf.Clamp01(ca.x + cb.x),
						   Mathf.Clamp01(ca.y + cb.y),
						   Mathf.Clamp01(ca.z + cb.z),
						   Mathf.Clamp01(ca.w + cb.w));
	}

	// Converts hex string to Color
    public static Color GetColor(string hex){
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

	public static Color ParseHex(string hex){ 
		return GetColor(hex); 
	}

    public static string ParseColor(Color color){ 
		return "#" + ColorUtility.ToHtmlStringRGB(color); 
	}

    #endregion

    #region Strings

    public static string ToUppercase(this string value, bool caps){
        if(value.Length > 0){
            if(caps) return value.ToUpper();
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

    public static string print<T>(this IList<T> array)
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

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[gen.Next(s.Length)]).ToArray());
    }


    /// <summary>
    /// Formats the difference in time (prettify)
    /// </summary>
    /// <returns>The difference in time.</returns>
    /// <param name="diff">Difference in time (in milliseconds)</param>
    public static string FormatDifferenceInTime(Int32 diff)
    {
        if (diff <= 0)
            return "Just now"; // Return just now if happened recently

        DateTime current = System.DateTime.Now;
        DateTime previous = current.AddMilliseconds(-diff); // Subtract difference in time from local time

        //Debug.Log(previous);

        TimeSpan span = current - previous;

        int days = (current.Day - previous.Day); // IMPORTANT: use actual day value for yesterday vs. today distinction
        if(days > 0)
        {
            if (days == 1)
                return "Yesterday";
            return "A while ago";
        }
        else
        {
            string unit = "null";
            int amount = 0;


            int hours = span.Hours;
            if (hours >= 2)
                return "Today";
            else
            {
                if(hours == 1)
                {
                    unit = "hour";
                    amount = hours;
                }
                else
                {
                    int minutes = span.Minutes;
                    if(minutes > 0)
                    {
                        unit = "minute";
                        amount = minutes;
                    }
                    else
                    {
                        unit = "second";
                        amount = span.Seconds;
                    }
                }
            }


            return string.Format("{0} {1}{2} ago", amount, unit, (amount > 1) ? "s" : ""); // Return formatted difference in time with plural appended (conditional)
        }
    }

    public static string ConvertToValidPhoneNumber(this string phonenumber)
    {
        if (string.IsNullOrEmpty(phonenumber))
            return null;


        string formatted = phonenumber;

        if (formatted.Length == 12) // Might contain country code
        {
            string prefix = phonenumber.Substring(0, 2);
            //Debug.LogFormat("Detected prefix: {0}", prefix);

            if (prefix == "+1")
            { // Does not precede with country code, INVALID 
                formatted = phonenumber.Substring(2);
                //Debug.LogFormat("Detected number with valid prefix, removing: {0}", formatted);
            }
            else
                formatted = null;

        }
        else
        {
            if (formatted.Length != 10) // Does not contain country code 
            {
                //Debug.Log("Number was invalid, did not contain 10 digits");
                formatted = null;
            }
            else
            {
                //Debug.Log("Detected number with NO prefix");
            }
        }


        if (formatted != null)
        {
            for (int i = 0; i < formatted.Length; i++)
            {
                try
                {
                    string character = formatted[i] + "";
                    int digit = int.Parse(character);
                }
                catch (System.Exception e)
                {
                    formatted = null;
                    break;
                }
            }
        }


        if (formatted != null)
        {
            return "+1" + formatted; // Append country code to phonenumber at end
        }
        return null;
    }

    #endregion

    #region Errors

    public static System.Exception PrependErrorMessage(this System.Exception err, string prepend)
    {
        if (err != null)
        {
            if (err.Message.Contains("Network error*") || err.Message.Contains("Server error*"))
                return err;

            err = new SystemException(err.Message.Insert(0, prepend));
        }

        return err;
    }

    #endregion

    #region Miscellaneous

    public static Texture2D GenerateThumbnailFromPath(string path, int width = 16, int height = 16)
    {
        int size = (width * height);

        Texture2D tex = new Texture2D(width, height);

        Color[] colors = new Color[size];
        for (int i = 0; i < size; i++)
        {
            float val = 1f;
            if (i < path.Length)
                val = ((int)path[i]) / 255f;

            colors[i] = Color.HSVToRGB(Random.Range(0f, 1f), 1f, val);
        }

        tex.SetPixels(colors);
        tex.Apply();

        return tex;
    }

    #endregion
}


