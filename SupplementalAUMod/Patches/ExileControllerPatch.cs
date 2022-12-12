using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static AUMod.Roles;
using static AUMod.CustomMapOptions;
using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace AUMod.Patches
{
    [HarmonyPatch]
    class ExileControllerWrapUpPatch {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            CustomMapOptions.MeetingEndedUpdate();
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPriority(Priority.First)]
    class ExcileControllerBeginPatch {
        public static void Prefix(ExileController __instance,
            [HarmonyArgument(0)] ref GameData.PlayerInfo exiled, [HarmonyArgument(1)] bool tie)
        {
            if (Madmate.madmate != null
                && Madmate.exileCrewmate
                && AmongUsClient.Instance.AmHost
                && exiled != null
                && exiled.PlayerId == Madmate.madmate.PlayerId) {
                // pick random crewmate
                PlayerControl target = pickRandomCrewmate(exiled.PlayerId);
                if (target != null) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte)CustomRPC.UncheckedExilePlayer,
                        Hazel.SendOption.Reliable,
                        -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedExilePlayer(target.PlayerId);
                }
            }
        }

        private static PlayerControl pickRandomCrewmate(int exiledPlayerId)
        {
            var possibleTargets = new List<PlayerControl>();
            // make possible targets
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                if (player.Data.Disconnected)
                    continue;
                if (player.Data.Role.IsImpostor)
                    continue;
                if (player.Data.IsDead)
                    continue;
                if (player.PlayerId == exiledPlayerId)
                    continue;
                possibleTargets.Add(player);
            }
            return possibleTargets[Roles.rnd.Next(0, possibleTargets.Count)];
        }
    }

}
