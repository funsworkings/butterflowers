/// <summary>
/// Extensions for core data types
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;
using Rand = System.Random;
using Random = UnityEngine.Random;

namespace uwu.Extensions
{
	public static class Extensions
	{
		static readonly Rand gen = new Rand();

		#region Errors

		public static Exception PrependErrorMessage(this Exception err, string prepend)
		{
			if (err != null) {
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
			var size = width * height;

			var tex = new Texture2D(width, height);

			var colors = new Color[size];
			for (var i = 0; i < size; i++) {
				var val = 1f;
				if (i < path.Length)
					val = path[i] / 255f;

				colors[i] = Color.HSVToRGB(Random.Range(0f, 1f), 1f, val);
			}

			tex.SetPixels(colors);
			tex.Apply();

			return tex;
		}

		#endregion

		#region Date/time

		public static DateTime UTCStart()
		{
			return new DateTime(1970, 1, 1);
		}

		public static DateTime RandomDay()
		{
			var start = UTCStart();
			var range = (DateTime.UtcNow - start).Days;
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
			var indexes =
				Enumerable.Range(0, values.Length).ToArray();

			// Build the return list.
			var results = new List<T>();

			// Randomize the first num_values indexes.
			for (var i = 0; i < num_values; i++) {
				// Pick a random entry between i and values.Length - 1.
				var j = gen.Next(i, values.Length);

				// Swap the values.
				var temp = indexes[i];
				indexes[i] = indexes[j];
				indexes[j] = temp;

				// Save the ith value.
				results.Add(values[indexes[i]]);
			}

			// Return the selected items.
			return results;
		}

		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			if (length > data.Length)
				length = data.Length;

			var result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		#endregion

		#region Transforms

		public static void ChangeLayersRecursively(Transform trans, string name, bool self)
		{
			if (self) trans.gameObject.layer = LayerMask.NameToLayer(name);
			foreach (Transform child in trans) {
				child.gameObject.layer = LayerMask.NameToLayer(name);
				ChangeLayersRecursively(child, name, false);
			}
		}

		public static void CopyTransformValuesFrom(this Transform to, Transform from, bool local = false)
		{
			if (local) {
				to.localPosition = from.localPosition;
				to.localRotation = from.localRotation;
				to.localScale = from.localScale;
			}
			else {
				to.position = from.position;
				to.rotation = from.rotation;
				to.localScale = from.localScale;
			}
		}

		public static void ResetTransformValues(this Transform t)
		{
			if (t is RectTransform)
				(t as RectTransform).pivot = Vector3.one * .5f;

			t.localScale = Vector3.one;
			t.localEulerAngles = Vector3.zero;
			t.localPosition = Vector3.zero;
		}
		
		public static float3 WorldToLocal(this float4x4 transform, float3 point)
		{
			return math.transform(math.inverse(transform), point);
		}
 
		public static float3 LocalToWorld(this float4x4 transform, float3 point)
		{
			return math.transform(transform, point);
		}

		#endregion

		#region Textures

		public static Texture2D GetRandomSection(Texture texture, int width, int height)
		{
			var w = texture.width;
			var h = texture.height;

			var x = Random.Range(0, w - width);
			var y = Random.Range(0, h - height);

			return GetSection(texture, x, y, width, height);
		}

		public static Texture2D GetSection(Texture texture, int x, int y, int width, int height)
		{
			var aTex = ToTexture2D(texture);
			var bTex = new Texture2D(width, height, aTex.format, false);

			var cols = aTex.GetPixels(x, y, width, height);
			bTex.SetPixels(cols);
			bTex.Apply();

			return bTex;
		}

		public static Texture2D ToTexture2D(Texture texture)
		{
			return (Texture2D) texture;
		}

		public static Texture2D EncodeBytesToTexture2D(byte[] bytes)
		{
			var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false, false);

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

			if (min > max) {
				var temp = max;
				max = min;
				min = temp;
			}

			return number * (max - min) + min;
		}

		// Remaps a value from [min0 - max0] to [min1 - max1]
		public static float Remap(this float number, float min0, float max0, float min1, float max1)
		{
			if (min1 == max1)
				return min1; // Return value immediately if min and max are the same

			if (min0 > max0) {
				var temp = max0;
				max0 = min0;
				min0 = temp;
			}

			if (min1 > max1) {
				var temp = max1;
				max1 = min1;
				min1 = temp;
			}

			var interval = Mathf.Clamp01((number - min0) / (max0 - min0));
			return interval * (max1 - min1) + min1;
		}

