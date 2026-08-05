using Spine.Api;
using Spine.UI.SettingsFramework;
using TaskInterrupt.Patches;
using TaskInterrupt.Runtime;
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

        // Goofy mode renames the settings entry too. The mod list itself still
        // reads About.xml, which is loaded once at startup and cannot follow a
        // runtime toggle.
        protected override string SettingsCategoryLabel =>
            TaskInterruptText.Translate("TaskInterrupt_Name");
    }
}
