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