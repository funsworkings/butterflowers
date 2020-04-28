using System.Linq;
using System.IO;

using UnityEngine;


public static class Helpers
{


    #region Images

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
