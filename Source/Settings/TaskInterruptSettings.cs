using Spine.Api;
using Verse;

namespace TaskInterrupt.Settings
{
    /// <summary>
    /// Stores global presentation and confirmation preferences; interruption
    /// safety itself deliberately remains fixed policy rather than save data.
    /// </summary>
    public sealed class TaskInterruptSettings : ModSettings
    {
        public bool ShowGizmo = true;
        public bool ConfirmForcedTasks = true;
        public bool GoofyMode;

        public override void ExposeData()
        {
            SpineApi.Settings.Scribe(
                this,
                TaskInterruptSettingsRegistry.Definitions);
            base.ExposeData();
        }
    }
}
