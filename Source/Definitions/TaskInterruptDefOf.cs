using RimWorld;
using Verse;

namespace TaskInterrupt.Definitions
{
    [DefOf]
    internal static class TaskInterruptDefOf
    {
#pragma warning disable CS0649 // Assigned by RimWorld's DefOf loader.
        public static KeyBindingDef TaskInterrupt_CancelCurrentTask;
#pragma warning restore CS0649

        static TaskInterruptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TaskInterruptDefOf));
        }
    }
}
