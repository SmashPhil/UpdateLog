using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using UnityEngine.Networking;

namespace UpdateLogTool
{
	public class UpdateLog
	{
		public ModContentPack Mod { get; private set; }
		public UpdateLogData UpdateData { get; private set; }

		public string CurrentFolder { get; private set; }

		public Dictionary<string, Texture2D> cachedTextures;
		public Dictionary<string, List<Texture2D>> cachedGifs = new Dictionary<string, List<Texture2D>>();

		public UpdateLog (ModContentPack mod, string loadFolder, string path, bool updateVersion = true)
		{
			Mod = mod;
			CurrentFolder = loadFolder;
			UpdateData = FileReader.ParseUpdateData(path);
			if (updateVersion)
			{
				UpdateData.currentVersion = UpdateLogVersionFile(UpdateData.currentVersion);
			}
			UpdateData.rightIconBar ??= new List<UpdateLogData.HyperlinkedIcon>();
			UpdateData.leftIconBar ??= new List<UpdateLogData.HyperlinkedIcon>();
			UpdateData.images ??= new List<UpdateLogData.UploadedImages>();

			cachedTextures = new Dictionary<string, Texture2D>();
			if (Directory.Exists(FileReader.UpdateImagesDirectory(mod, loadFolder)))
			{
				foreach (var file in Directory.GetFiles(FileReader.UpdateImagesDirectory(mod, loadFolder)))
				{
					try
					{
						byte[] fileData = File.ReadAllBytes(file);
						Texture2D tex = new Texture2D(2, 2, TextureFormat.Alpha8, true);
						tex.LoadImage(fileData);
						tex.name = file.Split('\\').Last();
						cachedTextures.Add(tex.name, tex);
					}
					catch (Exception ex)
					{
						Log.Error($"Unable to load file {file} into Texture2D. Are you using an unsupported image type? Exception=\"{ex.Message}\"");
					}
				}
			}
			if (Directory.Exists(FileReader.UpdateGifDirectory(mod, loadFolder)))
			{
				string[] gifDirectories = Directory.GetDirectories(FileReader.UpdateGifDirectory(mod, loadFolder));
				foreach (string gif in gifDirectories)
				{
					string gifName = "Invalid Name";
					try
					{
						gifName = Path.GetFileName(gif);
						cachedGifs.Add(gifName, new List<Texture2D>());
						foreach (var file in Directory.GetFiles(gif))
						{
							byte[] fileData = File.ReadAllBytes(file);
							Texture2D tex = new Texture2D(2, 2, TextureFormat.Alpha8, true);
							tex.LoadImage(fileData);
							tex.name = file.Split('\\').Last();
							cachedGifs[gifName].Add(tex);
						}
					}
					catch (Exception ex)
					{
						Log.Error($"Unable to load gif {gifName}. Are you using an unsupported image type? Skipping gif contents. Exception=\"{ex.Message}\"");
					}
				}
			}
		}

		public UpdateLog(ModContentPack mod, string loadFolder, bool updateVersion = true)
		{
			Mod = mod;
			CurrentFolder = loadFolder;
			UpdateData = FileReader.ParseUpdateData(Path.Combine(FileReader.UpdateLogDirectory(Mod, CurrentFolder), FileReader.UpdateLogFileName));
			if (updateVersion)
			{
				UpdateData.currentVersion = UpdateLogVersionFile(UpdateData.currentVersion);
			}
			if (UpdateData.rightIconBar is null)
			{
				UpdateData.rightIconBar = new List<UpdateLogData.HyperlinkedIcon>();
			}
			if (UpdateData.leftIconBar is null)
			{
				UpdateData.leftIconBar = new List<UpdateLogData.HyperlinkedIcon>();
			}
			cachedTextures = new Dictionary<string, Texture2D>();
			if (Directory.Exists(FileReader.UpdateImagesDirectory(mod, loadFolder)))
			{
				foreach (var file in Directory.GetFiles(FileReader.UpdateImagesDirectory(mod, loadFolder)))
				{
					try
					{
						byte[] fileData = File.ReadAllBytes(file);
						Texture2D tex = new Texture2D(2, 2, TextureFormat.Alpha8, true);
						tex.LoadImage(fileData);
						tex.name = file.Split('\\').Last();
						cachedTextures.Add(tex.name, tex);
					}
					catch (Exception ex)
					{
						Log.Error($"Unable to load file {file} into Texture2D. Are you using an unsupported image type? Exception=\"{ex.Message}\"");
					}
				}
			}
			if (Directory.Exists(FileReader.UpdateGifDirectory(mod, loadFolder)))
			{
				string[] gifDirectories = Directory.GetDirectories(FileReader.UpdateGifDirectory(mod, loadFolder));
				foreach (string gif in gifDirectories)
				{
					string gifName = "Invalid Name";
					try
					{
						gifName = Path.GetFileName(gif);
						cachedGifs.Add(gifName, new List<Texture2D>());
						foreach (var file in Directory.GetFiles(gif))
						{
							byte[] fileData = File.ReadAllBytes(file);
							Texture2D tex = new Texture2D(2, 2, TextureFormat.Alpha8, true);
							tex.LoadImage(fileData);
							tex.name = file.Split('\\').Last();
							cachedGifs[gifName].Add(tex);
						}
					}
					catch (Exception ex)
					{
						Log.Error($"Unable to load gif {gifName}. Are you using an unsupported image type? Skipping gif contents. Exception=\"{ex.Message}\"");
					}
				}
			}
		}

