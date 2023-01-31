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
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch {
        // Helpers

        static PlayerControl setTarget(
            bool onlyCrewmates = false,
            bool targetPlayersInVents = false,
            List<PlayerControl> untargetablePlayers = null,
            PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = AmongUs.GameOptions.GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.KillDistance), 0, 2)];
            if (!ShipStatus.Instance)
                return result;
            if (targetingPlayer == null)
                targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead)
                return result;

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++) {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor)) {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)) {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents)) {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)) {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        static Vent setTargetVent()
        {
            if (!ShipStatus.Instance)
                return null;

            GameData.PlayerInfo me = GameData.Instance.GetPlayerById(PlayerControl.LocalPlayer.PlayerId);
            UnhollowerBaseLib.Il2CppReferenceArray<Vent> allVents = ShipStatus.Instance.AllVents;

            for (int i = 0; i < allVents.Count; i++) {
                Vent vent = allVents[i];
                float useless = 0;
                bool canUse, couldUse;
                VentCanUsePatch.Prefix(vent, ref useless, me, out canUse, out couldUse);
                if (canUse)
                    return vent;
            }
            return null;
        }

        static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null)
                return;

            target.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics?.currentBodySprite?.BodySprite.material.SetColor("_OutlineColor", color);
        }

        // Update functions

        static void setBasePlayerOutlines()
        {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                if (target == null || target.cosmetics?.currentBodySprite?.BodySprite.material == null)
                    continue;
                target.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 0f);
            }
        }

        // looking for an available vent
        static void madmateSetTarget()
        {
            if (Madmate.madmate == null || Madmate.madmate != PlayerControl.LocalPlayer)
                return;
            Madmate.currentTarget = setTargetVent();
        }

        static void sheriffSetTarget()
        {
            if (Sheriff.sheriff == null || Sheriff.sheriff != PlayerControl.LocalPlayer)
                return;
            Sheriff.currentTarget = setTarget();
            setPlayerOutline(Sheriff.currentTarget, Sheriff.color);
        }

        static void impostorSetTarget()
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || !PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead) { // !isImpostor || !canMove || isDead
                HudManager.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            target = setTarget(true, true);

            HudManager.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
        }

        public static void updatePlayerInfo()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p != PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead)
                    continue;

                Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo == null) {
                    playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                    playerInfo.transform.localPosition += Vector3.up * 0.5f;
                    playerInfo.fontSize *= 0.75f;
                    playerInfo.gameObject.name = "Info";
                }

                PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (meetingInfo == null && playerVoteArea != null) {
                    meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                    meetingInfo.transform.localPosition += Vector3.down * 0.20f;
                    meetingInfo.fontSize *= 0.75f;
                    meetingInfo.gameObject.name = "Info";
                }

                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                string roleNames = String.Join(" ", RoleInfo.getRoleInfoForPlayer(p).Select(x => Helpers.cs(x.color, x.name)).ToArray());
                var isComms = PlayerControl.LocalPlayer.myTasks.ToArray().Any(task => task.TaskType == TaskTypes.FixComms);
                string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({(isComms ? "?" : tasksCompleted)}/{tasksTotal})</color>" : "";

                string playerInfoText = "";
                string meetingInfoText = "";
                if (p == PlayerControl.LocalPlayer) {
                    playerInfoText = $"{roleNames}";
                    if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists) {
                        TMPro.TextMeshPro tabText = DestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                        tabText.SetText($"Tasks {taskInfo}");
                    }
                    meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                }

                playerInfo.text = playerInfoText;
                playerInfo.gameObject.SetActive(p.Visible);
                if (meetingInfo != null)
                    meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
            }
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                return;

            if (PlayerControl.LocalPlayer == __instance) {
                // Update player outlines
                setBasePlayerOutlines();

                // Update Role Description
                Helpers.refreshRoleDescription(__instance);

                // Update Player Info
                updatePlayerInfo();

                // Madmate
                madmateSetTarget();
                // Sheriff
                sheriffSetTarget();
                // Impostor
                impostorSetTarget();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        /*
         * TODO
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.Role.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.Role.IsImpostor = true;
            __instance.Data.IsDead = false;
        }
         */

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            /*
             * TODO
            if (resetToCrewmate)
                __instance.Data.Role.IsImpostor = false;
            if (resetToDead)
                __instance.Data.IsDead = true;
             */

            // Remove fake tasks when player dies
            if (target.hasFakeTasks())
                target.clearAllTasks();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Remove fake tasks when player dies
            if (__instance.hasFakeTasks())
                __instance.clearAllTasks();
        }
    }
}
