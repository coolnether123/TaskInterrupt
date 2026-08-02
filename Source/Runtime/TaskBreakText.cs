using TaskBreak.Domain;
using Verse;

namespace TaskBreak.Runtime
{
    internal static class TaskBreakText
    {
        internal static string Reason(TaskBreakBlockReason reason)
        {
            switch (reason)
            {
                case TaskBreakBlockReason.NotPlayerControlled:
                    return "TaskBreak_Reason_NotControlled".Translate();
                case TaskBreakBlockReason.NoCurrentTask:
                    return "TaskBreak_Reason_NoTask".Translate();
                case TaskBreakBlockReason.Incapacitated:
                    return "TaskBreak_Reason_Incapacitated".Translate();
                case TaskBreakBlockReason.MentalState:
                    return "TaskBreak_Reason_MentalState".Translate();
                case TaskBreakBlockReason.Drafted:
                    return "TaskBreak_Reason_Drafted".Translate();
                case TaskBreakBlockReason.Deathrest:
                    return "TaskBreak_Reason_Deathrest".Translate();
                case TaskBreakBlockReason.FormingCaravan:
                    return "TaskBreak_Reason_Caravan".Translate();
                case TaskBreakBlockReason.OrganizedActivity:
                    return "TaskBreak_Reason_Organized".Translate();
                case TaskBreakBlockReason.GameProtected:
                    return "TaskBreak_Reason_GameProtected".Translate();
                case TaskBreakBlockReason.MustComplete:
                    return "TaskBreak_Reason_MustComplete".Translate();
                case TaskBreakBlockReason.QuestOwned:
                    return "TaskBreak_Reason_Quest".Translate();
                case TaskBreakBlockReason.RitualOwned:
                    return "TaskBreak_Reason_Ritual".Translate();
                case TaskBreakBlockReason.Labor:
                    return "TaskBreak_Reason_Labor".Translate();
                case TaskBreakBlockReason.MedicalCare:
                    return "TaskBreak_Reason_Medical".Translate();
                case TaskBreakBlockReason.ActivationCooldown:
                    return "TaskBreak_Reason_Cooldown".Translate();
                default:
                    return string.Empty;
            }
        }
    }
}
