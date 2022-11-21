using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AUMod.Roles;
using static AUMod.GameHistory;
using UnityEngine;

namespace AUMod.Patches
{
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.PickRandomConsoles))]
    public static class NormalPlayerTaskPickRandomConsolesPatch {
        public static void Postfix(NormalPlayerTask __instance, TaskTypes taskType, byte[] consoleIds)
        {
            if (taskType != TaskTypes.FixWiring)
                return;

            List<Console> wiringTasks = ShipStatus.Instance.AllConsoles.Where((global::Console t) => t.TaskTypes.Contains(taskType)).ToList<global::Console>();
            for (int i = 0; i < __instance.Data.Length; i++) {
                int index = Roles.rnd.Next(0, wiringTasks.Count);
                __instance.Data[i] = (byte)wiringTasks[index].ConsoleId;
                wiringTasks.RemoveAt(index);
            }
        }
    }
}
