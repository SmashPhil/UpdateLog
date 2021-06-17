using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace UpdateLogTool
{
	public class GifSegment : AnimatedSegment
	{
		public override (string, string) Tags => ("<gif", "</gif>");

		protected override int DefaultFramesPerSecond => 30;

		public override void SegmentAction(Listing_Rich lister, string innerText)
		{
			string[] innerTexts = innerText.Split('>');
			if (innerTexts.Length != 2)
			{
				Log.ErrorOnce($"Incorrect split for inner bracket text of gif {innerText}.", innerText.GetHashCode());
				return;
			}
			string bracketText = innerTexts[0];
			string innerInnerText = innerTexts[1];

			int width; int height; int fps; int delay;
			(width, height, fps, delay) = InnerProperties(bracketText);

			if (lister.CurrentLog is UpdateLog log && log.cachedGifs.TryGetValue(innerInnerText, out List<Texture2D> textures))
			{
				lister.DrawTexture(textures[CurrentFrame(textures.Count, fps, delay)], height);
			}
			else
			{
				Log.ErrorOnce($"Failed to retrieve cached gif frame for {innerInnerText}.", innerInnerText.GetHashCode());
			}
		}
	}
}
