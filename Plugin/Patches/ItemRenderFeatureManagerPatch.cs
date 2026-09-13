using System;
using HarmonyLib;

namespace PeakMap.Patches;

[HarmonyPatch(typeof(ItemRenderFeatureManager), "OnDisable")]
public class ItemRenderFeatureManagerPatch
{
    public static Exception Finalizer(Exception __exception)
    {
        if (__exception != null)
        {
            PeakMapPlugin.Log.LogWarning("Suppressed exception in ItemRenderFeatureManager.OnDisable: " + __exception.Message);
        }
        return null;
    }
}