﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace UpdateLog
{
    [StaticConstructorOnStartup]
    public static class UpdateHandler
    {
        public static readonly HashSet<UpdateLog> modUpdates = new HashSet<UpdateLog>();

        public static readonly HashSet<UpdateLog> updateList = new HashSet<UpdateLog>();

        static UpdateHandler()
        {
            var harmony = new Harmony("smashphil.updatelog");
            //Harmony.DEBUG = true;
            harmony.Patch(original: AccessTools.Method(typeof(UIRoot_Entry), nameof(UIRoot_Entry.Init)),
                postfix: new HarmonyMethod(typeof(UpdateHandler),
                nameof(UpdateOnStartup)));
            harmony.Patch(original: AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.FinalizeInit)),
                postfix: new HarmonyMethod(typeof(UpdateHandler),
                nameof(UpdateOnGameInit)));
            harmony.Patch(original: AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.StartedNewGame)),
                postfix: new HarmonyMethod(typeof(UpdateHandler),
                nameof(UpdateOnNewGame)));
            harmony.Patch(original: AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame)),
                postfix: new HarmonyMethod(typeof(UpdateHandler),
                nameof(UpdateOnLoadedGame)));

            modUpdates = new HashSet<UpdateLog>();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                UpdateLog log = mod.ReadFile();
                if (log != null)
                {
                    modUpdates.Add(log);
                }
            }

            SegmentParser.ParseAndCreateSegments();
            SegmentParser.GenerateRegexText();
        }

        public static void CheckUpdates(UpdateFor updating)
        {
            updateList.Clear();
            foreach (UpdateLog log in modUpdates)
            {
                if (log.UpdateData.updateOn == updating && log.UpdateData.update)
                {
                    log.NotifyModUpdated();
                    if (!log.UpdateData.description.NullOrEmpty())
                    {
                        updateList.Add(log);
                    }
                }
            }
            if (updateList.Any())
            {
                Find.WindowStack.Add(new Dialog_NewUpdate(updateList));
            }
        }

        public static void UpdateOnStartup()
        {
            CheckUpdates(UpdateFor.Startup);
        }

        public static void UpdateOnGameInit()
        {
            CheckUpdates(UpdateFor.GameInit);
        }

        public static void UpdateOnNewGame()
        {
            CheckUpdates(UpdateFor.NewGame);
        }

        public static void UpdateOnLoadedGame()
        {
            CheckUpdates(UpdateFor.LoadedGame);
        }
    }
}
