using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace UpdateLog
{
    public class Listing_Rich : Listing_Standard
    {
        public Listing_Rich()
        {
        }

        public Listing_Rich(GameFont font) : base(font)
        {
        }

        public void DrawTexture(Texture2D texture, float height)
        {
            float imageWidth = ((float)texture.width / texture.height) * height;
            NewColumnIfNeeded(height);
            Rect rect = GetRect(height);
            Rect imageRect = new Rect(rect)
            {
                x = (rect.width - imageWidth) / 2,
                height = height,
                width = imageWidth
            };
            GUI.DrawTexture(imageRect, texture);
        }

        public void Hyperlink(string name, string url)
        {
            var color = GUI.color;
            var textSize = Text.CalcSize(name);
            Rect rect = GetRect(textSize.y);
            rect.width = textSize.x;
            if (Mouse.IsOver(rect))
            {
                GUI.color = GenUI.MouseoverColor;
                Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height * 0.75f, textSize.x);
            }
            else
            {
                GUI.color = new Color(0.6f, .85f, 1);
            }
            Widgets.Label(rect, name);
            
            if (Widgets.ButtonInvisible(rect))
            {
                Application.OpenURL(url);
            }
            GUI.color = color;
        }
    }
}
