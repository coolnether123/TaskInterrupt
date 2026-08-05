using Spine.Api;
using Verse;

namespace TaskInterrupt.Settings
{
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
