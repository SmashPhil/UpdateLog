using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace UpdateLogTool;

[StaticConstructorOnStartup]
public static class UpdateHandler
{
  public static readonly HashSet<UpdateLog> modUpdates = [];

  public static readonly HashSet<UpdateLog> updateList = [];

  static UpdateHandler()
  {
    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
    {
      UpdateLog log = FileReader.LoadUpdateLog(mod);
      if (log != null)
      {
        modUpdates.Add(log);
      }
    }
    SegmentParser.ParseAndCreateSegments();
    SegmentParser.GenerateRegexText();
  }

  public static UpdateLog UpdateLogData(ModContentPack mod)
  {
    return modUpdates.FirstOrDefault(m => m.Mod == mod);
  }

  public static void CheckUpdates(UpdateFor updating)
  {
    updateList.Clear();
    foreach (UpdateLog log in modUpdates)
    {
      if (log.UpdateData.updateOn == updating)
      {
        if (log.UpdateData.update)
        {
          log.NotifyModUpdated();
          if (!log.UpdateData.description.NullOrEmpty())
          {
            updateList.Add(log);
          }
        }
        log.SaveUpdateStatus();
      }
    }
    if (updateList.Any())
    {
      Find.WindowStack.Add(new Dialog_NewUpdate(updateList));
    }
  }
}