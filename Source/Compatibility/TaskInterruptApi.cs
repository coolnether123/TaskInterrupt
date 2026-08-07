using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace TaskInterrupt.Compatibility
{
    /// <summary>
    /// Resolves the small set of RimWorld API seams that changed across the
    /// supported engine generations. Gameplay policy remains in the domain
    /// layer; this adapter only translates the live engine object model.
    /// </summary>
    internal static class TaskInterruptApi
    {
        private const BindingFlags InstanceMembers =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticMembers =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static bool IsPlayerControlled(Pawn pawn)
        {
            return pawn != null &&
                (ReadBool(pawn, "IsColonistPlayerControlled") ||
                 ReadBool(pawn, "IsColonyMechPlayerControlled") ||
                 ReadBool(pawn, "UnderPlayerControl"));
        }

        internal static Job CurrentJob(Pawn pawn)
        {
            return ReadMember(ReadMember(pawn, "jobs"), "curJob") as Job;
        }

        internal static bool IsDead(Pawn pawn)
        {
            return ReadBool(pawn, "Dead");
        }

        internal static bool IsDowned(Pawn pawn)
        {
            return ReadBool(pawn, "Downed");
        }

        internal static bool IsRestUntilHealed(Job job)
        {
            return ReadBool(job, "restUntilHealed");
        }

        internal static bool IsPlayerForced(Job job)
        {
            return ReadBool(job, "playerForced");
        }

        internal static Pawn TargetPawn(Job job)
        {
            object target = ReadMember(job, "targetA");
            return ReadMember(target, "Pawn") as Pawn;
        }

        internal static bool IsDrafted(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            bool? direct = ReadNullableBool(pawn, "Drafted");
            if (direct.HasValue)
            {
                return direct.Value;
            }

            object controller = ReadMember(pawn, "playerController") ??
                ReadMember(pawn, "drafter");
            return ReadBool(controller, "Drafted");
        }

        internal static bool IsMentalState(Pawn pawn)
        {
            return ReadBool(pawn, "InMentalState");
        }

        internal static bool IsDeathresting(Pawn pawn)
        {
            return ReadBool(pawn, "Deathresting");
        }

        internal static bool IsFormingCaravan(Pawn pawn)
        {
            object result = InvokeStaticPawnMethod("IsFormingCaravan", pawn);
            return result is bool && (bool)result;
        }

        internal static bool HasLord(Pawn pawn)
        {
            return InvokeStaticPawnMethod("GetLord", pawn) != null;
        }

        internal static bool IsCurrentJobPlayerInterruptible(Pawn pawn)
        {
            object jobs = ReadMember(pawn, "jobs");
            if (jobs == null)
            {
                return false;
            }

            MethodInfo method = jobs.GetType().GetMethod(
                "IsCurrentJobPlayerInterruptible",
                InstanceMembers,
                null,
                Type.EmptyTypes,
                null);
            if (method == null)
            {
                return false;
            }

            object result = method.Invoke(jobs, null);
            return result is bool && (bool)result;
        }

        internal static bool HasForceCompleteBeforeNextJob(Job job)
        {
            return ReadBool(ReadMember(job, "def"), "forceCompleteBeforeNextJob");
        }

        internal static bool HasQuest(Job job)
        {
            return ReadMember(job, "quest") != null;
        }

        internal static bool HasRitualTag(Job job)
        {
            object tag = ReadMember(job, "ritualTag");
            return tag != null && !string.IsNullOrEmpty(tag.ToString());
        }

        internal static bool IsInLabor(Pawn pawn)
        {
            object health = ReadMember(pawn, "health") ??
                ReadMember(pawn, "healthTracker");
            object hediffSet = ReadMember(health, "hediffSet");
            if (hediffSet == null)
            {
                return false;
            }

            MethodInfo method = hediffSet.GetType().GetMethod(
                "InLabor",
                InstanceMembers,
                null,
                Type.EmptyTypes,
                null);
            object result = method?.Invoke(hediffSet, null);
            return result is bool && (bool)result;
        }

        internal static bool IsMedicalJob(Job job)
        {
            if (job == null)
            {
                return false;
            }

            object def = ReadMember(job, "def");
            string defName = ReadString(def, "defName");
            if (string.Equals(defName, "TendPatient", StringComparison.Ordinal))
            {
                return true;
            }

            object workGiver = ReadMember(job, "workGiverDef");
            object workType = ReadMember(workGiver, "workType");
            if (string.Equals(
                ReadString(workType, "defName"),
                "Doctor",
                StringComparison.Ordinal))
            {
                return true;
            }

            object bill = ReadMember(job, "bill");
            return bill != null &&
                bill.GetType().Name.IndexOf(
                    "Medical",
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool IsInBed(Pawn pawn)
        {
            MethodInfo method = pawn?.GetType().GetMethod(
                "InBed",
                InstanceMembers,
                null,
                Type.EmptyTypes,
                null);
            object result = method?.Invoke(pawn, null);
            return result is bool && (bool)result;
        }

        internal static Map MapHeld(Pawn pawn)
        {
            return ReadMember(pawn, "MapHeld") as Map ??
                ReadMember(pawn, "Map") as Map;
        }

        internal static IEnumerable<Pawn> SpawnedPawns(Map map)
        {
            object mapPawns = ReadMember(map, "mapPawns");
            object pawns = ReadMember(mapPawns, "AllPawnsSpawned") ??
                ReadMember(mapPawns, "SpawnedPawnsInFaction");
            IEnumerable enumerable = pawns as IEnumerable;
            if (enumerable == null)
            {
                yield break;
            }

            foreach (object item in enumerable)
            {
                Pawn pawn = item as Pawn;
                if (pawn != null)
                {
                    yield return pawn;
                }
            }
        }

        internal static List<Pawn> SelectedPawns()
        {
            List<Pawn> pawns = new List<Pawn>();
            object selector = ReadStaticMember(typeof(Find), "Selector");
            object selected = ReadMember(selector, "SelectedPawns") ??
                ReadMember(selector, "SelectedObjects");
            IEnumerable enumerable = selected as IEnumerable;
            if (enumerable == null)
            {
                return pawns;
            }

            foreach (object item in enumerable)
            {
                Pawn pawn = item as Pawn;
                if (pawn != null)
                {
                    pawns.Add(pawn);
                }
            }

            return pawns;
        }

        internal static int ThingId(Pawn pawn)
        {
            object value = ReadMember(pawn, "thingIDNumber");
            return value is int ? (int)value : 0;
        }

        internal static int CurrentTick()
        {
            object tickManager = ReadStaticMember(typeof(Find), "TickManager");
            object value = ReadMember(tickManager, "TicksGame");
            return value is int ? (int)value : 0;
        }

        internal static bool EndCurrentJob(Pawn pawn)
        {
            object jobs = ReadMember(pawn, "jobs");
            if (jobs == null)
            {
                return false;
            }

            MethodInfo[] methods = jobs.GetType().GetMethods(InstanceMembers);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != "EndCurrentJob")
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length < 1 ||
                    !parameters[0].ParameterType.IsEnum)
                {
                    continue;
                }

                object condition = ParseEnum(
                    parameters[0].ParameterType,
                    "InterruptForced",
                    "ForcedInterrupt");
                if (condition == null)
                {
                    continue;
                }

                object[] arguments = new object[parameters.Length];
                arguments[0] = condition;
                for (int parameterIndex = 1;
                    parameterIndex < parameters.Length;
                    parameterIndex++)
                {
                    ParameterInfo parameter = parameters[parameterIndex];
                    if (parameter.HasDefaultValue)
                    {
                        arguments[parameterIndex] = parameter.DefaultValue;
                    }
                    else if (parameter.ParameterType.IsValueType)
                    {
                        arguments[parameterIndex] = Activator.CreateInstance(
                            parameter.ParameterType);
                    }
                }
                method.Invoke(jobs, arguments);
                return true;
            }

            return false;
        }

        internal static void LookBool(ref bool value, string label, bool defaultValue)
        {
            MethodInfo method = null;
            string[] names = { "Look", "LookValue" };
            Type valuesType = typeof(Scribe_Values);
            for (int i = 0; i < names.Length && method == null; i++)
            {
                MethodInfo[] methods = valuesType.GetMethods(StaticMembers);
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo candidate = methods[j];
                    ParameterInfo[] parameters = candidate.GetParameters();
                    if (candidate.Name == names[i] &&
                        parameters.Length >= 2 &&
                        parameters[0].ParameterType == typeof(bool).MakeByRefType() &&
                        parameters[1].ParameterType == typeof(string))
                    {
                        method = candidate;
                        break;
                    }
                }
            }

            if (method == null)
            {
                return;
            }

            ParameterInfo[] signature = method.GetParameters();
            object[] arguments = new object[signature.Length];
            arguments[0] = value;
            arguments[1] = label;
            if (signature.Length > 2)
            {
                arguments[2] = defaultValue;
            }
            if (signature.Length > 3)
            {
                arguments[3] = false;
            }
            method.Invoke(null, arguments);
            if (arguments[0] is bool)
            {
                value = (bool)arguments[0];
            }
        }

        internal static void MarkPlayerInterruptedForced(Pawn pawn)
        {
            object job = ReadMember(ReadMember(pawn, "jobs"), "curJob");
            SetMember(job, "playerInterruptedForced", true);
        }

        private static object InvokeStaticPawnMethod(string name, Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            Type[] types;
            try
            {
                types = pawn.GetType().Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException error)
            {
                types = error.Types;
            }

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type == null)
                {
                    continue;
                }

                MethodInfo[] methods = type.GetMethods(StaticMembers);
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo method = methods[j];
                    ParameterInfo[] parameters = method.GetParameters();
                    if (method.Name == name &&
                        parameters.Length == 1 &&
                        parameters[0].ParameterType.IsAssignableFrom(pawn.GetType()))
                    {
                        return method.Invoke(null, new object[] { pawn });
                    }
                }
            }

            return null;
        }

        private static object ReadStaticMember(Type type, string name)
        {
            PropertyInfo property = type.GetProperty(name, StaticMembers);
            if (property != null)
            {
                return property.GetValue(null, null);
            }

            FieldInfo field = type.GetField(name, StaticMembers);
            return field?.GetValue(null);
        }

        private static object ReadMember(object instance, string name)
        {
            if (instance == null)
            {
                return null;
            }

            Type type = instance as Type ?? instance.GetType();
            BindingFlags flags = instance is Type ? StaticMembers : InstanceMembers;
            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property.GetValue(instance is Type ? null : instance, null);
            }

            FieldInfo field = type.GetField(name, flags);
            return field?.GetValue(instance is Type ? null : instance);
        }

        private static bool ReadBool(object instance, string name)
        {
            bool? value = ReadNullableBool(instance, name);
            return value.HasValue && value.Value;
        }

        private static bool? ReadNullableBool(object instance, string name)
        {
            object value = ReadMember(instance, name);
            return value is bool ? (bool?)value : null;
        }

        private static string ReadString(object instance, string name)
        {
            object value = ReadMember(instance, name);
            return value == null ? null : value.ToString();
        }

        private static void SetMember(object instance, string name, object value)
        {
            if (instance == null)
            {
                return;
            }

            PropertyInfo property = instance.GetType().GetProperty(name, InstanceMembers);
            if (property != null && property.CanWrite)
            {
                property.SetValue(instance, value, null);
                return;
            }

            FieldInfo field = instance.GetType().GetField(name, InstanceMembers);
            if (field != null)
            {
                field.SetValue(instance, value);
            }
        }

        private static object ParseEnum(Type enumType, params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                try
                {
                    return Enum.Parse(enumType, names[i]);
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }
    }
}
