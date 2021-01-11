using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;
using RimWorld;
using UnityEngine;

namespace UpdateLog
{
    public class Dialog_NewUpdate : Window
    {
        public const float PreviewImageHeight = 200;

        public const float PaginationButtonHeight = 15;

        public const int BarIconSize = 25;

        private readonly UpdateLog[] logs;
        private int selectedLogIndex;

        private Vector2 scrollPosition = new Vector2(0, 0);
        private float cachedViewHeight;
        private bool cachedHeightDirty;

        /* Currently Displayed */
        private ModContentPack mod;
        private ModMetaData metaData;
        private UpdateLog log;
        /* ------------------- */

        private List<DescriptionData> segments = new List<DescriptionData>();

        private List<Tuple<string, string, Texture2D>> cachedIconBar = new List<Tuple<string, string, Texture2D>>();

        public Dialog_NewUpdate(HashSet<UpdateLog> logs)
        {
            if (logs.EnumerableNullOrEmpty())
            {
                Close();
                return;
            }
            this.logs = logs.ToArray();
            selectedLogIndex = 0;
            CurrentLog = logs.FirstOrDefault();

            forcePause = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(600, 740);

        public UpdateLog CurrentLog
        {
            get
            {
                return log;
            }
            set
            {
                if (log == value)
                {
                    return;
                }
                log = value;
                mod = log.Mod;
                metaData = ModLister.GetModWithIdentifier(mod.PackageId);
                segments = EnhancedText.ParseDescriptionData(log).ToList();
                RecacheHyperlinks();
                cachedHeightDirty = true;
            }
        }

        public void RecacheHyperlinks()
        {
            cachedIconBar.Clear();
            if (CurrentLog.UpdateData.iconBar.NullOrEmpty())
            {
                return;
            }
            foreach (var iconObj in CurrentLog.UpdateData.iconBar)
            {
                if (CurrentLog.cachedTextures.TryGetValue(iconObj.icon, out Texture2D texture))
                {
                    cachedIconBar.Add(new Tuple<string, string, Texture2D>(iconObj.name, iconObj.url, texture));
                }
                else
                {
                    Log.ErrorOnce($"Unable to retrieve cached texture for icon: {iconObj.icon}", iconObj.icon.GetHashCode());
                }
            }
        }

        public void RecacheHeight()
        {
            float height = 0;
            foreach (DescriptionData data in segments)
            {
                height += data.HeightRequired;
            }
            cachedViewHeight = height - 10;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (cachedHeightDirty)
            {
                RecacheHeight();
            }
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            var font = Text.Font;
            Text.Font = GameFont.Medium;
            var color = GUI.color;

            Texture2D previewImage = metaData.PreviewImage;

            float pWidth = previewImage?.width ?? 0;
            float pHeight = previewImage?.height ?? 0;
            float imageWidth = ((float)pWidth / pHeight) * PreviewImageHeight;
            Rect previewRect = new Rect(inRect)
            {
                x = (inRect.width - imageWidth) / 2,
                height = PreviewImageHeight,
                width = imageWidth
            };
            if (previewImage != null)
            {
                GUI.DrawTexture(previewRect, previewImage);
            }

            Rect modLabelRect = new Rect(inRect)
            {
                y = previewRect.y + previewRect.height + 5,
                height = Text.CalcHeight(mod.Name, inRect.width)
            };
            Widgets.Label(modLabelRect, mod.Name);

            Widgets.DrawLineHorizontal(0, modLabelRect.y + modLabelRect.height, modLabelRect.width);

            Rect iconBarRect = new Rect(inRect.width - BarIconSize, modLabelRect.y, BarIconSize, BarIconSize);
            foreach (var barIcon in cachedIconBar)
            {
                if (Mouse.IsOver(iconBarRect))
                {
                    GUI.color = GenUI.MouseoverColor;
                    if (!barIcon.Item1.NullOrEmpty())
                    {
                        TooltipHandler.TipRegion(iconBarRect, barIcon.Item1);
                    }
                }
                if (Widgets.ButtonInvisible(iconBarRect))
                {
                    Application.OpenURL(barIcon.Item2);
                }
                Widgets.DrawTextureFitted(iconBarRect, barIcon.Item3, 1);
                iconBarRect.x -= BarIconSize + 10;
                GUI.color = color;
            }

            float descY = modLabelRect.y + modLabelRect.height + 5;

            Rect lowerRect = new Rect(inRect.x, descY, inRect.width, inRect.height - descY);

            Listing_Rich lister = new Listing_Rich();

            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - 16, cachedViewHeight);

            lister.BeginScrollView(lowerRect, ref scrollPosition, ref viewRect);
            
            foreach (DescriptionData segment in segments)
            {
                if (segment.font is GameFont segFont)
                {
                    Text.Font = segFont;
                }
                if (segment.anchor is TextAnchor segAnchor)
                {
                    Text.Anchor = segAnchor;
                }

                if (segment.texture != null)
                {
                    lister.DrawTexture(segment.texture, 100);
                }
                else if (!segment.url.NullOrEmpty())
                {
                    lister.Hyperlink(segment.text, segment.url);
                }
                else if (!segment.text.NullOrEmpty())
                {
                    Rect label = lister.Label(segment.text);
                    //WIP
                    if (!segment.underlineText.NullOrEmpty())
                    {
                        Widgets.DrawLineHorizontal(label.x, label.y + label.height, label.width);
                        //string[] segmentsSplitByUnderline = Regex.Split(segment.text, @"(<u>.*?<\/u>)", RegexOptions.Singleline, TimeSpan.FromSeconds(1));
                        //foreach (string uSegment in segmentsSplitByUnderline)
                        //{
                        //    float x = Text.CalcSize(uSegment).x;
                        //    Widgets.DrawLineHorizontal(label.x, label.y, width);
                        //}
                    }
                }
            }

            lister.EndScrollView(ref viewRect);

            Rect bottomButtonsRect = new Rect(0, inRect.height - PaginationButtonHeight * 2, PaginationButtonHeight * 2, PaginationButtonHeight);
            if (selectedLogIndex > 0 && Widgets.ButtonText(bottomButtonsRect, "<"))
            {
                selectedLogIndex--;
                CurrentLog = logs[selectedLogIndex];
            }
            bottomButtonsRect.x = inRect.width - PaginationButtonHeight * 2;
            if (selectedLogIndex < (logs.Length - 1) && Widgets.ButtonText(bottomButtonsRect, ">"))
            {
                selectedLogIndex++;
                CurrentLog = logs[selectedLogIndex];
            }
            Text.Anchor = anchor;
            Text.Font = font;
            GUI.color = color;
        }
    }
}
