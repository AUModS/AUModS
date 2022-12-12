using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using static AUMod.Roles;

namespace AUMod {
static class CustomMapOptions {
    // Set values
    public static bool showRoleSummary = true;

    // Updating values
    public static float adminRemain = 10f;
    public static float camerasRemain = 10f;
    public static float vitalsRemain = 10f;
    public static TMPro.TextMeshPro adminText = null;
    public static TMPro.TextMeshPro camerasText = null;
    public static TMPro.TextMeshPro vitalsText = null;
    private static float timerTextX = -3.5f;
    private static float adminTextY = -3.6f;
    private static float camerasTextY = -3.8f;
    private static float vitalsTextY = -4.0f;

    public static void clearAndReloadMapOptions()
    {
        adminRemain = CustomOptionHolder.adminMaxTimeRemaining.getFloat();
        camerasRemain = CustomOptionHolder.camerasMaxTimeRemaining.getFloat();
        vitalsRemain = CustomOptionHolder.vitalsMaxTimeRemaining.getFloat();
        ClearTimerText();
        UpdateTimerText();
        showRoleSummary = true;
    }

    public static bool canUseAdmin
    {
        get {
            return adminRemain > 0f;
        }
    }

    public static bool canUseCameras
    {
        get {
            return camerasRemain > 0f;
        }
    }

    public static bool canUseVitals
    {
        get {
            return vitalsRemain > 0f;
        }
    }

    public static void MeetingEndedUpdate()
    {
        ClearTimerText();
        UpdateTimerText();
    }

    private static void UpdateSingleTimerText(ref TMPro.TextMeshPro timerText, float textX, float textY, string textVal)
    {
        timerText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText,
            FastDestroyableSingleton<HudManager>.Instance.transform);
        timerText.transform.localPosition = new Vector3(textX, textY, 0);
        timerText.text = textVal;
        timerText.gameObject.SetActive(true);
    }

    private static void UpdateTimerText()
    {
        if (FastDestroyableSingleton<HudManager>.Instance == null)
            return;

        UpdateSingleTimerText(ref adminText, timerTextX, adminTextY,
            canUseAdmin ? $"Admin: {adminRemain.ToString("0.00")} sec remaining"
                        : "Admin: ran out of time");
        UpdateSingleTimerText(ref camerasText, timerTextX, camerasTextY,
            canUseCameras ? $"Cameras: {camerasRemain.ToString("0.00")} sec remaining"
                          : "Cameras: ran out of time");
        UpdateSingleTimerText(ref vitalsText, timerTextX, vitalsTextY,
            canUseVitals ? $"Vitals: {vitalsRemain.ToString("0.00")} sec remaining"
                         : "Vitals: ran out of time");
    }

    private static void ClearTimerText()
    {
        if (adminText != null)
            UnityEngine.Object.Destroy(adminText);
        adminText = null;
        if (camerasText != null)
            UnityEngine.Object.Destroy(camerasText);
        camerasText = null;
        if (vitalsText != null)
            UnityEngine.Object.Destroy(vitalsText);
        vitalsText = null;
    }
}
}
