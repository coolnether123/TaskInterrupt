using System.Collections.Generic;
using Spine.UI.SettingsFramework;

namespace TaskInterrupt.Settings
{
    internal static class TaskInterruptSettingsRegistry
    {
        internal static readonly IReadOnlyList<SettingDefinition> Definitions =
            new[]
            {
                SettingDefinitions.Header(
                    "controls.header",
                    "Controls",
                    "TaskInterrupt_Settings_Controls"),
                SettingDefinitions.Toggle(
                    "controls.gizmo",
                    nameof(TaskInterruptSettings.ShowGizmo),
                    "Show task interrupt command",
                    "TaskInterrupt_Settings_ShowGizmo",
                    tooltipKey: "TaskInterrupt_Settings_ShowGizmo_Tip",
                    scribeKey: "showGizmo"),
                SettingDefinitions.Toggle(
                    "safety.forced",
                    nameof(TaskInterruptSettings.ConfirmForcedTasks),
                    "Confirm forced tasks",
                    "TaskInterrupt_Settings_ConfirmForced",
                    tooltipKey: "TaskInterrupt_Settings_ConfirmForced_Tip",
                    scribeKey: "confirmForcedTasks"),
                // Cosmetic only. Advanced because nobody should meet it by
                // accident, and off by default because the plain wording is
                // what a first-time player needs to understand the command.
                SettingDefinitions.Toggle(
                    "fun.goofy",
                    nameof(TaskInterruptSettings.GoofyMode),
                    "Goofy mode",
                    "TaskInterrupt_Settings_Goofy",
                    tooltipKey: "TaskInterrupt_Settings_Goofy_Tip",
                    simple: false,
                    scribeKey: "goofyMode")
            };
    }
}
