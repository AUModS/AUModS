using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AUMod.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string fullCredentials = $@"<size=130%><color=#ff351f>AUModS</color></size> v{AUModPlugin.Version.ToString()}";

        public static string mainMenuCredentials = $@"<color=#FCCE03FF>AUModS</color>";

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class MainMenuManagerPatch {
            private static GameObject modStamp;

            public static void Postfix(MainMenuManager __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");

                modStamp = new GameObject("ModStamp");
                var rend = modStamp.AddComponent<SpriteRenderer>();
                rend.sprite = AUModPlugin.GetModStamp();
                rend.color = new(1, 1, 1, 0.5f);
                modStamp.transform.parent = amongUsLogo.transform;
                modStamp.transform.localScale *= 0.6f;
                modStamp.transform.position = Vector3.up;
            }
        }

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch {
            static void Postfix(VersionShower __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null)
                    return;

                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0.1f, 0);
                credentials.SetText(mainMenuCredentials);
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;

                var version = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(credentials);
                version.transform.position = new Vector3(0, -0.25f, 0);
                version.SetText($"v{AUModPlugin.Version.ToString()}");
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch {
            private static GameObject modStamp;

            static void Postfix(PingTracker __instance)
            {
                if (modStamp == null) {
                    modStamp = new GameObject("ModStamp");
                    var rend = modStamp.AddComponent<SpriteRenderer>();
                    rend.sprite = AUModPlugin.GetModStamp();
                    rend.color = new(1, 1, 1, 0.5f);
                    modStamp.transform.parent = __instance.transform.parent;
                    modStamp.transform.localScale *= 0.6f;
                }
                float offset = (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) ? 0.75f : 0f;
                modStamp.transform.position = FastDestroyableSingleton<HudManager>.Instance.MapButton.transform.position + Vector3.down * offset;

                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                    __instance.text.text = $"<size=130%><color=#ff351f>AUModS</color></size> v{AUModPlugin.Version.ToString()}\n" + __instance.text.text;
                    if (PlayerControl.LocalPlayer.Data.IsDead) {
                        __instance.transform.localPosition = new Vector3(
                            3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    } else {
                        __instance.transform.localPosition = new Vector3(
                            4.2f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    }
                } else {
                    __instance.text.text = $"{fullCredentials}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(
                        3.5f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                }
            }
        }
    }
}
