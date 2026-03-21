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
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("Awake")]
    public class robotMiningPatch
    {
        [HarmonyPatch("AttackWith")]
        [HarmonyPrefix]
        public static void roverPatch(DynamicThing __instance, Attack attack, bool doAction = true)
        {
            aimeeUberModPlugin.Log.LogInfo("Patching aimee rover mount");
            Rover rover = Rover.IsNearby(__instance) as Rover;
            if (attack.SourceItem is Wrench && rover != null && __instance is RobotMining)
            {
                //move aimee to world if she has a parentslot
                if (__instance.ParentSlot != null)
                {
                    OnServer.MoveToWorld(__instance);
                }
                //else she has no parent, attach her when using wrench
                else
                {
                    rover.Attach(__instance);
                }
            }
        }
    }
}