using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Verse;
using UnityEngine;

namespace UpdateLog
{
    public static class EnhancedText
    {
        public const string imageTag = "<img>";
        public const string imageEndTag = "</img>";

        public const string fontTag = "<font>";
        public const string fontEndTag = "</font>";

        public const string anchorTag = "<anchor>";
        public const string anchorEndTag = "</anchor>";

        public const string hyperlinkTag = "<link>";
        public const string hyperLinkEndTag = "</link>";

        public const string underlineTag = "<u>";
        public const string underlineEndTag = "</u>";

        private static bool TaggedSegment(string segment) => segment.Contains(imageTag) || 
                                                             segment.Contains(fontTag) || 
                                                             segment.Contains(anchorTag) || 
                                                             segment.Contains(hyperlinkTag);

        /// <summary>
        /// Parse UpdateLog's description into DescriptionData with RichText and custom brackets.
        /// </summary>
        /// <param name="log"></param>
        public static IEnumerable<DescriptionData> ParseDescriptionData(UpdateLog log)
        {
            string description = log.UpdateData.EnhancedDescription;

            string[] segments = Regex.Split(description, 
                @"(<img>.*?<\/img>)|(<font>.*?<\/font>)|(<anchor>.*?<\/anchor>)|(<link>.*?<\/link>\(.*?\))|(<link>.*?<\/link>)|(.+?(?=<font>|<anchor>|<link>|<img>|$))", 
                RegexOptions.Singleline, TimeSpan.FromSeconds(3));
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (segment.NullOrEmpty() || segment == Environment.NewLine)
                {
                    continue;
                }
                else if (!TaggedSegment(segment))
                {
                    string[] underlinedSegments = Regex.Matches(segment,
                        @"(<u>.*?<\/u>)",
                        RegexOptions.Singleline, TimeSpan.FromSeconds(1)).Cast<Match>().Select(m => m.Value)
                                                                         .Where(s => !string.IsNullOrWhiteSpace(s) && !s.NullOrEmpty() && s != Environment.NewLine).ToArray();
                    segment = segment.Replace(underlineTag, "");
                    segment = segment.Replace(underlineEndTag, "");
                    yield return new DescriptionData(segment)
                    {
                        underlineText = underlinedSegments
                    };
                }
                else
                {
                    if (segment.Contains(imageTag))
                    {
                        segment = segment.Replace(imageTag, "");
                        segment = segment.Replace(imageEndTag, "");
                        string filePath = Path.Combine(FileReader.UpdateImagesDirectory(log.Mod, log.CurrentFolder), segment);
                        if (File.Exists(filePath))
                        {
                            if (log.cachedTextures.TryGetValue(segment, out Texture2D tex))
                            {
                                yield return new DescriptionData(segment)
                                {
                                    texture = tex
                                };
                            }
                            else
                            {
                                Log.Error($"Failed to retrieve cached texture for {segment}");
                            }
                        }
                    }
                    else if (segment.Contains(fontTag))
                    {
                        segment = segment.Replace(fontTag, "");
                        segment = segment.Replace(fontEndTag, "");
                        yield return new DescriptionData()
                        {
                            font = (GameFont)Enum.Parse(typeof(GameFont), segment, true)
                        };
                    }
                    else if (segment.Contains(anchorTag))
                    {
                        segment = segment.Replace(anchorTag, "");
                        segment = segment.Replace(anchorEndTag, "");
                        yield return new DescriptionData()
                        {
                            anchor = (TextAnchor)Enum.Parse(typeof(TextAnchor), segment, true)
                        };
                    }
                    else if (segment.Contains(hyperlinkTag))
                    {
                        segment = segment.Replace(hyperlinkTag, "");
                        segment = segment.Replace("(", "");
                        segment = segment.Replace(")", "");
                        string[] splitSegment = segment.Split(new string[] { hyperLinkEndTag }, StringSplitOptions.RemoveEmptyEntries);
                        yield return new DescriptionData(segment)
                        {
                            url = splitSegment[0],
                            text = splitSegment.Length == 2 ? splitSegment[1] : splitSegment[0]
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Parse specific string of data. All images will be required to be located in Textures/ if using this method to parse out DescriptionData
        /// </summary>
        /// <param name="description"></param>
        public static IEnumerable<DescriptionData> ParseDescriptionData(string description)
        {
            description = description.Replace('[', '<');
            description = description.Replace(']', '>');

            string[] segments = Regex.Split(description, 
                @"(<img>.*?<\/img>)|(<font>.*?<\/font>)|(<anchor>.*?<\/anchor>)|(<link>.*?<\/link>\(.*?\))|(<link>.*?<\/link>)|(.+?(?=<font>|<anchor>|<link>|<img>|$))", 
                RegexOptions.Singleline, TimeSpan.FromSeconds(3));
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (segment.NullOrEmpty() || segment == Environment.NewLine)
                {
                    continue;
                }
                else if (!TaggedSegment(segment))
                {
                    string[] underlinedSegments = Regex.Matches(segment,
                        @"(<u>.*?<\/u>)",
                        RegexOptions.Singleline, TimeSpan.FromSeconds(1)).Cast<Match>().Select(m => m.Value)
                                                                         .Where(s => !string.IsNullOrWhiteSpace(s) && !s.NullOrEmpty() && s != Environment.NewLine).ToArray();
                    segment = segment.Replace(underlineTag, "");
                    segment = segment.Replace(underlineEndTag, "");
                    yield return new DescriptionData(segment)
                    {
                        underlineText = underlinedSegments
                    };
                }
                else
                {
                    if (segment.Contains(imageTag))
                    {
                        segment = segment.Replace(imageTag, "");
                        segment = segment.Replace(imageEndTag, "");
                        yield return new DescriptionData(segment)
                        {
                            texture = ContentFinder<Texture2D>.Get(segment)
                        };
                    }
                    else if (segment.Contains(fontTag))
                    {
                        segment = segment.Replace(fontTag, "");
                        segment = segment.Replace(fontEndTag, "");
                        yield return new DescriptionData()
                        {
                            font = (GameFont)Enum.Parse(typeof(GameFont), segment, true)
                        };
                    }
                    else if (segment.Contains(anchorTag))
                    {
                        segment = segment.Replace(anchorTag, "");
                        segment = segment.Replace(anchorEndTag, "");
                        yield return new DescriptionData()
                        {
                            anchor = (TextAnchor)Enum.Parse(typeof(TextAnchor), segment, true)
                        };
                    }
                    else if (segment.Contains(hyperlinkTag))
                    {
                        segment = segment.Replace(hyperlinkTag, "");
                        segment = segment.Replace("(", "");
                        segment = segment.Replace(")", "");
                        string[] splitSegment = segment.Split(new string[] { hyperLinkEndTag }, StringSplitOptions.RemoveEmptyEntries);
                        yield return new DescriptionData(segment)
                        {
                            url = splitSegment[0],
                            text = splitSegment.Length > 1 ? splitSegment[1] : splitSegment[0]
                        };
                    }
                }
            }
        }
    }
}
