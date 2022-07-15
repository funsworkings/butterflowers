using System.Threading.Tasks;
using UnityEngine;

namespace live_simulation
{
    public static class GradientGenerator
    {
        public static (Texture2D, Gradient) RequestGradient(int width, int height, int steps, float hue_min = 0f, float hue_max = 1f, float sat_min = 0f, float sat_max = 1f, float val_min = 0f, float val_max = 1f)
        {
            TaskCompletionSource<Texture2D> _taskCompletionSource = new TaskCompletionSource<Texture2D>();
            Task<Texture2D> _task = _taskCompletionSource.Task;
            
            steps = Mathf.Clamp(steps, 2, 8); // Ensure valid steps
            width = Mathf.Max(1, width); // Ensure valid width
            height = Mathf.Max(1, height); // Ensure valid height

            Gradient _gradient = new Gradient();
            var _alphaKeys = new GradientAlphaKey[steps];
            var _colorKeys = new GradientColorKey[steps];
            _gradient.mode = GradientMode.Blend; // Ensure blend why would it be anything else really
            
            for (int i = 0; i < steps; i++)
            {
                Color col = Random.ColorHSV(hue_min, hue_max, sat_min, sat_max, val_min, val_max);
                float alpha = 1f;
                float interval = 1f * i / (steps - 1);

                _alphaKeys[i] = new GradientAlphaKey(alpha, interval);
                _colorKeys[i] = new GradientColorKey(col, interval);
            }
            _gradient.SetKeys(_colorKeys, _alphaKeys);

            Texture2D _texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            float ti = 0f;
            Color tCol = Color.black;

            for (int x = 0; x < width; x++)
            {
                ti = 1f * x / width;
                tCol = _gradient.Evaluate(ti);
                    
                for (int y = 0; y < height; y++)
                {
                    _texture.SetPixel(x, y, tCol);
                }
            }
            _texture.Apply();
            return (_texture, _gradient);
        }
    }
}