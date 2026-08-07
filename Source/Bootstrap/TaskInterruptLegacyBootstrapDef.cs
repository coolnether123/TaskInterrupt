using TaskInterrupt.Patches;
using Verse;

namespace TaskInterrupt.Bootstrap
{
    /// <summary>
    /// Alpha 4 has no native Mod class or startup-attribute dispatcher. Def
    /// construction is the stable load hook on that engine generation; newer
    /// versions simply construct this harmless marker after their normal Mod
    /// entry point has already installed the patch.
    /// </summary>
    public sealed class TaskInterruptLegacyBootstrapDef : Def
    {
        public TaskInterruptLegacyBootstrapDef()
        {
#if TASK_INTERRUPT_NO_MOD_API
            TaskInterruptPatches.Install();
#endif
        }
    }
}
