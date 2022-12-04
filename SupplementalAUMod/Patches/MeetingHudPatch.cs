using HarmonyLib;
using UnityEngine;

namespace AUMod.Patches
{
    [HarmonyPatch]
    public static class MeetingHudPatch {
        private static GameObject blackScreen;
        public static GameObject getBlackScreen()
        {
            if (blackScreen)
                return blackScreen;
            var hudManager = FastDestroyableSingleton<HudManager>.Instance;
            var spriteRenderer = Object.Instantiate(hudManager.FullScreen, hudManager.transform);
            spriteRenderer.color = Color.black;
            spriteRenderer.enabled = true;
            blackScreen = spriteRenderer.gameObject;
            blackScreen.transform.localPosition = MeetingHud.Instance.BlackBackground.transform.localPosition;
            blackScreen.SetActive(false);
            return blackScreen;
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class MeetingHudStartPatch {
            public static void Postfix(MeetingHud __instance)
            {
                getBlackScreen().SetActive(true);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
        public static class MeetingHudOnDestroyPatch {
            public static void Postfix(MeetingHud __instance)
            {
                getBlackScreen().SetActive(false);
            }
        }
    }
}
