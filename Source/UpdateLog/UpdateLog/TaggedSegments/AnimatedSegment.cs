using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace UpdateLogTool
{
	public abstract class AnimatedSegment : TaggedSegment
	{
		protected abstract int DefaultFramesPerSecond { get; }

		protected virtual int CurrentFrame(int maxFrames, int fps) => Mathf.FloorToInt(Time.time * fps) % maxFrames;

		public override int HeightOccupied(string innerText)
		{
			string[] innerTexts = innerText.Split('>');
			if (innerTexts.Length != 2)
			{
				Log.ErrorOnce($"Incorrect split for inner bracket text of gif {innerText}.", innerText.GetHashCode());
				return Mathf.FloorToInt(Dialog_NewUpdate.PreviewImageHeight);
			}
			string bracketText = innerTexts[0];

			(_, int height, _) = InnerProperties(bracketText);
			return height;
		}

		protected virtual (int width, int height, int fps) InnerProperties(string bracketText)
		{
			int width = Mathf.FloorToInt(Dialog_NewUpdate.PreviewImageHeight);
			int height = width;
			int fps = DefaultFramesPerSecond;
			string step = "Splitting bracketText";
			try
			{
				string[] properties = bracketText.Split(' ', '>');
				foreach (string property in properties.Where(s => !string.IsNullOrWhiteSpace(s)))
				{
					step = property;
					string[] propertyData = property.Split('=');
					string name = propertyData[0];
					string value = propertyData[1];
					if (name.ToUpperInvariant() == "HEIGHT")
					{
						int.TryParse(value, out height);
					}
					else if (name.ToUpperInvariant() == "WIDTH")
					{
						int.TryParse(value, out width);
					}
					else if (name.ToUpperInvariant() == "FPS")
					{
						int.TryParse(value, out fps);
					}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorOnce($"Exception thrown grabbing inner properties {bracketText}\nFailed: {step}\nException={ex.Message}", bracketText.GetHashCode());
			}

			return (width, height, fps);
		}
	}
}
