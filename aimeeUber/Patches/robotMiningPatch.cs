using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Localization2;
using Assets.Scripts.Networking;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using Assets.Scripts.Vehicles;
using Assets.Scripts.Events;
using Assets.Scripts.Genetics;
using Assets.Scripts.Inventory;
using Assets.Scripts.Serialization;
using TerrainSystem;
using Trading;
using UnityEngine;
using Util;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;


namespace aimeeUberMod
{
    [HarmonyPatch(typeof(RobotMining))]
    [HarmonyPatch("AttackWith")]
    public class robotMiningPatch
    {
        [HarmonyPrefix]
        public static DelayedActionInstance attackWithPatch(RobotMining __instance, Attack attack, ref Thing.DelayedActionInstance __result, bool doAction = true)
        {
            if (attack.SourceItem != null && attack.SourceItem is Wrench && Rover.IsNearby(__instance) is Rover rover)
            {
                aimeeUberModPlugin.Log?.LogInfo("Patching aimee rover mount");

                Thing.DelayedActionInstance result = new Thing.DelayedActionInstance
                {
                    Duration = 1f,
                    ActionMessage = ((__instance.ParentSlot != null) ? ActionStrings.Disconnect : ActionStrings.Connect)
                };

                if (doAction)
                {
                    if (__instance.ParentSlot == null)
                    {
                        rover.Attach(__instance);
                    }
                    else
                    {
                        OnServer.MoveToWorld(__instance);
                    }
                }
                return result;
            }
            return result;
        }
    }
}