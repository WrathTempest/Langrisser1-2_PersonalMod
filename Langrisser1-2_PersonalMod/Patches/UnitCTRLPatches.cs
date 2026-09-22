using GameCommon;
using HarmonyLib;
using Langrisser1_2_PersonalMod.Utils;
using System;
using System.Collections.Generic;

namespace Langrisser1_2_PersonalMod.Patches
{
    [HarmonyPatch]
    internal class UnitCTRLPatches
    {
        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.GetHP_MAX))]
        [HarmonyPostfix]
        public static void ModifyHPMax(UnitCTRL __instance, int flag, ref int __result)
        {
            int num = __instance.classData.HP_MAX + __instance.charactorData.HP_MAX;
            num += __instance.GetLevelData(4, 0);

            if (flag == 1)
            {                
                num += __instance.GetItemEffect(11);
                num += Helpers.ApplySkillEffect(__instance, 12, num);
            }

            // Original game scaling and difficulty modifiers
            if ((__instance.GetPlayerType() == 2 || __instance.GetPlayerType() == 3) &&
                (__instance.unitManager.gameSystemWork.sysData3 == 2 || __instance.unitManager.gameSystemWork.sysData3 == 3) &&
                __instance.unitManager.gameSystemWork.sysData2 >= 1)
            {
                int num2 = __instance.unitManager.gameSystemWork.sysData2 * 20;
                num += num * num2 / 100;
            }

            // Rounding down to nearest 10
            num -= num % 10;

            // Min/Max bounds clamping
            if (num <= 9)
            {
                num = 10;
            }
            if (num >= 10000)
            {
                num = 10000;
            }
            __result = num;

            if (Helpers.IsSkillLearned(__instance, "Legend"))
            {
                Helpers.LearnAllMagic(__instance);
            }
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.GetHP_MAXDisplay))]
        [HarmonyPostfix]
        public static void ModifyHPMaxDisplay(UnitCTRL __instance, int flag, ref int __result)
        {
            int num = __instance.classData.HP_MAX + __instance.charactorData.HP_MAX;
            num += __instance.GetLevelData(4, 0);

            if (flag == 1)
            {
                num += __instance.GetItemEffect(11);
                num += Helpers.ApplySkillEffect(__instance, 12, num);
            }

            // Original game scaling and difficulty modifiers
            if ((__instance.GetPlayerType() == 2 || __instance.GetPlayerType() == 3) &&
                (__instance.unitManager.gameSystemWork.sysData3 == 2 || __instance.unitManager.gameSystemWork.sysData3 == 3) &&
                __instance.unitManager.gameSystemWork.sysData2 >= 1)
            {
                int num2 = __instance.unitManager.gameSystemWork.sysData2 * 20;
                num += num * num2 / 100;
            }

            // Rounding down to nearest 10
            num -= num % 10;

            // Min/Max bounds clamping
            if (num <= 9)
            {
                num = 10;
            }
            if (num >= 10000)
            {
                num = 10000;
            }
            __result = num;
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.GetM_Attack))]
        [HarmonyPostfix]
        public static void ModifyMAttack(UnitCTRL __instance, int flag, ref int __result)
        {
            int num = __instance.classData.m_Attack + __instance.charactorData.m_Attack;
            num += __instance.GetLevelData(2, 0);
            if (flag == 0 || flag == 1)
            {
                num += __instance.GetItemEffect(8);
                num += Helpers.ApplySkillEffect(__instance, 8, num);
            }
            if (flag == 1)
            {
                if (__instance.GetUnitCategory() != 0)
                {
                    int commandEffect = __instance.GetCommandEffect(2, 1);
                    num += num * commandEffect / 100;
                }
                num += __instance.GetMagicEffect(10);
            }
            if (num <= 0)
            {
                num = 1;
            }
            if (num >= 1000)
            {
                num = 999;
            }
            __result = num;
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.GetM_Defense))]
        [HarmonyPostfix]
        public static void ModifyMDefense(UnitCTRL __instance, int flag, ref int __result)
        {
            int num = __instance.classData.m_Defense + __instance.charactorData.m_Defense;
            num += __instance.GetLevelData(3, 0);
            if (flag == 0 || flag == 1)
            {
                num += __instance.GetItemEffect(9);
                num += Helpers.ApplySkillEffect(__instance, 9, num);
            }
            if (flag == 1)
            {
                if (__instance.GetUnitCategory() != 0)
                {
                    int commandEffect = __instance.GetCommandEffect(3, 1);
                    num += num * commandEffect / 100;
                }
                num += __instance.GetMagicEffect(11);
            }
            if (num <= 0)
            {
                num = 1;
            }
            if (num >= 1000)
            {
                num = 999;
            }
            __result = num;
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.GetP_Attack))]
        [HarmonyPrefix]
        public static bool ModifyPAttack(UnitCTRL __instance, int flag, ref int __result)
        {
            int num = __instance.classData.p_Attack + __instance.charactorData.p_Attack;
            num += __instance.GetLevelData(0, 0);
            if (flag == 0 || flag == 1)
            {
                num += __instance.GetItemEffect(6);
                num += Helpers.ApplySkillEffect(__instance, 6, num);
            }
            if (flag == 1)
            {
                if (__instance.GetUnitCategory() != 0)
                {
                    int commandEffect = __instance.GetCommandEffect(0, 1);
                    num += num * commandEffect / 100;
                }
                num += __instance.GetMagicEffect(8);
            }
            if (num <= 0)
            {
                num = 1;
            }
            if (num >= 1000)
            {
                num = 999;
            }
            __result = num;
            return false;
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.GetP_Defense))]
        [HarmonyPostfix]
        public static void ModifyPDefense(UnitCTRL __instance, int flag, ref int __result)
        {
            int num = __instance.classData.p_Defense + __instance.charactorData.p_Defense;
            num += __instance.GetLevelData(1, 0);
            if (flag == 0 || flag == 1)
            {
                num += __instance.GetItemEffect(7);
                num += Helpers.ApplySkillEffect(__instance, 7, num);
            }
            if (flag == 1)
            {
                if (__instance.GetUnitCategory() != 0)
                {
                    int commandEffect = __instance.GetCommandEffect(1, 1);
                    num += num * commandEffect / 100;
                }
                num += __instance.GetMagicEffect(9);
            }
            if (num <= 0)
            {
                num = 1;
            }
            if (num >= 1000)
            {
                num = 999;
            }
            __result = num;
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.GetMoveRange))]
        [HarmonyPostfix]
        public static void ModifyMovement(UnitCTRL __instance, int flag, ref int __result)
        {
            int num = __instance.classData.moveRange;
            if (flag == 0 || flag == 1)
            {
                num += __instance.GetSkillEffect(10);
                num += __instance.GetItemEffect(10);
            }
            if (flag == 1)
            {
                if (__instance.GetUnitCategory() != 0 && __instance.unitWork.memberList[0] > -1)
                {
                    if (Helpers.GetSkillData(__instance.unitManager.GetUnitCTRL((int)__instance.unitWork.memberList[0]), "Legend", out SkillData skillData))
                    {
                        num += skillData.move;
                    }
                }
                int magicEffect = __instance.GetMagicEffect(14);
                num += magicEffect;
                
            }
            if (num <= -1)
            {
                num = 0;
            }
            if (num >= 31)
            {
                num = 30;
            }
            __result = num;
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.AddClassPoint))]
        [HarmonyPrefix]
        public static void ClassPointMult(ref int data)
        {
            data = data < 0 ? data : data * 2;
        }

        [HarmonyPatch(typeof(UnitCTRL), nameof(UnitCTRL.AddStageMoney))]
        [HarmonyPrefix]
        public static void StageMoneyMult(ref int data)
        {
            data = data < 0 ? data : data * 2;
        }
    }
}