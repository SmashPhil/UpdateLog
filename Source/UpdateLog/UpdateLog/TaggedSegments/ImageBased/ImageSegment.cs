using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace UpdateLogTool
{
	public class ImageSegment : ContainerSegment
	{
		public override (string, string) Tags => ("<img", "</img>");

		public override void SegmentAction(Listing_Rich lister, string innerText)
		{
			string innerInnerText = GetInnerText(innerText);
			if (lister.CurrentLog is UpdateLog log && log.cachedTextures.TryGetValue(innerInnerText, out Texture2D tex))
			{
				(int width, _, _) = ContainerAttributes(innerText);
				int height = HeightOccupied(log, innerText);
				lister.DrawTexture(tex, width, height);
			}
			else
			{
				Log.ErrorOnce($"Failed to retrieve cached texture for {innerInnerText}.", innerInnerText.GetHashCode());
			}
		}
	}
}
