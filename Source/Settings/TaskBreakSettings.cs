using Spine.Api;
using Verse;

namespace TaskBreak.Settings
{
    public sealed class TaskBreakSettings : ModSettings
    {
        public bool ShowGizmo = true;
        public bool ConfirmForcedTasks = true;

        public override void ExposeData()
        {
            SpineApi.Settings.Scribe(
                this,
                TaskBreakSettingsRegistry.Definitions);
            base.ExposeData();
        }
    }
}
