namespace TaskInterrupt.Settings
{
#if TASK_INTERRUPT_USE_SPINE
    using Spine.UI.SettingsFramework;

    /// <summary>
    /// Declares the complete settings surface once for Spine to render, bind,
    /// and persist without a mod-specific settings window.
    /// </summary>
    internal static class TaskInterruptSettingsRegistry
    {
        internal static readonly SettingsSchema<TaskInterruptSettings> Schema =
            new SettingsSchema<TaskInterruptSettings>(
                SettingsSchemaConventions.LowerCamelCase);

        static TaskInterruptSettingsRegistry()
        {
            var controls = Schema.Section(
                "controls.header",
                "Controls",
                "TaskInterrupt_Settings_Controls");
            controls.Toggle("controls.gizmo", settings => settings.ShowGizmo,
                "Show Interrupt Task Button")
                .Localized("TaskInterrupt_Settings_ShowGizmo", "TaskInterrupt_Settings_ShowGizmo_Tip");
            controls.Toggle("safety.forced",
                settings => settings.ConfirmForcedTasks,
                "Show Confirm Dialog when interrupting forced tasks")
                .Localized("TaskInterrupt_Settings_ConfirmForced", "TaskInterrupt_Settings_ConfirmForced_Tip");
            // Cosmetic only. Advanced because nobody should meet it by
            // accident, and off by default because the plain wording is
            // what a first-time player needs to understand the command.
            controls.Toggle("fun.goofy", settings => settings.GoofyMode,
                "Goofy mode").AdvancedOnly()
                .Localized("TaskInterrupt_Settings_Goofy", "TaskInterrupt_Settings_Goofy_Tip");
        }
    }
#else
    internal static class TaskInterruptSettingsRegistry
    {
    }
#endif
}
