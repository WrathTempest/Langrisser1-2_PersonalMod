using GameCommon;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Langrisser1_2_PersonalMod.Utils
{
    internal static class Helpers
    {
        public static int ApplySkillEffect(UnitCTRL unit, int flag, int baseStat)
        {
            if (unit == null || unit.unitWork == null || unit.unitManager == null)
                return baseStat;

            int flatBonus = 0;
            int percentBonus = 0;

            for (int i = 0; i < 2; i++)
            {
                int skillId = unit.unitWork.skillSelect[i];
                if (skillId <= -1) continue;

                SkillData skillData = unit.unitManager.GetSkillData(skillId);
                if (skillData == null) continue;

                int rawValue = GetRawSkillValue(skillData, flag);

                if (rawValue >= 1000)
                {
                    percentBonus += (rawValue - 1000);
                }
                else if (rawValue > 0)
                {
                    flatBonus += rawValue;
                }
            }

            // Apply flat additions first, then percentage multiplier
            int total = baseStat + flatBonus;
            if (percentBonus > 0)
            {
                total += (total * percentBonus) / 100;
            }

            return total - baseStat;
        }
        public static void GetSkillEffectSplit(UnitCTRL unit, int flag, out int flatBonus, out int percentBonus)
        {
            flatBonus = 0;
            percentBonus = 0;

            if (unit == null || unit.unitWork == null || unit.unitManager == null) return;

            for (int i = 0; i < 2; i++)
            {
                int skillId = unit.unitWork.skillSelect[i];
                if (skillId <= -1) continue;

                SkillData skillData = unit.unitManager.GetSkillData(skillId);
                if (skillData == null) continue;

                // Extract raw value based on flag
                int rawValue = GetRawSkillValue(skillData, flag);

                if (rawValue >= 1000)
                {
                    percentBonus += (rawValue - 1000); // 1100 becomes +100%
                }
                else if (rawValue > 0)
                {
                    flatBonus += rawValue;             // 50 becomes +50 flat
                }
            }
        }

        private static int GetRawSkillValue(SkillData skillData, int flag)
        {
            switch (flag)
            {
                case 6: return skillData.unitAT;
                case 7: return skillData.unitDF;
                case 8: return skillData.unitMAT;
                case 9: return skillData.unitMDF;
                case 12: return skillData.HP;
                case 13: return skillData.MP;
                default: return 0;
            }
        }
        /// Unlocks all magic spells (IDs 0 to 127).
        public static void LearnAllMagic(UnitCTRL unit)
        {
            if (unit == null || unit.unitWork == null) return;

            // Setting ulong.MaxValue sets all 64 bits to 1
            unit.unitWork.magicFlag1 = ulong.MaxValue;
            unit.unitWork.magicFlag2 = ulong.MaxValue;
        }

        /// Unlocks all skills (IDs 0 to 127) and optionally fills empty equipment slots.
        public static void LearnAllSkills(UnitCTRL unit, bool autoEquipIfEmpty = true)
        {
            if (unit == null || unit.unitWork == null) return;

            // Setting ulong.MaxValue sets all 64 bits to 1
            unit.unitWork.skillFlag1 = ulong.MaxValue;
            unit.unitWork.skillFlag2 = ulong.MaxValue;

            // Mimics original LearnSkill behavior: equips skills 0 and 1 if active slots are empty
            if (autoEquipIfEmpty && unit.unitWork.skillSelect != null)
            {
                if (unit.unitWork.skillSelect[0] == -1)
                {
                    unit.unitWork.skillSelect[0] = 0;
                }
                if (unit.unitWork.skillSelect.Length > 1 && unit.unitWork.skillSelect[1] == -1)
                {
                    unit.unitWork.skillSelect[1] = 1;
                }
            }
        }

        /// Unlocks both all magic and all skills for a unit.
        public static void LearnAll(UnitCTRL unit)
        {
            LearnAllMagic(unit);
            LearnAllSkills(unit);
        }
        /// <summary>
        /// Checks if a unit has learned a specific magic spell.
        /// </summary>
        public static bool IsMagicLearned(UnitCTRL unit, int magicNumber)
        {
            if (unit == null || unit.unitWork == null || magicNumber < 0 || magicNumber >= 128)
            {
                return false;
            }

            if (magicNumber < 64)
            {
                ulong mask = 1UL << magicNumber;
                return (unit.unitWork.magicFlag1 & mask) != 0;
            }
            else
            {
                ulong mask = 1UL << (magicNumber - 64);
                return (unit.unitWork.magicFlag2 & mask) != 0;
            }
        }

        /// <summary>
        /// Checks if a unit has learned a specific skill.
        /// </summary>
        public static bool IsSkillLearned(UnitCTRL unit, int skillNumber)
        {
            if (unit == null || unit.unitWork == null || skillNumber < 0 || skillNumber >= 128)
            {
                return false;
            }

            if (skillNumber < 64)
            {
                ulong mask = 1UL << skillNumber;
                return (unit.unitWork.skillFlag1 & mask) != 0;
            }
            else
            {
                ulong mask = 1UL << (skillNumber - 64);
                return (unit.unitWork.skillFlag2 & mask) != 0;
            }
        }
        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            // Use AccessTools to get the field
            return AccessTools.FieldRefAccess<T>(instance.GetType(), fieldName)(instance);
        }
        public static void SetPrivateField<T>(object instance, string fieldName, T newValue)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            Type type = instance.GetType();
            FieldInfo field = null;

            while (type != null)
            {
                field = type.GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (field != null)
                    break;

                type = type.BaseType;
            }

            if (field == null)
                throw new MissingFieldException(instance.GetType().FullName, fieldName);

            field.SetValue(instance, newValue);
        }

        public static T GetPrivateProperty<T>(object instance, string propertyName)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();

            // Try property first
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (prop != null)
            {
                return (T)prop.GetValue(instance, null);
            }

            // Fallback: try getter method directly (get_PropertyName)
            var getter = type.GetMethod(
                "get_" + propertyName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (getter != null)
            {
                return (T)getter.Invoke(instance, null);
            }

            throw new MissingMemberException(type.FullName, propertyName);
        }

        public static void SetPrivateProperty<T>(object instance, string propertyName, T value)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();

            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (prop != null)
            {
                prop.SetValue(instance, value, null);
                return;
            }

            var setter = type.GetMethod(
                "set_" + propertyName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (setter != null)
            {
                setter.Invoke(instance, new object[] { value });
                return;
            }

            throw new MissingMemberException(type.FullName, propertyName);
        }

        public static string GetRealCaller(int skipFrames = 1)
        {
            var stackTrace = new StackTrace(skipFrames, true);
            foreach (var frame in stackTrace.GetFrames())
            {
                MethodBase method = frame.GetMethod();
                if (method == null) continue;

                // Skip Harmony-generated dynamic methods
                if (method.Name.Contains("DMD<")) continue;

                // Skip helper class itself
                if (method.DeclaringType == typeof(Helpers)) continue;

                return $"{method.DeclaringType.FullName}.{method.Name}";
            }

            return "UnknownCaller";
        }

        /// <summary>
        /// Logs a traced call for debugging.
        /// </summary>
        public static void LogCaller(string message = "", int skipFrames = 1)
        {
            string caller = GetRealCaller(skipFrames + 1); // +1 for hero.battleDataBehaviour.battleData method
            //Main.Log.LogInfo($"{message} Called by: {caller}");
        }

        public static object Call(
    object instance,
    string methodName,
    Type[] paramTypes,
    params object[] args)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Type type = instance.GetType();

            MethodInfo method = AccessTools.Method(type, methodName, paramTypes);

            if (method == null)
                throw new MissingMethodException(type.FullName, methodName);

            return method.Invoke(instance, args);
        }

        public static object Call(
        object instance,
        string methodName,
        params object[] args)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Type type = instance.GetType();

            MethodInfo method = AccessTools.Method(type, methodName);

            if (method == null)
                throw new MissingMethodException(type.FullName, methodName);

            return method.Invoke(instance, args);
        }

        public static object CallStatic(
            Type type,
            string methodName,
            params object[] args)
        {
            MethodInfo method = AccessTools.Method(type, methodName);

            if (method == null)
                throw new MissingMethodException(type.FullName, methodName);

            return method.Invoke(null, args);
        }
    }
}
