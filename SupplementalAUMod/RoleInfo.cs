using HarmonyLib;
using System.Linq;
using System;
using System.Collections.Generic;
using static AUMod.Roles;
using UnityEngine;

namespace AUMod {
class RoleInfo {
    public Color color;
    public string name;
    public string introDescription;
    public string shortDescription;
    public RoleId roleId;

    RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId)
    {
        this.color = color;
        this.name = name;
        this.introDescription = introDescription;
        this.shortDescription = shortDescription;
        this.roleId = roleId;
    }

    public static RoleInfo sheriff = new RoleInfo(
        "Sheriff",
        Sheriff.color,
        "Shoot the <color=#FF1919FF>Impostors</color>",
        "Shoot the Impostors",
        RoleId.Sheriff);
    public static RoleInfo madmate = new RoleInfo(
        "Madmate",
        Madmate.color,
        "Help the <color=#FF1919FF>Impostors</color>",
        "Help the Impostors",
        RoleId.Madmate);
    public static RoleInfo evilHacker = new RoleInfo(
        "EvilHacker",
        EvilHacker.color,
        Helpers.cs(Palette.ImpostorRed, "Hack systems and kill everyone"),
        "Sabotage and kill everyone", RoleId.EvilHacker);
    public static RoleInfo impostor = new RoleInfo(
        "Impostor",
        Palette.ImpostorRed,
        Helpers.cs(Palette.ImpostorRed, "Sabotage and kill everyone"),
        "Sabotage and kill everyone", RoleId.Impostor);
    public static RoleInfo crewmate = new RoleInfo(
        "Crewmate", Color.white,
        "Find the Impostors",
        "Find the Impostors",
        RoleId.Crewmate);

    public static List<RoleInfo> allRoleInfos = new List<RoleInfo>() {
        impostor,
        crewmate,
        sheriff,
        madmate,
        evilHacker
    };

    public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p)
    {
        List<RoleInfo> infos = new List<RoleInfo>();
        if (p == null)
            return infos;

        // Special roles
        if (p == Sheriff.sheriff)
            infos.Add(sheriff);
        if (p == Madmate.madmate)
            infos.Add(madmate);
        if (p == EvilHacker.evilHacker)
            infos.Add(evilHacker);
        if (infos.Count == 0 && p.Data.Role.IsImpostor)
            infos.Add(impostor);
        if (infos.Count == 0 && !p.Data.Role.IsImpostor)
            infos.Add(crewmate);

        return infos;
    }

    public static String GetRole(PlayerControl p)
    {
        string roleName;
        roleName = String.Join("", getRoleInfoForPlayer(p).Select(x => x.name).ToArray());
        return roleName;
    }
}
}
