namespace TaskInterrupt.Domain
{
    /// <summary>
    /// Identifies the first safety boundary that rejected an interruption so
    /// the pure policy can remain independent of localized player feedback.
    /// </summary>
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

    /// <summary>
    /// Carries the policy outcome and forced-work confirmation requirement as
    /// one immutable result shared by commands and batch execution.
    /// </summary>
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
