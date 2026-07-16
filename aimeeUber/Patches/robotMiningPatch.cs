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
        private static readonly Dictionary<long, float> LastMountByReferenceId = new Dictionary<long, float>();
        private const float ImmediateDisconnectWindowSeconds = 0.25f;

        [HarmonyPrefix]
        public static bool attackWithPatch(RobotMining __instance, Attack attack, ref Thing.DelayedActionInstance __result, bool doAction = true)
        {
            if (attack.SourceItem is Wrench)
            {
                __result = new Thing.DelayedActionInstance
                {
                    Duration = 1f,
                    ActionMessage = ((__instance.ParentSlot != null) ? ActionStrings.Disconnect : ActionStrings.Connect)
                };

                if (!doAction)
                {
                    return false;
                }

                if (__instance.ParentSlot != null)
                {
                    if (__instance.ParentSlot.Parent is Rover)
                    {
                        if (LastMountByReferenceId.TryGetValue(__instance.ReferenceId, out float lastMountTime) && Time.time - lastMountTime <= ImmediateDisconnectWindowSeconds)
                        {
                            return false;
                        }

                        OnServer.MoveToWorld(__instance);
                        LastMountByReferenceId.Remove(__instance.ReferenceId);
                        return false;
                    }

                    return true;
                }

                if (Rover.IsNearby(__instance) is Rover rover)
                {
                    if (rover.Attach(__instance) && __instance.ParentSlot != null)
                    {
                        LastMountByReferenceId[__instance.ReferenceId] = Time.time;
                    }
                }

                return false;
            }
            return true;
        }
    }
}