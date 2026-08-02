using Spine.Api;
using Spine.UI.ContextualSettings;
using Spine.UI.SettingsFramework;
using TaskBreak.Patches;
using TaskBreak.Settings;
using UnityEngine;
using Verse;

namespace TaskBreak.Bootstrap
{
    public sealed class TaskBreakMod : Mod
    {
        private readonly TaskBreakSettings settings;
        private IModSettingsPage settingsPage;

        public TaskBreakMod(ModContentPack content)
            : base(content)
        {
            SpineApi.Runtime.Require(new SpineRequirement(
                "CoolNether123.TaskBreak",
                new SemanticVersion(1, 2, 0),
                SpineCapability.Settings |
                SpineCapability.HarmonyPatching |
                SpineCapability.ContextualSettings |
                SpineCapability.ModSettingsPages));

            settings = GetSettings<TaskBreakSettings>();
            Settings = settings;
            Instance = this;
            TaskBreakPatches.Install();
        }

        private static TaskBreakMod Instance { get; set; }

        public static TaskBreakSettings Settings { get; private set; }

        internal static IContextualSettingsLease ContextualSettings =>
            Instance?.GetSettingsPage().ContextualSettings;

        public override string SettingsCategory()
        {
            return "TaskBreak_Name".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettingsPage().Draw(inRect);
        }

        private IModSettingsPage GetSettingsPage()
        {
            if (settingsPage == null)
            {
                settingsPage = SpineApi.Settings.Acquire(
                    "CoolNether123.TaskBreak",
                    this,
                    settings,
                    TaskBreakSettingsRegistry.Definitions,
                    WriteSettings,
                    new ModSettingsPageOptions { RowHeight = 38f });
            }

            return settingsPage;
        }
    }
}
