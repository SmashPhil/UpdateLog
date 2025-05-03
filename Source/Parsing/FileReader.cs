using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using UnityEngine.Networking;

namespace UpdateLogTool
{
  public static class FileReader
  {
    public const string UpdateLogFolder = "Updates";
    public const string UpdateLogOldFolder = "Previous";
    public const string UpdateLogFileName = "UpdateLog.xml";
    public const string UpdateLogImageFolder = "Images";
    public const string UpdateLogGifFolder = "Gifs";

    public static string UpdateLogDirectory(ModContentPack mod, string folderName) =>
      Path.Combine(mod.RootDir, folderName, UpdateLogFolder);

    public static string UpdateLogOldDirectory(ModContentPack mod, string folderName) =>
      Path.Combine(mod.RootDir, folderName, UpdateLogFolder, UpdateLogOldFolder);

    public static string UpdateImagesDirectory(ModContentPack mod, string folderName) =>
      Path.Combine(mod.RootDir, folderName, UpdateLogFolder, UpdateLogImageFolder);

    public static string UpdateImagesDirectory(UpdateLog log) =>
      UpdateImagesDirectory(log.Mod, log.CurrentFolder);

    public static string UpdateGifDirectory(ModContentPack mod, string folderName) =>
      Path.Combine(mod.RootDir, folderName, UpdateLogFolder, UpdateLogGifFolder);

    public static string UpdateGifDirectory(UpdateLog log) =>
      UpdateImagesDirectory(log.Mod, log.CurrentFolder);

