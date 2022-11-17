using HarmonyLib;
using System;
using UnityEngine;
using System.Linq;

namespace AUMod.Patches
{
    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Detoriorate))]
    public static class HeliSabotageSystemPatch {
        static void Prefix(HeliSabotageSystem __instance, float deltaTime)
        {
            if (!__instance.IsActive)
                return;
            if (AirshipStatus.Instance == null)
                return;

            if (__instance.Countdown > CustomOptionHolder.heliSabotageSystemTimeLimit.getFloat())
                __instance.Countdown = CustomOptionHolder.heliSabotageSystemTimeLimit.getFloat();
        }
    }
}
