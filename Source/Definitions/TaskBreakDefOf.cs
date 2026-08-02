using RimWorld;
using Verse;

namespace TaskBreak.Definitions
{
    [DefOf]
    internal static class TaskBreakDefOf
    {
#pragma warning disable CS0649 // Assigned by RimWorld's DefOf loader.
        public static KeyBindingDef TaskBreak_CancelCurrentTask;
#pragma warning restore CS0649

        static TaskBreakDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TaskBreakDefOf));
        }
    }
}
