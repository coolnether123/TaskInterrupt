using System.Collections.Generic;
using Spine.UI.SettingsFramework;

namespace TaskBreak.Settings
{
    internal static class TaskBreakSettingsRegistry
    {
        internal static readonly IReadOnlyList<SettingDefinition> Definitions =
            new[]
            {
                SettingDefinitions.Header(
                    "controls.header",
                    "Controls",
                    "TaskBreak_Settings_Controls"),
                SettingDefinitions.Toggle(
                    "controls.gizmo",
                    nameof(TaskBreakSettings.ShowGizmo),
                    "Show task break command",
                    "TaskBreak_Settings_ShowGizmo",
                    tooltipKey: "TaskBreak_Settings_ShowGizmo_Tip",
                    scribeKey: "showGizmo"),
                SettingDefinitions.Toggle(
                    "safety.forced",
                    nameof(TaskBreakSettings.ConfirmForcedTasks),
                    "Confirm forced tasks",
                    "TaskBreak_Settings_ConfirmForced",
                    tooltipKey: "TaskBreak_Settings_ConfirmForced_Tip",
                    scribeKey: "confirmForcedTasks")
            };
    }
}
