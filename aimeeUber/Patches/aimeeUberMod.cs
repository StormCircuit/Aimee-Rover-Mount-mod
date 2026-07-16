using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Assets.Scripts.Objects;
using UnityEngine;

namespace aimeeUberMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class aimeeUberModPlugin : BaseUnityPlugin
    {
      public const string pluginGuid = "com.username.aimeeUberMod";
      public const string pluginName = "aimeeUberMod";
      public const string pluginVersion = "1.0";

      public static ManualLogSource Log;
      public static Boolean modEnabled;
  public const float RoverMountOffsetY = -0.5f;

      void Awake()
      {
        try
        {
          Log = Logger;

          var harmony = new Harmony(pluginGuid);
          harmony.PatchAll();
          Log.LogInfo("Patch succeeded");
        }
        catch (Exception e)
        {
          if (Log != null)
          {
            Log.LogError("Patch Failed");
            Log.LogError(e.ToString());
          }
          else
          {
            Debug.LogError("[" + pluginName + "]: Patch Failed");
            Debug.LogError(e.ToString());
          }
        }
      }    
  }
}
