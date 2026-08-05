namespace TaskInterrupt.Domain
{
    public readonly struct TaskInterruptFacts
    {
        public TaskInterruptFacts(
            bool isPlayerControlled,
            bool hasCurrentTask,
            bool isIncapacitated,
            bool isInMentalState,
            bool isDrafted,
            bool isDeathresting,
            bool isFormingCaravan,
            bool hasOrganizedLord,
            bool isPlayerInterruptible,
            bool mustCompleteBeforeNextTask,
            bool isQuestOwned,
            bool isRitualOwned,
            bool isInLabor,
            bool isMedicalCare,
            bool isPlayerForced)
        {
            IsPlayerControlled = isPlayerControlled;
            HasCurrentTask = hasCurrentTask;
            IsIncapacitated = isIncapacitated;
            IsInMentalState = isInMentalState;
            IsDrafted = isDrafted;
            IsDeathresting = isDeathresting;
            IsFormingCaravan = isFormingCaravan;
            HasOrganizedLord = hasOrganizedLord;
            IsPlayerInterruptible = isPlayerInterruptible;
            MustCompleteBeforeNextTask = mustCompleteBeforeNextTask;
            IsQuestOwned = isQuestOwned;
            IsRitualOwned = isRitualOwned;
            IsInLabor = isInLabor;
            IsMedicalCare = isMedicalCare;
            IsPlayerForced = isPlayerForced;
        }

        public bool IsPlayerControlled { get; }
        public bool HasCurrentTask { get; }
        public bool IsIncapacitated { get; }
        public bool IsInMentalState { get; }
        public bool IsDrafted { get; }
        public bool IsDeathresting { get; }
        public bool IsFormingCaravan { get; }
        public bool HasOrganizedLord { get; }
        public bool IsPlayerInterruptible { get; }
        public bool MustCompleteBeforeNextTask { get; }
        public bool IsQuestOwned { get; }
        public bool IsRitualOwned { get; }
        public bool IsInLabor { get; }
        public bool IsMedicalCare { get; }
        public bool IsPlayerForced { get; }
    }
}
