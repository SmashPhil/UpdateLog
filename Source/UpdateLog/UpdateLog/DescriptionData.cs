using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace UpdateLog
{
    public class DescriptionData
    {
        public DescriptionData()
        {
        }

        public DescriptionData(string text)
        {
            this.text = text;
        }

        public GameFont? font;
        public TextAnchor? anchor;

        public Texture2D texture;
        public string url;

        public string text;

        public string[] underlineText;

        public float HeightRequired
        {
            get
            {
                var prevFont = Text.Font;
                float height = 0;
                if (font != null)
                {
                    Text.Font = font.Value;
                }
                if (texture != null)
                {
                    height += Dialog_NewUpdate.PreviewImageHeight;
                }
                if (!text.NullOrEmpty())
                {
                    height += Text.CalcHeight(text, 600);
                }

                Text.Font = prevFont;
                return height;
            }
        }
    }
}
