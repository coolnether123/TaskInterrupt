using TaskInterrupt.Patches;
using TaskInterrupt.Settings;
using UnityEngine;
using Verse;

#if TASK_INTERRUPT_USE_SPINE
using Spine.Api;
using Spine.UI.SettingsFramework;
#endif

namespace TaskInterrupt.Bootstrap
{
    /// <summary>
    /// Composes Spine-owned settings with the mod's single Harmony integration,
    /// keeping startup concerns out of interruption policy and presentation.
    /// </summary>
#if TASK_INTERRUPT_USE_SPINE
    public sealed class TaskInterruptMod : SpineMod<TaskInterruptSettings>
    {
        public TaskInterruptMod(ModContentPack content)
            : base(
                content,
                "CoolNether123.TaskInterrupt",
                new SemanticVersion(1, 0, 0),
                TaskInterruptSettingsRegistry.Definitions,
                SpineCapability.HarmonyPatching,
                new ModSettingsPageOptions { RowHeight = 38f })
        {
            TaskInterruptPatches.Install();
        }

        // Deliberately not routed through TaskInterruptText: Goofy mode renames
        // the command and its messages, not the mod. Someone looking for the
        // settings page should always find it under the same name.
        protected override string SettingsCategoryLabel =>
            "TaskInterrupt_Name".Translate();
    }
#elif !TASK_INTERRUPT_NO_MOD_API
    public sealed class TaskInterruptMod : Mod
    {
        public static TaskInterruptSettings Settings { get; private set; }

        public TaskInterruptMod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<TaskInterruptSettings>();
            TaskInterruptPatches.Install();
        }

        public override string SettingsCategory()
        {
            return "TaskInterrupt_Name".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.Draw(inRect);
        }
    }
#elif TASK_INTERRUPT_STATIC_BOOTSTRAP
    [StaticConstructorOnStartup]
    internal static class TaskInterruptMod
    {
        internal static readonly TaskInterruptSettings Settings =
            new TaskInterruptSettings();

        static TaskInterruptMod()
        {
            TaskInterruptPatches.Install();
        }
    }
#else
    internal static class TaskInterruptMod
    {
        internal static readonly TaskInterruptSettings Settings =
            new TaskInterruptSettings();
    }
#endif
}
