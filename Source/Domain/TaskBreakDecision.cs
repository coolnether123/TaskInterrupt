namespace TaskBreak.Domain
{
    public enum TaskBreakBlockReason
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

    public readonly struct TaskBreakDecision
    {
        public TaskBreakDecision(
            TaskBreakBlockReason blockReason,
            bool requiresForcedConfirmation = false)
        {
            BlockReason = blockReason;
            RequiresForcedConfirmation =
                blockReason == TaskBreakBlockReason.None &&
                requiresForcedConfirmation;
        }

        public TaskBreakBlockReason BlockReason { get; }

        public bool CanBreak => BlockReason == TaskBreakBlockReason.None;

        public bool RequiresForcedConfirmation { get; }
    }
}
