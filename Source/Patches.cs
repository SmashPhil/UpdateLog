using HarmonyLib;
using SmashTools.Patching;
using SmashTools.Performance;
using Verse;

namespace UpdateLogTool;

internal class Patches : IPatchCategory
{
  private const int DelayUpdateLog = 500; // ms

  // Character Editor stalls for a few seconds on the main menu's first frame. Delay the update log for a small
  // amount so CE finishes its blocking operation first, otherwise it will pop up before the main menu draws and
  // then trigger CE to run when the update log is closed, causing people to think the update log is what's freezing
  // the main menu for a few seconds.
  private const string CharacterEditor = "void.charactereditor";

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch(original: AccessTools.Method(typeof(UIRoot_Entry), nameof(UIRoot_Entry.Init)),
      postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(UpdateOnStartup)),
        priority: Priority.Last));
    HarmonyPatcher.Patch(
      original: AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.FinalizeInit)),
      postfix: new HarmonyMethod(typeof(Patches),
        nameof(UpdateOnGameInit)));
    HarmonyPatcher.Patch(
      original: AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.StartedNewGame)),
      postfix: new HarmonyMethod(typeof(Patches),
        nameof(UpdateOnNewGame)));
    HarmonyPatcher.Patch(
      original: AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame)),
      postfix: new HarmonyMethod(typeof(Patches),
        nameof(UpdateOnLoadedGame)));
  }

  public static void UpdateOnStartup()
  {
    LongEventHandler.ExecuteWhenFinished(delegate
    {
      new Debouncer(() => UpdateHandler.CheckUpdates(UpdateFor.Startup), DelayUpdateLog).Invoke();
    });
  }

  public static void UpdateOnGameInit()
  {
    LongEventHandler.ExecuteWhenFinished(delegate
    {
      new Debouncer(() => UpdateHandler.CheckUpdates(UpdateFor.GameInit), DelayUpdateLog).Invoke();
    });
  }

  public static void UpdateOnNewGame()
  {
    LongEventHandler.ExecuteWhenFinished(delegate
    {
      new Debouncer(() => UpdateHandler.CheckUpdates(UpdateFor.NewGame), DelayUpdateLog).Invoke();
    });
  }

  public static void UpdateOnLoadedGame()
  {
    LongEventHandler.ExecuteWhenFinished(delegate
    {
      new Debouncer(() => UpdateHandler.CheckUpdates(UpdateFor.LoadedGame), DelayUpdateLog).Invoke();
    });
  }
}