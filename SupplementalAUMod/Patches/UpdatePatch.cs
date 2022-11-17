using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static AUMod.Roles;
using System.Collections.Generic;
using System.Linq;

namespace AUMod.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch {
        private static bool canMadmateSeeImpostorName()
        {
            if (Madmate.madmate == null)
                return false;
            if (Madmate.madmate != PlayerControl.LocalPlayer)
                return false;

            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Madmate.madmate.Data, true);
            return playerTotal - playerCompleted <= 0;
        }

        static void resetNameTagsAndColorsToSeeImpostors()
        {
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
            impostors.RemoveAll(x => !x.Data.Role.IsImpostor);
            foreach (PlayerControl player in impostors)
                player.cosmetics.nameText.color = Palette.ImpostorRed;
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                    PlayerControl pc = Helpers.playerById((byte)player.TargetPlayerId);
                    if (pc != null && pc.Data.Role.IsImpostor)
                        player.NameText.color = Palette.ImpostorRed;
                }
            }
        }

        static void setPlayerNameColor(PlayerControl p, Color color)
        {
            p.cosmetics.nameText.color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors()
        {
            if (Sheriff.sheriff != null && Sheriff.sheriff == PlayerControl.LocalPlayer)
                setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
            if (Madmate.madmate != null && Madmate.madmate == PlayerControl.LocalPlayer)
                setPlayerNameColor(Madmate.madmate, Madmate.color);
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                return;

            CustomButton.HudUpdate();
            if (canMadmateSeeImpostorName())
                resetNameTagsAndColorsToSeeImpostors();
            setNameColors();
        }
    }
}
