using HarmonyLib;

namespace PeakMap.Patches.Automation;

[HarmonyPatch(typeof(AirportCheckInKiosk), nameof(AirportCheckInKiosk.BeginIslandLoadRPC))]
public class AirportCheckInKioskPatch
{
    public static void Prefix(ref string sceneName)
    {
        if (PeakMapPlugin.LevelQueue.Count > 0)
        {
            PeakMapPlugin.BatchActive = true;
            string forced = PeakMapPlugin.LevelQueue.Dequeue();
            PeakMapPlugin.Log.LogWarning($"Forcing scene '{sceneName}' -> '{forced}' ({PeakMapPlugin.LevelQueue.Count} remaining in batch)");
            sceneName = forced;
            PeakMapPlugin.CurrentSceneName = forced;
        }
    }
}