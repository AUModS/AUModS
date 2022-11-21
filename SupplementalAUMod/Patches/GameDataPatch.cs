using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AUMod.Roles;
using static AUMod.GameHistory;
using UnityEngine;

namespace AUMod.Patches
{
    // this part should be in GameDataPatch.cs
    [HarmonyPatch(typeof(GameData), nameof(GameData.RpcSetTasks))]
    public static class RpcSetTasksPatch {
        public static bool Prefix(GameData __instance, [HarmonyArgument(0)] byte playerId, [HarmonyArgument(1)] byte[] taskTypeIds)
        {
            if (Madmate.madmate != null && Madmate.madmate.PlayerId == playerId && !Madmate.taskReset) {
                Madmate.setupTasks();
                return false;
            }

            return true;
        }
    }
}
