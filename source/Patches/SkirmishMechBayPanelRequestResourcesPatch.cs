﻿using System;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace MechEngineMod
{
    [HarmonyPatch(typeof(SkirmishMechBayPanel), "RequestResources")]
    public static class SkirmishMechBayPanelRequestResourcesPatch
    {
        public static void Prefix(SkirmishMechBayPanel __instance)
        {
            try
            {
                __instance.dataManager.RequestAllResourcesOfType(BattleTechResourceType.HeatSinkDef);
                __instance.dataManager.RequestAllResourcesOfType(BattleTechResourceType.UpgradeDef);
                __instance.dataManager.RequestAllResourcesOfType(BattleTechResourceType.WeaponDef);
                __instance.dataManager.RequestAllResourcesOfType(BattleTechResourceType.AmmunitionBoxDef);
            }
            catch (Exception e)
            {
                Control.mod.Logger.LogError(e);
            }
        }
    }
}