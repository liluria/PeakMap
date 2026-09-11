using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace PeakMap;

[BepInPlugin("PeakMapPlugin", "PEAK Map Plugin", "1.0.0")]
public class PeakMapPlugin : BaseUnityPlugin
{
    public static string ModFolder { get; private set; }
    public static ManualLogSource Log { get; private set; }
    private Harmony _harmony;
    
    private void Awake()
    {
        ModFolder = Path.GetDirectoryName(Info.Location.Replace("PeakMap.dll", "output\\"));
        Log = Logger;
        _harmony = new Harmony("PeakMapPlugin");
        _harmony.PatchAll();
        Log.LogInfo("Initialized PeakMapPlugin!");
        Log.LogError("Attention ! PLUGIN SHOULD BE RUN AS SOON AS PATCH DROPS, TO ENSURE CORRECT DAY-LEVEL SEQUENCE");
    }

    private void OnDestroy()
    {
        try
        {
            _harmony?.UnpatchSelf();
            Log.LogInfo("Unpatched PeakMapPlugin!");
        } catch {
            //
        }
    }
    
}