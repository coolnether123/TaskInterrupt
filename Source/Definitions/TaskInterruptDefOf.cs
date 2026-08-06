using RimWorld;
using Verse;

namespace TaskInterrupt.Definitions
{
    /// <summary>
    /// Provides the native keybinding definition through RimWorld's DefOf
    /// lifecycle so input remains owned by the game's command system.
    /// </summary>
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
