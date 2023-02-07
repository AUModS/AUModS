using HarmonyLib;
using static AUMod.Roles;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AUMod {
[HarmonyPatch]
public static class TasksHandler {

    public static Tuple<int, int> taskInfo(GameData.PlayerInfo playerInfo, bool madmateCount = false)
    {
        int TotalTasks = 0;
        int CompletedTasks = 0;
        if (playerInfo.Disconnected)
            return Tuple.Create(0, 0);
        if (playerInfo.Tasks == null)
            return Tuple.Create(0, 0);
        if (!playerInfo.Object)
            return Tuple.Create(0, 0);
        if (!GameOptionsManager.Instance.CurrentGameOptions.GetBool(AmongUs.GameOptions.BoolOptionNames.GhostsDoTasks) && playerInfo.IsDead)
            return Tuple.Create(0, 0);
        if (playerInfo.Role.IsImpostor)
            return Tuple.Create(0, 0);
        if (playerInfo.Object.hasFakeTasks())
            return Tuple.Create(0, 0);
        if (!playerInfo.Role)
            return Tuple.Create(0, 0);
        if (!playerInfo.Role.TasksCountTowardProgress)
            return Tuple.Create(0, 0);
        if (playerInfo.Object == Madmate.madmate && !(madmateCount && PlayerControl.LocalPlayer == Madmate.madmate))
            return Tuple.Create(0, 0);

        foreach (var task in playerInfo.Tasks) {
            TotalTasks++;
            if (task.Complete)
                CompletedTasks++;
        }
        return Tuple.Create(CompletedTasks, TotalTasks);
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
    private static class GameDataRecomputeTaskCountsPatch {
        private static bool Prefix(GameData __instance)
        {
            __instance.TotalTasks = 0;
            __instance.CompletedTasks = 0;
            for (int i = 0; i < __instance.AllPlayers.Count; i++) {
                GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                var (playerCompleted, playerTotal) = taskInfo(playerInfo);
                __instance.TotalTasks += playerTotal;
                __instance.CompletedTasks += playerCompleted;
            }
            return false;
        }
    }
}
}
