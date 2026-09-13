using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PeakMap.Patches.Automation;
using Zorro.Core;

namespace PeakMap;

public enum PostGatherActionType
{
    ReturnToMainMenu,
    QuitGame
}

[BepInPlugin("PeakMapPlugin", "PEAK Map Plugin", "1.0.0")]
public class PeakMapPlugin : BaseUnityPlugin
{
    public static string ModFolder { get; private set; }
    public static ManualLogSource Log { get; private set; }
    public static ConfigEntry<string> TargetLevel { get; private set; }
    public static ConfigEntry<bool> SkipMainMenu { get; private set; }
    public static ConfigEntry<bool> SkipAirport { get; private set; }
    public static ConfigEntry<PostGatherActionType> PostGatherAction { get; private set; }
    public static string CurrentSceneName { get; set; }
    public static bool SkipSpawnWait { get; set; }
    public static Queue<string> LevelQueue { get; private set; } = new();
    public static bool BatchActive { get; set; }
    public static PeakMapPlugin Instance { get; private set; }
    private Harmony _harmony;

    private void Awake()
    {
        Instance = this;
        ModFolder = Path.GetDirectoryName(Info.Location.Replace("PeakMap.dll", "output\\"));
        Log = Logger;
        TargetLevel = Config.Bind("General", "TargetLevel", "",
            "Force a specific level/seed scene (e.g. \"Level_7\"), a range (e.g. \"Level_0-Level_4\") to batch-generate several, or leave empty for normal daily behavior.");
        SkipMainMenu = Config.Bind("General", "SkipMainMenu", false,
            "If true, auto-skips the title screen and lands you at the Airport lobby on launch. Set to false to stay at the title screen so you can set TargetLevel first.");
        SkipAirport = Config.Bind("General", "SkipAirport", true,
            "If true, auto-starts a run as soon as the Airport lobby loads.");
        PostGatherAction = Config.Bind("General", "PostGatherAction", PostGatherActionType.ReturnToMainMenu,
            "What happens after the LAST level in a batch finishes gathering (every level before it always returns to menu automatically): ReturnToMainMenu, or QuitGame.");
        LevelQueue = ParseTargetLevels(TargetLevel.Value);
        TargetLevel.SettingChanged += (_, _) =>
        {
            LevelQueue = ParseTargetLevels(TargetLevel.Value);
            Log.LogWarning($"TargetLevel changed, rebuilt queue with {LevelQueue.Count} level(s) pending.");
        };
        _harmony = new Harmony("PeakMapPlugin");
        _harmony.PatchAll();
        Log.LogInfo("Initialized PeakMapPlugin!");
    }

    private static Queue<string> ParseTargetLevels(string input)
    {
        Queue<string> queue = new Queue<string>();
        if (string.IsNullOrWhiteSpace(input)) return queue;
        Match match = Regex.Match(input.Trim(), @"^Level_(\d+)\s*-\s*Level_(\d+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            int start = int.Parse(match.Groups[1].Value);
            int end = int.Parse(match.Groups[2].Value);
            int step = start <= end ? 1 : -1;
            for (int i = start; step > 0 ? i <= end : i >= end; i += step)
                queue.Enqueue("Level_" + i);
        }
        else
        {
            queue.Enqueue(input.Trim());
        }
        return queue;
    }

    public static string GetOutputFolder()
    {
        if (string.IsNullOrWhiteSpace(CurrentSceneName)) return ModFolder;
        string folder = Path.Combine(ModFolder, CurrentSceneName);
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static void ReturnToMainMenu()
    {
        Log.LogWarning("Leaving Photon room and returning to main menu");
        SkipSpawnWait = true;
        GameHandler.GetService<ConnectionService>().StateMachine.SwitchState<DisconnectingState>();
        NetworkConnector.LeaveRoom();
    }

    private void OnDestroy()
    {
        try { _harmony?.UnpatchSelf(); Log.LogInfo("Unpatched PeakMapPlugin!"); } catch { }
    }
}