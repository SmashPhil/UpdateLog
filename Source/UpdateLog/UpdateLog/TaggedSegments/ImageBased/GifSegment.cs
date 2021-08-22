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
			string innerInnerText = GetInnerText(innerText);
			if (lister.CurrentLog is UpdateLog log && log.cachedGifs.TryGetValue(innerInnerText, out List<Texture2D> textures))
			{
				(int width, _, Dictionary<string, object> lookup) = ContainerAttributes(innerText);
				int height = HeightOccupied(log, innerText);
				lister.DrawTexture(textures[CurrentFrame(textures.Count, (int)lookup["FPS"], (int)lookup["DELAY"])], width, height);
			}
			else
			{
				Log.ErrorOnce($"Failed to retrieve cached gif frame for {innerInnerText}.", innerInnerText.GetHashCode());
			}
		}
	}
}
