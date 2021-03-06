﻿using System;
using BattleTech;
using Harmony;

namespace MechEngineMod
{
    [HarmonyPatch(typeof(SimGameState), "RespondToDefsLoadComplete")]
    public static class SimGameStateRespondToDefsLoadCompletePatch
    {
        public static void Prefix(SimGameState __instance)
        {
            try
            {
                MechDefMods.PostProcessAfterLoading(__instance.DataManager);
            }
            catch (Exception e)
            {
                Control.mod.Logger.LogError(e);
            }
        }
    }
}