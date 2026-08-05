namespace TaskInterrupt.Domain
{
    public enum TaskInterruptBlockReason
    {
        None = 0,
        NotPlayerControlled,
        NoCurrentTask,
        Incapacitated,
        MentalState,
        Drafted,
        Deathrest,
        FormingCaravan,
        OrganizedActivity,
        GameProtected,
        MustComplete,
        QuestOwned,
        RitualOwned,
        Labor,
        MedicalCare,
        ActivationCooldown
    }

    public readonly struct TaskInterruptDecision
    {
        public TaskInterruptDecision(
            TaskInterruptBlockReason blockReason,
            bool requiresForcedConfirmation = false)
        {
            BlockReason = blockReason;
            RequiresForcedConfirmation =
                blockReason == TaskInterruptBlockReason.None &&
                requiresForcedConfirmation;
        }

        public TaskInterruptBlockReason BlockReason { get; }

        public bool CanBreak => BlockReason == TaskInterruptBlockReason.None;

        public bool RequiresForcedConfirmation { get; }
    }
}
