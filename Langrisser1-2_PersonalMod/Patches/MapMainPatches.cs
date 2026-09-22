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
    internal class MapMainPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapMain), nameof(MapMain.CheckMagicUse))]
        public static void AllowMultipleSummons(MapMain __instance, UnitCTRL unitctrl, int magicnumber, ref int __result)
        {
            if (!Helpers.IsSkillLearned(unitctrl, "Legend")) return;
            MagicData magicData = __instance.unitManager.GetMagicData(magicnumber);
            if (magicData.category != 9) return;
            __result = 1;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapMain), nameof(MapMain.MakeCommandWindowFlag))]
        public static void AllowMagicAfterMoving(int player, UnitCTRL unitctrl, MapInfo mapInfo, ref int __result)
        {
            // Only player-controlled units
            if (unitctrl.GetPlayerType() != 0)
            {
                return;
            }

            // Only care about the "already moved" state
            if (unitctrl.GetMoveState() != 2)
            {
                return;
            }

            // Respect any active magic-block effects, same as the original method does
            int magicBlockEffect = unitctrl.GetMagicEffect(0) + unitctrl.GetMagicEffect(2);
            if (magicBlockEffect != 0)
            {
                return;
            }

            // Don't add the bit twice if it's already set
            if ((__result & 4) != 0)
            {
                return;
            }

            int bootOption = Helpers.StaticHelpers.GetField<int>("StDebug", "bootOption");
            if (bootOption != 99 && unitctrl.GetMagicCount() >= 1)
            {
                __result += 4;
            }
        }

    }
}