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

    internal struct MountedAimeeState
    {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public List<ColliderState> ColliderStates;
    }

    [HarmonyPatch(typeof(Rover), "OnChildEnterInventory")]
    public static class RoverChildEnterInventoryPatch
    {
        private static readonly Dictionary<long, MountedAimeeState> MountedAimeeStatesByReferenceId = new Dictionary<long, MountedAimeeState>();

        internal static bool TryGetState(long referenceId, out MountedAimeeState state)
        {
            return MountedAimeeStatesByReferenceId.TryGetValue(referenceId, out state);
        }

        internal static void ClearStates(long referenceId)
        {
            MountedAimeeStatesByReferenceId.Remove(referenceId);
        }

        [HarmonyPostfix]
        public static void Postfix(Rover __instance, DynamicThing newChild)
        {
            if (newChild is not RobotMining aimee)
            {
                return;
            }

            if (!MountedAimeeStatesByReferenceId.ContainsKey(aimee.ReferenceId))
            {
                Transform transform = aimee.transform;
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
                }

                MountedAimeeStatesByReferenceId[aimee.ReferenceId] = new MountedAimeeState
                {
                    LocalPosition = transform.localPosition,
                    LocalRotation = transform.localRotation,
                    ColliderStates = states,
                };
            }

            Transform aimeeTransform = aimee.transform;
            if (TryGetState(aimee.ReferenceId, out MountedAimeeState mountedState))
            {
                aimeeTransform.localPosition = mountedState.LocalPosition + new Vector3(0f, aimeeUberModPlugin.RoverMountOffsetY, 0f);

                Transform roverTransform = __instance != null ? __instance.transform : null;
                Transform parentTransform = aimeeTransform.parent;
                if (roverTransform != null && parentTransform != null)
                {
                    Vector3 upward = roverTransform.up;
                    Vector3 localToRover = roverTransform.InverseTransformPoint(aimeeTransform.position);
                    float sideSign = Mathf.Sign(localToRover.x);
                    if (Mathf.Abs(sideSign) < 0.5f)
                    {
                        sideSign = Vector3.Dot(aimeeTransform.position - roverTransform.position, roverTransform.right) >= 0f ? 1f : -1f;
                    }

                    Vector3 inward = sideSign > 0f ? -roverTransform.right : roverTransform.right;
                    inward = Vector3.ProjectOnPlane(inward, upward);

                    if (inward.sqrMagnitude > 0.0001f)
                    {
                        inward.Normalize();
                        Quaternion targetWorldRotation = Quaternion.LookRotation(upward, -inward);

                        float pitchTrimDegrees = aimeeUberModPlugin.RoverMountPitchTrimDegrees;
                        if (Mathf.Abs(pitchTrimDegrees) > 0.0001f)
                        {
                            Vector3 pitchAxis = targetWorldRotation * Vector3.right;
                            targetWorldRotation = Quaternion.AngleAxis(pitchTrimDegrees, pitchAxis) * targetWorldRotation;
                        }

                        aimeeTransform.localRotation = Quaternion.Inverse(parentTransform.rotation) * targetWorldRotation;
                    }
                    else
                    {
                        aimeeTransform.localRotation = mountedState.LocalRotation;
                    }
                }
                else
                {
                    aimeeTransform.localRotation = mountedState.LocalRotation;
                }
            }

            foreach (Collider selfCollider in aimee._selfColliders)
            {
                if (selfCollider == null)
                {
                    continue;
                }

                selfCollider.isTrigger = false;
                selfCollider.enabled = true;
            }
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

            if (!RoverChildEnterInventoryPatch.TryGetState(aimee.ReferenceId, out MountedAimeeState previousState))
            {
                return;
            }

            foreach (ColliderState state in previousState.ColliderStates)
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