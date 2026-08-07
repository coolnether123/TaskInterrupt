#if TASK_INTERRUPT_HAS_DEFOF
using System;
using System.Reflection;
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
            MethodInfo ensure = typeof(DefOfHelper).GetMethod(
                "EnsureInitializedInCtor",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(Type) },
                null);
            if (ensure != null)
            {
                ensure.Invoke(null, new object[] { typeof(TaskInterruptDefOf) });
            }
        }
    }
}
#endif
