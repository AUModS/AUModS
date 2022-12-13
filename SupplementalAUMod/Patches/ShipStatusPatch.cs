using HarmonyLib;
using static AUMod.Roles;
using UnityEngine;
using Hazel;

namespace AUMod.Patches
{
    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null)
                return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null)
                return true;

            float num = (float)switchSystem.Value / 255f;

            if (player == null || player.IsDead) // IsDead
                __result = __instance.MaxLightRadius;
            // Impostor, Jackal/Sidekick, Spy, or Madmate with Impostor vision
            else if (player.Role.IsImpostor || (Madmate.madmate != null && Madmate.madmate.PlayerId == player.PlayerId && Madmate.hasImpostorVision))
                __result = __instance.MaxLightRadius * GameOptionsManager.Instance.CurrentGameOptions.GetFloat(AmongUs.GameOptions.FloatOptionNames.ImpostorLightMod);
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) * GameOptionsManager.Instance.CurrentGameOptions.GetFloat(AmongUs.GameOptions.FloatOptionNames.CrewLightMod);
            return false;
        }

        private static int originalNumCommonTasksOption = 0;
        private static int originalNumShortTasksOption = 0;
        private static int originalNumLongTasksOption = 0;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static bool Prefix(ShipStatus __instance)
        {
            var commonTaskCount = __instance.CommonTasks.Count;
            var normalTaskCount = __instance.NormalTasks.Count;
            var longTaskCount = __instance.LongTasks.Count;
            originalNumCommonTasksOption = GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.NumCommonTasks);
            originalNumShortTasksOption = GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.NumShortTasks);
            originalNumLongTasksOption = GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.NumLongTasks);
            if (originalNumCommonTasksOption > commonTaskCount)
                GameOptionsManager.Instance.CurrentGameOptions.SetInt(AmongUs.GameOptions.Int32OptionNames.NumCommonTasks, commonTaskCount);
            if (originalNumShortTasksOption > normalTaskCount)
                GameOptionsManager.Instance.CurrentGameOptions.SetInt(AmongUs.GameOptions.Int32OptionNames.NumShortTasks, normalTaskCount);
            if (originalNumLongTasksOption > longTaskCount)
                GameOptionsManager.Instance.CurrentGameOptions.SetInt(AmongUs.GameOptions.Int32OptionNames.NumLongTasks, longTaskCount);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static void Postfix3(ShipStatus __instance)
        {
            // Restore original settings after the tasks have been selected
            GameOptionsManager.Instance.CurrentGameOptions.SetInt(AmongUs.GameOptions.Int32OptionNames.NumCommonTasks, originalNumCommonTasksOption);
            GameOptionsManager.Instance.CurrentGameOptions.SetInt(AmongUs.GameOptions.Int32OptionNames.NumShortTasks, originalNumShortTasksOption);
            GameOptionsManager.Instance.CurrentGameOptions.SetInt(AmongUs.GameOptions.Int32OptionNames.NumLongTasks, originalNumLongTasksOption);
        }
    }
}
