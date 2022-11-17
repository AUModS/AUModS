using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Hazel;
using System;
using UnhollowerBaseLib;

namespace AUMod.Patches
{
    public class GameStartManagerPatch {
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        private static bool versionSent = false;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix(GameStartManager __instance)
            {
                // Trigger version refresh
                versionSent = false;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch {
            private static bool update = false;
            private static string currentText = "";

            public static void Prefix(GameStartManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance)
                    return; // Not host or no instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance)
            {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !versionSent) {
                    PerformVersionHandshake();
                }

                // Host update with version handshake infos
                if (AmongUsClient.Instance.AmHost) {
                    bool blockStart = false;
                    string message = "";
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray()) {
                        if (client.Character == null)
                            continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;
                        else if (!playerVersions.ContainsKey(client.Id)) {
                            blockStart = true;
                            message += GetWarningMessage(client, "has a different or no version of AUMod");
                        } else {
                            PlayerVersion PV = playerVersions[client.Id];
                            int diff = AUModPlugin.Version.CompareTo(PV.version);
                            if (diff == 0 && PV.GuidMatches())
                                continue;

                            blockStart = true;

                            string version = playerVersions[client.Id].version.ToString();
                            if (diff > 0) {
                                message += GetWarningMessage(client, $"has an older version of AUMod v{version}");
                            } else if (diff < 0) {
                                message += GetWarningMessage(client, $"has a newer version of AUMod v{version}");
                            } else {
                                // case !PV.GuidMatches()
                                // version presumably matches, check if Guid matches
                                message += GetWarningMessage(client, $"has a modified version of AUMod v{version}");
                            }
                        }
                    }
                    if (blockStart) {
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }
                }

                if (update)
                    currentText = __instance.PlayerCounter.text;
            }
        }

        private static void PerformVersionHandshake()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.VersionHandshake,
                Hazel.SendOption.Reliable,
                -1);
            writer.Write((byte)AUModPlugin.Version.Major);
            writer.Write((byte)AUModPlugin.Version.Minor);
            writer.Write((byte)AUModPlugin.Version.Build);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write((byte)(AUModPlugin.Version.Revision < 0 ? 0xFF : AUModPlugin.Version.Revision));
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.versionHandshake(AUModPlugin.Version.Major,
                AUModPlugin.Version.Minor,
                AUModPlugin.Version.Build,
                AUModPlugin.Version.Revision,
                Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId,
                AmongUsClient.Instance.ClientId);
            versionSent = true;
        }

        private static string GetWarningMessage(InnerNet.ClientData client, string warning)
        {
            return $"<color=#FF0000FF>{client.Character.Data.PlayerName} {warning}\n</color>";
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame {
            public static bool Prefix(GameStartManager __instance)
            {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                return continueStart;
            }
        }

        public class PlayerVersion {
            public readonly Version version;
            public readonly Guid guid;

            public PlayerVersion(Version version, Guid guid)
            {
                this.version = version;
                this.guid = guid;
            }

            public bool GuidMatches()
            {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
            }
        }
    }
}
