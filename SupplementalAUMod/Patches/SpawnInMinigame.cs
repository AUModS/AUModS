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
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class SpawnInMinigamePatch {
        public static void Postfix(SpawnInMinigame __instance, PlayerTask task)
        {
            __instance.Close();
        }
    }
}
