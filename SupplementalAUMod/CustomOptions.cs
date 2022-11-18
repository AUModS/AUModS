using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using System.Text;
using static AUMod.Roles;

namespace AUMod {
public enum CustomOptionType {
    General,
    Role,
    RoleAdvanced
}

public class CustomOptionHolder {
    public static string[] rates = new string[] { "0%", "100%" };
    public static string[] presets = new string[] { "Preset 1" };

    public static CustomOption adminMaxTimeRemaining;
    public static CustomOption camerasMaxTimeRemaining;
    public static CustomOption vitalsMaxTimeRemaining;
    public static CustomOption heliSabotageSystemTimeLimit;

    public static CustomOption sheriffSpawnRate;
    public static CustomOption sheriffCooldown;
    public static CustomOption sheriffNumberOfShots;
    public static CustomOption sheriffCanKillNeutrals;
    public static CustomOption sheriffCanKillCrewmates;

    public static CustomOption madmateSpawnRate;
    public static CustomOption madmateCanDieToSheriff;
    public static CustomOption madmateCanEnterVents;
    public static CustomOption madmateHasImpostorVision;
    public static CustomOption madmateCanFixComm;
    public static CustomOption madmateExileCrewmate;
    public static CustomTasksOption madmateTasks;

    public static CustomOption evilHackerSpawnRate;

