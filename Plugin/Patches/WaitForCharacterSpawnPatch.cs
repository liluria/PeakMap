using HarmonyLib;

namespace PeakMap.Patches;

[HarmonyPatch(typeof(LoadingScreenHandler), "WaitForCharacterSpawn")]
public class WaitForCharacterSpawnPatch
{
	public static void Prefix(ref float timeout)
	{
		if (PeakMapPlugin.SkipSpawnWait)
		{
			PeakMapPlugin.SkipSpawnWait = false;
			timeout = 0f;
		}
	}
}