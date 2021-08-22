using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace UpdateLogTool
{
	public abstract class ContainerSegment : TaggedSegment
	{
		protected string GetInnerText(string fullText)
		{
			string[] innerTexts = fullText.Split('>');
			string innerText = innerTexts.Last();
			return innerText;
		}

		public override int HeightOccupied(UpdateLog log, string fullText)
		{
			string[] fullTexts = fullText.Split('>');
			if (fullTexts.Length < 1)
			{
				Log.ErrorOnce($"Incorrect split for bracketText in {fullText}.", fullText.GetHashCode());
				return Mathf.FloorToInt(Dialog_NewUpdate.PreviewImageHeight);
			}
			string innerText = fullTexts.Last();
			int width = Mathf.FloorToInt(Dialog_NewUpdate.DialogWidth);
			int height = width;
			if (fullTexts.Length > 1 && !fullTexts.FirstOrDefault().NullOrEmpty())
			{
				string bracketText = fullTexts[0];
				innerText = fullTexts.Last();
				(int widthOut, int heightOut, _) = ContainerAttributes(bracketText);
				width = widthOut;
				height = height == heightOut ? width : heightOut;
			}
			string innerInnerText = GetInnerText(innerText);
			if (log.cachedTextures.TryGetValue(innerInnerText, out Texture2D texture) && height == width)
			{
				height = Mathf.CeilToInt(texture.height / (float)texture.width * width);
			}
			return height;
		}

		protected virtual (string name, string value) ParseAttribute(string fullAttribute)
		{
			string[] propertyData = fullAttribute.Split('=');
			string name = propertyData[0];
			string value = propertyData[1];
			return (name, value);
		}

		protected virtual Dictionary<string, object> HandleCustomAttribute(string name, string value)
		{
			Log.ErrorOnce($"Attribute: {name} is not valid for {GetType()} segment.", name.GetHashCode());
			return new Dictionary<string, object>();
		}

		protected (int width, int height, Dictionary<string, object> customValues) ContainerAttributes(string bracketText)
		{
			int width = Mathf.FloorToInt(Dialog_NewUpdate.DialogWidth);
			int height = width;
			Dictionary<string, object> customValues = new Dictionary<string, object>();
			string step = "Splitting bracketText";
			try
			{
				string[] attributes = bracketText.Split('>');
				if (attributes.Length > 0)
				{
					string[] properties = attributes[0].Split(' ');
					foreach (string attribute in properties.Where(s => !string.IsNullOrWhiteSpace(s)))
					{
						step = attribute;
						(string name, string value) = ParseAttribute(attribute);
						value = value.Trim('\"');
						if (name.ToUpperInvariant() == "HEIGHT")
						{
							int.TryParse(value, out height);
						}
						else if (name.ToUpperInvariant() == "WIDTH")
						{
							int.TryParse(value, out width);
						}
						else
						{
							customValues.AddRange(HandleCustomAttribute(name, value));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorOnce($"Exception thrown grabbing inner properties {bracketText}\nFailed: {step}\nException={ex.Message}", bracketText.GetHashCode());
			}

			return (width, height, customValues);
		}
	}
}
