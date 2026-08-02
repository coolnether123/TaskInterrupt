namespace TaskBreak.Domain
{
    public static class TaskBreakPolicy
    {
        public static TaskBreakDecision Evaluate(TaskBreakFacts facts)
        {
            if (!facts.IsPlayerControlled)
            {
                return Blocked(TaskBreakBlockReason.NotPlayerControlled);
            }

            if (!facts.HasCurrentTask)
            {
                return Blocked(TaskBreakBlockReason.NoCurrentTask);
            }

            if (facts.IsIncapacitated)
            {
                return Blocked(TaskBreakBlockReason.Incapacitated);
            }

            if (facts.IsInMentalState)
            {
                return Blocked(TaskBreakBlockReason.MentalState);
            }

            if (facts.IsDrafted)
            {
                return Blocked(TaskBreakBlockReason.Drafted);
            }

            if (facts.IsDeathresting)
            {
                return Blocked(TaskBreakBlockReason.Deathrest);
            }

            if (facts.IsFormingCaravan)
            {
                return Blocked(TaskBreakBlockReason.FormingCaravan);
            }

            if (facts.HasOrganizedLord)
            {
                return Blocked(TaskBreakBlockReason.OrganizedActivity);
            }

            if (!facts.IsPlayerInterruptible)
            {
                return Blocked(TaskBreakBlockReason.GameProtected);
            }

            if (facts.MustCompleteBeforeNextTask)
            {
                return Blocked(TaskBreakBlockReason.MustComplete);
            }

            if (facts.IsQuestOwned)
            {
                return Blocked(TaskBreakBlockReason.QuestOwned);
            }

            if (facts.IsRitualOwned)
            {
                return Blocked(TaskBreakBlockReason.RitualOwned);
            }

            if (facts.IsInLabor)
            {
                return Blocked(TaskBreakBlockReason.Labor);
            }

            if (facts.IsMedicalCare)
            {
                return Blocked(TaskBreakBlockReason.MedicalCare);
            }

            return new TaskBreakDecision(
                TaskBreakBlockReason.None,
                facts.IsPlayerForced);
        }

        private static TaskBreakDecision Blocked(
            TaskBreakBlockReason reason)
        {
            return new TaskBreakDecision(reason);
        }
    }
}
