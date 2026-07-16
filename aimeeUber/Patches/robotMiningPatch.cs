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


namespace aimeeUberMod {
    public static class RobotMiningMountGuards {
        internal static readonly Dictionary<long, float> LastMountByReferenceId = new Dictionary<long, float>();
        internal static readonly Dictionary<long, float> LastDetachByReferenceId = new Dictionary<long, float>();
        internal const float ImmediateDisconnectWindowSeconds = 0.25f;
        internal const float ReattachSuppressionWindowSeconds = 0.35f;

        internal static bool IsMountedOnRover(RobotMining robot) {
            if (robot == null) {
                return false;
            }

            return (robot.ParentSlot != null && robot.ParentSlot.Parent is Rover) || robot.RootParent is Rover;
        }
    }

    // patch Aimee's attackWith logic so that she can accept the wrench
    [HarmonyPatch(typeof(RobotMining))]
    [HarmonyPatch("AttackWith")]
    public static class robotMiningPatch {

        [HarmonyPrefix]
        public static bool attackWithPatch(RobotMining __instance, Attack attack, ref Thing.DelayedActionInstance __result, bool doAction = true) {
            if (attack.SourceItem is Wrench) {
                __result = new Thing.DelayedActionInstance
                {
                    Duration = 1f,
                    ActionMessage = ((__instance.ParentSlot != null) ? ActionStrings.Disconnect : ActionStrings.Connect)
                };

                if (!doAction) {
                    return false;
                }

                if (__instance.ParentSlot != null) {
                    bool isMountedOnRover = RobotMiningMountGuards.IsMountedOnRover(__instance);
                    if (isMountedOnRover) {
                        if (RobotMiningMountGuards.LastMountByReferenceId.TryGetValue(__instance.ReferenceId, out float lastMountTime) && Time.time - lastMountTime <= RobotMiningMountGuards.ImmediateDisconnectWindowSeconds) {
                            return false;
                        }

                        OnServer.MoveToWorld(__instance);
                        RobotMiningMountGuards.LastMountByReferenceId.Remove(__instance.ReferenceId);
                        if (__instance.ParentSlot == null) {
                            RobotMiningMountGuards.LastDetachByReferenceId[__instance.ReferenceId] = Time.time;
                        }
                        return false;
                    }

                    return true;
                }

                if (Rover.IsNearby(__instance) is Rover rover) {
                    if (RobotMiningMountGuards.LastDetachByReferenceId.TryGetValue(__instance.ReferenceId, out float lastDetachTime) && Time.time - lastDetachTime <= RobotMiningMountGuards.ReattachSuppressionWindowSeconds) {
                        return false;
                    }

                    if (rover.Attach(__instance) && __instance.ParentSlot != null) {
                        // Ensure robot code/movement is halted before mounted operation.
                        if (__instance.OnOff) {
                            OnServer.Interact(__instance.InteractOnOff, 0);
                        }

                        RobotMiningMountGuards.LastMountByReferenceId[__instance.ReferenceId] = Time.time;
                        RobotMiningMountGuards.LastDetachByReferenceId.Remove(__instance.ReferenceId);
                    }
                }

                return false;
            }
            return true;
        }
    }

    // patch Aimee's Execute logic so that she doesn't run her programmable chip to avoid transform desync.
    //Solves Issue 6
    [HarmonyPatch(typeof(RobotMining), "Execute")]
    public static class RobotMiningExecutePatch {
        [HarmonyPrefix]
        public static bool Prefix(RobotMining __instance) {
            // Block programmable chip execution while mounted to avoid transform desync.
            return !RobotMiningMountGuards.IsMountedOnRover(__instance);
        }
    }

    // Also patch her setLogicValue so external IC fails
    //Solves Issue 6
    [HarmonyPatch(typeof(RobotMining), "SetLogicValue")]
    public static class RobotMiningSetLogicValuePatch {
        [HarmonyPrefix]
        public static bool Prefix(RobotMining __instance) {
            // Prevent external IC/transmitter logic writes while mounted.
            return !RobotMiningMountGuards.IsMountedOnRover(__instance);
        }
    }

    // Patch her to be incapable of writing logic to prevent internal IC from writing
    //Solves Issue 6
    [HarmonyPatch(typeof(RobotMining), "CanLogicWrite")]
    public static class RobotMiningCanLogicWritePatch {
        [HarmonyPostfix]
        public static void Postfix(RobotMining __instance, ref bool __result) {
            if (RobotMiningMountGuards.IsMountedOnRover(__instance)) {
                __result = false;
            }
        }
    }
}