    public static string cs(Color c, string s)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    public static void Load()
    {

        adminMaxTimeRemaining = CustomOption.Create(100, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Admin Map Available Time"), CustomOptionType.General, 10f, 0f, 120f, 1f);
        camerasMaxTimeRemaining = CustomOption.Create(101, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Cameras Available Time"), CustomOptionType.General, 10f, 0f, 120f, 1f);
        vitalsMaxTimeRemaining = CustomOption.Create(102, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Vitals Available Time"), CustomOptionType.General, 10f, 0f, 120f, 1f);
        heliSabotageSystemTimeLimit = CustomOption.Create(103, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Time Limit of Avert Crash Time"), CustomOptionType.General, 90f, 5f, 120f, 5f);

        sheriffSpawnRate = CustomOption.Create(110, cs(Sheriff.color, "Sheriff"), CustomOptionType.Role, rates, null, true);
        sheriffCooldown = CustomOption.Create(111, "Sheriff Cooldown", CustomOptionType.RoleAdvanced, 30f, 10f, 60f, 2.5f, sheriffSpawnRate);
        sheriffNumberOfShots = CustomOption.Create(112, "Sheriff Number Of Shots", CustomOptionType.RoleAdvanced, 1f, 1f, 15f, 1f, sheriffSpawnRate);
        sheriffCanKillNeutrals = CustomOption.Create(113, "Sheriff Can Kill Neutrals", CustomOptionType.RoleAdvanced, false, sheriffSpawnRate);
        sheriffCanKillCrewmates = CustomOption.Create(114, "Sheriff Can Kill Crewmates", CustomOptionType.RoleAdvanced, false, sheriffSpawnRate);

        madmateSpawnRate = CustomOption.Create(120, cs(Madmate.color, "Madmate"), CustomOptionType.Role, rates, null, true);
        madmateCanDieToSheriff = CustomOption.Create(121, "Madmate Can Die To Sheriff", CustomOptionType.RoleAdvanced, true, madmateSpawnRate);
        madmateCanEnterVents = CustomOption.Create(122, "Madmate Can Enter Vents", CustomOptionType.RoleAdvanced, true, madmateSpawnRate);
        madmateHasImpostorVision = CustomOption.Create(123, "Madmate Has Impostor Vision", CustomOptionType.RoleAdvanced, true, madmateSpawnRate);
        madmateCanFixComm = CustomOption.Create(124, "Madmate Can Fix Comm", CustomOptionType.RoleAdvanced, false, madmateSpawnRate);
        madmateExileCrewmate = CustomOption.Create(125, "Exile a Crewmate where Madmate is Exiled", CustomOptionType.RoleAdvanced, false, madmateSpawnRate);
        madmateTasks = new CustomTasksOption(126, CustomOptionType.RoleAdvanced, madmateSpawnRate);

        evilHackerSpawnRate = CustomOption.Create(130, cs(EvilHacker.color, "EvilHacker"), CustomOptionType.Role, rates, null, true);
    }
}

public class CustomOption {
    public static List<CustomOption> options = new List<CustomOption>();
    public static List<CustomOption> roleOptions = new List<CustomOption>();
    public static List<CustomOption> roleAdvancedOptions = new List<CustomOption>();
    public static int preset = 0;

    public int id;
    public string name;
    public System.Object[] selections;

    public int defaultSelection;
    public ConfigEntry<int> entry;
    public int selection;
    public OptionBehaviour optionBehaviour;
    public CustomOption parent;
    public bool isHeader;

    // Option creation
    public CustomOption()
    {
    }

    public CustomOption(
        int id,
        string name,
        CustomOptionType optionType,
        System.Object[] selections,
        System.Object defaultValue,
        CustomOption parent,
        bool isHeader)
    {
        this.id = id;
        this.name = parent == null ? name : "- " + name;
        this.selections = selections;
        int index = Array.IndexOf(selections, defaultValue);
        this.defaultSelection = index >= 0 ? index : 0;
        this.parent = parent;
        this.isHeader = isHeader;
        selection = 0;

        // id 0 is preset selection in TheOtherRoles
        if (id != 0) {
            entry = AUModPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
            selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
        }
        options.Add(this);
        /*
         * TODO
        switch (optionType) {
        case CustomOptionType.General:
            options.Add(this);
            break;
        case CustomOptionType.Role:
            roleOptions.Add(this);
            break;
        case CustomOptionType.RoleAdvanced:
            roleAdvancedOptions.Add(this);
            break;
        }
         */
    }

    public static CustomOption Create(
        int id,
        string name,
        CustomOptionType optionType,
        string[] selections,
        CustomOption parent = null,
        bool isHeader = false)
    {
        return new CustomOption(id, name, optionType, selections, "", parent, isHeader);
    }

    public static CustomOption Create(
        int id,
        string name,
        CustomOptionType optionType,
        float defaultValue,
        float min,
        float max,
        float step,
        CustomOption parent = null,
        bool isHeader = false)
    {
        List<float> selections = new List<float>();
        for (float s = min; s <= max; s += step)
            selections.Add(s);
        return new CustomOption(id, name, optionType, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader);
    }

    public static CustomOption Create(
        int id,
        string name,
        CustomOptionType optionType,
        bool defaultValue,
        CustomOption parent = null,
        bool isHeader = false)
    {
        var ColoredOff = Helpers.cs(Color.red, "Off");
        var ColoredOn = Helpers.cs(Color.green, "On");
        return new CustomOption(id, name, optionType, new string[] { ColoredOff, ColoredOn }, defaultValue ? ColoredOn : ColoredOff, parent, isHeader);
    }

    public static void ShareOptionSelections()
    {
        if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null)
            return;

        foreach (CustomOption option in CustomOption.options) {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.ShareOptionSelection,
                Hazel.SendOption.Reliable);
            messageWriter.WritePacked((uint)option.id);
            messageWriter.WritePacked((uint)Convert.ToUInt32(option.selection));
            messageWriter.EndMessage();
        }
    }

    public static void ConfigureStringOption(ref StringOption stringOption, CustomOption option)
    {
        stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => {});
        stringOption.TitleText.text = option.name;
        stringOption.Value = stringOption.oldValue = option.selection;
        stringOption.ValueText.text = option.selections[option.selection].ToString();
    }

    public static void ConfigureRoleOptionsData(ref RoleOptionSetting roleOption, CustomOption option)
    {
    }

    public static CustomOption GetCustomOption(CustomOptionType type, OptionBehaviour optionBehaviour)
    {
        return CustomOption.options.FirstOrDefault(option => option.optionBehaviour == optionBehaviour);
        /*
         * TODO
        switch (type) {
        case CustomOptionType.General:
            return CustomOption.options.FirstOrDefault(option => option.optionBehaviour == optionBehaviour);
        case CustomOptionType.Role:
            return CustomOption.roleOptions.FirstOrDefault(option => option.optionBehaviour == optionBehaviour);
        case CustomOptionType.RoleAdvanced:
            return CustomOption.roleAdvancedOptions.FirstOrDefault(option => option.optionBehaviour == optionBehaviour);
        }
        return null;
         */
    }

    // Getter

    public int getSelection()
    {
        return selection;
    }

    public bool getBool()
    {
        return selection > 0;
    }

    public float getFloat()
    {
        return (float)selections[selection];
    }

    // Option changes

    public void updateSelection(int newSelection)
    {
        selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
        if (optionBehaviour != null && optionBehaviour is StringOption stringOption) {
            stringOption.oldValue = stringOption.Value = selection;
            stringOption.ValueText.text = selections[selection].ToString();

            if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) {
                if (entry != null)
                    entry.Value = selection; // Save selection to config

                ShareOptionSelections(); // Share all selections
            }
        }
    }
}

public class CustomTasksOption : CustomOption {
    public CustomOption commonTasksOption = null;
    public CustomOption longTasksOption = null;
    public CustomOption shortTasksOption = null;

    public int commonTasks
    {
        get {
            return commonTasksOption == null ? 0 : Mathf.RoundToInt(commonTasksOption.getSelection());
        }
    }

    public int longTasks
    {
        get {
            return longTasksOption == null ? 0 : Mathf.RoundToInt(longTasksOption.getSelection());
        }
    }

    public int shortTasks
    {
        get {
            return shortTasksOption == null ? 0 : Mathf.RoundToInt(shortTasksOption.getSelection());
        }
    }

    public List<byte> generateTasks()
    {
        return Helpers.generateTasks(commonTasks, shortTasks, longTasks);
    }

