using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace PeakMap.Patches.Automation;

[HarmonyPatch(typeof(LoadingScreenHandler))]
public class LoadingScreenHandlerPatch
{
    public static bool Found = false;

    [HarmonyPatch("LoadingRoutine")]
    [HarmonyPostfix]
    public static void LoadingRoutinePostfix(ref IEnumerator __result)
    {
        __result = WrapLoadingRoutine(__result);
    }

    private static IEnumerator WrapLoadingRoutine(IEnumerator original)
    {
        while (original.MoveNext())
        {
            yield return original.Current;
        }

        if (!Found && PeakMapPlugin.SkipAirport.Value)
        {
            Item passportItem = Object.FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(n => n.name.Equals("Passport(Clone)"));

            if (passportItem != null)
            {
                Found = true;
                PeakMapPlugin.Log.LogWarning("Found passport, loading scene...");
                Object.FindFirstObjectByType<AirportCheckInKiosk>(FindObjectsInactive.Include).StartGame(0);
            }
        }
    }
}