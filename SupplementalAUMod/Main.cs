using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;

namespace AUMod {
[BepInPlugin(Id, "AUMod", VersionString)]
[BepInProcess("Among Us.exe")]
public class AUModPlugin : BasePlugin {
    public const string Id = "me.tomarai.aumod";
    public const string VersionString = "22.12.14.1";
    public static System.Version Version = System.Version.Parse(VersionString);

    public static Sprite ModStamp = null;

    public Harmony Harmony { get; } = new Harmony(Id);
    public static AUModPlugin Instance;

    public override void Load()
    {
        Instance = this;

        CustomOptionHolder.Load();
        Harmony.PatchAll();
    }
}
}
