using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using Settings;

namespace UIExt.Utilities {

    using Style = Palette.Style;

    [ExecuteInEditMode]
    public class ColorDriver : MonoBehaviour
    {
        public enum Type { Empty, Image, RawImage, Text, TMPText }
        Type type = Type.Empty;

        public Style style = Style.Default;
        public Style defaultStyle = Style.Default;
        bool defaulted = false;
        [Range(0, 1)]
        public float opacity = 1f;

        [SerializeField] Palette styling = null;

        Image image;
        RawImage rawImage;

        Text text;
        TMP_Text tMP_Text;


        void Awake()
        {
            image = GetComponent<Image>();
            if (image == null)
            {
                rawImage = GetComponent<RawImage>();
                if (rawImage == null)
                {
                    tMP_Text = GetComponent<TMP_Text>();
                    if (tMP_Text == null)
                    {
                        text = GetComponent<Text>();
                        if (text == null)
                            type = Type.Empty;
                        else
                            type = Type.Text;
                    }
                    else
                        type = Type.TMPText;
                }
                else
                    type = Type.RawImage;
            }
            else
                type = Type.Image;


            // if (Application.isPlaying)
            //{
            defaultStyle = style;        
            //}
        }

        // Update is called once per frame
        void Update()
        {
            UpdateColor();
        }

        public void UpdateColor()
        {
            if (type == Type.Empty || styling == null) return;

            Color color = styling.FetchColorFromSetting(style, opacity);

            if (type == Type.Image)
            {
                if (!Application.isPlaying)
                {
                    if (image == null)
                        image = GetComponent<Image>();
                }

                if (image == null)
                    return;

                image.color = color;
            }
            else if (type == Type.RawImage)
            {
                if (!Application.isPlaying)
                {
                    if (rawImage == null)
                        rawImage = GetComponent<RawImage>();
                }

                if (rawImage == null)
                    return;

                rawImage.material.color = color;
            }
            else if (type == Type.TMPText)
            {
                if (!Application.isPlaying)
                {
                    if (tMP_Text == null)
                        tMP_Text = GetComponent<TMP_Text>();
                }

                if (tMP_Text == null)
                    return;

                tMP_Text.color = color;
            }
            else if (type == Type.Text)
            {
                if (!Application.isPlaying)
                {
                    if (text == null)
                        text = GetComponent<Text>();
                }

                if (text == null)
                    return;

                text.color = color;
            }
        }

        public void SetColor(int _style)
        {
            style = (Settings.Palette.Style)_style;
        }

        public void ResetColor()
        {
            style = defaultStyle;
        }
    }

}
