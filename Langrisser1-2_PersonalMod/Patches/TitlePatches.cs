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
    internal class TitlePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BackUpDataFunction), nameof(BackUpDataFunction.PC_Check_Game))]
        public static void LogSaveCheckErrors(ref int __result, string _Name, BackUpDataFunction __instance)
        {
            int num = 0;
            MainPlugin.Log.LogError($"In PC_CheckGame!");
            try
            {
                if (File.Exists(_Name))
                {
                    byte[] encryptedData = File.ReadAllBytes(_Name);
                    byte[] decryptedData = (byte[])Helpers.Call(__instance, "Rfc_DeAESlize", encryptedData);
                    using (MemoryStream memoryStream = new MemoryStream(decryptedData))
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        GameBackupData gameBackupData = (GameBackupData)binaryFormatter.Deserialize(memoryStream);
                        int majicnumber = Helpers.GetPrivateField<int>(__instance, "majicnumber");
                        if (gameBackupData != null && gameBackupData.flag == majicnumber)
                        {
                            num = 2;
                        }
                    }
                }
            }
            catch
            {
                num = 0;
            }
        }

        [HarmonyPatch(typeof(TitleMain), nameof(TitleMain.Update_Main))]
        [HarmonyPostfix]
        public static void ApplyCustomSkill(TitleMain __instance)
        {
            if (__instance.playerCharaCTRL == null) return;
            if (__instance.unitManager == null) return;
            if (__instance.titleQuestionRewardData == null) return;
            List<int> skillIDlist = new List<int>()
            {
                __instance.playerCharaCTRL.charactorData.skillData1,
                __instance.playerCharaCTRL.charactorData.skillData2
            };
            List<int> magicIDlist = new List<int>()
            {
                __instance.playerCharaCTRL.charactorData.magicData1,
                __instance.playerCharaCTRL.charactorData.magicData2
            };
            foreach (int skill in skillIDlist)
            {
                if (!Helpers.IsSkillLearned(__instance.playerCharaCTRL, skill))
                {
                    __instance.playerCharaCTRL.LearnSkill(skill);
                }
            }
            foreach (int magic in magicIDlist)
            {
                if (!Helpers.IsMagicLearned(__instance.playerCharaCTRL, magic))
                {
                    __instance.playerCharaCTRL.LearnMagic(magic);
                }
            }
        }
    }
}