		// A non-bullshit (non-rick) version of remap
		//(remapNoRickBullshit)
		public static float RemapNRB(this float oldValue, float oldMin, float oldMax, float newMin, float newMax,
			bool clamped = true)
		{
			if (clamped) {
				var realOldMax = Mathf.Max(oldMin, oldMax);
				var realOldMin = Mathf.Min(oldMin, oldMax);
				oldValue = Mathf.Clamp(oldValue, realOldMin, realOldMax);
			}

			var oldRange = oldMax - oldMin;
			var newRange = newMax - newMin;
			var newValue = (oldValue - oldMin) * newRange / oldRange + newMin;


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
		public static float Integrate(Func<float, float> f, float x_low, float x_high, int N_steps)
		{
			var h = (x_high - x_low) / N_steps;
			var res = (f(x_low) + f(x_high)) / 2;
			for (var i = 1; i < N_steps; i++) res += f(x_low + i * h);
			return h * res;
		}

		#endregion

		#region Screen conversions

		public static float pt = 1f / 163f; // 1 point per inch

		// Default scaling to WIDTH
		public static float NormalizeToScreen(float f)
		{
			return f / Screen.width;
		}

		public static Vector2 NormalizeToScreen(Vector2 v)
		{
			return new Vector2(v.x / Screen.width, v.y / Screen.height);
		}

		public static Vector3 NormalizeToScreen(Vector3 v)
		{
			return new Vector3(v.x / Screen.width, v.y / Screen.height, v.z);
		}

		public static Rect RectTransformToScreenSpace(RectTransform transform)
		{
			var size = Vector2.Scale(transform.rect.size, transform.lossyScale);
			return new Rect((Vector2) transform.position - size * 0.5f, size);
		}

		public static float PixelsToPoints(float pixels)
		{
			return PixelsToPoints((int) pixels);
		}

		public static float PixelsToPoints(int pixels)
		{
			var dpi = Screen.dpi;
			return 163f * pixels / dpi;
		}

		public static float PointsToPixels(float points)
		{
			return PointsToPixels((int) points);
		}

		public static float PointsToPixels(int points)
		{
			var dpi = Screen.dpi;
			return points * dpi / 163f;
		}

		public static bool IsVisible(this Transform target, UnityEngine.Camera cam, out Vector2 screen)
		{
			if (cam == null) {
				screen = Vector2.zero;
				return false;
			}

			Vector2 screenPosition = cam.WorldToScreenPoint(target.position);

			var x = screenPosition.x;
			var y = screenPosition.y;

			screen = screenPosition;
			return x >= 0f && x <= Screen.width && y >= 0f && y <= Screen.height;
		}

		#endregion

		#region Vectors

		public static Vector3 Zero(Vector3 v, char param)
		{
			switch (param) {
				case 'x':
					return new Vector3(0f, v.y, v.z);
				case 'y':
					return new Vector3(v.x, 0f, v.z);
				case 'z':
					return new Vector3(v.x, v.y, 0f);
				default:
					Debug.Log("Invalid parameter for vector conversion.");
					return Vector3.zero;
			}
		}

		public static Vector2 RemoveZ(Vector3 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static Vector3 Absolute(Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x),
				Mathf.Abs(v.y),
				Mathf.Abs(v.z));
		}

		public static Vector3 CompositeMax(Vector3 v0, Vector3 v1)
		{
			return new Vector3(Mathf.Max(v0.x, v1.x),
				Mathf.Max(v0.y, v1.y),
				Mathf.Max(v0.z, v1.z));
		}

		public static Vector3 AbsoluteMax(Vector3 v0, Vector3 v1)
		{
			if (v0.magnitude >= v1.magnitude)
				return v0;

			return v1;
		}

		public static Vector3 NormalizedDirection(Vector3 a, Vector3 b)
		{
			return (b - a).normalized;
		}

		public static Vector3 RandomInsideBounds(Vector3 v)
		{
			return RandomInsideBounds(v, false);
		}

		public static Vector3 RandomInsideBounds(Vector3 v, bool negative)
		{
			float m_x = 0f, m_y = 0f, m_z = 0f;

			if (negative) {
				m_x = -v.x;
				m_y = -v.y;
				m_z = -v.z;
			}

			return new Vector3(Random.Range(m_x, v.x), Random.Range(m_y, v.y), Random.Range(m_z, v.z));
		}

		public static Vector2 Clamp(Vector2 v, float b)
		{
			return Clamp(v, b, b);
		}

