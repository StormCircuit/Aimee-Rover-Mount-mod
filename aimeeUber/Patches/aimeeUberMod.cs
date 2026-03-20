using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace aimeeUberMod
{
  internal class aimeeUberModPlugin : BaseUnityPlugin
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
