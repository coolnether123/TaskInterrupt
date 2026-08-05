using Spine.Api;
using Spine.UI.SettingsFramework;
using TaskInterrupt.Patches;
using TaskInterrupt.Settings;
using Verse;

namespace TaskInterrupt.Bootstrap
{
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
}
