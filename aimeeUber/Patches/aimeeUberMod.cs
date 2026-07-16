using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace aimeeUberMod {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class aimeeUberModPlugin : BaseUnityPlugin {
        public const string pluginGuid = "com.username.aimeeUberMod";
        public const string pluginName = "aimeeUberMod";
        public const string pluginVersion = "1.0";

        public static ManualLogSource Log;

        //used to offset Aimee specifically, solves Issue 3
        public const float RoverMountOffsetY = -0.5f;

        //used to pitch aimee a little when mounted part of Issue 3
        public const float RoverMountPitchTrimDegrees = 25f;

        void Awake() {
            try {
                Log = Logger;

                var harmony = new Harmony(pluginGuid);
                harmony.PatchAll();
                Log.LogInfo("Patch succeeded");
            }
            catch (Exception e) {
                if (Log != null) {
                    Log.LogError("Patch Failed");
                    Log.LogError(e.ToString());
                }
                else {
                    Debug.LogError("[" + pluginName + "]: Patch Failed");
                    Debug.LogError(e.ToString());
                }
            }
        }
    }
}