		public static Vector2 Clamp(Vector2 v, float x, float y)
		{
			return new Vector2(Mathf.Clamp(v.x, -x, x),
				Mathf.Clamp(v.y, -y, y));
		}

		public static Vector3 Clamp(Vector3 v, float b)
		{
			return Clamp(v, b, b, b);
		}

		public static Vector3 Clamp(Vector3 v, float x, float y, float z)
		{
			return new Vector3(Mathf.Clamp(v.x, -x, x),
				Mathf.Clamp(v.y, -y, y),
				Mathf.Clamp(v.z, -z, z));
		}

		public static Vector3 MatchY(this Vector3 val, Vector3 compare)
		{
			return new Vector3(val.x, compare.y, val.z);
		}

		public static float DistanceBetweenVectors(this IEnumerable<Vector3> points)
		{
			float d = 0f;
			for (int i = 0; i < points.Count(); i++) {
				if (i > 0) {
					d += Vector3.Distance(points.ElementAt(i - 1), points.ElementAt(i));
				}
			}

			return d;
		}

		#endregion

		#region Colors

		public static Color SetOpacity(float a, Color c)
		{
			return new Color(c.r, c.g, c.b, a);
		}

		public static Color SetColor(Color src, Color dest)
		{
			return new Color(dest.r, dest.g, dest.b, src.a);
		}

		public static Color Blend(Color a, Color b, Vector2 weights)
		{
			// Ensure weights are clamped 0-1
			weights.x = Mathf.Clamp01(weights.x);
			weights.y = Mathf.Clamp01(weights.y);

			var ca = new Vector4(a.r, a.g, a.b, a.a) * weights.x;
			var cb = new Vector4(b.r, b.g, b.b, b.a) * weights.y;

			return new Color(Mathf.Clamp01(ca.x + cb.x),
				Mathf.Clamp01(ca.y + cb.y),
				Mathf.Clamp01(ca.z + cb.z),
				Mathf.Clamp01(ca.w + cb.w));
		}

		// Converts hex string to Color
		public static Color GetColor(string hex)
		{
			var color = Color.white;
			ColorUtility.TryParseHtmlString(hex, out color);
			return color;
		}

		public static Color ParseHex(string hex)
		{
			return GetColor(hex);
		}

		public static string ParseColor(Color color)
		{
			return "#" + ColorUtility.ToHtmlStringRGB(color);
		}

		#endregion

		#region Strings

		public static string ToUppercase(this string value, bool caps)
		{
			if (value.Length > 0) {
				if (caps) return value.ToUpper();
				return (value[0] + "").ToUpper() + value.Substring(1);
			}

			return value;
		}

		public static bool isNumeric(this string value)
		{
			if (value == null) return false;

			try {
				var num = int.Parse(value);
			}
			catch (Exception e) {
				return false;
			}

			return true;
		}