    public static UpdateLog LoadUpdateLog(ModContentPack mod)
    {
      try
      {
        var loadFolders = ModFoldersForVersion(mod);
        if (!loadFolders.NullOrEmpty())
        {
          foreach (string folder in loadFolders)
          {
            if (File.Exists(Path.Combine(UpdateLogDirectory(mod, folder), UpdateLogFileName)))
            {
              return new UpdateLog(mod, folder);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(
          $"Exception thrown while attempting to read in UpdateLog data for {mod.Name}.\nException=\"{ex}\"");
      }
      return null;
    }

    public static List<UpdateLog> ReadPreviousFiles(this ModContentPack mod)
    {
      List<UpdateLog> updates = new List<UpdateLog>();
      try
      {
        var loadFolders = ModFoldersForVersion(mod);
        if (!loadFolders.NullOrEmpty())
        {
          foreach (string folder in loadFolders)
          {
            if (Directory.Exists(UpdateLogDirectory(mod, folder)))
            {
              if (File.Exists(Path.Combine(UpdateLogDirectory(mod, folder), UpdateLogFileName)))
              {
                updates.Add(new UpdateLog(mod, folder));
              }
              if (Directory.Exists(UpdateLogOldDirectory(mod, folder)))
              {
                foreach (string filePath in Directory.EnumerateFiles(
                  UpdateLogOldDirectory(mod, folder), "*.xml"))
                {
                  if (File.Exists(filePath))
                  {
                    updates.Add(new UpdateLog(mod, folder, filePath, false));
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(
          $"Exception thrown while attempting to read in UpdateLog data for {mod.Name}.\nException=\"{ex}\"");
      }
      return updates;
    }

    public static List<string> ModFoldersForVersion(ModContentPack mod)
    {
      ModMetaData metaData = ModLister.GetModWithIdentifier(mod.PackageId);
      List<LoadFolder> loadFolders = new List<LoadFolder>();
      if ((metaData?.loadFolders) != null && metaData.loadFolders.DefinedVersions().Count > 0)
      {
        loadFolders =
          metaData.LoadFoldersForVersion(VersionControl.CurrentVersionStringWithoutBuild);
        if (!loadFolders.NullOrEmpty())
        {
          return loadFolders.Select(lf => lf.folderName).ToList();
        }
      }

      loadFolders = new List<LoadFolder>();

      int num = VersionControl.CurrentVersion.Major;
      int num2 = VersionControl.CurrentVersion.Minor;
      do
      {
        if (num2 == 0)
        {
          num--;
          num2 = 9;
        }
        else
        {
          num2--;
        }
        if (num < 1)
        {
          loadFolders = metaData.LoadFoldersForVersion("default");
          if (loadFolders != null)
          {
            return loadFolders.Select(lf => lf.folderName).ToList();
          }
          return DefaultFoldersForVersion(mod).ToList();
        }
        loadFolders = metaData.LoadFoldersForVersion(num + "." + num2);
      } while (loadFolders.NullOrEmpty());
      return loadFolders.Select(lf => lf.folderName).ToList();
    }

    public static IEnumerable<string> DefaultFoldersForVersion(ModContentPack mod)
    {
      ModMetaData metaData = ModLister.GetModWithIdentifier(mod.PackageId);

      string rootDir = mod.RootDir;
      string text = Path.Combine(rootDir, VersionControl.CurrentVersionStringWithoutBuild);
      if (Directory.Exists(text))
      {
        yield return text;
      }
      else
      {
        Version version = new Version(0, 0);
        DirectoryInfo[] directories = metaData.RootDir.GetDirectories();
        for (int i = 0; i < directories.Length; i++)
        {
          if (VersionControl.TryParseVersionString(directories[i].Name, out Version version2) &&
            version2 > version)
          {
            version = version2;
          }
        }
        if (version.Major > 0)
        {
          yield return Path.Combine(rootDir, version.ToString());
        }
      }
      string text2 = Path.Combine(rootDir, ModContentPack.CommonFolderName);
      if (Directory.Exists(text2))
      {
        yield return text2;
      }
      yield return rootDir;
    }

    /// <summary>
    /// Manually parsing UpdateLog.UpdateLogData due to issue with ObjectFromXml<T> parsing lists in direct DocumentElement object
    /// </summary>
    /// <param name="filePath"></param>
    public static UpdateLog.UpdateLogData ParseUpdateData(string filePath)
    {
      string xmlContent = File.ReadAllText(filePath);
      UpdateLog.UpdateLogData data = new UpdateLog.UpdateLogData();
      try
      {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlContent);
        foreach (XmlNode node in xmlDocument.DocumentElement.ChildNodes)
        {
          switch (node.Name)
          {
            case "currentVersion":
              data.currentVersion = node.InnerText;
              break;
            case "updateOn":
              data.updateOn = (UpdateFor)Enum.Parse(typeof(UpdateFor), node.InnerText);
              break;
            case "description":
              data.description = node.InnerText;
              break;
            case "rightIconBar":
              data.rightIconBar = ListFromXml(node);
              break;
            case "leftIconBar":
              data.leftIconBar = ListFromXml(node);
              break;
            case "actionOnUpdate":
              data.actionOnUpdate = node.InnerText;
              break;
            case "images":
              data.images = ImageListFromXml(node);
              break;
            case "testing":
            {
              data.testing = bool.TryParse(node.InnerText, out bool result) && result;
            }
              break;
            case "update":
            {
              data.update = bool.TryParse(node.InnerText, out bool result) && result;
            }
              break;
            case "#comment":
              continue;
            default:
              Log.Error($"Failed to find {node.Name} in manual parsing.");
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(
          $"Exception loading file at {filePath}. Loading defaults instead. Exception={ex}");
      }
      return data;
    }

    private static List<UpdateLog.UpdateLogData.HyperlinkedIcon> ListFromXml(XmlNode listRootNode)
    {
      List<UpdateLog.UpdateLogData.HyperlinkedIcon> list =
        new List<UpdateLog.UpdateLogData.HyperlinkedIcon>();
      try
      {
        foreach (XmlNode xmlNode in listRootNode.ChildNodes)
        {
          try
          {
            list.Add(
              DirectXmlToObject.ObjectFromXml<UpdateLog.UpdateLogData.HyperlinkedIcon>(xmlNode,
                true));
          }
          catch (Exception ex)
          {
            Log.Error(
              $"Exception loading list element from XML. Ex={ex}\nXml={listRootNode.OuterXml}");
          }
        }
      }
      catch (Exception ex2)
      {
        Log.Error(
          $"Exception loading list element from XML. Ex={ex2.Message}\nXml={listRootNode.OuterXml}");
      }
      return list;
    }

    private static List<UpdateLog.UpdateLogData.UploadedImages> ImageListFromXml(
      XmlNode listRootNode)
    {
      List<UpdateLog.UpdateLogData.UploadedImages> list =
        new List<UpdateLog.UpdateLogData.UploadedImages>();
      try
      {
        foreach (XmlNode xmlNode in listRootNode.ChildNodes)
        {
          try
          {
            list.Add(
              DirectXmlToObject
               .ObjectFromXml<UpdateLog.UpdateLogData.UploadedImages>(xmlNode, true));
          }
          catch (Exception ex)
          {
            Log.Error(
              $"Exception loading list element from XML. Ex={ex}\nXml={listRootNode.OuterXml}");
          }
        }
      }
      catch (Exception ex2)
      {
        Log.Error(
          $"Exception loading list element from XML. Ex={ex2}\nXml={listRootNode.OuterXml}");
      }
      return list;
    }

    public static async Task<Texture2D> GetTextureFromURL(string url)
    {
      using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
      {
        UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();

        while (!operation.isDone)
        {
          await Task.Delay(33);
        }

        if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
          webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
          return null;
        }
        return DownloadHandlerTexture.GetContent(webRequest);
      }
    }
  }
}