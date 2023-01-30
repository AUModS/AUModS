using HarmonyLib;
using UnityEngine;
using InnerNet;

namespace AUMod.Patches
{
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class KeyboardJoystickUpdatePatch {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F5)) {
                GameManager.Instance.RpcEndGame((GameOverReason)99, false);
            }
        }
    }
}
