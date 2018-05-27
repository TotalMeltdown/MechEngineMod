﻿using System;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;

namespace MechEngineMod
{
    [HarmonyPatch(typeof(MechLabInventoryWidget), "OnAddItem")]
    public static class MechLabInventoryWidgetCreateInventoryItemPatch
    {
        public static void Prefix(MechLabInventoryWidget __instance, DataManager ___dataManager, IMechLabDraggableItem item)
        {
            try
            {
                var panel = __instance.ParentDropTarget as MechLabPanel;
                if (panel == null)
                {
                    return;
                }
                if (panel.baseWorkOrder == null)
                {
                    return;
                }

                if (!panel.IsSimGame)
                {
                    return;
                }

                if (item == null)
                {
                    return;
                }

                EnginePersistence.OnAddItem(__instance, panel, ___dataManager, item);
            }
            catch (Exception e)
            {
                Control.mod.Logger.LogError(e);
            }
        }
    }
}