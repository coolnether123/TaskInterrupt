using System.Collections.Generic;
using Spine.UI.SettingsFramework;

namespace TaskBreak.Settings
{
    internal static class TaskBreakSettingsRegistry
    {
        internal static readonly IReadOnlyList<SettingDefinition> Definitions =
            new[]
            {
                new SettingDefinition
                {
                    Id = "controls.header",
                    Type = SettingType.Header,
                    Label = "Controls",
                    LabelKey = "TaskBreak_Settings_Controls",
                    SortOrder = 0,
                    ShowInSimpleView = true
                },
                new SettingDefinition
                {
                    Id = "controls.gizmo",
                    FieldName = nameof(TaskBreakSettings.ShowGizmo),
                    ScribeKey = "showGizmo",
                    Type = SettingType.Bool,
                    Label = "Show task break command",
                    LabelKey = "TaskBreak_Settings_ShowGizmo",
                    TooltipKey = "TaskBreak_Settings_ShowGizmo_Tip",
                    DefaultValue = true,
                    SortOrder = 10,
                    ShowInSimpleView = true
                },
                new SettingDefinition
                {
                    Id = "safety.forced",
                    FieldName = nameof(
                        TaskBreakSettings.ConfirmForcedTasks),
                    ScribeKey = "confirmForcedTasks",
                    Type = SettingType.Bool,
                    Label = "Confirm forced tasks",
                    LabelKey = "TaskBreak_Settings_ConfirmForced",
                    TooltipKey = "TaskBreak_Settings_ConfirmForced_Tip",
                    DefaultValue = true,
                    SortOrder = 20,
                    ShowInSimpleView = true
                }
            };
    }
}
