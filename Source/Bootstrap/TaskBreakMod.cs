using Spine.Api;
using Spine.UI.SettingsFramework;
using TaskBreak.Patches;
using TaskBreak.Settings;
using Verse;

namespace TaskBreak.Bootstrap
{
    public sealed class TaskBreakMod : SpineMod<TaskBreakSettings>
    {
        public TaskBreakMod(ModContentPack content)
            : base(
                content,
                "CoolNether123.TaskBreak",
                new SemanticVersion(1, 0, 0),
                TaskBreakSettingsRegistry.Definitions,
                SpineCapability.HarmonyPatching,
                new ModSettingsPageOptions { RowHeight = 38f })
        {
            TaskBreakPatches.Install();
        }

        protected override string SettingsCategoryLabel =>
            "TaskBreak_Name".Translate();
    }
}
