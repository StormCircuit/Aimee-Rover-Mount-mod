using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace aimeeUberMod
{
    [HarmonyPatch(typeof(RobotMining))]
    [HarmonyPatch("Awake")]
    public class robotMiningPatch
    {
        [HarmonyPatch("DelayedActionInstance")]
        [HarmonyPostfix]
        public static void roverPatch(RobotMining __instance)
        {
            aimeeUberModPlugin.Log.LogInfo("Patching aimee rover mount");
        }
    }
}