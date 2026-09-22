using GameCommon;
using HarmonyLib;
using Langrisser1_2_PersonalMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;

namespace Langrisser1_2_PersonalMod.Patches
{
    [HarmonyPatch]
    internal class WindowManagerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WindowManager), nameof(WindowManager.SetResultTop))]
        public static void AllowMultipleSummons(List<UnitCTRL> unitlist)
        {
            foreach (var unit in unitlist)
            {
                unit.AddClassPoint(5);
            }
        }

    }
}