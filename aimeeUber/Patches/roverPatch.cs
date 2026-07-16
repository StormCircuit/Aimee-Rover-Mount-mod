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
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using Assets.Scripts.Vehicles;
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
    internal struct ColliderState
    {
        public Collider Collider;
        public bool Enabled;
        public bool IsTrigger;
    }

    [HarmonyPatch(typeof(Rover), "OnChildEnterInventory")]
    public static class RoverChildEnterInventoryPatch
    {
        private static readonly Dictionary<long, List<ColliderState>> AimeeColliderStatesByReferenceId = new Dictionary<long, List<ColliderState>>();
        private static readonly HashSet<long> OffsetAppliedByReferenceId = new HashSet<long>();

        internal static IReadOnlyList<ColliderState> GetStates(long referenceId)
        {
            if (AimeeColliderStatesByReferenceId.TryGetValue(referenceId, out List<ColliderState> states))
            {
                return states;
            }
            return null;
        }

        internal static void ClearStates(long referenceId)
        {
            AimeeColliderStatesByReferenceId.Remove(referenceId);
            OffsetAppliedByReferenceId.Remove(referenceId);
        }

        [HarmonyPostfix]
        public static void Postfix(DynamicThing newChild)
        {
            if (newChild is not RobotMining aimee)
            {
                return;
            }

            if (!OffsetAppliedByReferenceId.Contains(aimee.ReferenceId))
            {
                Transform transform = aimee.transform;
                transform.localPosition += new Vector3(0f, aimeeUberModPlugin.RoverMountOffsetY, 0f);
                OffsetAppliedByReferenceId.Add(aimee.ReferenceId);
            }

            List<ColliderState> states = new List<ColliderState>(aimee._selfColliders.Count);
            foreach (Collider selfCollider in aimee._selfColliders)
            {
                if (selfCollider == null)
                {
                    continue;
                }

                states.Add(new ColliderState
                {
                    Collider = selfCollider,
                    Enabled = selfCollider.enabled,
                    IsTrigger = selfCollider.isTrigger,
                });

                selfCollider.isTrigger = true;
                selfCollider.enabled = true;
            }

            AimeeColliderStatesByReferenceId[aimee.ReferenceId] = states;
        }
    }

    [HarmonyPatch(typeof(Rover), "OnChildExitInventory")]
    public static class RoverChildExitInventoryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(DynamicThing newChild)
        {
            if (newChild is not RobotMining aimee)
            {
                return;
            }

            IReadOnlyList<ColliderState> previousStates = RoverChildEnterInventoryPatch.GetStates(aimee.ReferenceId);
            if (previousStates == null)
            {
                return;
            }

            foreach (ColliderState state in previousStates)
            {
                if (state.Collider == null)
                {
                    continue;
                }

                state.Collider.isTrigger = state.IsTrigger;
                state.Collider.enabled = state.Enabled;
            }

            RoverChildEnterInventoryPatch.ClearStates(aimee.ReferenceId);
        }
    }

    [HarmonyPatch(typeof(Rover), "Attach")]
    public static class RoverAttachPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Rover __instance, Thing target, ref bool __result)
        {
            if (target is RobotMining)
            {
                for (int i = 0; i < __instance.ConnectionSlots.Length; i++)
                {
                    if (__instance.ConnectionSlots[i].TryAttachTank(target))
                    {
                        __result = true;
                        return false;
                    }
                }
                __result = false;
                return false;
            }
            return true;
        }
    }
}