namespace TaskInterrupt.Domain
{
    /// <summary>
    /// Applies the fail-closed interruption rules without touching RimWorld
    /// objects, UI, or job state.
    /// </summary>
    public static class TaskInterruptPolicy
    {
        public static TaskInterruptDecision Evaluate(TaskInterruptFacts facts)
        {
            if (!facts.IsPlayerControlled)
            {
                return Blocked(TaskInterruptBlockReason.NotPlayerControlled);
            }

            if (!facts.HasCurrentTask)
            {
                return Blocked(TaskInterruptBlockReason.NoCurrentTask);
            }

            if (facts.IsIncapacitated)
            {
                return Blocked(TaskInterruptBlockReason.Incapacitated);
            }

            if (facts.IsInMentalState)
            {
                return Blocked(TaskInterruptBlockReason.MentalState);
            }

            if (facts.IsDrafted)
            {
                return Blocked(TaskInterruptBlockReason.Drafted);
            }

            if (facts.IsDeathresting)
            {
                return Blocked(TaskInterruptBlockReason.Deathrest);
            }

            if (facts.IsFormingCaravan)
            {
                return Blocked(TaskInterruptBlockReason.FormingCaravan);
            }

            if (facts.HasOrganizedLord)
            {
                return Blocked(TaskInterruptBlockReason.OrganizedActivity);
            }

            if (!facts.IsPlayerInterruptible)
            {
                return Blocked(TaskInterruptBlockReason.GameProtected);
            }

            if (facts.MustCompleteBeforeNextTask)
            {
                return Blocked(TaskInterruptBlockReason.MustComplete);
            }

            if (facts.IsQuestOwned)
            {
                return Blocked(TaskInterruptBlockReason.QuestOwned);
            }

            if (facts.IsRitualOwned)
            {
                return Blocked(TaskInterruptBlockReason.RitualOwned);
            }

            if (facts.IsInLabor)
            {
                return Blocked(TaskInterruptBlockReason.Labor);
            }

            if (facts.IsMedicalCare)
            {
                return Blocked(TaskInterruptBlockReason.MedicalCare);
            }

            return new TaskInterruptDecision(
                TaskInterruptBlockReason.None,
                facts.IsPlayerForced);
        }

        private static TaskInterruptDecision Blocked(
            TaskInterruptBlockReason reason)
        {
            return new TaskInterruptDecision(reason);
        }
    }
}