		public string UpdateLogVersionFile(string xmlVersion)
		{
			if (File.Exists(Path.Combine(Mod.RootDir, "Version.txt")))
			{
				string assemblyVersion = File.ReadAllText(Path.Combine(Mod.RootDir, "Version.txt"));
				return assemblyVersion;
			}
			return xmlVersion;
		}

		public void NotifyModUpdated()
		{
			if (!UpdateData.testing)
			{
				UpdateData.update = false;
			}
			UpdateData.InvokeActionOnUpdate();
		}

		public void SaveUpdateStatus()
		{
			if (!Directory.Exists(FileReader.UpdateLogDirectory(Mod, CurrentFolder)))
			{
				Log.Error($"[{Mod.Name}] Unable to save UpdateLog info as directory {FileReader.UpdateLogDirectory(Mod, CurrentFolder)} does not exist.");
				return;
			}
			try
			{
				XDocument doc = new XDocument(new XElement("UpdateLog",
											  new XComment("Can utilize Version.txt file placed in mod's root directory"),
											  new XElement("currentVersion", UpdateData.currentVersion), 
											  new XComment(string.Join(",", Enum.GetNames(typeof(UpdateFor)))),
											  new XElement("updateOn", UpdateData.updateOn),
											  new XComment("Full description shown in update page"),
											  new XElement("description", UpdateData.description),
											  new XComment("Static parameterless method to execute when update log is executed"),
											  new XElement("actionOnUpdate", UpdateData.actionOnUpdate),
											  new XComment("Show update log on next startup."),
											  new XElement("update", UpdateData.update),
											  new XComment("Testing mode prevents the update from saving over the UpdateLog file"),
											  new XElement("testing", UpdateData.testing)));
				if (!UpdateData.rightIconBar.NullOrEmpty())
				{
					doc.Element("UpdateLog").Add(new XComment("Icon bar shown to the right of the mod's name"));
					doc.Element("UpdateLog").Add(new XElement("rightIconBar"));
					foreach (var item in UpdateData.rightIconBar)
					{
						doc.Element("UpdateLog").Element("rightIconBar").Add(new XElement("li",
																		new XElement("name", item.name),
																		new XElement("icon", item.icon),
																		new XElement("url", item.url)));
					}
				}
				if (!UpdateData.leftIconBar.NullOrEmpty())
				{
					doc.Element("UpdateLog").Add(new XComment("Icon bar shown to the left of the mod's name"));
					doc.Element("UpdateLog").Add(new XElement("leftIconBar"));
					foreach (var item in UpdateData.leftIconBar)
					{
						doc.Element("UpdateLog").Element("leftIconBar").Add(new XElement("li",
																		new XElement("name", item.name),
																		new XElement("icon", item.icon),
																		new XElement("url", item.url)));
					}
				}
				if (!UpdateData.images.NullOrEmpty())
				{
					doc.Element("UpdateLog").Add(new XComment("WIP - Do not use right now"));
					doc.Element("UpdateLog").Add(new XElement("images"));
					foreach (var item in UpdateData.images)
					{
						doc.Element("UpdateLog").Element("images").Add(new XElement("li", 
																	new XElement("name", item.name), 
																	new XElement("urls")));
						foreach (var url in item.urls)
						{
							doc.Element("UpdateLog").Element("images").Element("li").Element("urls").Add(new XElement("li",
																										new XElement(url)));
						}
					}
				}
				doc.Save(Path.Combine(FileReader.UpdateLogDirectory(Mod, CurrentFolder), FileReader.UpdateLogFileName));
			}
			catch (Exception ex)
			{
				Log.Error($"[UpdateLog] Unable to save UpdateLog config info. Exception=\"{ex.Message}\"");
			}
		}

		public class UpdateLogData
		{
			public string currentVersion;
			public UpdateFor updateOn = UpdateFor.GameInit;
			public string description;

			public string actionOnUpdate;

			public List<HyperlinkedIcon> rightIconBar;
			public List<HyperlinkedIcon> leftIconBar;

			public List<UploadedImages> images;

			public bool testing;
			public bool update;

			public void InvokeActionOnUpdate()
			{
				if (actionOnUpdate.NullOrEmpty())
				{
					return;
				}
				try
				{
					string[] methodFullName = actionOnUpdate.Split('.');
					Type classType = GenTypes.GetTypeInAnyAssembly(methodFullName[0] + "." + methodFullName[1], methodFullName[0]);
					classType.GetMethod(methodFullName[2]).Invoke(null, null);
				}
				catch (MissingMethodException)
				{
					Log.Warning($"Unable to assign method on update. Method could not be found: {actionOnUpdate}");
				}
			}

			public string EnhancedDescription
			{
				get
				{
					string enhancedDesc = description;
					enhancedDesc = enhancedDesc.Replace('[', '<');
					enhancedDesc = enhancedDesc.Replace(']', '>');
					return enhancedDesc;
				}
			}

			public struct HyperlinkedIcon
			{
				/// <summary>
				/// Name of link to be displayed upon highlight
				/// </summary>
				public string name;
				/// <summary>
				/// Texture file name relative to UpgradeLog/Images files. Only include name, not additional file path
				/// </summary>
				public string icon;
				/// <summary>
				/// URL to webpage that will be linked upon clicking the icon
				/// </summary>
				public string url;
			}

			public struct UploadedImages
			{
				public string name;
				/// <summary>
				/// url to directory containing image
				/// </summary>
				public List<string> urls;
			}
		}
	}
}
