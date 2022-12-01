using UnityEngine;

namespace AUMod
{
    public static class FarSight {
        private static bool toggle;
        private static Sprite buttonSprite;

        public static Sprite getButtonSprite()
        {
            if (buttonSprite)
                return buttonSprite;
            buttonSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.OptionsButton].Image;
            return buttonSprite;
        }
        public static void activate(bool isActive)
        {
            if (isActive == toggle)
                return;
            var hudManager = FastDestroyableSingleton<HudManager>.Instance;
            var modStamp = FastDestroyableSingleton<ModManager>.Instance.ModStamp.transform;
            if (isActive) {
                Camera.main.orthographicSize = hudManager.UICamera.orthographicSize *= 7f;
                hudManager.transform.localScale *= 7f;
                modStamp.localScale *= 7f;
            } else {
                Camera.main.orthographicSize = hudManager.UICamera.orthographicSize /= 7f;
                hudManager.transform.localScale /= 7f;
                modStamp.localScale /= 7f;
            }
            toggle = isActive;
        }
        public static void buttonAction() =>
            activate(!toggle);
        public static void Update()
        {
            bool forceDeactivate = !FastDestroyableSingleton<HudManager>.Instance.UseButton.isActiveAndEnabled ||
                                   !PlayerControl.LocalPlayer.CanMove;
            if (forceDeactivate && toggle)
                activate(false);
        }
    }
}