    public CustomTasksOption(int id, CustomOptionType type, CustomOption parent = null)
    {
        const int baseId = 10000;
        commonTasksOption = Create(id + baseId + 0, "numCommonTasks", type, 1f, 0f, 4f, 1f, parent);
        longTasksOption = Create(id + baseId + 1, "numLongTasks", type, 1f, 0f, 15f, 1f, parent);
        shortTasksOption = Create(id + baseId + 2, "numShortTasks", type, 1f, 0f, 23f, 1f, parent);
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
class GameOptionsMenuStartPatch {
    public static void Postfix(GameOptionsMenu __instance)
    {
        var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
        if (template == null)
            return;

        List<OptionBehaviour> allOptions = __instance.Children.ToList();
        for (int i = 0; i < CustomOption.options.Count; i++) {
            CustomOption option = CustomOption.options[i];
            if (option.optionBehaviour == null) {
                StringOption stringOption = UnityEngine.Object.Instantiate(template, template.transform.parent);
                allOptions.Add(stringOption);

                stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => {});
                stringOption.TitleText.text = option.name;
                stringOption.Value = stringOption.oldValue = option.selection;
                stringOption.ValueText.text = option.selections[option.selection].ToString();

                option.optionBehaviour = stringOption;
            }

            option.optionBehaviour.gameObject.SetActive(true);
        }

        // Modify number of tasks
        var commonTasksOption = allOptions.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
        if (commonTasksOption != null)
            commonTasksOption.ValidRange = new FloatRange(0f, 4f);

        var shortTasksOption = allOptions.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
        if (shortTasksOption != null)
            shortTasksOption.ValidRange = new FloatRange(0f, 23f);

        var longTasksOption = allOptions.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
        if (longTasksOption != null)
            longTasksOption.ValidRange = new FloatRange(0f, 15f);

        __instance.Children = allOptions.ToArray();
    }
}

[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Start))]
class RolesSettingsMenuPatch {
    public static void Postfix(RolesSettingsMenu __instance)
    {
        var template = UnityEngine.Object.FindObjectsOfType<RoleOptionSetting>().FirstOrDefault();
        if (template == null)
            return;

        List<OptionBehaviour> allOptions = __instance.Children.ToList();
        for (int i = 0; i < CustomOption.roleOptions.Count; i++) {
            CustomOption option = CustomOption.roleOptions[i];
            if (option.optionBehaviour == null) {
                RoleOptionSetting roleOption = UnityEngine.Object.Instantiate(template, template.transform.parent);
                allOptions.Add(roleOption);

                /* roleOption.UpdateValuesAndText(); */

                option.optionBehaviour = roleOption;
            }

            option.optionBehaviour.gameObject.SetActive(true);
        }

        __instance.Children = allOptions.ToArray();
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
public class StringOptionEnablePatch {
    public static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.GetCustomOption(CustomOptionType.General, __instance);
        if (option == null)
            return true;

        __instance.OnValueChanged = new Action<OptionBehaviour>((o) => {});
        __instance.TitleText.text = option.name;
        __instance.Value = __instance.oldValue = option.selection;
        __instance.ValueText.text = option.selections[option.selection].ToString();

        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
public class StringOptionIncreasePatch {
    public static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.GetCustomOption(CustomOptionType.General, __instance);
        if (option == null)
            return true;

        option.updateSelection(option.selection + 1);
        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
public class StringOptionDecreasePatch {
    public static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.GetCustomOption(CustomOptionType.General, __instance);
        if (option == null)
            return true;

        option.updateSelection(option.selection - 1);
        return false;
    }
}

[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.IncreaseCount))]
public class RoleOptionSettingIncreaseCountPatch {
    public static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.GetCustomOption(CustomOptionType.Role, __instance);
        if (option == null)
            return true;

        option.updateSelection(option.selection + 1);
        return false;
    }
}

[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.DecreaseCount))]
public class RoleOptionSettingDecreaseCountPatch {
    public static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.GetCustomOption(CustomOptionType.Role, __instance);
        if (option == null)
            return true;

        option.updateSelection(option.selection - 1);
        return false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public class RpcSyncSettingsPatch {
    public static void Postfix()
    {
        CustomOption.ShareOptionSelections();
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
public class GameOptionsMenuUpdatePatch {
    private static float timer = 1f;
    public static void Postfix(GameOptionsMenu __instance)
    {
        __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -0.5F + __instance.Children.Length * 0.55F;
        timer += Time.deltaTime;
        if (timer < 0.1f)
            return;
        timer = 0f;

        float offset = -7.85f;
        foreach (CustomOption option in CustomOption.options) {
            if (option?.optionBehaviour != null && option.optionBehaviour.gameObject != null) {
                bool enabled = true;
                var parent = option.parent;
                while (parent != null && enabled) {
                    enabled = parent.selection != 0;
                    parent = parent.parent;
                }
                option.optionBehaviour.gameObject.SetActive(enabled);
                if (enabled) {
                    offset -= option.isHeader ? 0.75f : 0.5f;
                    option.optionBehaviour.transform.localPosition = new Vector3(option.optionBehaviour.transform.localPosition.x, offset, option.optionBehaviour.transform.localPosition.z);
                }
            }
        }
    }
}
}
