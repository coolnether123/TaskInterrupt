using System;
using TaskInterrupt.Domain;
using Verse;

namespace TaskInterrupt.Runtime
{
    /// <summary>
    /// Single point of truth for player-facing wording. Every key passes
    /// through <see cref="Key"/>, so Goofy mode is one substitution rather than
    /// a conditional at each call site.
    /// </summary>
    internal static class TaskInterruptText
    {
        private const string Prefix = "TaskInterrupt_";
        private const string GoofyPrefix = "TaskInterrupt_Goofy_";

        /// <summary>
        /// Returns the Goofy variant of <paramref name="key"/> when Goofy mode
        /// is on and that variant is actually translated, otherwise the key
        /// itself. A translation that only covers the plain keys therefore
        /// degrades to plain wording rather than to a red missing-key string.
        /// </summary>
        internal static string Key(string key)
        {
            if (string.IsNullOrEmpty(key) ||
                !GoofyMode.Active ||
                !key.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return key;
            }

            string goofy = GoofyPrefix + key.Substring(Prefix.Length);
#if TASK_INTERRUPT_NO_MOD_API
            return key;
#else
            return goofy.CanTranslate() ? goofy : key;
#endif
        }

        internal static string Translate(string key)
        {
            return Key(key).Translate().ToString();
        }

        internal static string Translate(string key, params object[] args)
        {
            string translated = Translate(key);
            return args == null || args.Length == 0
                ? translated
                : string.Format(translated, args);
        }

        internal static string Reason(TaskInterruptBlockReason reason)
        {
            string key = ReasonKey(reason);
            return key.Length == 0 ? string.Empty : Translate(key);
        }

        private static string ReasonKey(TaskInterruptBlockReason reason)
        {
            switch (reason)
            {
                case TaskInterruptBlockReason.NotPlayerControlled:
                    return "TaskInterrupt_Reason_NotControlled";
                case TaskInterruptBlockReason.NoCurrentTask:
                    return "TaskInterrupt_Reason_NoTask";
                case TaskInterruptBlockReason.Incapacitated:
                    return "TaskInterrupt_Reason_Incapacitated";
                case TaskInterruptBlockReason.MentalState:
                    return "TaskInterrupt_Reason_MentalState";
                case TaskInterruptBlockReason.Drafted:
                    return "TaskInterrupt_Reason_Drafted";
                case TaskInterruptBlockReason.Deathrest:
                    return "TaskInterrupt_Reason_Deathrest";
                case TaskInterruptBlockReason.FormingCaravan:
                    return "TaskInterrupt_Reason_Caravan";
                case TaskInterruptBlockReason.OrganizedActivity:
                    return "TaskInterrupt_Reason_Organized";
                case TaskInterruptBlockReason.GameProtected:
                    return "TaskInterrupt_Reason_GameProtected";
                case TaskInterruptBlockReason.MustComplete:
                    return "TaskInterrupt_Reason_MustComplete";
                case TaskInterruptBlockReason.QuestOwned:
                    return "TaskInterrupt_Reason_Quest";
                case TaskInterruptBlockReason.RitualOwned:
                    return "TaskInterrupt_Reason_Ritual";
                case TaskInterruptBlockReason.Labor:
                    return "TaskInterrupt_Reason_Labor";
                case TaskInterruptBlockReason.MedicalCare:
                    return "TaskInterrupt_Reason_Medical";
                case TaskInterruptBlockReason.ActivationCooldown:
                    return "TaskInterrupt_Reason_Cooldown";
                default:
                    return string.Empty;
            }
        }
    }
}
