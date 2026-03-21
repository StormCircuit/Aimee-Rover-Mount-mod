using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Assets.Scripts.Objects;

namespace aimeeUberMod
{
    [BepInPlugin("aimeeUberMod", "Aimee Rideshare Program", "0.0.0.1")]
    public class aimeeUberModPlugin : BaseUnityPlugin
    {
      public static ManualLogSource Log;
      public static Boolean modEnabled;

      void Awake()
      {
        Log = Logger;



        var harmony = new Harmony("aimeeUberMod");
        harmony.PatchAll();
      }    
  }
}
