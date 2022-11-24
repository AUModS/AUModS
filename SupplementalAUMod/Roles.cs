using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

namespace AUMod {
[HarmonyPatch]
public static class Roles {
    public static System.Random rnd = new System.Random((int)DateTime.Now.Ticks);

    public static void clearAndReloadRoles()
    {
        Sheriff.clearAndReload();
        Madmate.clearAndReload();
        EvilHacker.clearAndReload();
    }

    public static class Sheriff {
        public static PlayerControl sheriff;
        public static Color color = new Color32(248, 205, 70, byte.MaxValue);

        public static float cooldown = 30f;
        public static int remainingShots = 1;
        public static bool canKillNeutrals = false;
        public static bool canKillCrewmates = false;
        public static bool madmateCanDieToSheriff = false;

        public static PlayerControl currentTarget;

        public static void clearAndReload()
        {
            sheriff = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
            remainingShots = Mathf.RoundToInt(CustomOptionHolder.sheriffNumberOfShots.getFloat());
            canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.getBool();
            canKillCrewmates = CustomOptionHolder.sheriffCanKillCrewmates.getBool();
            madmateCanDieToSheriff = CustomOptionHolder.madmateCanDieToSheriff.getBool();
        }
    }

    public static class Madmate {
        public static PlayerControl madmate;
        public static Color color = Palette.ImpostorRed;

        public static bool canEnterVents = true;
        public static bool hasImpostorVision = true;
        public static bool canFixComm = false;
        public static bool exileCrewmate = true;
        public static bool taskReset = false;

        public static int commonTasks
        {
            get {
                return CustomOptionHolder.madmateTasks.commonTasks;
            }
        }

        public static int longTasks
        {
            get {
                return CustomOptionHolder.madmateTasks.longTasks;
            }
        }

        public static int shortTasks
        {
            get {
                return CustomOptionHolder.madmateTasks.shortTasks;
            }
        }

        public static Vent currentTarget = null;

        public static void clearAndReload()
        {
            madmate = null;
            currentTarget = null;
            canEnterVents = CustomOptionHolder.madmateCanEnterVents.getBool();
            hasImpostorVision = CustomOptionHolder.madmateHasImpostorVision.getBool();
            canFixComm = CustomOptionHolder.madmateCanFixComm.getBool();
            exileCrewmate = CustomOptionHolder.madmateExileCrewmate.getBool();
            taskReset = false;
        }

        public static void setupTasks()
        {
            taskReset = true;
            Madmate.madmate.generateAndAssignTasks(commonTasks, longTasks, shortTasks);
        }
    }

    public static class EvilHacker {
        public static PlayerControl evilHacker;
        public static Color color = Palette.ImpostorRed;

        private static Sprite buttonSprite;
        private static string buttonText;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite)
                return buttonSprite;
            UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton];
            buttonSprite = button.Image;
            return buttonSprite;
        }
        public static string getButtonText()
        {
            if (buttonText != null)
                return buttonText;
            buttonText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin);
            return buttonText;
        }

        public static void clearAndReload()
        {
            evilHacker = null;
        }
    }
}
}
