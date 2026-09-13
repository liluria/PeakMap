using System.Collections;
using HarmonyLib;
using UnityEngine;
using Zorro.Core;

namespace PeakMap.Patches.Automation;

[HarmonyPatch(typeof(MainMenu))]
public class MainMenuPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void StartPostfix()
    {
        bool batchRemaining = PeakMapPlugin.BatchActive && PeakMapPlugin.LevelQueue.Count > 0;
        if (!PeakMapPlugin.SkipMainMenu.Value && !batchRemaining)
        {
            return;
        }
        LoadingScreenHandlerPatch.Found = false;
        PeakMapPlugin.Instance.StartCoroutine(DelayedStart());
    }

    private static IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(3f);
        RetrievableResourceSingleton<LoadingScreenHandler>.Instance.Load(LoadingScreen.LoadingScreenType.Basic, null, StartOfflineModeRoutine());
    }

    private static IEnumerator StartOfflineModeRoutine()
    {
        yield return MainMenu.DisconnectForOfflineMode();
        yield return RetrievableResourceSingleton<LoadingScreenHandler>.Instance.LoadSceneProcess("Airport", networked: false, yieldForCharacterSpawn: true);
    }
}