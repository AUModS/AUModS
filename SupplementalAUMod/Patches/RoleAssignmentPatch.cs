using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;
using System;
using static AUMod.Roles;

namespace AUMod.Patches
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch {
        private static List<Tuple<byte, byte>> playerRoleMap = new List<Tuple<byte, byte>>();

        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.ResetVaribles,
                Hazel.SendOption.Reliable,
                -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();

            if (!DestroyableSingleton<TutorialManager>.InstanceExists) // Don't assign Roles in Tutorial
                assignRoles();
        }

        private static void assignRoles()
        {
            var data = getRoleAssignmentData();
            // Assign roles that should always be in the game next
            // Always perform this so far
            assignEnsuredRoles(data);
            setRolesAgain();
        }

        private static void setRolesAgain()
        {
            while (playerRoleMap.Any()) {
                byte amount = (byte)Math.Min(playerRoleMap.Count, 20);
                var writer = AmongUsClient.Instance !.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.WorkaroundSetRoles,
                    SendOption.Reliable,
                    -1);
                writer.Write(amount);
                for (int i = 0; i < amount; i++) {
                    var option = playerRoleMap[0];
                    playerRoleMap.RemoveAt(0);
                    writer.WritePacked((uint)option.Item1);
                    writer.WritePacked((uint)option.Item2);
                }
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        private static RoleAssignmentData getRoleAssignmentData()
        {
            // Get the players that we want to assign the roles to.
            // Madmate and Sheriff are assigned to natural crewmates.
            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            // Fill in the lists with the roles that should be assigned to players.
            // Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> impSettings = new Dictionary<byte, int>();

            crewSettings.Add((byte)RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Madmate, CustomOptionHolder.madmateSpawnRate.getSelection());

            impSettings.Add((byte)RoleId.EvilHacker, CustomOptionHolder.evilHackerSpawnRate.getSelection());

            return new RoleAssignmentData {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                impSettings = impSettings,
            };
        }

        private static void assignEnsuredRoles(RoleAssignmentData data)
        {
            // Get all roles where the chance to occur is set to 100%
            List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value == CustomOptionHolder.rates.Length - 1).Select(x => x.Key).ToList();
            List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value == CustomOptionHolder.rates.Length - 1).Select(x => x.Key).ToList();
            int crewmateRolesCount = ensuredCrewmateRoles.Count;
            int impostorRolesCount = ensuredImpostorRoles.Count;

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while ((data.crewmates.Count > 0 && crewmateRolesCount > 0 && ensuredCrewmateRoles.Count > 0) || (data.impostors.Count > 0 && impostorRolesCount > 0 && ensuredImpostorRoles.Count > 0)) {
                Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && crewmateRolesCount > 0 && ensuredCrewmateRoles.Count > 0)
                    rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
                if (data.impostors.Count > 0 && impostorRolesCount > 0 && ensuredImpostorRoles.Count > 0)
                    rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role)
                // then select one of the roles from the selected pool to a player
                // and remove the role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == RoleType.Crewmate ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                rolesToAssign[roleType].RemoveAt(index);

                // Adjust the role limit
                switch (roleType) {
                case RoleType.Crewmate:
                    crewmateRolesCount--;
                    break;
                case RoleType.Neutral:
                    /* data.maxNeutralRoles--; */
                    break;
                case RoleType.Impostor:
                    impostorRolesCount--;
                    break;
                }
            }
        }

        private static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, bool removePlayer = true)
        {
            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;
            if (removePlayer)
                playerList.RemoveAt(index);

            playerRoleMap.Add(new Tuple<byte, byte>(playerId, roleId));

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.SetRole,
                Hazel.SendOption.Reliable,
                -1);
            writer.Write(roleId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setRole(roleId, playerId);
            return playerId;
        }

        private class RoleAssignmentData {
            public List<PlayerControl> crewmates { get; set; }
            public List<PlayerControl> impostors { get; set; }
            public Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            public Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
        }

        private enum RoleType {
            Crewmate = 0,
            Neutral = 1,
            Impostor = 2
        }
    }
}
