﻿using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using UnityEngine;

namespace MechEngineMod
{
    // methods that are used for inv -> mech and mech -> inv
    internal static class EnginePersistence
    {
        // auto strip engine when dismount 
        internal static void DismountWidgetOnAddItem(MechLabDismountWidget widget, MechLabPanel panel, IMechLabDraggableItem item)
        {
            StripEngine(panel, item);
        }

        // auto strip engine when put back to inventory
        internal static void InventoryWidgetOnAddItem(MechLabInventoryWidget widget, MechLabPanel panel,  IMechLabDraggableItem item)
        {
            if (StripEngine(panel, item))
            {
                widget.RefreshFilterToggles();
            }
        }

        internal static bool StripEngine(MechLabPanel panel, IMechLabDraggableItem item)
        {
            if (item.ItemType != MechLabDraggableItemType.MechComponentItem)
            {
                return false;
            }

            var componentRef = item.ComponentRef;

            var engineRef = componentRef.GetEngineRef();
            if (engineRef == null)
            {
                return false;
            }

            //Control.mod.Logger.LogDebug("MechLabInventoryWidget.OnAddItem " + componentRef.Def.Description.Id + " UID=" + componentRef.SimGameUID);

            foreach (var componentDefID in engineRef.GetInternalComponents())
            {
                //Control.mod.Logger.LogDebug("MechLabInventoryWidget.OnAddItem extracting componentDefID=" + componentDefID);
                var @ref = CreateMechComponentRef(componentDefID, panel.sim, panel.dataManager);

                var mechLabItemSlotElement = panel.CreateMechComponentItem(@ref, false, item.MountedLocation, item.DropParent);
                mechLabItemSlotElement.gameObject.transform.localScale = Vector3.one;
                panel.OnAddItem(mechLabItemSlotElement, false);
            }
            engineRef.ClearInternalComponents();

            SaveEngineState(engineRef, panel);

            return true;
        }

        internal static MechComponentRef CreateMechComponentRef(string id, SimGameState sim, DataManager dataManager)
        {
            var def = dataManager.GetObjectOfType<HeatSinkDef>(id, BattleTechResourceType.HeatSinkDef);

            var @ref = new MechComponentRef(def.Description.Id, sim.GenerateSimGameUID(), def.ComponentType, ChassisLocations.None);
            @ref.DataManager = dataManager;
            @ref.SetComponentDef(def);

            return @ref;
        }

        internal static void SaveEngineState(EngineRef engineRef, MechLabPanel panel)
        {
            var oldSimGameUID = engineRef.mechComponentRef.SimGameUID;
            var newSimGameUID = engineRef.GetNewSimGameUID();
            if (oldSimGameUID != newSimGameUID)
            {
                // Control.mod.Logger.LogDebug("saving new state of engine=" + engineRef.engineDef.Def.Description.Id + " old=" + oldSimGameUID + " new=" + newSimGameUID);

                engineRef.mechComponentRef.SetSimGameUID(newSimGameUID);
                // the only ones actually relying on SimGameUID are these work orders, so we have to change them
                // hopefully no previous or other working order lists are affected
                ChangeWorkOrderSimGameUID(panel, oldSimGameUID, newSimGameUID);
            }
        }

        private static void ChangeWorkOrderSimGameUID(MechLabPanel panel, string oldSimGameUID, string newSimGameUID)
        {
            if (!panel.IsSimGame || panel.baseWorkOrder == null)
            {
                return;
            }

            foreach (var entry in panel.baseWorkOrder.SubEntries)
            {
                var install = entry as WorkOrderEntry_InstallComponent;
                var repair = entry as WorkOrderEntry_RepairComponent;

                Traverse traverse;
                if (install != null)
                {
                    traverse = Traverse.Create(install);
                }
                else if (repair != null)
                {
                    traverse = Traverse.Create(repair);
                }
                else
                {
                    continue;
                }

                traverse = traverse.Property("ComponentSimGameUID");
                var componentSimGameUID = traverse.GetValue<string>();

                if (componentSimGameUID != oldSimGameUID)
                {
                    continue;
                }

                traverse.SetValue(newSimGameUID);
            }
        }

        internal static void OnAddItemStat(SimGameState sim, MechComponentRef mechComponentRef)
        {
            if (mechComponentRef == null)
            {
                return;
            }

            //if (mechComponentRef.DamageLevel == ComponentDamageLevel.Destroyed)
            //{
            //    return;
            //}

            var engineRef = mechComponentRef.GetEngineRef();
            if (engineRef == null)
            {
                return;
            }

            foreach (var componentDefID in engineRef.GetInternalComponents())
            {
                //Control.mod.Logger.LogDebug("SimGameState.OnAddItemStat extracting componentDefID=" + componentDefID);
                sim.AddItemStat(componentDefID, typeof(HeatSinkDef), false);
            }
        }
    }
}