		public static string print<T>(this IList<T> array)
		{
			var combined = "[";

			if (array != null && array.Count > 0)
				for (var i = 0; i < array.Count; i++) {
					var el = array[i];
					combined += el.ToString();

					if (i < array.Count - 1)
						combined += ", ";
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

		public static string ReplaceEnclosingPattern(string input, string flag, string replace)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var patt = string.Format("{0}.*?{0}", flag);
			return Regex.Replace(input, patt,
				delegate(Match match)
				{
					var val = match.Value;
					val = val.Replace(flag, "");

					return string.Format(replace, val.Trim());
				});
		}

		public static string AbbreviateFilename(this string file)
		{
			var _file = file;
			var len = _file == null ? 0 : _file.Length;

			if (len == 0) return "";

			var split = new string[] { };

			if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
				split = _file.Split('/');
			else
				split = _file.Split('\\');


			if (split.Length > 0) {
				_file = split[split.Length - 1];
				len = _file.Length;
			}

			var max = 36;

			if (len <= max)
				return _file;

			var abbrev = Mathf.Min(max, len);
			var start = len - abbrev;

			return "..." + _file.Substring(start, abbrev);
		}

		/// <summary>
		///     Formats the difference in time (prettify)
		/// </summary>
		/// <returns>The difference in time.</returns>
		/// <param name="diff">Difference in time (in milliseconds)</param>
		public static string FormatDifferenceInTime(int diff)
		{
			if (diff <= 0)
				return "Just now"; // Return just now if happened recently

			var current = DateTime.Now;
			var previous = current.AddMilliseconds(-diff); // Subtract difference in time from local time

			//Debug.Log(previous);

			var span = current - previous;

			var days = current.Day - previous.Day; // IMPORTANT: use actual day value for yesterday vs. today distinction
			if (days > 0) {
				if (days == 1)
					return "Yesterday";
				return "A while ago";
			}

			var unit = "null";
			var amount = 0;


			var hours = span.Hours;
			if (hours >= 2) {
				return "Today";
			}

			if (hours == 1) {
				unit = "hour";
				amount = hours;
			}
			else {
				var minutes = span.Minutes;
				if (minutes > 0) {
					unit = "minute";
					amount = minutes;
				}
				else {
					unit = "second";
					amount = span.Seconds;
				}
			}


			return string.Format("{0} {1}{2} ago", amount, unit,
				amount > 1 ? "s" : ""); // Return formatted difference in time with plural appended (conditional)
		}

		public static string ConvertToValidPhoneNumber(this string phonenumber)
		{
			if (string.IsNullOrEmpty(phonenumber))
				return null;


			var formatted = phonenumber;

			if (formatted.Length == 12) // Might contain country code
			{
				var prefix = phonenumber.Substring(0, 2);
				//Debug.LogFormat("Detected prefix: {0}", prefix);

				if (prefix == "+1")
					// Does not precede with country code, INVALID 
					formatted = phonenumber.Substring(2);
				//Debug.LogFormat("Detected number with valid prefix, removing: {0}", formatted);
				else
					formatted = null;
			}
			else {
				if (formatted.Length != 10) // Does not contain country code 
					//Debug.Log("Number was invalid, did not contain 10 digits");
					formatted = null;
			}


			if (formatted != null)
				for (var i = 0; i < formatted.Length; i++)
					try {
						var character = formatted[i] + "";
						var digit = int.Parse(character);
					}
					catch (Exception e) {
						formatted = null;
						break;
					}


			if (formatted != null) return "+1" + formatted; // Append country code to phonenumber at end
			return null;
		}

		public static string Scramble(this string body, float strength)
		{
			if (string.IsNullOrEmpty(body))
				return body;

			var len = body.Length;
			if (len == 1)
				return body;

			strength = Mathf.Clamp01(strength);
			var iterations = Mathf.FloorToInt(strength * len);

			var chars = body.ToCharArray();
			for (var i = 0; i < iterations; i++) {
				var ind = Random.Range(0, len);

				var a = chars[ind];
				var b = chars[i];
				var c = a;

				chars[i] = c;
				chars[ind] = b;
			}

			return new string(chars);
		}

		#endregion
		
		#region Types
		
		public static bool castToBoolean(this int i){ return (i != 0); }
		
		#endregion
		
		#region Math
		
		public static float StandardDeviation(float[] values)
		{
			var m = values.Average();
			float v = values.Select(f => Mathf.Pow(m - f, 2f)).Average();
			float d = Mathf.Sqrt(v);

			return d;
		}

		public static float PercentageDifference(float v1, float v2)
		{
			float average = (v1 + v2) / 2f;
			float percent = (average == 0f)? 0f : (v2 - v1) / Mathf.Abs(v1);

			return percent;
		}

		public static float PercentageDifferenceWithMax(float v1, float v2, ref float maxDelta)
		{
			float delta = PercentageDifference(v1, v2);
			
			float deltaM = Mathf.Abs(delta);
			if (deltaM > maxDelta) maxDelta = deltaM;

			return delta;
		}
		
		#endregion
		
		#region Lerp

		public static object GenericLerp<E>(object a, object b, float interval)
		{
			var type = typeof(E);
			if (type == typeof(float)) 
				return Mathf.Lerp((float) a, (float) b, interval);
			else if (type == typeof(int))
				return (int)Mathf.Lerp((float) a, (float) b, interval);
			else if (type == typeof(Vector2))
				return Vector2.Lerp((Vector2) a, (Vector2) b, interval);
			else if (type == typeof(Vector3))
				return Vector3.Lerp((Vector3) a, (Vector3) b, interval);
			else if (type == typeof(Quaternion))
				return Quaternion.Lerp((Quaternion) a, (Quaternion) b, interval);
			
			throw new System.Exception("Unsupported type detected for generic lerp!");
		}
		
		#endregion
		
		#region Camera

		public static void FitToCamera(this Transform transform, UnityEngine.Camera camera, float thickness)
		{
			var depth = camera.nearClipPlane + (0.01f + thickness / 2f);

			Transform cacheParent = transform.parent;
			transform.parent = camera.transform;

			transform.localEulerAngles = Vector3.zero;
			transform.localPosition = new Vector3(0f, 0f, depth + thickness/2f);

			var height = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * depth * 2f;
			transform.localScale = new Vector3(height * camera.aspect, height, thickness);

			transform.parent = cacheParent;
		}
		
		/// <summary>
		/// Convert point from world space to screen space
		/// </summary>
		/// <param name="point">Point in World Space</param>
		/// <param name="cameraPos">Camera position in World Space</param>
		/// <param name="camProjMatrix">Camera.projectionMatrix</param>
		/// <param name="camUp">Camera.transform.up</param>
		/// <param name="camRight">Camera.transform.right</param>
		/// <param name="camForward">Camera.transform.forward</param>
		/// <param name="pixelWidth">Camera.pixelWidth</param>
		/// <param name="pixelHeight">Camera.pixelHeight</param>
		/// <param name="scaleFactor">Canvas.scaleFactor</param>
		/// <returns></returns>
		public static float2 ConvertWorldToScreenCoordinates(float3 point, float3 cameraPos, float4x4 camProjMatrix, float3 camUp, float3 camRight, float3 camForward, float pixelWidth, float pixelHeight, float scaleFactor)
		{
			/*
			* 1 convert P_world to P_camera
			*/
			float4 pointInCameraCoodinates = ConvertWorldToCameraCoordinates(point, cameraPos, camUp, camRight, camForward);
 
 
			/*
			* 2 convert P_camera to P_clipped
			*/
			float4 pointInClipCoordinates = math.mul(camProjMatrix, pointInCameraCoodinates);
 
			/*
			* 3 convert P_clipped to P_ndc
			* Normalized Device Coordinates
			*/
			float4 pointInNdc = pointInClipCoordinates / pointInClipCoordinates.w;
 
 
			/*
			* 4 convert P_ndc to P_screen
			*/
			float2 pointInScreenCoordinates;
			pointInScreenCoordinates.x = pixelWidth / 2.0f * (pointInNdc.x + 1);
			pointInScreenCoordinates.y = pixelHeight / 2.0f * (pointInNdc.y + 1);
 
 
			// return screencoordinates with canvas scale factor (if canvas coords required)
			return pointInScreenCoordinates / scaleFactor;
		}
		
		private static float4 ConvertWorldToCameraCoordinates(float3 point, float3 cameraPos, float3 camUp, float3 camRight, float3 camForward)
		{
			// translate the point by the negative camera-offset
			//and convert to Vector4
			float4 translatedPoint = new float4(point - cameraPos, 1f);
 
			// create transformation matrix
			float4x4 transformationMatrix = float4x4.identity;
			transformationMatrix.c0 = new float4(camRight.x, camUp.x, -camForward.x, 0);
			transformationMatrix.c1 = new float4(camRight.y, camUp.y, -camForward.y, 0);
			transformationMatrix.c2 = new float4(camRight.z, camUp.z, -camForward.z, 0);
 
			float4 transformedPoint = math.mul(transformationMatrix, translatedPoint);
 
			return transformedPoint;
		}
		
		#endregion
		
		#region Screen
		
		//Get the screen size of an object in pixels, given its distance and diameter.
		public static float DistanceAndDiameterToPixelSize(float distance, float diameter, UnityEngine.Camera camera){
       
			float pixelSize = (diameter * Mathf.Rad2Deg * Screen.height) / (distance * camera.fieldOfView);
			return pixelSize;
		}
 
		//Get the distance of an object, given its screen size in pixels and diameter.
		public static float PixelSizeAndDiameterToDistance(float pixelSize, float diameter, UnityEngine.Camera camera){
 
			float distance = (diameter * Mathf.Rad2Deg * Screen.height) / (pixelSize * camera.fieldOfView);
			return distance;
		}
 
		//Get the diameter of an object, given its screen size in pixels and distance.
		public static float PixelSizeAndDistanceToDiameter(float pixelSize, float distance, UnityEngine.Camera camera){
 
			float diameter = (pixelSize * distance * camera.fieldOfView) / (Mathf.Rad2Deg * Screen.height);
			return diameter;
		}
		
		#endregion
		
		#region Animation curves
		
		public static float[] GenerateCurveArray(this AnimationCurve self)
		{
			float[] returnArray = new float[256];
			for (int j = 0; j <= 255; j++)
			{
				returnArray[j] = self.Evaluate(j / 256f);            
			}              
			return returnArray;
		}
		
		#endregion
	}
}